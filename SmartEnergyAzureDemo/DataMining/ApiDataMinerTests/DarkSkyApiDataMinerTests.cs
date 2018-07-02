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
    using WeatherApiInteraction.DarkSkyWeatherMining;

    [TestClass]
    public class DarkSkyApiDataMinerTests
    {
        private string databaseConnectionString = CloudConfigurationManager.GetSetting("SQLAzureDatabaseEntityFrameworkConnectionString");

        [TestMethod]
        public void TestMineHistoricWeatherValues_ByGPS()
        {
            // Arrange 
            var startDateTime = DateTime.Now.AddDays(-10);
            var endDateTime = DateTime.Now.AddDays(-1);

            var latitude = 53.3498;
            var longtitude = -6.2603;
            var smartGridRegionName = "Ireland";
            var timeZone = "GMT Standard Time";

            var darkSkyApiUrl = CloudConfigurationManager.GetSetting("DarkSkyApiUrl");
            var darkSkyApiKey = CloudConfigurationManager.GetSetting("DarkSkyApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 5;
            var maxNumberOfCallsPerDay = 500;

            int regionId;
            using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
            {
                regionId =
                    _objectModel.AddWeatherRegion(smartGridRegionName, timeZone, latitude, longtitude, null).WeatherRegionID;
            }

            var darkSkyWeatherInteraction =
                new DarkSkyWeatherInteraction(
                    selfThrottlingMethod,
                    maxNumberOfCallsPerMinute,
                    maxNumberOfCallsPerDay);

            DarkSkyWeatherDataMiner weatherDataMiner = new DarkSkyWeatherDataMiner(
                                                    darkSkyApiUrl,
                                                    darkSkyApiKey,
                                                    selfThrottlingMethod,
                                                    databaseConnectionString,
                                                    maxNumberOfCallsPerMinute,
                                                    maxNumberOfCallsPerDay,
                                                    darkSkyWeatherInteraction);

            // Act
            weatherDataMiner.MineHistoricWeatherValues(startDateTime, endDateTime, latitude, longtitude, regionId);

            // Assert
            //Verify that each data point has been recorded in the database
            var results = darkSkyWeatherInteraction.GetHistoricWeatherData(
                darkSkyApiUrl,
                darkSkyApiKey,
                latitude, 
                longtitude,
                startDateTime,
                endDateTime);
            using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
            {
                foreach (var result in results)
                {
                    var dataPoint = _objectModel.FindWeatherDataPoint(regionId, result.dateTime);
                    Assert.IsNotNull(dataPoint);
                }
            }
        }

        [TestMethod]
        public void TestMineForecastcWeatherValues_ByGPS()
        {
            // Arrange
            var startDateTime = DateTime.UtcNow;
            var endDateTime = DateTime.UtcNow.AddDays(10);

            var latitude = 53.3498;
            var longtitude = -6.2603;
            var smartGridRegionName = "Ireland";
            var timeZone = "GMT Standard Time";

            var darkSkyApiUrl = CloudConfigurationManager.GetSetting("DarkSkyApiUrl");
            var darkSkyApiKey = CloudConfigurationManager.GetSetting("DarkSkyApiKey");
            var selfThrottlingMethod = "AzureTableStorageCallRecollection";
            var maxNumberOfCallsPerMinute = 5;
            var maxNumberOfCallsPerDay = 500;

            int regionId;
            using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
            {
                regionId =
                    _objectModel.AddWeatherRegion(smartGridRegionName, timeZone, latitude, longtitude, null).WeatherRegionID;
            }

            var darkSkyWeatherInteraction =
                new DarkSkyWeatherInteraction(
                    selfThrottlingMethod,
                    maxNumberOfCallsPerMinute,
                    maxNumberOfCallsPerDay);

            DarkSkyWeatherDataMiner weatherDataMiner = new DarkSkyWeatherDataMiner(
                                                    darkSkyApiUrl,
                                                    darkSkyApiKey,
                                                    selfThrottlingMethod,
                                                    databaseConnectionString,
                                                    maxNumberOfCallsPerMinute,
                                                    maxNumberOfCallsPerDay,
                                                    darkSkyWeatherInteraction);

            // Act
            weatherDataMiner.MineForecastWeatherValues(startDateTime, endDateTime, latitude, longtitude, regionId);

            // Assert
            //Verify that each data point has been recorded in the database
            var results = darkSkyWeatherInteraction.GetForecastWeatherData(
                darkSkyApiUrl,
                darkSkyApiKey,
                latitude,
                longtitude,
                startDateTime,
                endDateTime);
            using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
            {
                foreach (var result in results)
                {
                    var dataPoint = _objectModel.FindWeatherDataPoint(regionId, result.dateTime);
                    Assert.IsNotNull(dataPoint);
                }
            }
        }
    }
}
