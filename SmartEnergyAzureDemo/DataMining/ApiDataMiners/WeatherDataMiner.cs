// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

using SmartEnergyOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDataMiners
{
    using SmartEnergyOM;

    using WeatherApiInteraction;

    using ApiInteraction;

    using CentralLogger;

    using WeatherApiInteraction.WundergroundHistoricDataClasses;
    using WeatherApiInteraction.WundergroundTenDayHourlyForecastDataClasses;

    /// <summary>
    /// A class enabling the retrieval of data from the WunderGround Weather Data API and storage of it in the application's database
    /// </summary>
    public class WeatherDataMiner
    {
        private string wundergroundApiUrl;
        private string wundergroundApiKey;
        private WundergroundWeatherInteraction wundergroundWeatherInteraction;
        private readonly string DatabaseConnectionString;

        /// <summary>
        /// Create an instance of a WeatherDataMiner for the given wundergroundApiUrl and wundergroundApiKey
        /// </summary>
        /// <param name="wunderGroundUrl">The Url of the Wunderground API</param>
        /// <param name="wundergroundApiKey">The Wunderground API Key</param>
        /// <param name="selfThrottlingMethod">The method to use to limit calls to the API to below the given threshold. Options: {None, InMemoryCallRecollection, AzureTableStorageCallRecollection}</param>
        /// <param name="databaseConnectionString">Entity Framrwork Connection string for the Applications' Database</param>
        /// <param name="maxNumberOfCallsPerMinute">Maximum number of calls to make to the API per minute</param>
        /// <param name="wundergroundWeatherInteraction">Optional WundergroundWeatherInteraction object. If not passed, it will be created.</param>
        public WeatherDataMiner(
            string wundergroundApiUrl,
            string wundergroundApiKey,
            string selfThrottlingMethod,
            string databaseConnectionString,
            int maxNumberOfCallsPerMinute = -1,
            WundergroundWeatherInteraction wundergroundWeatherInteraction = null)
        {
            if (wundergroundWeatherInteraction == null)
            {
                this.wundergroundWeatherInteraction = new WundergroundWeatherInteraction(
                                                          selfThrottlingMethod,
                                                          maxNumberOfCallsPerMinute);
            }
            else
            {
                this.wundergroundWeatherInteraction = wundergroundWeatherInteraction;
            }

            this.wundergroundApiUrl = wundergroundApiUrl;
            this.wundergroundApiKey = wundergroundApiKey;
            this.DatabaseConnectionString = databaseConnectionString;
        }

        /// <summary>
        /// Mine historic weather records from the Wunderground Weather service for the given regionSubUrl (e.g. CA/San_Francisco) and add it to the database
        /// </summary>
        /// <param name="startDateTime">startDateTime</param>
        /// <param name="endDateTime">endDateTime</param>
        /// <param name="regionSubUrl">Sub Url of the region on the Wunderground weather API e.g. CA/San_Francisco</param>
        /// <param name="regionId">regionId of this region in the application's database</param>
        public void MineHistoricWeatherValues(
            DateTime? startDateTime,
            DateTime? endDateTime,
            string regionSubUrl,
            int regionId)
        {
            var historicStartDateTime = startDateTime ?? DateTime.Now.AddDays(-2);
            var historicEndDateTime = endDateTime ?? DateTime.Now.AddMinutes(15);

            try
            {
                var results = this.wundergroundWeatherInteraction.GetHistoricWeatherData(
                    this.wundergroundApiUrl,
                    regionSubUrl,
                    this.wundergroundApiKey,
                    historicStartDateTime,
                    historicEndDateTime);

                Logger.Information(
                    $"Received {results.Count} HistoricWeatherValues Results for region with SubUrl {regionSubUrl} from Wunderground. Inserting them into the database",
                    "WeatherDataMiner.MineHistoricWeatherValues()");

                // Insert results in the database 
                this.InsertHistoricWeatherValuesIntoDatabase(regionId, results);
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"WeatherDataMiner: MineHistoricWeatherValues(): Exception encountered while retrieving historical Weather Data figures for {regionSubUrl} between {historicStartDateTime} and {historicEndDateTime}.",
                    "WeatherDataMiner.MineHistoricWeatherValues()",
                    null,
                    e);
            }
        }

        /// <summary>
        /// Mine historic weather records from the Wunderground Weather service for the given GPS Coords (e.g. 58.279231, 6.892410) and add it to the database
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
                var results = this.wundergroundWeatherInteraction.GetHistoricWeatherData(
                    this.wundergroundApiUrl,
                    gpsLat,
                    gpsLong,
                    this.wundergroundApiKey,
                    historicStartDateTime,
                    historicEndDateTime);

                Logger.Information(
                    $"Received {results.Count} HistoricWeatherValues Results for region with GPS {gpsLat},{gpsLong} from Wunderground. Inserting them into the database",
                    "WeatherDataMiner.MineHistoricWeatherValues()");

                // Insert results in the database 
                this.InsertHistoricWeatherValuesIntoDatabase(regionId, results);
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"WeatherDataMiner: MineHistoricWeatherValues(): Exception encountered while retrieving historical Weather Data figures for GPS {gpsLat},{gpsLong} between {historicStartDateTime} and {historicEndDateTime}.",
                    "WeatherDataMiner.MineHistoricWeatherValues()",
                    null,
                    e);
            }
        }

        private void InsertHistoricWeatherValuesIntoDatabase(int regionId, List<Observation> results)
        {
            using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
            {
                foreach (var res in results)
                {
                    var dateTime = res.observationDateTime;
                    double tempcelcuis;
                    double.TryParse(res.tempm, out tempcelcuis);
                    double dewPoint;
                    double.TryParse(res.dewptm, out dewPoint);
                    double humidityPercentage;
                    double.TryParse(res.hum, out humidityPercentage);
                    double windSpeedKmPh;
                    double.TryParse(res.wspdm, out windSpeedKmPh);
                    double windGustKmPh;
                    double.TryParse(res.wgustm, out windGustKmPh);
                    double windDirectionDegrees;
                    double.TryParse(res.wdird, out windDirectionDegrees);
                    double windChill;
                    double.TryParse(res.windchillm, out windChill);
                    double visibilityMetric;
                    double.TryParse(res.vism, out visibilityMetric);
                    double snow;
                    double.TryParse(res.snow, out snow);
                    double pressure;
                    double.TryParse(res.pressurem, out pressure);
                    double precipitation;
                    double.TryParse(res.precipm, out precipitation);
                    var conditionDescription = res.conds ?? string.Empty;

                    // Clean up "-9999" values that sometimes come back
                    if (windSpeedKmPh < 0)
                    {
                        windSpeedKmPh = -1;
                    }
                    if (windGustKmPh < 0)
                    {
                        windGustKmPh = -1;
                    }

                    _objectModel.InsertOrUpdateWeatherDataPoints(
                        regionId,
                        dateTime,
                        tempcelcuis,
                        dewPoint,
                        windSpeedKmPh,
                        windGustKmPh,
                        windDirectionDegrees,
                        windChill,
                        visibilityMetric,
                        null,
                        // No UNIndex in Wunderground historical data entries
                        precipitation,
                        snow,
                        pressure,
                        humidityPercentage,
                        conditionDescription,
                        false);
                }
            }
        }

        /// <summary>
        /// Retrieve forecast weather data from the Wunderground Weather service for the given regionSubUrl (e.g. CA/San_Francisco) and add it to the database
        /// </summary>
        /// <param name="regionSubUrl">Sub Url of the region on the Wunderground weather API e.g. CA/San_Francisco</param>
        /// <param name="regionId">regionId of this region in the application's database</param>
        public void MineTenDayHourlyForecastWeatherValues(string regionSubUrl, int regionId)
        {
            try
            {
                var results =
                    this.wundergroundWeatherInteraction.GetTenDayHourlyForecastWeatherData(
                        this.wundergroundApiUrl,
                        regionSubUrl,
                        this.wundergroundApiKey);

                Logger.Information(
                    $"Received {results.Count} TenDayHourlyForecastWeatherValues Results for region with SubUrl {regionSubUrl} from Wunderground. Inserting them into the database",
                    "WeatherDataMiner.MineTenDayHourlyForecastWeatherValues()");

                // Insert results in the database 
                this.InsertTenDatForecastValuesIntoDatabase(regionId, results);
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"WeatherDataMiner: MineTenDayHourlyForecastWeatherValues(): Exception encountered while retrieving historical Weather Data figures for {regionSubUrl}.",
                    "WeatherDataMiner.MineTenDayHourlyForecastWeatherValues()",
                    null,
                    e);
            }
        }

        /// <summary>
        /// Retrieve forecast weather data from the Wunderground Weather service for the given GPS Coords (e.g. 58.279231, 6.892410) and add it to the database
        /// </summary>
        /// <param name="gpsLat">Latitude</param>
        /// <param name="gpsLong">Longtitude</param>
        /// <param name="regionId">regionId of this region in the application's database</param>
        public void MineTenDayHourlyForecastWeatherValues(double gpsLat, double gpsLong, int regionId)
        {
            try
            {
                var results =
                    this.wundergroundWeatherInteraction.GetTenDayHourlyForecastWeatherData(
                        this.wundergroundApiUrl,
                        gpsLat,
                        gpsLong,
                        this.wundergroundApiKey);

                Logger.Information(
                    $"Received {results.Count} TenDayHourlyForecastWeatherValues Results for region with GPS {gpsLat},{gpsLong} from Wunderground. Inserting them into the database",
                    "WeatherDataMiner.MineTenDayHourlyForecastWeatherValues()");

                // Insert results in the database 
                this.InsertTenDatForecastValuesIntoDatabase(regionId, results);
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"WeatherDataMiner: MineTenDayHourlyForecastWeatherValues(): Exception encountered while retrieving historical Weather Data figures for GPS {gpsLat},{gpsLong}.",
                    "WeatherDataMiner.MineTenDayHourlyForecastWeatherValues()",
                    null,
                    e);
            }
        }

        /// <summary>
        /// InsertTenDatForecastValuesIntoDatabase
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="results"></param>
        private void InsertTenDatForecastValuesIntoDatabase(int regionId, List<HourlyForecast> results)
        {
            using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
            {
                foreach (var res in results)
                {
                    var dateTime = res.observationDateTime;
                    double tempcelcuis;
                    double.TryParse(res.temp.metric, out tempcelcuis);
                    double dewPoint;
                    double.TryParse(res.dewpoint.metric, out dewPoint);
                    double windSpeedKmPh;
                    double.TryParse(res.wspd.metric, out windSpeedKmPh);
                    double windDirectionDegrees;
                    double.TryParse(res.wdir.degrees, out windDirectionDegrees);
                    double windChill;
                    double.TryParse(res.windchill.metric, out windChill);
                    double uvIndex;
                    double.TryParse(res.uvi, out uvIndex);
                    double snow;
                    double.TryParse(res.snow.metric, out snow);
                    double pressure;
                    double.TryParse(res.mslp.metric, out pressure);
                    double humidityPercentage;
                    double.TryParse(res.humidity, out humidityPercentage);
                    double precipitation;
                    double.TryParse(res.qpf.metric, out precipitation);
                    var conditionDescription = res.condition ?? string.Empty;

                    // Clean up "-9999" values that sometimes come back
                    if (windSpeedKmPh < 0)
                    {
                        windSpeedKmPh = -1;
                    }

                    _objectModel.InsertOrUpdateWeatherDataPoints(
                        regionId,
                        dateTime,
                        tempcelcuis,
                        dewPoint,
                        windSpeedKmPh,
                        null,
                        // No wind gusts in Wunderground forecasts
                        windDirectionDegrees,
                        windChill,
                        null,
                        // No visibilty in Wunderground forecasts
                        uvIndex,
                        precipitation,
                        snow,
                        pressure,
                        humidityPercentage,
                        conditionDescription,
                        true);
                }
            }
        }
    }
}
