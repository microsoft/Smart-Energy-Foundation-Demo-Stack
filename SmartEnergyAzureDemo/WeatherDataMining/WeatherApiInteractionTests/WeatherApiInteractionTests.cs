// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WeatherApiInteractionTests
{
    using System.Linq;

    using Microsoft.Azure;

    using WeatherApiInteraction;

    using ApiInteraction;

    [TestClass]
    public class WeatherApiInteractionTests
    {
        private readonly string wundergroundApiUrl = CloudConfigurationManager.GetSetting("WundergroundApiUrl");

        private readonly string wundergroundApiKey = CloudConfigurationManager.GetSetting("WundergroundApiKey");

        private readonly WundergroundWeatherInteraction wundergroundWeatherInteraction =
            new WundergroundWeatherInteraction(SelfThrottlingMethod.AzureTableStorageCallRecollection, 9);

        [TestMethod]
        public void TestGetHistoricWeatherData()
        {
            // Arrange
            var regionSubUrl = "CA/San_Francisco";

            // Act
            var results = this.wundergroundWeatherInteraction.GetHistoricWeatherData(
                this.wundergroundApiUrl,
                regionSubUrl,
                wundergroundApiKey,
                DateTime.UtcNow.AddDays(-2));

            // Assert
            Assert.IsTrue(results.Count > 0);
            foreach (var VARIABLE in results.OrderBy(x => x.observationDateTime))
            {
                Console.WriteLine($"Pulled value for {VARIABLE.observationDateTime} of {VARIABLE.tempi}");
            }
        }

        [TestMethod]
        public void TestGetTenDayWeatherForecastWeatherData_ByCityName()
        {
            // Arrange
            var regionSubUrl = "CA/San_Francisco";

            // Act
            var results = this.wundergroundWeatherInteraction.GetTenDayHourlyForecastWeatherData(
                this.wundergroundApiUrl,
                regionSubUrl,
                this.wundergroundApiKey);

            // Assert
            Assert.IsTrue(results.Count > 0);
            foreach (var hourlyForecast in results.OrderBy(x => x.observationDateTime))
            {
                Console.WriteLine($"Pulled value for {hourlyForecast.observationDateTime} of {hourlyForecast.temp.metric}");
            }
        }

        [TestMethod]
        public void TestGetTenDayWeatherForecastWeatherData_ByGPSCoordinates()
        {
            // Arrange
            var latitude = 58.279231;
            var longtitude = 6.892410;

            // Act
            var results = this.wundergroundWeatherInteraction.GetTenDayHourlyForecastWeatherData(
                this.wundergroundApiUrl,
                latitude,
                longtitude,
                this.wundergroundApiKey);

            // Assert
            Assert.IsTrue(results.Count > 0);
            foreach (var hourlyForecast in results.OrderBy(x => x.observationDateTime))
            {
                Console.WriteLine($"Pulled value for {hourlyForecast.observationDateTime} of {hourlyForecast.temp.metric}");
            }
        }
    }
}
