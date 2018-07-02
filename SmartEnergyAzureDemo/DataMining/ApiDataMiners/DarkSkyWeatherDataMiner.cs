// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace ApiDataMiners
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using WeatherApiInteraction.DarkSkyWeatherMining;
    using CentralLogger;
    using SmartEnergyOM;

    /// <summary>
    /// A class enabling the retrieval of data from the Dark Sky Weather Data API and storage of it in the application's database
    /// </summary>
    public class DarkSkyWeatherDataMiner
    {        
            private string darkSkyApiUrl;
            private string darkSkyApiKey;
            private DarkSkyWeatherInteraction darkSkyWeatherInteraction;
            private readonly string DatabaseConnectionString;

        /// <summary>
        /// Create an instance of a WeatherDataMiner for the given wundergroundApiUrl and wundergroundApiKey
        /// </summary>
        /// <param name="wunderGroundUrl">The Url of the Wunderground API</param>
        /// <param name="wundergroundApiKey">The Wunderground API Key</param>
        /// <param name="selfThrottlingMethod">The method to use to limit calls to the API to below the given threshold. Options: {None, InMemoryCallRecollection, AzureTableStorageCallRecollection}</param>
        /// <param name="databaseConnectionString">Entity Framrwork Connection string for the Applications' Database</param>
        /// <param name="maxNumberOfCallsPerMinute">Maximum number of calls to make to the API per minute</param>
        /// <param name="darkSkyWeatherInteraction">Optional WundergroundWeatherInteraction object. If not passed, it will be created.</param>
        public DarkSkyWeatherDataMiner(
            string darkSkyApiUrl,
            string darkSkyApiKey,
            string selfThrottlingMethod,
            string databaseConnectionString,
            int maxNumberOfCallsPerMinute = -1,
            int maxNumberOfCallsPerDay = -1,
            DarkSkyWeatherInteraction darkSkyWeatherInteraction = null)
        {
            if (darkSkyWeatherInteraction == null)
            {
                this.darkSkyWeatherInteraction = new DarkSkyWeatherInteraction(
                                                          selfThrottlingMethod,
                                                          maxNumberOfCallsPerMinute,
                                                          maxNumberOfCallsPerDay);
            }
            else
            {
                this.darkSkyWeatherInteraction = darkSkyWeatherInteraction;
            }

            this.darkSkyApiUrl = darkSkyApiUrl;
            this.darkSkyApiKey = darkSkyApiKey;
            this.DatabaseConnectionString = databaseConnectionString;
        }

        /// <summary>
        /// Mine historic weather records from the Dark Sky Weather service for the given GPS Coords (e.g. 58.279231, 6.892410) and add it to the database
        /// </summary>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="gpsLat">Latitude</param>
        /// <param name="gpsLong">Longtitude</param>
        /// <param name="regionId">regionId of this region in the application's database</param>
        public void MineHistoricWeatherValues(
            DateTime? startDateTime,
            DateTime? endDateTime,
            double gpsLat,
            double gpsLong,
            int regionId)
        {
            var historicStartDateTime = startDateTime ?? DateTime.Now.AddDays(-2);
            var historicEndDateTime = endDateTime ?? DateTime.Now.AddMinutes(15);

            try
            {
                var results = this.darkSkyWeatherInteraction.GetHistoricWeatherData(
                    this.darkSkyApiUrl,
                    this.darkSkyApiKey,
                    gpsLat,
                    gpsLong,                    
                    historicStartDateTime,
                    historicEndDateTime);

                Logger.Information(
                    $"Received {results.Count} HistoricWeatherValues Results for region with GPS {gpsLat},{gpsLong} from Dark Sky. Inserting them into the database",
                    "DarkSkyWeatherDataMiner.MineHistoricWeatherValues()");

                // Insert results in the database 
                this.InsertWeatherValuesIntoDatabase(regionId, results, false);
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"DarkSkyWeatherDataMiner: MineHistoricWeatherValues(): Exception encountered while retrieving historical Weather Data figures for GPS {gpsLat},{gpsLong} between {historicStartDateTime} and {historicEndDateTime}.",
                    "DarkSkyWeatherDataMiner.MineHistoricWeatherValues()",
                    null,
                    e);
            }
        }

        /// <summary>
        /// Mine forecast weather records from the Dark Sky Weather service for the given GPS Coords (e.g. 58.279231, 6.892410) and add it to the database
        /// </summary>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="gpsLat">Latitude</param>
        /// <param name="gpsLong">Longtitude</param>
        /// <param name="regionId">regionId of this region in the application's database</param>
        public void MineForecastWeatherValues(
            DateTime? startDateTime,
            DateTime? endDateTime,
            double gpsLat,
            double gpsLong,
            int regionId)
        {
            var _startDateTime = startDateTime ?? DateTime.UtcNow;
            var _endDateTime = endDateTime ?? DateTime.UtcNow.AddDays(10);

            try
            {
                var results = this.darkSkyWeatherInteraction.GetForecastWeatherData(
                    this.darkSkyApiUrl,
                    this.darkSkyApiKey,
                    gpsLat,
                    gpsLong,
                    _startDateTime,
                    _endDateTime);

                Logger.Information(
                    $"Received {results.Count} HistoricWeatherValues Results for region with GPS {gpsLat},{gpsLong} from Dark Sky. Inserting them into the database",
                    "DarkSkyWeatherDataMiner.MineHistoricWeatherValues()");

                // Insert results in the database 
                this.InsertWeatherValuesIntoDatabase(regionId, results, true);
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"DarkSkyWeatherDataMiner: MineHistoricWeatherValues(): Exception encountered while retrieving historical Weather Data figures for GPS {gpsLat},{gpsLong} between {_startDateTime} and {_endDateTime}.",
                    "DarkSkyWeatherDataMiner.MineHistoricWeatherValues()",
                    null,
                    e);
            }
        }

        private void InsertWeatherValuesIntoDatabase(int regionId, List<HourlyDatum> results, bool areForecastRows = false)
        {
            using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
            {
                foreach (var res in results)
                {
                    var dateTime = res.dateTime;

                    _objectModel.InsertOrUpdateWeatherDataPoints(
                        regionId,
                        dateTime,
                        res.temperature,
                        res.dewPoint,
                        res.windSpeed,
                        null,
                        res.windBearing,
                        res.apparentTemperature,
                        res.visibility,
                        null,
                        null,
                        null,
                        res.pressure,
                        res.humidity,
                        res.summary,
                        areForecastRows);
                }
            }
        }
    }
}
