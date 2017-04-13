// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace ApiDataMinerTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    using ApiDataMiners;

    using EmissionsApiInteraction;

    using Microsoft.Azure;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SmartEnergyOM;

    using WeatherApiInteraction;

    /// <summary>
    /// These test methods demonstrate how to use individual data mining methods. 
    /// </summary>
    [TestClass]
    public class ApiDataMinerTests
    {
        private string databaseConnectionString = CloudConfigurationManager.GetSetting("SQLAzureDatabaseEntityFrameworkConnectionString");
        
        [TestMethod]
        public void TestMineHistoricWeatherValues()
        {
            // Arrange 
            var regionSubUrl = "us/nj/princeton";
            string smartGridRegionName = "PJM";
            var timeZone = "Eastern Standard Time";
            var regionLat = 40.348444276169;
            var regionLong = -74.6428556442261;

            var startDateTime = DateTime.Now.AddDays(-3);
            var endDateTime = DateTime.Now.AddDays(-1);

            var wundergroundApiUrl = CloudConfigurationManager.GetSetting("WundergroundApiUrl");
            var wundergroundApiKey = CloudConfigurationManager.GetSetting("WundergroundApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 5;

            int regionId;
            using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
            {
                regionId =
                    _objectModel.AddWeatherRegion(smartGridRegionName, timeZone, regionLat, regionLong).WeatherRegionID;
            }


            var wundergroundWeatherInteraction =
                new WundergroundWeatherInteraction(
                    selfThrottlingMethod,
                    maxNumberOfCallsPerMinute);

            WeatherDataMiner weatherDataMiner = new WeatherDataMiner(
                                                    wundergroundApiUrl,
                                                    wundergroundApiKey,
                                                    selfThrottlingMethod,
                                                    databaseConnectionString,
                                                    maxNumberOfCallsPerMinute,
                                                    wundergroundWeatherInteraction);

            // Act
            weatherDataMiner.MineHistoricWeatherValues(startDateTime, endDateTime, regionSubUrl, regionId);

            // Assert
            // Verify that each data point has been recorded in the database
            var results = wundergroundWeatherInteraction.GetHistoricWeatherData(
                wundergroundApiUrl,
                regionSubUrl,
                wundergroundApiKey,
                startDateTime,
                endDateTime);

            using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
            {
                foreach (var result in results)
                {
                    var dataPoint = _objectModel.FindWeatherDataPoint(regionId, result.observationDateTime);
                    Assert.IsNotNull(dataPoint);
                }
            }
        }

        [TestMethod]
        public void TestMineHistoricMarginalCarbonResults()
        {
            // Arrange 
            var startDateTime = DateTime.Now.AddDays(-10);
            var endDateTime =  DateTime.Now.AddDays(-9);

            var wattTimeApiUrl = CloudConfigurationManager.GetSetting("WattTimeApiUrl");
            var wattTimeApiKey = CloudConfigurationManager.GetSetting("WattTimeApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 9;

            List<WattTimeBalancingAuthorityInformation> regionsToMine =
                new List<WattTimeBalancingAuthorityInformation>
                    {
                        new WattTimeBalancingAuthorityInformation(
                            "PJM",
                            "US_PJM",
                            "Eastern Standard Time",
                            40.348444276169,
                            -74.6428556442261)
                    };

            foreach (var region in regionsToMine)
            {
                int regionId;
                using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                {
                    regionId =
                        _objectModel.AddEmissionsRegion(region.smartGridRegionName, region.timeZone, region.regionLat, region.regionLong)
                            .EmissionsRegionID;

                    _objectModel.AddMarketWeatherEmissionsRegionMapping(
                    region.smartGridRegionName,
                    null,
                    null,
                    regionId);
                }


                var wattTimeInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);

                CarbonEmissionsMiner carbonEmissionsMiner = new CarbonEmissionsMiner(
                                                                wattTimeApiUrl,
                                                                wattTimeApiKey,
                                                                selfThrottlingMethod,
                                                                databaseConnectionString,
                                                                maxNumberOfCallsPerMinute,
                                                                wattTimeInteraction);

                // Act
                carbonEmissionsMiner.MineHistoricMarginalCarbonResults(
                    startDateTime,
                    endDateTime,
                    region.regionWattTimeName,
                    regionId);

                // Assert
                // Verify that each data point has been recorded in the database
                var results = wattTimeInteraction.GetObservedMarginalCarbonResults(
                    wattTimeApiUrl,
                    region.regionWattTimeName,
                    startDateTime,
                    endDateTime,
                    null,
                    wattTimeApiKey);

                using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                {
                    foreach (var result in results)
                    {
                        var dataPoint = _objectModel.FindCarbonEmissionsDataPoint(regionId, result.timestamp);
                        Assert.IsNotNull(dataPoint);
                    }
                }
            }
        }

        [TestMethod]
        public void TestMineHistoricSystemWideCarbonResults()
        {
            // Arrange 

            var startDateTime = DateTime.Now.AddDays(-10);
            var endDateTime = DateTime.Now.AddDays(-5);

            var wattTimeApiUrl = CloudConfigurationManager.GetSetting("WattTimeApiUrl");
            var wattTimeApiKey = CloudConfigurationManager.GetSetting("WattTimeApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 9;

            List<WattTimeBalancingAuthorityInformation> regionsToMine =
                new List<WattTimeBalancingAuthorityInformation>
                    {
                        new WattTimeBalancingAuthorityInformation(
                            "PJM",
                            "US_PJM",
                            "Eastern Standard Time",
                            40.348444276169,
                            -74.6428556442261)
                    };

            foreach (var region in regionsToMine)
            {
                int regionId;
                using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                {
                    regionId =
                        _objectModel.AddEmissionsRegion(
                            region.smartGridRegionName,
                            region.timeZone,
                            region.regionLat,
                            region.regionLong).EmissionsRegionID;

                    _objectModel.AddMarketWeatherEmissionsRegionMapping(
                        region.smartGridRegionName,
                        null,
                        null,
                        regionId);
                }


                var wattTimeInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);

                CarbonEmissionsMiner carbonEmissionsMiner = new CarbonEmissionsMiner(
                                                                wattTimeApiUrl,
                                                                wattTimeApiKey,
                                                                selfThrottlingMethod,
                                                                databaseConnectionString,
                                                                maxNumberOfCallsPerMinute,
                                                                wattTimeInteraction);

                // Act
                carbonEmissionsMiner.MineHistoricSystemWideCarbonResults(
                    startDateTime,
                    endDateTime,
                    region.regionWattTimeName,
                    regionId);

                // Assert
                // Verify that each data point has been recorded in the database
                var results = wattTimeInteraction.GetGenerationMixAndSystemWideEmissionsResults(
                    wattTimeApiUrl,
                    region.regionWattTimeName,
                    startDateTime,
                    endDateTime,
                    null,
                    wattTimeApiKey);

                using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                {
                    foreach (var result in results)
                    {
                        var dataPoint = _objectModel.FindCarbonEmissionsDataPoint(regionId, result.timestamp);
                        Assert.IsNotNull(dataPoint);
                    }
                }
            }
        }

        [TestMethod]
        public void TestMineHistoricCarbonResults()
        {
            // Arrange 

            var startDateTime = DateTime.Now.AddDays(-10);
            var endDateTime = DateTime.Now.AddDays(-9);

            var wattTimeApiUrl = CloudConfigurationManager.GetSetting("WattTimeApiUrl");
            var wattTimeApiKey = CloudConfigurationManager.GetSetting("WattTimeApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 20;

            List<WattTimeBalancingAuthorityInformation> regionsToMine =
                new List<WattTimeBalancingAuthorityInformation>
                    {
                        new WattTimeBalancingAuthorityInformation(
                            "PJM",
                            "US_PJM",
                            "Eastern Standard Time",
                            40.348444276169,
                            -74.6428556442261)
                    };

            foreach (var region in regionsToMine)
            {
                int regionId;
                using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                {
                    regionId =
                        _objectModel.AddEmissionsRegion(region.smartGridRegionName, region.timeZone, region.regionLat, region.regionLong)
                            .EmissionsRegionID;

                    _objectModel.AddMarketWeatherEmissionsRegionMapping(
                    region.smartGridRegionName,
                    null,
                    null,
                    regionId);
                }


                var wattTimeInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);

                CarbonEmissionsMiner carbonEmissionsMiner = new CarbonEmissionsMiner(
                                                                wattTimeApiUrl,
                                                                wattTimeApiKey,
                                                                selfThrottlingMethod,
                                                                databaseConnectionString,
                                                                maxNumberOfCallsPerMinute,
                                                                wattTimeInteraction);

                // Act
                carbonEmissionsMiner.MineHistoricCarbonResults(
                    startDateTime,
                    endDateTime,
                    region.regionWattTimeName,
                    regionId);

                // Assert
                // Verify that each data point has been recorded in the database
                var marginalCarbonResults = wattTimeInteraction.GetObservedMarginalCarbonResults(
                    wattTimeApiUrl,
                    region.regionWattTimeName,
                    startDateTime,
                    endDateTime,
                    null,
                    wattTimeApiKey);

                using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                {
                    foreach (var result in marginalCarbonResults)
                    {
                        var dataPoint = _objectModel.FindCarbonEmissionsDataPoint(regionId, result.timestamp);
                        Assert.IsNotNull(dataPoint);
                    }
                }

                var systemWideResults = wattTimeInteraction.GetGenerationMixAndSystemWideEmissionsResults(
                    wattTimeApiUrl,
                    region.regionWattTimeName,
                    startDateTime,
                    endDateTime,
                    null,
                    wattTimeApiKey);

                using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                {
                    foreach (var result in systemWideResults)
                    {
                        var dataPoint = _objectModel.FindCarbonEmissionsDataPoint(regionId, result.timestamp);
                        Assert.IsNotNull(dataPoint);
                    }
                }
            }
        }

        [TestMethod]
        public void TestMineForecastMarginalCarbonResults()
        {
            // Arrange 
            var regionWattTimeName = "PJM";
            var smartGridRegionName = "PJM";
            var timeZone = "Eastern Standard Time";
            var regionLat = 40.348444276169;
            var regionLong = -74.6428556442261;

            var startDateTime = DateTime.UtcNow.AddDays(-2);
            var endDateTime = DateTime.UtcNow.AddDays(10);

            var wattTimeApiUrl = CloudConfigurationManager.GetSetting("WattTimeApiUrl");
            var wattTimeApiKey = CloudConfigurationManager.GetSetting("WattTimeApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 9;

            int regionId;
            using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
            {
                regionId =
                    _objectModel.AddEmissionsRegion(smartGridRegionName, timeZone, regionLat, regionLong)
                        .EmissionsRegionID;
            }


            var wattTimeInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);

            CarbonEmissionsMiner carbonEmissionsMiner = new CarbonEmissionsMiner(
                                                            wattTimeApiUrl,
                                                            wattTimeApiKey,
                                                            selfThrottlingMethod,
                                                            databaseConnectionString,
                                                            maxNumberOfCallsPerMinute,
                                                            wattTimeInteraction);

            // Act
            carbonEmissionsMiner.MineForecastMarginalCarbonResults(
                startDateTime,
                endDateTime,
                regionWattTimeName,
                regionId);

            // Assert
            // Verify that each data point has been recorded in the database
            var results = wattTimeInteraction.GetForecastMarginalCarbonResults(
                wattTimeApiUrl,
                regionWattTimeName,
                startDateTime,
                endDateTime,
                null,
                wattTimeApiKey);

            using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
            {
                foreach (var result in results)
                {
                    if (result.marginal_carbon.value != null)
                    {
                        var dataPoint = _objectModel.FindCarbonEmissionsDataPoint(regionId, result.timestamp);
                        Assert.IsNotNull(dataPoint);
                        Assert.AreEqual(
                            wattTimeInteraction.ConvertLbsPerMWhTo_GCo2PerkWh((double)result.marginal_carbon.value),
                            dataPoint.MarginalCO2Intensity_gCO2kWh);
                    }
                }
            }
        }

        [TestMethod]
        public void TestParseMinerSettingsFileAndMineData()
        {
            // Arrange
            var ConfigPath = @".\ApiDataMinerConfigs\ApiDataMinerConfigs.xml";

            // Act
            var apiDataMiner = new ApiDataMiner(databaseConnectionString);
            apiDataMiner.ParseMinerSettingsFileAndMineData(ConfigPath);

            // Assert
            using (var streamReader = new StreamReader(ConfigPath))
            {
                var xmlSerializer = new XmlSerializer(typeof(ApiMinerConfigLayout));
                var minerConfigs = (ApiMinerConfigLayout)xmlSerializer.Deserialize(streamReader);

                foreach (var regionConfiguration in minerConfigs.Regions)
                {
                    // Verify emissions were mined successfully for each region in the Config File
                    if (regionConfiguration.EmissionsMiningRegion != null)
                    {
                        var emissionsRegionName = regionConfiguration.EmissionsMiningRegion.friendlyName;
                        var timeZone = regionConfiguration.EmissionsMiningRegion.TimeZone;
                        var regionLat = regionConfiguration.EmissionsMiningRegion.Latitude;
                        var regionLong = regionConfiguration.EmissionsMiningRegion.Longitude;
                        var regionWattTimeName = regionConfiguration.EmissionsMiningRegion.EmissionsWattTimeAbbreviation;
                        var wattTimeApiUrl = regionConfiguration.EmissionsMiningRegion.ApiUrl;
                        var wattTimeApiKey = regionConfiguration.EmissionsMiningRegion.ApiKey;
                        var selfThrottlingMethod = regionConfiguration.WeatherMiningRegion.SelfThrottlingMethod;
                        var maxNumberOfCallsPerMinute =
                            regionConfiguration.WeatherMiningRegion.MaxNumberOfCallsPerMinute;
                        var startDateTime = DateTime.UtcNow.AddDays(-2);
                        var endDateTime = DateTime.UtcNow.AddDays(10);

                        var wattTimeInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);
                        var results = wattTimeInteraction.GetObservedMarginalCarbonResults(
                            wattTimeApiUrl,
                            regionWattTimeName,
                            startDateTime,
                            endDateTime,
                            null,
                            wattTimeApiKey);

                        int regionId;
                        using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                        {
                            regionId =
                                _objectModel.AddEmissionsRegion(emissionsRegionName, timeZone, regionLat, regionLong)
                                    .EmissionsRegionID;
                        }

                        using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                        {
                            foreach (var result in results)
                            {
                                var dataPoint = _objectModel.FindCarbonEmissionsDataPoint(regionId, result.timestamp);
                                Assert.IsNotNull(dataPoint);
                            }
                        }
                    }

                    // Verify weather was mined successfully for each region in the Config File
                    if (regionConfiguration.WeatherMiningRegion != null)
                    {
                        var emissionsRegionName = regionConfiguration.WeatherMiningRegion.friendlyName;
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

                        var wundergroundWeatherInteraction = new WundergroundWeatherInteraction(
                                                                 selfThrottlingMethod,
                                                                 maxNumberOfCallsPerMinute);
                        var results =
                            wundergroundWeatherInteraction.GetTenDayHourlyForecastWeatherData(
                                wundergroundApiUrl,
                                weatherRegionWundergroundSubUrl,
                                wundergroundApiKey);

                        int regionId;
                        using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                        {
                            regionId =
                                _objectModel.AddWeatherRegion(emissionsRegionName, timeZone, regionLat, regionLong)
                                    .WeatherRegionID;
                        }

                        using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                        {
                            foreach (var result in results)
                            {
                                var dataPoint = _objectModel.FindWeatherDataPoint(
                                    regionId,
                                    result.observationDateTime);
                                Assert.IsNotNull(dataPoint);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A class to represent a WattTime Balancing Authority along with it's timezone, latitude and longtitude for passing to Emissions Mining integration tests
        /// </summary>
        public class WattTimeBalancingAuthorityInformation
        {
            public string regionWattTimeName;
            public string smartGridRegionName;
            public string timeZone;
            public double regionLat;
            public double regionLong;

            public WattTimeBalancingAuthorityInformation(string regionWattTimeName, string smartGridRegionName, string timeZone, double regionLat, double regionLong)
            {
                this.regionWattTimeName = regionWattTimeName;
                this.smartGridRegionName = smartGridRegionName;
                this.timeZone = timeZone;
                this.regionLat = regionLat;
                this.regionLong = regionLong;
            }
        }
    }
}
