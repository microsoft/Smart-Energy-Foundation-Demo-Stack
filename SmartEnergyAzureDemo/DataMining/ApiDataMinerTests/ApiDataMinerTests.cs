// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiDataMinerTests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    using ApiDataMiners;

    using ApiInteraction;

    using EmissionsApiInteraction;

    using Microsoft.Azure;

    using SmartEnergyOM;

    using WeatherApiInteraction;
    using WeatherApiInteraction.WundergroundTenDayHourlyForecastDataClasses;

    [TestClass]
    public class ApiDataMinerTests
    {
        private string databaseConnectionString = CloudConfigurationManager.GetSetting("SQLAzureDatabaseEntityFrameworkConnectionString");
        
        [TestMethod]
        public void TestMineHistoricWeatherValues_ByWundergroundLocationName()
        {
            // Arrange 
            var regionSubUrl = "Norway/Kristiansand";
            var smartGridRegionName = "Norway_Kristiansand";
            var timeZone = "Central European Standard Time";
            var regionLat = 58.24158635676374;
            var regionLong = 8.096923830624974;

            var startDateTime = new DateTime(2017, 1, 1); // DateTime.Now.AddDays(-10);
            var endDateTime = new DateTime(2017, 2, 1); // var startDateTime = DateTime.Now.AddDays(-1);

            var wundergroundApiUrl = CloudConfigurationManager.GetSetting("WundergroundApiUrl");
            var wundergroundApiKey = CloudConfigurationManager.GetSetting("WundergroundApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 5;

            int regionId;
            using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
            {
                regionId =
                    _objectModel.AddWeatherRegion(smartGridRegionName, timeZone, regionLat, regionLong, regionSubUrl).WeatherRegionID;
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
        public void TestMineHistoricWeatherValues_ByGPS()
        {
            // Arrange 
            var startDateTime = new DateTime(2017, 1, 1); // DateTime.Now.AddDays(-10);
            var endDateTime = new DateTime(2017, 1, 2); // var startDateTime = DateTime.Now.AddDays(-1);
            
            var latitude = 58.279231;
            var longtitude = 6.892410;
            var smartGridRegionName = "Norway_Oye";
            var timeZone = "Central European Standard Time";

            var wundergroundApiUrl = CloudConfigurationManager.GetSetting("WundergroundApiUrl");
            var wundergroundApiKey = CloudConfigurationManager.GetSetting("WundergroundApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 5;

            int regionId;
            using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
            {
                regionId =
                    _objectModel.AddWeatherRegion(smartGridRegionName, timeZone, latitude, longtitude, null).WeatherRegionID;
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
            weatherDataMiner.MineHistoricWeatherValues(startDateTime, endDateTime, latitude, longtitude, regionId);

            // Assert
        }

        [TestMethod]
        public void TestMineHistoricMarginalCarbonResults()
        {
            // Arrange 
            var startDateTime = DateTime.Now.AddDays(-10);
            var endDateTime =  DateTime.Now.AddDays(-1);

            var wattTimeApiUrl = CloudConfigurationManager.GetSetting("WattTimeApiUrl");
            var wattTimeApiKey = CloudConfigurationManager.GetSetting("WattTimeApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 9;

            List<WattTimeBalancingAuthorityInformation> regionsToMine = new List<WattTimeBalancingAuthorityInformation>();

            regionsToMine.Add(new WattTimeBalancingAuthorityInformation("PSEI", "US_PugetSoundEnergy", "Pacific Standard Time", 47.68009593341535, -122.11638450372567));

            regionsToMine.Add(new WattTimeBalancingAuthorityInformation("BPA", "US_BPA", "Pacific Standard Time", 40.348444276169, -74.6428556442261));
            regionsToMine.Add(new WattTimeBalancingAuthorityInformation("CAISO", "US_CAISO", "Pacific Standard Time", 41.7324, -123.409423));
            regionsToMine.Add(new WattTimeBalancingAuthorityInformation("ERCOT", "US_ERCOT", "Central Standard Time", 32.79878236662912, -96.77856445062508));
            regionsToMine.Add(new WattTimeBalancingAuthorityInformation("ISONE", "US_ISONewEngland", "Eastern Standard Time", 42.70864591994315, -72.16918945062508));
            regionsToMine.Add(new WattTimeBalancingAuthorityInformation("MISO", "US_UpperMidwestISO", "Central Standard Time", 41.91853269857261, -93.55193137872567));
            regionsToMine.Add(new WattTimeBalancingAuthorityInformation("PJM", "US_PJM", "Eastern Standard Time", 40.348444276169, -74.6428556442261));
            regionsToMine.Add(new WattTimeBalancingAuthorityInformation("SPP", "US_SouthwesternPublicServiceISO", "Eastern Standard Time", 34.41133502036136, -103.19243430841317));

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
            var startDateTime = new DateTime(2016, 1, 1); // DateTime.Now.AddDays(-10);
            var endDateTime = new DateTime(2017, 1, 3); // var startDateTime = DateTime.Now.AddDays(-1);

            var wattTimeApiUrl = CloudConfigurationManager.GetSetting("WattTimeApiUrl");
            var wattTimeApiKey = CloudConfigurationManager.GetSetting("WattTimeApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 9;

            List<WattTimeBalancingAuthorityInformation> regionsToMine = new List<WattTimeBalancingAuthorityInformation>();
            
            regionsToMine.Add(new WattTimeBalancingAuthorityInformation("ERCOT", "US_ERCOT", "Central Standard Time", 32.79878236662912, -96.77856445062508));

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

            var startDateTime = new DateTime(2016, 7, 1); // DateTime.Now.AddDays(-10);
            var endDateTime = new DateTime(2016, 10, 1); // var startDateTime = DateTime.Now.AddDays(-1);

            var wattTimeApiUrl = CloudConfigurationManager.GetSetting("WattTimeApiUrl");
            var wattTimeApiKey = CloudConfigurationManager.GetSetting("WattTimeApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 200;


            List<WattTimeBalancingAuthorityInformation> regionsToMine = new List<WattTimeBalancingAuthorityInformation>();
            regionsToMine.Add(new WattTimeBalancingAuthorityInformation("PJM", "US_PJM", "Eastern Standard Time", 40.348444276169, -74.6428556442261));

            //regionsToMine.Add(new WattTimeBalancingAuthorityInformation("PSEI", "US_PugetSoundEnergy", "Pacific Standard Time", 47.68009593341535, -122.11638450372567));

            //regionsToMine.Add(new WattTimeBalancingAuthorityInformation("BPA", "US_BPA", "Pacific Standard Time", 40.348444276169, -74.6428556442261));
            //regionsToMine.Add(new WattTimeBalancingAuthorityInformation("CAISO", "US_CAISO", "Pacific Standard Time", 41.7324, -123.409423));
            //regionsToMine.Add(new WattTimeBalancingAuthorityInformation("ERCOT", "US_ERCOT", "Central Standard Time", 32.79878236662912, -96.77856445062508));
            //regionsToMine.Add(new WattTimeBalancingAuthorityInformation("ISONE", "US_ISONewEngland", "Eastern Standard Time", 42.70864591994315, -72.16918945062508));
            //regionsToMine.Add(new WattTimeBalancingAuthorityInformation("MISO", "US_UpperMidwestISO", "Central Standard Time", 41.91853269857261, -93.55193137872567));
            //regionsToMine.Add(new WattTimeBalancingAuthorityInformation("PJM", "US_PJM", "Eastern Standard Time", 40.348444276169, -74.6428556442261));
            //regionsToMine.Add(new WattTimeBalancingAuthorityInformation("SPP", "US_SouthwesternPublicServiceISO", "Eastern Standard Time", 34.41133502036136, -103.19243430841317));

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
                        List<HourlyForecast> results = new List<HourlyForecast>();

                        switch (regionConfiguration.WeatherMiningRegion.MiningMethod)
                        {
                            case "GPS":
                                results =
                               wundergroundWeatherInteraction.GetTenDayHourlyForecastWeatherData(
                                   wundergroundApiUrl,
                                   regionConfiguration.WeatherMiningRegion.Latitude,
                                   regionConfiguration.WeatherMiningRegion.Longitude,
                                   wundergroundApiKey);
                                break;

                            case "WundergroundPageSubUrl":
                            default:
                                results =
                                   wundergroundWeatherInteraction.GetTenDayHourlyForecastWeatherData(
                                       wundergroundApiUrl,
                                       weatherRegionWundergroundSubUrl,
                                       wundergroundApiKey);
                                break;
                        }

                       

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
