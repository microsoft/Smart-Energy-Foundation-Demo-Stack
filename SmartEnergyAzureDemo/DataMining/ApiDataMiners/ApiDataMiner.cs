// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace ApiDataMiners
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    using CentralLogger;

    using SmartEnergyOM;

    /// <summary>
    /// This class encapsulates the mining of data from several APIs, and the storage of that data in a single database
    /// </summary>
    public class ApiDataMiner
    {
        private readonly string DatabaseConnectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDataMiner"/> class.
        /// </summary>
        /// <param name="databaseConnectionString">Entity Framrwork Connection string for the Applications' Database</param>
        public ApiDataMiner(string databaseConnectionString)
        {
            this.DatabaseConnectionString = databaseConnectionString;
        }

        /// <summary>
        /// Parse a settings file and kick off data miners with those settings
        /// </summary>
        /// <param name="ConfigPath"></param>
        public void ParseMinerSettingsFileAndMineData(string ConfigPath)
        {
            using (var streamReader = new StreamReader(ConfigPath))
            {
                var xmlSerializer = new XmlSerializer(typeof(ApiMinerConfigLayout));
                var minerConfigs = (ApiMinerConfigLayout)xmlSerializer.Deserialize(streamReader);

                foreach (var minerConfig in minerConfigs.Regions)
                {
                    this.MineRegionData(minerConfig);
                }
            }
        }

        /// <summary>
        /// Take a config element containing the data for a region to be mined, mine the data and save it in the database
        /// </summary>
        /// <param name="regionConfiguration"></param>
        public void MineRegionData(ApiMinerConfigLayoutRegion regionConfiguration)
        {
            var regionGroupingName = regionConfiguration.friendlyName;

            using (new TimedOperation(
                                         $"Beginning Mining of all data for Region {regionGroupingName}",
                                         "ApiDataMiner.MineRegionData()"))
            {

                // Mine the regions emissions if an emissions node was supplied
                int? emissionsRegionId = null;
                if (regionConfiguration.EmissionsMiningRegion != null)
                {
                    var friendlyName = regionConfiguration.EmissionsMiningRegion.friendlyName;

                    using (
                        new TimedOperation(
                            $"Beginning Mining of emissions data for Region {friendlyName}",
                            "ApiDataMiner.MineRegionData()"))
                    {
                        var timeZone = regionConfiguration.EmissionsMiningRegion.TimeZone;
                        var regionLat = regionConfiguration.EmissionsMiningRegion.Latitude;
                        var regionLong = regionConfiguration.EmissionsMiningRegion.Longitude;
                        var regionWattTimeName = regionConfiguration.EmissionsMiningRegion.EmissionsWattTimeAbbreviation;
                        var wattTimeApiUrl = regionConfiguration.EmissionsMiningRegion.ApiUrl;
                        var wattTimeApiKey = regionConfiguration.EmissionsMiningRegion.ApiKey;
                        var selfThrottlingMethod = regionConfiguration.WeatherMiningRegion.SelfThrottlingMethod;
                        var maxNumberOfCallsPerMinute =
                            regionConfiguration.WeatherMiningRegion.MaxNumberOfCallsPerMinute;
                        var historicStartDateTime = DateTime.UtcNow.AddDays(-15);
                        var historicEndDateTime = DateTime.UtcNow.AddDays(1);
                        var forecastStartDateTime = DateTime.UtcNow.AddDays(-2);
                        var forecastEndDateTime = DateTime.UtcNow.AddDays(10);

                        using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                        {
                            emissionsRegionId =
                                _objectModel.AddEmissionsRegion(friendlyName, timeZone, regionLat, regionLong)
                                    .EmissionsRegionID;

                            CarbonEmissionsMiner carbonEmissionsMiner = new CarbonEmissionsMiner(
                                                                            wattTimeApiUrl,
                                                                            wattTimeApiKey,
                                                                            selfThrottlingMethod,
                                                                            this.DatabaseConnectionString,
                                                                            maxNumberOfCallsPerMinute);

                            // Mine Recent Actual Data
                            carbonEmissionsMiner.MineHistoricCarbonResults(
                                historicStartDateTime,
                                historicEndDateTime,
                                regionWattTimeName,
                                (int)emissionsRegionId);

                            // Mine Forecast Data
                            carbonEmissionsMiner.MineForecastMarginalCarbonResults(
                                forecastStartDateTime,
                                forecastEndDateTime,
                                regionWattTimeName,
                                (int)emissionsRegionId);
                        }
                    }
                }

                // Mine the regions emissions if an emissions node was supplied
                int? weatherRegionId = null;
                if (regionConfiguration.WeatherMiningRegion != null)
                {
                    var friendlyName = regionConfiguration.WeatherMiningRegion.friendlyName;

                    using (
                        new TimedOperation(
                            $"Beginning Mining of weather data for Region {friendlyName}",
                            "ApiDataMiner.MineRegionData()"))
                    {
                        
                        var timeZone = regionConfiguration.WeatherMiningRegion.TimeZone;
                        var regionLat = regionConfiguration.WeatherMiningRegion.Latitude;
                        var regionLong = regionConfiguration.WeatherMiningRegion.Longitude;
                        var weatherRegionWundergroundSubUrl =
                            regionConfiguration.WeatherMiningRegion.weatherRegionWundergroundSubUrl;
                        var wundergroundApiUrl = regionConfiguration.WeatherMiningRegion.ApiUrl;
                        var wundergroundApiKey = regionConfiguration.WeatherMiningRegion.ApiKey;
                        var selfThrottlingMethod = regionConfiguration.WeatherMiningRegion.SelfThrottlingMethod;
                        var maxNumberOfCallsPerMinute =
                            regionConfiguration.WeatherMiningRegion.MaxNumberOfCallsPerMinute;
                        var historicStartDateTime = DateTime.UtcNow.AddDays(-1);
                        var historicEndDateTime = DateTime.UtcNow.AddDays(1);

                        using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                        {
                            weatherRegionId =
                                _objectModel.AddWeatherRegion(
                                    friendlyName,
                                    timeZone,
                                    regionLat,
                                    regionLong,
                                    weatherRegionWundergroundSubUrl).WeatherRegionID;


                            WeatherDataMiner weatherDataMiner = new WeatherDataMiner(
                                                                    wundergroundApiUrl,
                                                                    wundergroundApiKey,
                                                                    selfThrottlingMethod,
                                                                    this.DatabaseConnectionString,
                                                                    maxNumberOfCallsPerMinute);

                            // Mine Recent Actual Data
                            weatherDataMiner.MineHistoricWeatherValues(
                                historicStartDateTime,
                                historicEndDateTime,
                                weatherRegionWundergroundSubUrl,
                                (int)weatherRegionId);

                            // Mine Forecast Data
                            weatherDataMiner.MineTenDayHourlyForecastWeatherValues(
                                weatherRegionWundergroundSubUrl,
                                (int)weatherRegionId);
                        }
                    }
                }

                // Group the Emissions and Weather regions if they haven't already been grouped
                using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                {
                    _objectModel.AddMarketWeatherEmissionsRegionMapping(
                        regionGroupingName,
                        null,
                        weatherRegionId,
                        emissionsRegionId);
                }
            }
        }
    }
}
