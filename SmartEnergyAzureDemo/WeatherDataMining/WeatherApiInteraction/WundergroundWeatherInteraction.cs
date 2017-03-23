// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace WeatherApiInteraction
{
    using System;
    using System.Collections.Generic;
    using ApiInteraction;
    using ApiInteraction.Helper;

    using WeatherApiInteraction.WundergroundHistoricDataClasses;

    /// <summary>
    /// Class which provides methods to retrieve data from the Wunderground Weather API (https://www.wunderground.com/). Requires the caller to register 
    /// for their own Wunderground API Key and adhere to it's usage terms. 
    /// </summary>
    public class WundergroundWeatherInteraction
    {
        // Variables to manage self-throttling of calls to the API
        private readonly ApiInteractionHelper apiInteractionHelper;
        private string apiNameForThrottlingRecords = "Wunderground";
        private SelfThrottlingMethod _selfThrottlingMethod;

        /// <summary>
        /// Create a new WundergroundWeatherInteraction object
        /// </summary>
        /// <param name="_selfThrottlingMethod">The method to use to limit calls to the API to below the given threshold. Options: {None, InMemoryCallRecollection, AzureTableStorageCallRecollection}</param>
        /// <param name="maxNumberOfCallsPerMinute">Maximum number of calls to make to the API per minute</param>
        public WundergroundWeatherInteraction(string _selfThrottlingMethod, int maxNumberOfCallsPerMinute = -1)
        {
            this.apiInteractionHelper = new ApiInteractionHelper(ApiInteractionHelper.parseSelfThrottlingMethodstring(_selfThrottlingMethod), maxNumberOfCallsPerMinute);
        }

        /// <summary>
        /// Create a new WundergroundWeatherInteraction object
        /// </summary>
        /// <param name="_selfThrottlingMethod">The method to use to limit calls to the API to below the given threshold. Options: {None, InMemoryCallRecollection, AzureTableStorageCallRecollection}</param>
        /// <param name="maxNumberOfCallsPerMinute">Maximum number of calls to make to the API per minute</param>
        public WundergroundWeatherInteraction(
            SelfThrottlingMethod _selfThrottlingMethod = SelfThrottlingMethod.None,
            int maxNumberOfCallsPerMinute = -1)
        {
            this.apiInteractionHelper = new ApiInteractionHelper(_selfThrottlingMethod, maxNumberOfCallsPerMinute);
        }

        /// <summary>
        /// Retrieve historic weather datapoints from the Wunderground Api
        /// </summary>
        /// <param name="wunderGroundUrl">The Url of the Wunderground API</param>
        /// <param name="regionSubUrl">Sub Url of the region on the Wunderground weather API e.g. CA/San_Francisco</param>
        /// <param name="wundergroundApiKey">The Wunderground API Key</param>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="timeout">Optional Timeout value for the call</param>
        /// <returns>Historic weather datapoints from the Wunderground Api</returns>
        public List<Observation> GetHistoricWeatherData(
            string wunderGroundUrl,
            string regionSubUrl,
            string wundergroundApiKey,
            DateTime startDateTime,
            DateTime? endDateTime = null,
            TimeSpan? timeout = null)
        {
            const string SubUrl = "history_";
            var currentDateTime = startDateTime;
            DateTime queryFinishDateTime = endDateTime ?? DateTime.UtcNow;
            var fullObservationsList = new List<Observation>();

            while (currentDateTime.Date <= queryFinishDateTime)
            {
                var queryDateString = $"{currentDateTime:yyyyMMdd}";
                var apiQueryUrl =
                    $"{wunderGroundUrl}{wundergroundApiKey}/{SubUrl}{queryDateString}/q/{regionSubUrl}.json";

                var webApiHelper = new WebApiSerializerHelper<RootObject>();
                var response = this.apiInteractionHelper.ExecuteThrottledApiCall<WundergroundHistoricDataClasses.RootObject> (timeout, webApiHelper, apiQueryUrl, null, wundergroundApiKey, apiNameForThrottlingRecords);

                fullObservationsList.AddRange(response.history.observations);
                currentDateTime = currentDateTime.AddDays(1);
            }

            var processedObservationsList = this.AddConcreteDateTimeToHistoricWeatherDatapointList(fullObservationsList);

            return processedObservationsList;
        }

        /// <summary>
        /// Retrieve forecast weather datapoints from the Wunderground Api
        /// </summary>
        /// <param name="wunderGroundUrl">The Url of the Wunderground API</param>
        /// <param name="regionSubUrl">Sub Url of the region on the Wunderground weather API e.g. CA/San_Francisco</param>
        /// <param name="wundergroundApiKey">The Wunderground API Key</param>
        /// <param name="timeout">Optional Timeout value for the call</param>
        /// <returns>Forecast weather datapoints from the Wunderground Api</returns>
        public List<WundergroundThreeDayForecastSummaryDataClasses.Forecastday2> GetThreeDayForecastSummaryWeatherData(
            string wunderGroundUrl,
            string regionSubUrl,
            string wundergroundApiKey,
            TimeSpan? timeout = null)
        {
            const string SubUrl = "forecast";

            var apiQueryUrl = $"{wunderGroundUrl}{wundergroundApiKey}/{SubUrl}/q/{regionSubUrl}.json";
            var webApiHelper = new WebApiSerializerHelper<WundergroundThreeDayForecastSummaryDataClasses.RootObject>();
            var response = this.apiInteractionHelper.ExecuteThrottledApiCall(timeout, webApiHelper, apiQueryUrl, null, wundergroundApiKey, apiNameForThrottlingRecords);

            var processedObservationsList = this.AddConcreteDateTimeToThreeDayWeatherForecastList(response.forecast.simpleforecast.forecastday);
            return processedObservationsList;
        }

        /// <summary>
        /// Retrieve forecast weather datapoints from the Wunderground Api
        /// </summary>
        /// <param name="wunderGroundUrl">The Url of the Wunderground API</param>
        /// <param name="regionSubUrl">Sub Url of the region on the Wunderground weather API e.g. CA/San_Francisco</param>
        /// <param name="wundergroundApiKey">The Wunderground API Key</param>
        /// <param name="timeout">Optional Timeout value for the call</param>
        /// <returns>Forecast weather datapoints from the Wunderground Api</returns>
        public List<WundergroundTenDayHourlyForecastDataClasses.HourlyForecast> GetTenDayHourlyForecastWeatherData(
            string wunderGroundUrl,
            string regionSubUrl,
            string wundergroundApiKey,
            TimeSpan? timeout = null)
        {
            const string SubUrl = "hourly10day";

            var response = this.ExecuteWunderApiCall<WundergroundTenDayHourlyForecastDataClasses.RootObject>(wunderGroundUrl, regionSubUrl, wundergroundApiKey, timeout, SubUrl);

            var processedObservationsList = this.AddConcreteDateTimeToTenDayForecastList(response.hourly_forecast);
            return processedObservationsList;
        }

        #region Private Methods
        /// <summary>
        /// Parse the UTC DateTime object from each object in the list, and add that as a concrete DateTime object
        /// </summary>
        /// <param name="observationsList"></param>
        /// <returns></returns>
        private List<Observation> AddConcreteDateTimeToHistoricWeatherDatapointList(List<Observation> observationsList)
        {
            var processedObservationsList = new List<Observation>();

            foreach (var observation in observationsList)
            {
                var processedObservation = observation;
                var year = int.Parse(observation.utcdate.year);
                var month = int.Parse(observation.utcdate.mon);
                var day = int.Parse(observation.utcdate.mday);
                var hour = int.Parse(observation.utcdate.hour);
                var minute = int.Parse(observation.utcdate.min);
                processedObservation.observationDateTime = new DateTime(year, month, day, hour, minute, 0);

                processedObservationsList.Add(processedObservation);
            }

            return processedObservationsList;
        }

        /// <summary>
        /// Parse the UTC DateTime object from each object in the list, and add that as a concrete DateTime object
        /// </summary>
        /// <param name="forecastday"></param>
        /// <returns></returns>
        private List<WundergroundThreeDayForecastSummaryDataClasses.Forecastday2> AddConcreteDateTimeToThreeDayWeatherForecastList(List<WundergroundThreeDayForecastSummaryDataClasses.Forecastday2> forecastday)
        {
            var processedObservationsList = new List<WundergroundThreeDayForecastSummaryDataClasses.Forecastday2>();

            foreach (var observation in forecastday)
            {
                var processedObservation = observation;

                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Double.Parse(observation.date.epoch));
                processedObservation.observationDateTime = epoch;

                processedObservationsList.Add(processedObservation);
            }

            return processedObservationsList;
        }

        /// <summary>
        /// Parse the UTC DateTime object from each object in the list, and add that as a concrete DateTime object
        /// </summary>
        /// <param name="forecastday"></param>
        /// <returns></returns>
        private List<WundergroundTenDayHourlyForecastDataClasses.HourlyForecast> AddConcreteDateTimeToTenDayForecastList(List<WundergroundTenDayHourlyForecastDataClasses.HourlyForecast> forecastday)
        {
            var processedObservationsList = new List<WundergroundTenDayHourlyForecastDataClasses.HourlyForecast>();

            foreach (var observation in forecastday)
            {
                var processedObservation = observation;
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Double.Parse(observation.FCTTIME.epoch));
                processedObservation.observationDateTime = epoch;

                processedObservationsList.Add(processedObservation);
            }

            return processedObservationsList;
        }

        /// <summary>
        /// Execute the given call against the Wunderground API with the response serialized into type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wunderGroundUrl"></param>
        /// <param name="regionSubUrl"></param>
        /// <param name="wundergroundApiKey"></param>
        /// <param name="timeout"></param>
        /// <param name="SubUrl"></param>
        /// <returns></returns>
        private T ExecuteWunderApiCall<T>(
            string wunderGroundUrl,
            string regionSubUrl,
            string wundergroundApiKey,
            TimeSpan? timeout,
            string SubUrl)
        {
            var apiQueryUrl = $"{wunderGroundUrl}{wundergroundApiKey}/{SubUrl}/q/{regionSubUrl}.json";
            var webApiHelper = new WebApiSerializerHelper<T>();
            var response = this.apiInteractionHelper.ExecuteThrottledApiCall(timeout, webApiHelper, apiQueryUrl, null, wundergroundApiKey, apiNameForThrottlingRecords);
            return response;
        }
        #endregion
    }
}
