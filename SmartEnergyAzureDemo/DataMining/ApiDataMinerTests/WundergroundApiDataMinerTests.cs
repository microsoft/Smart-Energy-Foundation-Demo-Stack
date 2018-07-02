namespace ApiDataMiner.Functional.Tests
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
    using WeatherApiInteraction.WundergroundTenDayHourlyForecastDataClasses;

    [TestClass]
    public class WundergroundApiDataMinerTests
    {
        private string databaseConnectionString = CloudConfigurationManager.GetSetting("SQLAzureDatabaseEntityFrameworkConnectionString");

        [TestMethod]
        public void TestMineHistoricWeatherValues_ByWundergroundLocationName()
        {
            // Arrange 
            var regionSubUrl = "ie/dublin";
            var latitude = 53.3498;
            var longtitude = -6.2603;
            var smartGridRegionName = "Ireland";
            var timeZone = "GMT Standard Time";

            var startDateTime = DateTime.Now.AddDays(-10);
            var endDateTime = DateTime.Now.AddDays(-1);

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

            WundergroundWeatherDataMiner weatherDataMiner = new WundergroundWeatherDataMiner(
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

            var latitude = 53.3498;
            var longtitude = -6.2603;
            var smartGridRegionName = "Ireland";
            var timeZone = "GMT Standard Time";

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

            WundergroundWeatherDataMiner weatherDataMiner = new WundergroundWeatherDataMiner(
                                                    wundergroundApiUrl,
                                                    wundergroundApiKey,
                                                    selfThrottlingMethod,
                                                    databaseConnectionString,
                                                    maxNumberOfCallsPerMinute,
                                                    wundergroundWeatherInteraction);

            // Act
            weatherDataMiner.MineHistoricWeatherValues(startDateTime, endDateTime, latitude, longtitude, regionId);

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
    }
}
