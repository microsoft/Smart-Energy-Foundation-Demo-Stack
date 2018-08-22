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

        string wattTimeApiV2Url = CloudConfigurationManager.GetSetting("WattTimeApiV2Url");
        string WattTimeUsername = CloudConfigurationManager.GetSetting("WattTimeUsername");
        string WattTimePassword = CloudConfigurationManager.GetSetting("WattTimePassword");
        string WattTimeEmail = CloudConfigurationManager.GetSetting("WattTimeEmail");
        string WattTimeOrganization = CloudConfigurationManager.GetSetting("WattTimeOrganization");

        [TestMethod]
        public void TestGetMarginalCarbonResults()
        {
            // Arrange
            var regionAbbreviation = "PJM";
            EmissionsApiInteraction emissionsApiInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);

            // Act
            var pointsReturned = emissionsApiInteraction.GetMarginalCarbonResults(this.wattTimeApiUrl, regionAbbreviation, WattTimeUsername, WattTimePassword, DateTime.Now.AddDays(-2), DateTime.Now.AddDays(2), null, wattTimeApiKey);

            // Assert
            Assert.IsTrue(pointsReturned.Count > 0);
        }

        [TestMethod]
        public void TestGetMostRecentMarginalCarbonEmissionsResult()
        {
            // Arrange
            var regionAbbreviation = "PJM";
            EmissionsApiInteraction emissionsApiInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);
            emissionsApiInteraction.RegisterWithWattTime(this.wattTimeApiV2Url, WattTimeUsername, WattTimePassword, WattTimeEmail, WattTimeOrganization);

            // Act
            var pointsReturned = emissionsApiInteraction.GetMostRecentMarginalCarbonEmissionsResult(this.wattTimeApiV2Url, regionAbbreviation, WattTimeUsername, WattTimePassword);

            // Assert
            Assert.IsTrue(pointsReturned.value > -1);
        }        

        [TestMethod]
        public void TestGetGenerationMixAndSystemWideEmissionsResults()
        {
            // Arrange
            var regionAbbreviation = "PJM";
            EmissionsApiInteraction emissionsApiInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);
            emissionsApiInteraction.RegisterWithWattTime(this.wattTimeApiV2Url, WattTimeUsername, WattTimePassword, WattTimeEmail, WattTimeOrganization);

            // Act
            var pointsReturned = emissionsApiInteraction.GetGenerationMixAndSystemWideEmissionsResults(this.wattTimeApiUrl, regionAbbreviation, DateTime.Now.AddDays(-15), DateTime.Now.AddDays(2), null, wattTimeApiKey);

            // Assert
            Assert.IsTrue(pointsReturned.Count > 0);
        }

        [TestMethod]
        public void TestRegisterWithWattTime()
        {
            // Arrange
            EmissionsApiInteraction emissionsApiInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);

            // Act
            var tokenReturned = emissionsApiInteraction.RegisterWithWattTime(this.wattTimeApiV2Url, WattTimeUsername, WattTimePassword, WattTimeEmail, WattTimeOrganization);

            // Assert
            Assert.IsNotNull(tokenReturned);
        }

        [TestMethod]
        public void TestCarbonEmissionsRelativeMeritResults()
        {
            // Arrange
            var regionAbbreviation = "PJM";
            EmissionsApiInteraction emissionsApiInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);
            emissionsApiInteraction.RegisterWithWattTime(this.wattTimeApiV2Url, WattTimeUsername, WattTimePassword, WattTimeEmail, WattTimeOrganization);

            // Act
            var indexReturned = emissionsApiInteraction.GetCarbonEmissionsRelativeMeritResults(this.wattTimeApiV2Url, regionAbbreviation, WattTimeUsername, WattTimePassword);

            // Assert
            Assert.IsTrue(-1 < indexReturned.rating);
            Assert.IsTrue(indexReturned.rating < 6);
        }

        [TestMethod]
        public void TestRetrieveWattTimeAuthToken()
        {
            // Arrange
            EmissionsApiInteraction emissionsApiInteraction = new EmissionsApiInteraction(selfThrottlingMethod, maxNumberOfCallsPerMinute);
            emissionsApiInteraction.RegisterWithWattTime(this.wattTimeApiV2Url, WattTimeUsername, WattTimePassword, WattTimeEmail, WattTimeOrganization);

            // Act
            var tokenReturned = emissionsApiInteraction.RetrieveWattTimeAuthToken(this.wattTimeApiV2Url, WattTimeUsername, WattTimePassword);

            // Assert
            Assert.IsNotNull(tokenReturned);
        }
    }
}

