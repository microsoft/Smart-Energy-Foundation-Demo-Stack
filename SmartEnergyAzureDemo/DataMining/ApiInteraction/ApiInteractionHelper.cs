// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace ApiInteraction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using ApiInteraction.Helper;

    using CentralLogger;

    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Enum representing different methods of throttling one's own calls to an API. 
    /// </summary>
    public enum SelfThrottlingMethod
    {
        /// <summary>
        /// Apply no throttling
        /// </summary>
        None,

        /// <summary>
        /// Maintain a list inside this ApiInteractionHelper class of all previous calls 
        /// to the API to avoid exceeding the maximum quota. Will not work where multiple ApiInteractionHelper
        /// instances are used to make calls with the same API key
        /// </summary>
        InMemoryCallRecollection,

        /// <summary>
        /// Maintain a list inside Azure Table Storage of all previous calls to the API
        /// </summary>
        AzureTableStorageCallRecollection,
    };

    /// <summary>
    /// This class encapsulates the calling of an API with a self-throttling method built in, to avoid running over the API's maximum usage allowance. 
    /// </summary>
    public class ApiInteractionHelper
    {
        //Variables to manage inMemoryCallRecollection
        private readonly int maxNumberOfCallsPerMinute;
        private List<DateTime> recentApiCalls = new List<DateTime>();
        private SelfThrottlingMethod _selfThrottlingMethod;

        /// <summary>
        /// Create a new ApiInteractionHelper object
        /// </summary>
        /// <param name="_selfThrottlingMethod">The method to use to limit calls to the API to below the given threshold. Options: {None, InMemoryCallRecollection, AzureTableStorageCallRecollection}</param>
        /// <param name="maxNumberOfCallsPerMinute">Maximum number of calls to make to the API per minute</param>
        public ApiInteractionHelper(SelfThrottlingMethod _selfThrottlingMethod = SelfThrottlingMethod.None,
            int maxNumberOfCallsPerMinute = -1)
        {
            this._selfThrottlingMethod = _selfThrottlingMethod;
            this.maxNumberOfCallsPerMinute = maxNumberOfCallsPerMinute;
        }

        /// <summary>
        /// Make a call to an API with the given details, and apply the specified self-throttling method
        /// </summary>
        /// <typeparam name="T">Type to desearalize the data returned into</typeparam>
        /// <param name="timeout">Optional time to give up after</param>
        /// <param name="webApiHelper">WebApiSerializerHelper of a given type</param>
        /// <param name="apiQueryUrl">URL of the API</param>
        /// <param name="urlParameters">Optional paramaters for the API call</param>
        /// <param name="apiKey">Optional APIKey for the API</param>
        /// <param name="apiName">Optional Name of the API for logging</param>
        /// <returns>Results of the API call, desearilized into the given object type</returns>
        public T ExecuteThrottledApiCall<T>(
            TimeSpan? timeout,
            WebApiSerializerHelper<T> webApiHelper,
            string apiQueryUrl, string urlParameters, string apiKey = null, string apiName = null)
        {
            switch (_selfThrottlingMethod)
            {
                case SelfThrottlingMethod.InMemoryCallRecollection:
                    // Check if we have hit our limit for number of calls to the API
                    while (!this.VerifyInMemoryThrottledCallCanProceed())
                    {
                        Logger.Information($"Delaying issueing call to {apiName} API to ensure API throttling isn't exceeded at {DateTime.UtcNow}", "ApiInteractionHelper: ExecuteThrottledApiCall()");
                        Thread.Sleep(500);
                    }

                    // Add this call to the call tracker
                    this.recentApiCalls.Add(DateTime.UtcNow);
                    break;

                case SelfThrottlingMethod.AzureTableStorageCallRecollection:
                    // Check if we have hit our limit for number of calls to the API
                    while (!this.VerifyAzureTableStorageThrottledCallCanProceed(apiKey, apiName))
                    {
                        Logger.Information($"Delaying issueing call to {apiName} API to ensure API throttling isn't exceeded at {DateTime.UtcNow}", "ApiInteractionHelper: ExecuteThrottledApiCall()");
                        Thread.Sleep(500);
                    }

                    // Add this call to the call tracker
                    AzureTableStorageHelper.LogApiCallToTableStorage(new ApiCallRecordTableEntity(apiKey, apiName));
                    break;

                case SelfThrottlingMethod.None:
                default:
                    // Apply no throttling - proceed with call
                    break;

            }

            var response = webApiHelper.GetHttpResponseContentAsType<T>(apiQueryUrl, urlParameters, timeout, apiKey).Result;
            return response;
        }

        /// <summary>
        /// Return true if the number of calls to the API has not exceeded the maximum number of calls per minute in the last minute
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool VerifyInMemoryThrottledCallCanProceed()
        {
            // First clean the recent Api Calls list
            this.recentApiCalls.RemoveAll(x => x < DateTime.UtcNow.AddMinutes(-1));

            // Now check if we have capacity in our allocation to make a call now
            if (this.recentApiCalls.Count >= this.maxNumberOfCallsPerMinute)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Return true if the number of calls to the API has not exceeded the maximum number of calls per minute in the last minute
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool VerifyAzureTableStorageThrottledCallCanProceed(string apiKey, string apiName)
        {
            // Now check if we have capacity in our allocation to make a call now
            var dateFromLastMinute = DateTime.UtcNow.AddMinutes(-1);
            var dateToLastMinute = DateTime.UtcNow.AddMinutes(1);
            var messages = AzureTableStorageHelper.RetrieveLogMessagesFromTableStorage(apiKey, dateFromLastMinute, dateToLastMinute);

            if (messages == null)
            {
                return true;
            }
            if (messages.Count() >= this.maxNumberOfCallsPerMinute)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Take a string representing a selfThrottlingMethod method and return the Enum object representing that method. 
        /// </summary>
        /// <param name="selfThrottlingMethod"></param>
        /// <returns></returns>
        public static SelfThrottlingMethod parseSelfThrottlingMethodstring(string selfThrottlingMethod)
        {
            switch (selfThrottlingMethod)
            {
                case "InMemoryCallRecollection":
                    return SelfThrottlingMethod.InMemoryCallRecollection;

                case "AzureTableStorageCallRecollection":
                    return SelfThrottlingMethod.AzureTableStorageCallRecollection;

                case "None":
                default:
                    return SelfThrottlingMethod.None;
            }
        }
    }

    /// <summary>
    /// A class representing a row in Azure Table storage to store each call to an API. Used to track the number of calls to the given API to ensure the number
    /// of calls doesn't exceed the specified number. 
    /// </summary>
    public class ApiCallRecordTableEntity : TableEntity
    {
        public ApiCallRecordTableEntity(string callerIdentifier, string apiName)
        {
            this.PartitionKey = callerIdentifier;
            this.RowKey = $"{DateTime.UtcNow.Ticks}-{Guid.NewGuid()}";
            this.ApiName = apiName;
        }

        public string ApiName { get; set; }
    }
}
