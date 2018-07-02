// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace WeatherApiInteraction.DarkSkyWeatherMining
{    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using WeatherApiInteraction.WundergroundHistoricDataClasses;
    using WeatherApiInteraction.WundergroundTenDayHourlyForecastDataClasses;
    using ApiInteraction;
    using ApiInteraction.Helper;

    /// <summary>
    /// Class which provides methods to retrieve data from the Dark Sky Weather API (https://api.darksky.net). Requires the caller to register 
    /// for their own DarkSky API Key and adhere to it's usage terms. 
    /// </summary>
    public class DarkSkyWeatherInteraction
    {
        // Variables to manage self-throttling of calls to the API
        private readonly ApiInteractionHelper apiInteractionHelper;
        private string apiNameForThrottlingRecords = "DarkSky";
        private SelfThrottlingMethod _selfThrottlingMethod;

        /// <summary>
        /// Create a new DarkSkyWeatherInteraction object
        /// </summary>
        /// <param name="_selfThrottlingMethod">The method to use to limit calls to the API to below the given threshold. Options: {None, InMemoryCallRecollection, AzureTableStorageCallRecollection}</param>
        /// <param name="maxNumberOfCallsPerMinute">Maximum number of calls to make to the API per minute</param>
        public DarkSkyWeatherInteraction(string _selfThrottlingMethod, int maxNumberOfCallsPerMinute = -1, int maxNumberOfCallsPerDay = -1)
        {
            this.apiInteractionHelper = new ApiInteractionHelper(ApiInteractionHelper.parseSelfThrottlingMethodstring(_selfThrottlingMethod), maxNumberOfCallsPerMinute, maxNumberOfCallsPerDay);
        }

        /// <summary>
        /// Create a new DarkSkyWeatherInteraction object
        /// </summary>
        /// <param name="_selfThrottlingMethod">The method to use to limit calls to the API to below the given threshold. Options: {None, InMemoryCallRecollection, AzureTableStorageCallRecollection}</param>
        /// <param name="maxNumberOfCallsPerMinute">Maximum number of calls to make to the API per minute</param>
        public DarkSkyWeatherInteraction(
            SelfThrottlingMethod _selfThrottlingMethod = SelfThrottlingMethod.None,
            int maxNumberOfCallsPerMinute = -1,
            int maxNumberOfCallsPerDay = -1)
        {
            this.apiInteractionHelper = new ApiInteractionHelper(_selfThrottlingMethod, maxNumberOfCallsPerMinute, maxNumberOfCallsPerDay);
        }

        public List<HourlyDatum> GetHistoricWeatherData(
            string darkSkyUrl,
            string darkSkyApiKey,
            double gpsLat,
            double gpsLong,
            DateTime startDateTime,
            DateTime? endDateTime = null,
            TimeSpan? timeout = null)
        {
            const string SubUrl = "forecast";

            // Run through the range of dates required querying the API for that day's hourly observations
            var currentDateTime = startDateTime;
            DateTime queryFinishDateTime = endDateTime ?? DateTime.UtcNow;
            var dailyResultsList = new List<DarkSkyWeatherResultsList>();

            while (currentDateTime <= queryFinishDateTime)
            {
                var currentUnixTimeStamp = ConvertUtcDateTimeToUnixTimeStamp(currentDateTime);
                var customParams = $",{currentUnixTimeStamp}?exclude=currently&units=si";

                var dayResults = this.ExecuteDarkSkyGpsLookupApiCall<DarkSkyWeatherResultsList>(darkSkyUrl, darkSkyApiKey, SubUrl, gpsLat, gpsLong, darkSkyApiKey, timeout, customParams);

                dailyResultsList.Add(dayResults);
                currentDateTime = currentDateTime.AddDays(1);
            }

            return ProcessDailyResultsListAndReturnHourlyResultSet(dailyResultsList);
        }

        /// <summary>
        /// Get Forecast Weather Data between the given startDateTime and endDateTime for the given GPS Coordinates
        /// </summary>
        /// <param name="darkSkyUrl">darkSkyUrl</param>
        /// <param name="darkSkyApiKey">darkSkyApiKey</param>
        /// <param name="gpsLat">gpsLat</param>
        /// <param name="gpsLong">gpsLong</param>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="timeout">timeout</param>
        /// <returns>Hourly Forecast Weather Data between the given startDateTime and endDateTime for the given GPS Coordinates, where available</returns>
        public List<HourlyDatum> GetForecastWeatherData(
            string darkSkyUrl,
            string darkSkyApiKey,
            double gpsLat,
            double gpsLong,
            DateTime startDateTime,
            DateTime? endDateTime = null,
            TimeSpan? timeout = null)
        {
            const string SubUrl = "forecast";

            // Run through the range of dates required querying the API for that day's hourly observations
            var currentDateTime = startDateTime;
            DateTime queryFinishDateTime = endDateTime ?? DateTime.UtcNow;
            var dailyResultsList = new List<DarkSkyWeatherResultsList>();

            while (currentDateTime <= queryFinishDateTime)
            {
                var currentUnixTimeStamp = ConvertUtcDateTimeToUnixTimeStamp(currentDateTime);
                var customParams = $",{currentUnixTimeStamp}?units=si";

                var dayResults = this.ExecuteDarkSkyGpsLookupApiCall<DarkSkyWeatherResultsList>(darkSkyUrl, darkSkyApiKey, SubUrl, gpsLat, gpsLong, darkSkyApiKey, timeout, customParams);

                dailyResultsList.Add(dayResults);
                currentDateTime = currentDateTime.AddDays(1);
            }

            return ProcessDailyResultsListAndReturnHourlyResultSet(dailyResultsList);
        }

        /// <summary>
        /// Get Weather Forecast Weather Data for the next 10 days, where available, for the given GPS Coordinates
        /// </summary>
        /// </summary>
        /// <param name="darkSkyUrl">darkSkyUrl</param>
        /// <param name="darkSkyApiKey">darkSkyApiKey</param>
        /// <param name="gpsLat">gpsLat</param>
        /// <param name="gpsLong">gpsLong</param>
        /// <param name="timeout">timeout</param>
        /// <returns>Weather Forecast Weather Data for the next 10 days, where available, for the given GPS Coordinates</returns>
        public List<HourlyDatum> GetTenDayHourlyForecastWeatherData(
            string darkSkyUrl,
            string darkSkyApiKey,
            double gpsLat,
            double gpsLong,            
            TimeSpan? timeout = null)
        {
            var startDateTime = DateTime.UtcNow;
            var endDateTime = DateTime.UtcNow.AddDays(10);

            return GetForecastWeatherData(darkSkyUrl, darkSkyApiKey, gpsLat, gpsLong, startDateTime, endDateTime, timeout);
        }

        #region API and Data Conversion Methods
        /// <summary>
        /// Execute the given call against the Wunderground API with the response serialized into type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wunderGroundUrl"></param>
        /// <param name="gpsLat"></param>
        /// <param name="gpsLong"></param>
        /// <param name="apiKey"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private T ExecuteDarkSkyGpsLookupApiCall<T>(
            string darkSkyUrl,
            string darkSkyApiKey,
            string subUrl,
            double gpsLat,
            double gpsLong,
            string apiKey,
            TimeSpan? timeout,
            string customParams = null)
        {
            var apiQueryUrl =
                    $"{darkSkyUrl}{subUrl}/{darkSkyApiKey}/{gpsLat},{gpsLong}{customParams}";
            var webApiHelper = new WebApiSerializerHelper<T>();
            var response = this.apiInteractionHelper.ExecuteThrottledApiCall(timeout, webApiHelper, apiQueryUrl, null, apiKey, apiNameForThrottlingRecords);
            return response;
        }

        /// <summary>
        /// Process Daily DarkSky Results List And Return Hourly Result Set, adding concrete DateTime objects
        /// </summary>
        /// <param name="dailyResultsList"></param>
        /// <returns>Processed Daily DarkSky Results List And Return Hourly Result Set, adding concrete DateTime objects</returns>
        private List<HourlyDatum> ProcessDailyResultsListAndReturnHourlyResultSet(List<DarkSkyWeatherResultsList> dailyResultsList)
        {
            // Convert Unix DateTimes to UTC DateTime objects inside the objects
            var processedObservationsList = this.AddConcreteDateTimeToHistoricWeatherDatapointList(dailyResultsList);

            // Build the full hourly results set
            var fullHourlyResultSet = new List<HourlyDatum>();
            foreach (var concreteDailyResult in processedObservationsList)
            {
                fullHourlyResultSet.AddRange(concreteDailyResult.hourly.data);
            }

            return fullHourlyResultSet;
        }

        /// <summary>
        /// ConvertUtcDateTimeToUnixTimeStamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="truncateDecimalPlaces"></param>
        /// <returns></returns>
        private static double ConvertUtcDateTimeToUnixTimeStamp(DateTime dateTime, bool truncateDecimalPlaces = true)
        {
            DateTime unixStartDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStartDateTime).Ticks;
                        
            var unixTimeStamp = (double)unixTimeStampInTicks / TimeSpan.TicksPerSecond;

            if (truncateDecimalPlaces)
            {
                return TruncateDouble(unixTimeStamp, 0);
            }

            return unixTimeStamp;
        }

        /// <summary>
        /// ConvertUnixTimeStampToUtcDateTime
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        private static DateTime ConvertUnixTimeStampToUtcDateTime(double unixTime)
        {
            DateTime unixStartDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStartDateTime.Ticks + unixTimeStampInTicks, DateTimeKind.Utc);
        }

        /// <summary>
        /// Truncate a Double to the given number of decimal places
        /// </summary>
        /// <param name="value"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static double TruncateDouble(double value, int precision)
        {
            double step = Math.Pow(10, precision);
            double tmp = Math.Truncate(step * value);
            return tmp / step;
        }

        /// <summary>
        /// Parse the UTC DateTime object from each object in the list, and add that as a concrete DateTime object
        /// </summary>
        /// <param name="observationsList"></param>
        /// <returns></returns>
        public List<DarkSkyWeatherResultsList> AddConcreteDateTimeToHistoricWeatherDatapointList(List<DarkSkyWeatherResultsList> observationsList)
        {
            var processedObservationsList = new List<DarkSkyWeatherResultsList>();

            foreach (var observation in observationsList)
            {
                var processedObservation = observation;

                foreach ( var dailyDateObject in processedObservation.daily.data)
                {
                    dailyDateObject.apparentTemperatureHighTimeDateTime = ConvertUnixTimeStampToUtcDateTime(dailyDateObject.apparentTemperatureHighTime);
                    dailyDateObject.apparentTemperatureLowTimeDateTime = ConvertUnixTimeStampToUtcDateTime(dailyDateObject.apparentTemperatureLowTime);
                    dailyDateObject.apparentTemperatureMaxTimeDateTime = ConvertUnixTimeStampToUtcDateTime(dailyDateObject.apparentTemperatureMaxTime);
                    dailyDateObject.apparentTemperatureMinTimeDateTime = ConvertUnixTimeStampToUtcDateTime(dailyDateObject.apparentTemperatureMinTime);
                    dailyDateObject.sunriseTimeDateTime = ConvertUnixTimeStampToUtcDateTime(dailyDateObject.sunriseTime);
                    dailyDateObject.sunsetTimeDateTime = ConvertUnixTimeStampToUtcDateTime(dailyDateObject.sunsetTime);
                    dailyDateObject.temperatureHighTimeDateTime = ConvertUnixTimeStampToUtcDateTime(dailyDateObject.temperatureHighTime);
                    dailyDateObject.temperatureLowTimeDateTime = ConvertUnixTimeStampToUtcDateTime(dailyDateObject.temperatureLowTime);
                    dailyDateObject.temperatureMaxTimeDateTime = ConvertUnixTimeStampToUtcDateTime(dailyDateObject.temperatureMaxTime);
                    dailyDateObject.temperatureMinTimeDateTime = ConvertUnixTimeStampToUtcDateTime(dailyDateObject.temperatureMinTime);
                }

                foreach (var dailyDateObject in processedObservation.hourly.data)
                {
                    dailyDateObject.dateTime = ConvertUnixTimeStampToUtcDateTime(dailyDateObject.time);                    
                }

                processedObservationsList.Add(processedObservation);
            }

            return processedObservationsList;
        }


        #endregion
    }
}

