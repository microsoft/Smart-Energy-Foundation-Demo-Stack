// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace WeatherApiInteractionTests
{
    using System;
    using System.Linq;

    using ApiInteraction;

    using Microsoft.Azure;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using WeatherApiInteraction;

    [TestClass]
    public class WeatherApiInteractionTests
    {
        private readonly string wundergroundApiUrl =
            CloudConfigurationManager.GetSetting("WundergroundApiUrl");
        private readonly string wundergroundApiKey =
            CloudConfigurationManager.GetSetting("WundergroundApiKey");
        private readonly WundergroundWeatherInteraction wundergroundWeatherInteraction = 
            new WundergroundWeatherInteraction(SelfThrottlingMethod.AzureTableStorageCallRecollection, 5);
        
        [TestMethod]
        public void TestGetHistoricWeatherData()
        {
            // Arrange
            var regionSubUrl = "us/nj/princeton";

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
    }
}
