// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmissionsApiInteractionTests
{
    using EmissionsApiInteraction;

    using Microsoft.Azure;

    [TestClass]
    public class EmissionsApiInteractionTests
    {
        string selfThrottlingMethod = "AzureTableStorageCallRecollection";
        int maxNumberOfCallsPerMinute = 9;

        string wattTimeApiUrl = CloudConfigurationManager.GetSetting("WattTimeApiUrl");
        string wattTimeApiKey = CloudConfigurationManager.GetSetting("WattTimeApiKey");

        [TestMethod]
        public void TestGetMarginalCarbonResults()
        {
            // Arrange
            var regionAbbreviation = "PJM";
            EmissionsApiInteraction emissionsApiInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);

            // Act
            var pointsReturned = emissionsApiInteraction.GetMarginalCarbonResults(this.wattTimeApiUrl, regionAbbreviation, DateTime.Now.AddDays(-2), DateTime.Now.AddDays(2), null, wattTimeApiKey);

            // Assert
            Assert.IsTrue(pointsReturned.Count > 0);
        }

        [TestMethod]
        public void TestGetMostRecentMarginalCarbonEmissionsResult()
        {
            // Arrange
            var regionAbbreviation = "PJM";
            EmissionsApiInteraction emissionsApiInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);

            // Act
            var pointsReturned = emissionsApiInteraction.GetMostRecentMarginalCarbonEmissionsResult(this.wattTimeApiUrl, regionAbbreviation, null, wattTimeApiKey);

            // Assert
            Assert.IsTrue(pointsReturned.marginal_carbon.value > -1);
        }

        [TestMethod]
        public void TestGetGenerationMixAndSystemWideEmissionsResults()
        {
            // Arrange
            var regionAbbreviation = "PJM";
            EmissionsApiInteraction emissionsApiInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);

            // Act
            var pointsReturned = emissionsApiInteraction.GetGenerationMixAndSystemWideEmissionsResults(this.wattTimeApiUrl, regionAbbreviation, DateTime.Now.AddDays(-15), DateTime.Now.AddDays(2), null, wattTimeApiKey);

            // Assert
            Assert.IsTrue(pointsReturned.Count > 0);
        }
    }
}
