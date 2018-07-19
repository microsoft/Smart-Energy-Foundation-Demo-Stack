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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using System.Net;

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
        /// <param name="WattTimeUsername">WattTime Username</param>
        /// <param name="WattTimePassword">WattTime Password</param>
        /// <param name="timeout">Optional Timeout value</param>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="customUrlParams">Optional customUrlParams to add to construct a custom query</param>
        /// <returns>The latest marginal carbon data for a given region from the WattTime API</returns>
        public List<MarginalCarbonResultV2Api> GetMarginalCarbonResults(string WattTimeUrl, string regionAbbreviation, string WattTimeUsername, string WattTimePassword, DateTime? startDateTime = null, DateTime? endDateTime = null, TimeSpan? timeout = null, string customUrlParams = null)
        {
            var resultsList = new List<MarginalCarbonResultV2Api>();
            const string subUrl = "v2/data/";
            var apiQueryUrl = $"{WattTimeUrl}{subUrl}";
            string urlParameters = $"?ba={regionAbbreviation}&page_size=1000";


            var authToken = RetrieveWattTimeAuthToken(WattTimeUrl, WattTimeUsername, WattTimePassword);
            var authTokenKey = authToken.Token;


            urlParameters = AppendOptionalWattTimeFormattedStartAndEndDateTimeParameters(startDateTime, endDateTime, urlParameters, true);

            if (customUrlParams != null)
            {
                urlParameters = customUrlParams.StartsWith("&") ? $"{urlParameters}{customUrlParams}" : $"{urlParameters}&{customUrlParams}";
            }

            var webApiHelper = new WebApiSerializerHelper<List<MarginalCarbonResultV2Api>>();
            
            var response =
                this.apiInteractionHelper.ExecuteThrottledApiCallWithBearerAuthToken<List<MarginalCarbonResultV2Api>>(
                    timeout,
                    webApiHelper,
                    apiQueryUrl,
                    urlParameters,
                    authTokenKey,
                    apiNameForThrottlingRecords);
          
            return response;
        }

        /// <summary>
        /// Get Observed Marginal Carbon Results
        /// </summary>
        /// <param name="WattTimeUrl">URL of the Watt Time API</param>
        /// <param name="regionAbbreviation">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>#
        /// <param name="WattTimeUsername">WattTime Username</param>
        /// <param name="WattTimePassword">WattTime Password</param>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="timeout">Optional Timeout value</param>
        /// <param name="customUrlParams">Optional customUrlParams to add to construct a custom query</param>
        /// <returns></returns>
        public List<MarginalCarbonResultV2Api> GetObservedMarginalCarbonResults(
            string WattTimeUrl,
            string regionAbbreviation,
            string WattTimeUsername, 
            string WattTimePassword,
            DateTime? startDateTime = null,
            DateTime? endDateTime = null,
            TimeSpan? timeout = null,
            string customUrlParams = null)
        {
            string customParams = null;
            if (customUrlParams != null)
            {
                customParams = customUrlParams.StartsWith("&") ? $"{customParams}{customUrlParams}" : $"{customParams}&{customUrlParams}";
            }

            // First check for the most granular results available
            var fiveMinuteMarginalResults =  GetMarginalCarbonResults(WattTimeUrl, regionAbbreviation, WattTimeUsername, WattTimePassword, startDateTime, endDateTime, timeout, customParams);
            if (fiveMinuteMarginalResults.Any())
            {
                return fiveMinuteMarginalResults.Where(t=> t.datatype.Equals("MOER")).ToList();
            }
            else
            {
                return new List<MarginalCarbonResultV2Api>();
            }
        }

        /// <summary>
        /// Get Forecast Marginal Carbon Results
        /// </summary>
        /// <param name="WattTimeUrl">URL of the Watt Time API</param>
        /// <param name="regionAbbreviation">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="WattTimeUsername">WattTime Username</param>
        /// <param name="WattTimePassword">WattTime Password</param>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="timeout">Optional Timeout value</param>
        /// <param name="customUrlParams">Optional customUrlParams to add to construct a custom query</param>
        /// <returns></returns>
        public List<MarginalCarbonResultV2Api> GetForecastMarginalCarbonResults(
            string WattTimeUrl,
            string regionAbbreviation,
            string WattTimeUsername,
            string WattTimePassword,
            DateTime? startDateTime = null,
            DateTime? endDateTime = null,
            TimeSpan? timeout = null,
            string customUrlParams = null)
        {
            var customParams = "&market=DAHR";
            if (customUrlParams != null)
            {
                customParams = customUrlParams.StartsWith("&")
                                   ? $"{customParams}{customUrlParams}"
                                   : $"{customParams}&{customUrlParams}";
            }

            return GetMarginalCarbonResults(WattTimeUrl, regionAbbreviation, WattTimeUsername, WattTimePassword, startDateTime, endDateTime, timeout, customParams);
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
            const string subUrl = "v1/datapoints/";
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
        /// Retrieve latest Automated Emissions Reductions Index Result for a given region from the WattTime API
        /// </summary>
        /// <param name="WattTimeUrl">URL of the Watt Time API</param>
        /// <param name="regionAbbreviation">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="WattTimeUsername">WattTime Username</param>
        /// <param name="WattTimePassword">WattTime Password</param>
        /// <param name="timeout">Optional Timeout value</param>
        /// <returns></returns>
        public AutomatedEmissionsReductionsIndexResult GetCarbonEmissionsRelativeMeritResults(string WattTimeUrl, string regionAbbreviation, string WattTimeUsername, string WattTimePassword, TimeSpan? timeout = null)
        {
            //url: '/index/'

            //    params = {
            //    "ba":"CAISO_NP15", 
            //    "latitude":"", 
            //    "longitude":"", 
            //     "style": "rating", "percent", "switch" or "all". Default is "rating"
            //    }

            //    output:
            //    {
            //      "ba": "ERCOT",
            //      "freq": 0.8008281904610115,
            //      "market": "RTM",
            //      "percent": 53, -- 0 is best, 100 is worst
            //      "rating": 4, -- 0 is best, 5 is worst
            //      "switch": 0, -- 1 = don't use power. 0 = use power. 
            //      "validFor": 277,
            //      "validUntil": "2000-01-23T04:56:07.000+00:00"
            //    }

            const string subUrl = "v2/index/";
            var apiQueryUrl = $"{WattTimeUrl}{subUrl}";
            string urlParameters = $"?ba={regionAbbreviation}&style=all&page_size=1000";

            var authToken = RetrieveWattTimeAuthToken(WattTimeUrl, WattTimeUsername, WattTimePassword);
            var authTokenKey = authToken.Token;

            var webApiHelper = new WebApiSerializerHelper<AutomatedEmissionsReductionsIndexResult>();
            var result =
                this.apiInteractionHelper.ExecuteThrottledApiCallWithBearerAuthToken<AutomatedEmissionsReductionsIndexResult>(
                    timeout,
                    webApiHelper,
                    apiQueryUrl,
                    urlParameters,
                    authTokenKey,
                    apiNameForThrottlingRecords);
            
            return result;
        }

        /// <summary>
        /// Retrieve the most recent 5 minutely real time marginal carbon data for a given region from the WattTime API
        /// </summary>
        /// <param name="WattTimeUrl">URL of the Watt Time API</param>
        /// <param name="regionAbbreviation">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="WattTimeUsername">WattTime Username</param>
        /// <param name="WattTimePassword">WattTime Password</param>
        /// <param name="timeout">Optional Timeout value</param>
        /// <returns>The most recent 5 minutely real time marginal carbon data for a given region from the WattTime API</returns>
        public MarginalCarbonResultV2Api GetMostRecentMarginalCarbonEmissionsResult(string WattTimeUrl, string regionAbbreviation, string WattTimeUsername,
            string WattTimePassword, TimeSpan? timeout = null)
        {
            var customParams = "&market=RT5M";
            var response = GetMarginalCarbonResults(WattTimeUrl, regionAbbreviation, WattTimeUsername, WattTimePassword, DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1), timeout,customParams);

            var sortedResponses = response.OrderByDescending(x => x.point_time);

            return sortedResponses.FirstOrDefault();
        }


        /// <summary>
        /// Register the given user details with the WattTime API
        /// </summary>
        /// <param name="WattTimeUrl">URL of the Watt Time API</param>
        /// <param name="regionAbbreviation">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="WattTimeUsername">WattTime Username</param>
        /// <param name="WattTimePassword">WattTime Password</param>
        /// <param name="WattTimeEmail">Email address to use for the WattTime Service</param>
        /// <param name="WattTimeOrganization">Organization to use for the WattTime Service</param>
        /// <param name="ignoreExceptions">False to have any exceptions rethrown. True to ignore exceptions and just return false in the event of a failure or exception</param>
        /// <returns></returns>
        public bool RegisterWithWattTime(string WattTimeUrl, string WattTimeUsername, string WattTimePassword, string WattTimeEmail, string WattTimeOrganization, bool ignoreExceptions = true)
        {
            //url: 'https://api2.watttime.org/v2/register/'

            //endpoint: / register

            //    params = {
            //    "username":"contoso", 
            //    "password":"xyzzy", 
            //    "email":"contoso@EnvironmentalTechnology.org", 
            //     "org":"The Environmental Technology Company"} 

            //    output:
            //    { 'ok':'User created', 'user': 'contoso'}

            const string subUrl = "v2/register";
            var apiQueryUrl = $"{WattTimeUrl}{subUrl}";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(apiQueryUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            string username = WattTimeUsername;
            string password = WattTimePassword;
            string email = WattTimeEmail;
            string org = WattTimeOrganization;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(new
                {
                    username,
                    password,
                    email,
                    org
                });
                streamWriter.Write(json);
            }

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch(Exception e)
            {
                // Exception encountered. If caller has indicated they want to know if this call fails, rethrow exception. 
                if(!ignoreExceptions)
                {
                    throw (e);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Authenticates a WattTime user session and retrieve Auth Token for further calls
        /// </summary>
        /// <param name="WattTimeUrl">URL of the Watt Time API</param>
        /// <param name="WattTimeUsername">WattTime Username</param>
        /// <param name="WattTimePassword">WattTime Password</param>
        /// <returns>User's Auth Token for further API calls</returns>
        public LoginResult RetrieveWattTimeAuthToken(string WattTimeUrl, string username, string password)
        {
            //url: 'https://api2.watttime.org/v2/login/'

            //endpoint: / login

            //    params = {
            //    "WattTimeUsername":"contoso", 
            //    "WattTimePassword":"xyzzy" }

            //    output:
            //    ok: '{"token":"abcdef0123456789fedcabc"}'

            String encodedUsernameAndPassword = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));

            const string subUrl = "v2/login";
            var apiQueryUrl = $"{WattTimeUrl}{subUrl}";

            string loginRsp = string.Empty;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(apiQueryUrl);
            httpWebRequest.Method = "GET";
            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            httpWebRequest.Headers.Add("Authorization", "Basic " + encoded);

            using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        loginRsp = reader.ReadToEnd();
                    }
                }
            }

            var tokenObject = JsonConvert.DeserializeObject<LoginResult>(loginRsp);

            return tokenObject;
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
            string urlParameters, bool wattTimeApiV2Format = false)
        {
            if (startDateTime != null)
            {
                // This line may throw a warning in debugging mode: "Additional information: A UTC DateTime is being converted to text in a format that is only correct for local times. 
                // This can happen when calling DateTime.ToString using the 'z' format specifier, which will include a local time zone offset in the output. In that case, either use the 'Z'..."
                // This conversion is intentional, and the warning can be ignored. You can modify your exception settings to ignore this specific Warning. The warning will not be thrown 
                // in a Release build, or when it runs on Azure. 
                if (wattTimeApiV2Format)
                {
                    urlParameters = $"{urlParameters}&starttime={startDateTime.Value:yyyy-MM-ddTHH\\:mm\\:sszzz}";
                }
                else
                {
                    urlParameters = $"{urlParameters}&start_at={startDateTime.Value:yyyy-MM-ddTHH\\:mm\\:sszzz}";
                }
            }
            if (endDateTime != null)
            {
                // This line may throw a warning in debugging mode: "Additional information: A UTC DateTime is being converted to text in a format that is only correct for local times. 
                // This can happen when calling DateTime.ToString using the 'z' format specifier, which will include a local time zone offset in the output. In that case, either use the 'Z'..."
                // This conversion is intentional, and the warning can be ignored. You can modify your exception settings to ignore this specific Warning. The warning will not be thrown 
                // in a Release build, or when it runs on Azure. 
                if (wattTimeApiV2Format)
                {
                    urlParameters = $"{urlParameters}&endtime={endDateTime.Value:yyyy-MM-ddTHH\\:mm\\:sszzz}";
                }
                else
                {
                    urlParameters = $"{urlParameters}&end_at={endDateTime.Value:yyyy-MM-ddTHH\\:mm\\:sszzz}";
                }                
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
