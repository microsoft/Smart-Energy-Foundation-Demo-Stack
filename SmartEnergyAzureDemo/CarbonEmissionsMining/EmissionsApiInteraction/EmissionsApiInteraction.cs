// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace EmissionsApiInteraction
{
    using ApiInteraction;
    using ApiInteraction.Helper;

    /// <summary>
    /// Class which provides methods to retrieve data from the WattTime Emissions API (https://api.watttime.org/). Requires the caller to register 
    /// for their own WattTime API Key (https://api.watttime.org/plans/) and adhere to it's usage terms. 
    /// </summary>
    public class EmissionsApiInteraction
    {
        // Variables to manage self-throttling of calls to the API
        private readonly ApiInteractionHelper apiInteractionHelper;
        private string apiNameForThrottlingRecords = "WattTime";
        private SelfThrottlingMethod _selfThrottlingMethod;

        /// <summary>
        /// Create a new EmissionsApiInteraction object
        /// </summary>
        /// <param name="_selfThrottlingMethod">The method to use to limit calls to the API to below the given threshold. Options: {None, InMemoryCallRecollection, AzureTableStorageCallRecollection}</param>
        /// <param name="maxNumberOfCallsPerMinute">Maximum number of calls to make to the API per minute</param>
        public EmissionsApiInteraction(string _selfThrottlingMethod, int maxNumberOfCallsPerMinute = -1)
        {
            this.apiInteractionHelper = new ApiInteractionHelper(ApiInteractionHelper.parseSelfThrottlingMethodstring(_selfThrottlingMethod), maxNumberOfCallsPerMinute);
        }

        /// <summary>
        /// Retrieve latest marginal carbon data for a given region from the WattTime API
        /// </summary>
        /// <param name="WattTimeUrl">URL of the Watt Time API</param>
        /// <param name="regionAbbreviation">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="timeout">Optional Timeout value</param>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="wattTimeApiKey">>WattTime Api Key</param>
        /// <param name="customUrlParams">Optional customUrlParams to add to construct a custom query</param>
        /// <returns>The latest marginal carbon data for a given region from the WattTime API</returns>
        public List<MarginalCarbonResult.Result> GetMarginalCarbonResults(string WattTimeUrl, string regionAbbreviation, DateTime? startDateTime = null, DateTime? endDateTime = null, TimeSpan? timeout = null, string wattTimeApiKey = null, string customUrlParams = null)
        {
            var resultsList = new List<MarginalCarbonResult.Result>();
            const string subUrl = "marginal/";
            var apiQueryUrl = $"{WattTimeUrl}{subUrl}";
            string urlParameters = $"?ba={regionAbbreviation}&page_size=1000";


            urlParameters = AppendOptionalWattTimeFormattedStartAndEndDateTimeParameters(startDateTime, endDateTime, urlParameters);

            if (customUrlParams != null)
            {
                urlParameters = customUrlParams.StartsWith("&") ? $"{urlParameters}{customUrlParams}" : $"{urlParameters}&{customUrlParams}";
            }

            var webApiHelper = new WebApiSerializerHelper<MarginalCarbonResult.RootObject>();
            
            var response =
                this.apiInteractionHelper.ExecuteThrottledApiCall<MarginalCarbonResult.RootObject>(
                    timeout,
                    webApiHelper,
                    apiQueryUrl,
                    urlParameters,
                    wattTimeApiKey,
                    apiNameForThrottlingRecords);

            // Cycle through the next page until there are no more results
            resultsList = response.results;
            var nextPageUrl = response.next;
            while (nextPageUrl != null)
            {
                var furtherResults = this.apiInteractionHelper.ExecuteThrottledApiCall<MarginalCarbonResult.RootObject>(
                    timeout,
                    webApiHelper,
                    nextPageUrl,
                    null,
                    wattTimeApiKey,
                    apiNameForThrottlingRecords);
                
                resultsList.AddRange(furtherResults.results);
                nextPageUrl = furtherResults.next;
            }

            return resultsList;
        }

        /// <summary>
        /// Get Observed Marginal Carbon Results
        /// </summary>
        /// <param name="WattTimeUrl">URL of the Watt Time API</param>
        /// <param name="regionAbbreviation">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="timeout">Optional Timeout value</param>
        /// <param name="wattTimeApiKey">WattTime Api Key</param>
        /// <param name="customUrlParams">Optional customUrlParams to add to construct a custom query</param>
        /// <returns></returns>
        public List<MarginalCarbonResult.Result> GetObservedMarginalCarbonResults(
            string WattTimeUrl,
            string regionAbbreviation,
            DateTime? startDateTime = null,
            DateTime? endDateTime = null,
            TimeSpan? timeout = null,
            string wattTimeApiKey = null,
            string customUrlParams = null)
        {
            var customParams = "&market=RT5M";
            if (customUrlParams != null)
            {
                customParams = customUrlParams.StartsWith("&") ? $"{customParams}{customUrlParams}" : $"{customParams}&{customUrlParams}";
            }

            // First check for the most granular results available
            var fiveMinuteMarginalResults = GetMarginalCarbonResults(WattTimeUrl, regionAbbreviation, startDateTime, endDateTime, timeout, wattTimeApiKey, customParams);
            if (fiveMinuteMarginalResults.Any())
            {
                return fiveMinuteMarginalResults;
            }

            // There were no 5 minute Marginal results, try for hourly Marginal Values
            customParams = "&market=RTHR";
            if (customUrlParams != null)
            {
                customParams = customUrlParams.StartsWith("&") ? $"{customParams}{customUrlParams}" : $"{customParams}&{customUrlParams}";
            }
            var hourlyMarginalResults = GetMarginalCarbonResults(WattTimeUrl, regionAbbreviation, startDateTime, endDateTime, timeout, wattTimeApiKey, customParams);
            return hourlyMarginalResults;
        }

        /// <summary>
        /// Get Forecast Marginal Carbon Results
        /// </summary>
        /// <param name="WattTimeUrl">URL of the Watt Time API</param>
        /// <param name="regionAbbreviation">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="timeout">Optional Timeout value</param>
        /// <param name="wattTimeApiKey">WattTime Api Key</param>
        /// <param name="customUrlParams">Optional customUrlParams to add to construct a custom query</param>
        /// <returns></returns>
        public List<MarginalCarbonResult.Result> GetForecastMarginalCarbonResults(
            string WattTimeUrl,
            string regionAbbreviation,
            DateTime? startDateTime = null,
            DateTime? endDateTime = null,
            TimeSpan? timeout = null,
            string wattTimeApiKey = null,
            string customUrlParams = null)
        {
            var customParams = "&market=DAHR";
            if (customUrlParams != null)
            {
                customParams = customUrlParams.StartsWith("&")
                                   ? $"{customParams}{customUrlParams}"
                                   : $"{customParams}&{customUrlParams}";
            }

            return GetMarginalCarbonResults(WattTimeUrl, regionAbbreviation, startDateTime, endDateTime, timeout, wattTimeApiKey, customParams);
        }

        /// <summary>
        /// Retrieve latest Generation Mix and System Wide Emissions data for a given region from the WattTime API
        /// </summary>
        /// <param name="WattTimeUrl">URL of the Watt Time API</param>
        /// <param name="regionAbbreviation">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="timeout">Optional Timeout value</param>
        /// <param name="wattTimeApiKey">WattTime Api Key</param>
        /// <returns>Latest Generation Mix and System Wide Emissions data for a given region from the WattTime API</returns>
        public List<GenerationMixResultList.Result> GetGenerationMixAndSystemWideEmissionsResults(string WattTimeUrl, string regionAbbreviation, DateTime? startDateTime = null, DateTime? endDateTime = null, TimeSpan? timeout = null, string wattTimeApiKey = null)
        {
            var resultsList = new List<GenerationMixResultList.Result>();
            const string subUrl = "datapoints/";
            var apiQueryUrl = $"{WattTimeUrl}{subUrl}";
            string urlParameters = $"?ba={regionAbbreviation}&page_size=1000";

            urlParameters = AppendOptionalWattTimeFormattedStartAndEndDateTimeParameters(startDateTime, endDateTime, urlParameters);
            
            var webApiHelper = new WebApiSerializerHelper<GenerationMixResultList.RootObject>();
            var response =
                this.apiInteractionHelper.ExecuteThrottledApiCall<GenerationMixResultList.RootObject>(
                    timeout,
                    webApiHelper,
                    apiQueryUrl,
                    urlParameters,
                    wattTimeApiKey,
                    apiNameForThrottlingRecords);

            // Cycle through the next page until there are no more results
            resultsList = response.results;
            var nextPageUrl = response.next;
            while (nextPageUrl != null)
            {
                var furtherResults = this.apiInteractionHelper.ExecuteThrottledApiCall<GenerationMixResultList.RootObject>(
                    timeout,
                    webApiHelper,
                    nextPageUrl,
                    null,
                    wattTimeApiKey,
                    apiNameForThrottlingRecords);
                resultsList.AddRange(furtherResults.results);
                nextPageUrl = furtherResults.next;
            }

            return resultsList;
        }

        /// <summary>
        /// Retrieve the most recent 5 minutely real time marginal carbon data for a given region from the WattTime API
        /// </summary>
        /// <param name="WattTimeUrl">URL of the Watt Time API</param>
        /// <param name="regionAbbreviation">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="timeout">Optional Timeout value</param>
        /// <param name="wattTimeApiKey">WattTime Api Key</param>
        /// <returns>The most recent 5 minutely real time marginal carbon data for a given region from the WattTime API</returns>
        public MarginalCarbonResult.Result GetMostRecentMarginalCarbonEmissionsResult(string WattTimeUrl, string regionAbbreviation, TimeSpan? timeout = null, string wattTimeApiKey = null)
        {
            var customParams = "&market=RT5M";
            var response = GetMarginalCarbonResults(WattTimeUrl, regionAbbreviation, DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1), timeout, wattTimeApiKey, customParams);

            var sortedResponses = response.OrderByDescending(x => x.timestamp);

            return sortedResponses.FirstOrDefault();
        }

        /// <summary>
        /// Append the given start and end DateTime strings to Watt Time Query Url
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <param name="urlParameters"></param>
        /// <returns>The given Url String with the given start and end DateTime strings appended</returns>
        private static string AppendOptionalWattTimeFormattedStartAndEndDateTimeParameters(
            DateTime? startDateTime,
            DateTime? endDateTime,
            string urlParameters)
        {
            if (startDateTime != null)
            {
                // This line may throw a warning in debugging mode: "Additional information: A UTC DateTime is being converted to text in a format that is only correct for local times. 
                // This can happen when calling DateTime.ToString using the 'z' format specifier, which will include a local time zone offset in the output. In that case, either use the 'Z'..."
                // This conversion is intentional, and the warning can be ignored. You can modify your exception settings to ignore this specific Warning. The warning will not be thrown 
                // in a Release build, or when it runs on Azure. 
                urlParameters = $"{urlParameters}&start_at={startDateTime.Value:yyyy-MM-ddTHH\\:mm\\:sszzz}";
            }
            if (endDateTime != null)
            {
                // This line may throw a warning in debugging mode: "Additional information: A UTC DateTime is being converted to text in a format that is only correct for local times. 
                // This can happen when calling DateTime.ToString using the 'z' format specifier, which will include a local time zone offset in the output. In that case, either use the 'Z'..."
                // This conversion is intentional, and the warning can be ignored. You can modify your exception settings to ignore this specific Warning. The warning will not be thrown 
                // in a Release build, or when it runs on Azure. 
                urlParameters = $"{urlParameters}&end_at={endDateTime.Value:yyyy-MM-ddTHH\\:mm\\:sszzz}";
            }

            urlParameters = urlParameters.Replace("+", "%2B"); // Plus sign needs to be escaped when passing timezone data to WattTime
            return urlParameters;
        }

        /// <summary>
        /// Convert a value in LbsPerMWh to gCO2perkWh
        /// </summary>
        /// <param name="valueInLbsPerMWh"></param>
        /// <returns>The given value in LbsPerMWh converted into gCO2perkWh</returns>
        public double ConvertLbsPerMWhTo_GCo2PerkWh(double valueInLbsPerMWh)
        {
            const double OnePoundInGramms = 453.592;
            const int OneMWinKw = 1000;

            return valueInLbsPerMWh * OnePoundInGramms / OneMWinKw;
        }
    }
}
