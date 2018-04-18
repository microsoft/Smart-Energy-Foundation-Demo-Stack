// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataApi.Tests
{
    using Microsoft.Azure;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SmartEnergyAzureDataTypes;
    using SmartEnergyAzureDataTypes.Helpers;

    [TestClass]
    public class DataApiIntegrationTests
    {
        private Uri baseUrl = new Uri(CloudConfigurationManager.GetSetting("DataApiUrl"));

        private readonly WebApiHelper webApiHelper = new WebApiHelper();

        [TestMethod]
        public void TestGetEmissionsDataPointsForRegion()
        {
            //Arrange            
            string specificRegionDesired = null; // "US_PJM";
            string regionName = string.Empty;
            if (string.IsNullOrEmpty(specificRegionDesired))
            {
                string regionSubUrl = $"api/Regions";
                var responseRegions =
                    webApiHelper.GetHttpResponseContentAsType<List<EmissionsRegionDataPoint>>(
                        baseUrl.AbsoluteUri,
                        regionSubUrl).Result;
                Assert.IsTrue(responseRegions.Any(), "No regions were returned from WebAPI before test could be run");
                regionName = responseRegions.FirstOrDefault().FriendlyName;
            }
            else
            {
                regionName = specificRegionDesired;
            }

            string subUrl = $"api/Emissions/{regionName}";

            //Act
            var response =
                webApiHelper.GetHttpResponseContentAsType<List<GridEmissionsDataPoint>>(
                    baseUrl.AbsoluteUri,
                    subUrl).Result;

            //Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Any());
        }

        [TestMethod]
        public void TestGetEmissionsDataPointsForRegionAndDateTimes()
        {
            //Arrange
            var region = "US_PJM";
            string subUrl = $"api/Emissions/{region}";
            DateTime startDateTime = DateTime.UtcNow.AddHours(-1);
            DateTime endDateTime = DateTime.UtcNow;
            var dateTimeFlexabilityInMinutes = 30;
            var queryDictionary = new Dictionary<string, string>();
            queryDictionary.Add("startDateTime", webApiHelper.FormatDateTimeStringForWebApi(startDateTime.AddMinutes(-1)));
            queryDictionary.Add("endDateTime", webApiHelper.FormatDateTimeStringForWebApi(endDateTime.AddMinutes(1)));
            queryDictionary.Add("returnOnlyNonNullSystemWideEmissionsDataPoints", "false");
            queryDictionary.Add("returnOnlyNonNullMarginalEmissionsDataPoints", "false");
            queryDictionary.Add("dateTimeFlexabilityInMinutes", dateTimeFlexabilityInMinutes.ToString());

            var queryString = webApiHelper.CompileParamsAndEncode(queryDictionary);
            string querySubUrl = $"{subUrl}{queryString}";

            //Act
            var response =
                webApiHelper.GetHttpResponseContentAsType<List<GridEmissionsDataPoint>>(
                    baseUrl.AbsoluteUri,
                    querySubUrl).Result;

            //Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Any());
        }

        [TestMethod]
        public void TestGetAllAvailableRegions()
        {
            // Arrange 
            string regionSubUrl = $"api/Regions";

            // Act
            var responseRegions =
                webApiHelper.GetHttpResponseContentAsType<List<EmissionsRegionDataPoint>>(
                    baseUrl.AbsoluteUri,
                    regionSubUrl).Result;

            // Assert
            Assert.IsTrue(responseRegions.Any(), "No regions were returned from WebAPI");
            var regionName = responseRegions.FirstOrDefault().FriendlyName;
            Console.WriteLine($"Received {responseRegions.Count} regions back from Azure SmartEnergyDataApi:");
            foreach(var element in responseRegions)
            {
                Console.Write($"{element.FriendlyName}:{element.EmissionsRegionID},");
            }
        }

        [TestMethod]
        public void TestGetGridEmissionsRelativeMeritForRegionAndDateTimes()
        {
            //Arrange
            var region = "US_PJM";
            string subUrl = $"api/GridEmissionsRelativeMerit/{region}";
            DateTime startDateTime = DateTime.UtcNow.AddHours(-1);
            DateTime endDateTime = DateTime.UtcNow;
            var dateTimeFlexabilityInMinutes = 30;
            var queryDictionary = new Dictionary<string, string>();
            queryDictionary.Add("startDateTime", webApiHelper.FormatDateTimeStringForWebApi(startDateTime.AddMinutes(-1)));
            queryDictionary.Add("endDateTime", webApiHelper.FormatDateTimeStringForWebApi(endDateTime.AddMinutes(1)));
            queryDictionary.Add("dateTimeFlexabilityInMinutes", dateTimeFlexabilityInMinutes.ToString());

            var queryString = webApiHelper.CompileParamsAndEncode(queryDictionary);
            string querySubUrl = $"{subUrl}{queryString}";

            //Act
            var response =
                webApiHelper.GetHttpResponseContentAsType<List<EmissionsRelativeMeritDatapoint>>(
                    baseUrl.AbsoluteUri,
                    querySubUrl).Result;

            //Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Any());
        }
    }
}
