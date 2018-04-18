// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SmartEnergyOMTests
{
    using System.Collections.Generic;

    using Microsoft.Azure;

    using SmartEnergyOM;

    [TestClass]
    public class SmartEnergyOMIntegrationTests
    {
        private string databaseConnectionString = CloudConfigurationManager.GetSetting("SQLAzureDatabaseEntityFrameworkConnectionString");
        private SmartEnergyOM objectModel;

        [TestInitialize()]
        public void Initialize()
        {
            this.objectModel = new SmartEnergyOM(this.databaseConnectionString);
        }

        [TestMethod]
        public void TestAddEmissionsRegion()
        {
            // Arrange
            var emissionsRegionName = $"TestEmissionsRegionName_{Guid.NewGuid()}";
            const string regionAbbreviation = "PJM";
            const double regionLatitude = 37.7749;
            const double regionLongitude = 122.4194;
            const string timeZone = "Eastern Standard Time";

            // Act
            var emissionsRegion = this.objectModel.AddEmissionsRegion(
                emissionsRegionName,
                timeZone,
                regionLatitude,
                regionLongitude,
                regionAbbreviation);

            // Assert
            var retrievedEmissionsRegion = this.objectModel.FindEmissionsRegion(emissionsRegionName);
            Assert.IsNotNull(retrievedEmissionsRegion);
            Assert.AreEqual(retrievedEmissionsRegion.FriendlyName, emissionsRegionName);
            Assert.AreEqual(retrievedEmissionsRegion.Latitude, regionLatitude);
            Assert.AreEqual(retrievedEmissionsRegion.Longitude, regionLongitude);
            Assert.AreEqual(retrievedEmissionsRegion.EmissionsRegionWattTimeSubUrl, regionAbbreviation);

            // Clean Up
            this.objectModel.DeleteEmissionsRegion(emissionsRegionName);
        }

        [TestMethod]
        public void TestAddMarketWeatherEmissionsRegionMapping()
        {
            // Arrange
            var FriendlyName = $"US_PJM_{Guid.NewGuid()}";
            var MarketRegionId = 1;
            var WeatherRegionId = 1;
            var EmissionsRegionId = 5;

            // Act
            var RegionMapping = this.objectModel.AddMarketWeatherEmissionsRegionMapping(
                FriendlyName,
                MarketRegionId,
                WeatherRegionId,
                EmissionsRegionId);

            // Assert
            var retrievedMarketRegion = this.objectModel.FindMarketWeatherEmissionsRegionMapping(RegionMapping.RegionMappingID);
            Assert.IsNotNull(retrievedMarketRegion);
            Assert.AreEqual(retrievedMarketRegion.FriendlyName, FriendlyName);
            Assert.AreEqual(retrievedMarketRegion.MarketRegionID, MarketRegionId);
            Assert.AreEqual(retrievedMarketRegion.WeatherRegionID, WeatherRegionId);
            Assert.AreEqual(retrievedMarketRegion.EmissionsRegionID, EmissionsRegionId);
        }

        [TestMethod]
        public void TestAddWeatherRegion()
        {
            // Arrange
            var WeatherRegionName = $"TestWeatherRegionName_{Guid.NewGuid()}";
            const string WeatherRegionWundergroundSubUrl = "ie/dublin";
            const double regionLatitude = 53.3498;
            const double regionLongitude = -6.2603;
            const string timeZone = "GMT Standard Time";

            // Act
            var WeatherRegion = this.objectModel.AddWeatherRegion(
                WeatherRegionName,
                timeZone,
                regionLatitude,
                regionLongitude,
                WeatherRegionWundergroundSubUrl);

            // Assert
            var retrievedWeatherRegion = this.objectModel.FindWeatherRegion(WeatherRegionName);
            Assert.IsNotNull(retrievedWeatherRegion);
            Assert.AreEqual(retrievedWeatherRegion.FriendlyName, WeatherRegionName);
            Assert.AreEqual(retrievedWeatherRegion.Latitude, regionLatitude);
            Assert.AreEqual(retrievedWeatherRegion.Longitude, regionLongitude);
            Assert.AreEqual(retrievedWeatherRegion.WeatherRegionWundergroundSubUrl, WeatherRegionWundergroundSubUrl);
            Assert.AreEqual(retrievedWeatherRegion.TimeZoneUTCRelative.Offset, DateTimeOffset.UtcNow.Offset);

            // Clean Up
            this.objectModel.DeleteWeatherRegion(WeatherRegionName);
        }


        [TestMethod]
        public void TestAddMarketRegion()
        {
            // Arrange
            var MarketRegionName = $"TestMarketRegionName_{Guid.NewGuid()}";
            const double regionLatitude = 53.3498;
            const double regionLongitude = -6.2603;
            const string dummyMarketRegionCurrency = "Euro";
            const string timeZone = "GMT Standard Time";
            const double dummyRegionCurrencyworthPerUSD = 0.95;

            // Act
            var MarketRegion = this.objectModel.AddMarketRegion(
                MarketRegionName,
                dummyMarketRegionCurrency,
                dummyRegionCurrencyworthPerUSD,
                timeZone,
                regionLatitude,
                regionLongitude);

            // Assert
            var retrievedMarketRegion = this.objectModel.FindMarketRegion(MarketRegionName);
            Assert.IsNotNull(retrievedMarketRegion);
            Assert.AreEqual(retrievedMarketRegion.FriendlyName, MarketRegionName);
            Assert.AreEqual(retrievedMarketRegion.Latitude, regionLatitude);
            Assert.AreEqual(retrievedMarketRegion.Longitude, regionLongitude);
            Assert.AreEqual(retrievedMarketRegion.CurrencyName, dummyMarketRegionCurrency);
            Assert.AreEqual(retrievedMarketRegion.CurrencyValuePerUSD, dummyRegionCurrencyworthPerUSD);
            Assert.AreEqual(retrievedMarketRegion.TimeZoneUTCRelative.Offset, DateTimeOffset.UtcNow.Offset);

            // Clean Up
            this.objectModel.DeleteMarketRegion(MarketRegionName);
        }

        [TestMethod]
        public void TestFindCarbonEmissionsDataPoint()
        {
            // Arrange
            var emissionsRegionName = $"TestEmissionsRegionName_{Guid.NewGuid()}";
            const string regionAbbreviation = "PJM";
            const double regionLatitude = 37.7749;
            const double regionLongitude = 122.4194;
            const string timeZone = "Eastern Standard Time";
            var dateTimeOfRow = DateTime.UtcNow;
            var random = new Random();

            var SystemWideCO2Intensity_gCO2kWh = random.NextDouble();
            var SystemWideCO2Intensity_Forcast_gCO2kWh = random.NextDouble();
            var MarginalCO2Intensity_gCO2kWh = random.NextDouble();
            var MarginalCO2Intensity_Forcast_gCO2kWh = random.NextDouble();

            var emissionsRegion = this.objectModel.AddEmissionsRegion(
                emissionsRegionName,
                timeZone,
                regionLatitude,
                regionLongitude,
                regionAbbreviation);

            // Act
            this.objectModel.InsertOrUpdateCarbonEmissionsDataPoints(
                emissionsRegion.EmissionsRegionID,
                dateTimeOfRow,
                SystemWideCO2Intensity_gCO2kWh,
                SystemWideCO2Intensity_Forcast_gCO2kWh,
                MarginalCO2Intensity_gCO2kWh,
                MarginalCO2Intensity_Forcast_gCO2kWh);

            // Assert
            var retrievedEmissionsRegion =
                this.objectModel.FindCarbonEmissionsDataPoint(emissionsRegion.EmissionsRegionID, dateTimeOfRow);
            Assert.IsNotNull(retrievedEmissionsRegion);
            Assert.AreEqual(SystemWideCO2Intensity_gCO2kWh, retrievedEmissionsRegion.SystemWideCO2Intensity_gCO2kWh);
            Assert.AreEqual(SystemWideCO2Intensity_Forcast_gCO2kWh, retrievedEmissionsRegion.SystemWideCO2Intensity_Forcast_gCO2kWh);
            Assert.AreEqual(MarginalCO2Intensity_gCO2kWh, retrievedEmissionsRegion.MarginalCO2Intensity_gCO2kWh);
            Assert.AreEqual(MarginalCO2Intensity_Forcast_gCO2kWh, retrievedEmissionsRegion.MarginalCO2Intensity_Forcast_gCO2kWh);
            Assert.AreEqual(dateTimeOfRow.Date, retrievedEmissionsRegion.DateTimeUTC.Date);
            Assert.AreEqual(dateTimeOfRow.Hour, retrievedEmissionsRegion.DateTimeUTC.Hour);
            Assert.AreEqual(dateTimeOfRow.Second, retrievedEmissionsRegion.DateTimeUTC.Second);

            // Clean Up 
            this.objectModel.DeleteCarbonEmissionsDataPoints(emissionsRegion.EmissionsRegionID, dateTimeOfRow);
            this.objectModel.DeleteEmissionsRegion(emissionsRegionName);
        }

        [TestMethod]
        public void TestFindCarbonEmissionsDataPoints()
        {
            // Arrange
            var emissionsRegionName = $"TestEmissionsRegionName_{Guid.NewGuid()}";
            const string regionAbbreviation = "PJM";
            const double regionLatitude = 37.7749;
            const double regionLongitude = 122.4194;
            const string timeZone = "Eastern Standard Time";
            const int numberOfDatapointsToAdd = 2;
            var listOfPointsAddedForCleanup = new List<DateTime>();

            var random = new Random();
            var emissionsRegion = this.objectModel.AddEmissionsRegion(
                emissionsRegionName,
                timeZone,
                regionLatitude,
                regionLongitude,
                regionAbbreviation);

            var startDateTime = DateTime.UtcNow;

            // Add datapoints
            for (var i = 0; i < numberOfDatapointsToAdd; i++)
            {
                var dateTimeOfRow = DateTime.UtcNow;
                var SystemWideCO2Intensity_gCO2kWh = random.NextDouble();
                var SystemWideCO2Intensity_Forcast_gCO2kWh = random.NextDouble();
                var MarginalCO2Intensity_gCO2kWh = random.NextDouble();
                var MarginalCO2Intensity_Forcast_gCO2kWh = random.NextDouble();

                this.objectModel.InsertOrUpdateCarbonEmissionsDataPoints(
                    emissionsRegion.EmissionsRegionID,
                    dateTimeOfRow,
                    SystemWideCO2Intensity_gCO2kWh,
                    SystemWideCO2Intensity_Forcast_gCO2kWh,
                    MarginalCO2Intensity_gCO2kWh,
                    MarginalCO2Intensity_Forcast_gCO2kWh);
                listOfPointsAddedForCleanup.Add(dateTimeOfRow);
            }

            var endDateTime = DateTime.UtcNow;

            // Act
            var retrievedEmissionsDatapoints =
                 this.objectModel.FindCarbonEmissionsDataPoints(emissionsRegion.EmissionsRegionID, startDateTime, endDateTime);

            // Assert
            Assert.AreEqual(numberOfDatapointsToAdd, retrievedEmissionsDatapoints.Count);

            // Clean Up 
            foreach (var dateTimeOfAddedRow in listOfPointsAddedForCleanup)
            {
                this.objectModel.DeleteCarbonEmissionsDataPoints(emissionsRegion.EmissionsRegionID, dateTimeOfAddedRow);
            }
            this.objectModel.DeleteEmissionsRegion(emissionsRegionName);
        }

        [TestMethod]
        public void TestUpdateCarbonEmissionsDataPoints()
        {
            // Arrange
            var emissionsRegionName = $"TestEmissionsRegionName_{Guid.NewGuid()}";
            const string regionAbbreviation = "PJM";
            const double regionLatitude = 37.7749;
            const double regionLongitude = 122.4194;
            const string timeZone = "Eastern Standard Time";
            var dateTimeOfRow = DateTime.UtcNow;
            var random = new Random();

            var SystemWideCO2Intensity_gCO2kWh = random.NextDouble();
            var ForecastSystemWideCO2Intensity_gCO2kWh = random.NextDouble();
            var MarginalCO2Intensity_gCO2kWh = random.NextDouble();
            var ForecastMarginalCO2Intensity_gCO2kWh = random.NextDouble();

            //First insert a datapoint that will be updated
            var emissionsRegion = this.objectModel.AddEmissionsRegion(
                emissionsRegionName,
                timeZone,
                regionLatitude,
                regionLongitude,
                regionAbbreviation);

            this.objectModel.InsertOrUpdateCarbonEmissionsDataPoints(
                emissionsRegion.EmissionsRegionID,
                dateTimeOfRow,
                SystemWideCO2Intensity_gCO2kWh,
                ForecastSystemWideCO2Intensity_gCO2kWh,
                MarginalCO2Intensity_gCO2kWh,
                ForecastMarginalCO2Intensity_gCO2kWh);

            // Act
            SystemWideCO2Intensity_gCO2kWh = random.NextDouble();
            MarginalCO2Intensity_gCO2kWh = random.NextDouble();

            this.objectModel.InsertOrUpdateCarbonEmissionsDataPoints(
                emissionsRegion.EmissionsRegionID,
                dateTimeOfRow,
                SystemWideCO2Intensity_gCO2kWh,
                ForecastSystemWideCO2Intensity_gCO2kWh,
                MarginalCO2Intensity_gCO2kWh,
                ForecastMarginalCO2Intensity_gCO2kWh);

            // Assert
            var retrievedEmissionsRegion =
                this.objectModel.FindCarbonEmissionsDataPoint(emissionsRegion.EmissionsRegionID, dateTimeOfRow);
            Assert.IsNotNull(retrievedEmissionsRegion);
            Assert.AreEqual(SystemWideCO2Intensity_gCO2kWh, retrievedEmissionsRegion.SystemWideCO2Intensity_gCO2kWh);
            Assert.AreEqual(MarginalCO2Intensity_gCO2kWh, retrievedEmissionsRegion.MarginalCO2Intensity_gCO2kWh);
            Assert.AreEqual(dateTimeOfRow.Date, retrievedEmissionsRegion.DateTimeUTC.Date);
            Assert.AreEqual(dateTimeOfRow.Hour, retrievedEmissionsRegion.DateTimeUTC.Hour);
            Assert.AreEqual(dateTimeOfRow.Second, retrievedEmissionsRegion.DateTimeUTC.Second);

            // Clean Up 
            this.objectModel.DeleteCarbonEmissionsDataPoints(emissionsRegion.EmissionsRegionID, dateTimeOfRow);
            this.objectModel.DeleteEmissionsRegion(emissionsRegionName);
        }

        [TestMethod]
        public void TestUpdateWeatherDataPoints()
        {
            // Arrange
            var regionName = $"TestWeatherRegionName_{Guid.NewGuid()}";
            const string regionAbbreviation = "ie/dublin";
            const double regionLatitude = 53.3498;
            const double regionLongitude = -6.2603;
            const string timeZone = "GMT Standard Time";
            var dateTimeOfRow = DateTime.UtcNow;
            var random = new Random();

            var Temperature_Celcius = random.NextDouble();
            var DewPoint_Metric = random.NextDouble();
            var WindSpeed_Metric = random.NextDouble();
            var WindGust_Metric = random.NextDouble();
            var WindDirection_Degrees = random.NextDouble();
            var WindChill_Metric = random.NextDouble();
            var Visibility_Metric = random.NextDouble();
            var UVIndex = random.NextDouble();
            var Precipitation_Metric = random.NextDouble();
            var Snow_Metric = random.NextDouble();
            var Pressure_Metric = random.NextDouble();
            var Humidity_Percent = random.NextDouble();
            var ConditionDescription = random.NextDouble().ToString();
            var IsForecastRow = true;

            //First insert a datapoint that will be updated
            var region = this.objectModel.AddWeatherRegion(
                regionName,
                timeZone,
                regionLatitude,
                regionLongitude,
                regionAbbreviation);

            this.objectModel.InsertOrUpdateWeatherDataPoints(
                region.WeatherRegionID,
                dateTimeOfRow,
                Temperature_Celcius,
                DewPoint_Metric,
                WindSpeed_Metric,
                WindGust_Metric,
                WindDirection_Degrees,
                WindChill_Metric,
                Visibility_Metric,
                UVIndex,
                Precipitation_Metric,
                Snow_Metric,
                Pressure_Metric,
                Humidity_Percent,
                ConditionDescription,
                IsForecastRow);

            // Act
            Temperature_Celcius = random.NextDouble();
            DewPoint_Metric = random.NextDouble();
            WindSpeed_Metric = random.NextDouble();
            WindGust_Metric = random.NextDouble();
            WindDirection_Degrees = random.NextDouble();
            WindChill_Metric = random.NextDouble();
            Visibility_Metric = random.NextDouble();
            UVIndex = random.NextDouble();
            Precipitation_Metric = random.NextDouble();
            Snow_Metric = random.NextDouble();
            Pressure_Metric = random.NextDouble();
            Humidity_Percent = random.NextDouble();
            ConditionDescription = random.NextDouble().ToString();

            this.objectModel.InsertOrUpdateWeatherDataPoints(
                region.WeatherRegionID,
                dateTimeOfRow,
                Temperature_Celcius,
                DewPoint_Metric,
                WindSpeed_Metric,
                WindGust_Metric,
                WindDirection_Degrees,
                WindChill_Metric,
                Visibility_Metric,
                UVIndex,
                Precipitation_Metric,
                Snow_Metric,
                Pressure_Metric,
                Humidity_Percent,
                ConditionDescription,
                IsForecastRow);

            // Assert
            var retrievedEmissionsRegion =
                this.objectModel.FindWeatherDataPoint(region.WeatherRegionID, dateTimeOfRow);
            Assert.IsNotNull(retrievedEmissionsRegion);
            Assert.AreEqual(Temperature_Celcius, retrievedEmissionsRegion.Temperature_Celcius);
            Assert.AreEqual(DewPoint_Metric,retrievedEmissionsRegion.DewPoint_Metric);
            Assert.AreEqual(WindSpeed_Metric, retrievedEmissionsRegion.WindSpeed_Metric);
            Assert.AreEqual(WindGust_Metric,retrievedEmissionsRegion.WindGust_Metric);
            Assert.AreEqual(WindDirection_Degrees, retrievedEmissionsRegion.WindDirection_Degrees);
            Assert.AreEqual(DewPoint_Metric, retrievedEmissionsRegion.DewPoint_Metric);
            Assert.AreEqual(WindChill_Metric, retrievedEmissionsRegion.WindChill_Metric);
            Assert.AreEqual(Visibility_Metric, retrievedEmissionsRegion.Visibility_Metric);
            Assert.AreEqual(UVIndex, retrievedEmissionsRegion.UVIndex);
            Assert.AreEqual(Precipitation_Metric, retrievedEmissionsRegion.Precipitation_Metric);
            Assert.AreEqual(Snow_Metric, retrievedEmissionsRegion.Snow_Metric);
            Assert.AreEqual(Pressure_Metric, retrievedEmissionsRegion.Pressure_Metric);
            Assert.AreEqual(Humidity_Percent, retrievedEmissionsRegion.Humidity_Percent);
            Assert.AreEqual(ConditionDescription, retrievedEmissionsRegion.ConditionDescription);

            Assert.AreEqual(dateTimeOfRow.Date, retrievedEmissionsRegion.DateTimeUTC.Date);
            Assert.AreEqual(dateTimeOfRow.Hour, retrievedEmissionsRegion.DateTimeUTC.Hour);
            Assert.AreEqual(dateTimeOfRow.Second, retrievedEmissionsRegion.DateTimeUTC.Second);

            // Clean Up 
            this.objectModel.DeleteWeatherDataPoints(region.WeatherRegionID, dateTimeOfRow);
            this.objectModel.DeleteMarketRegion(regionName);
        }

        [TestMethod]
        public void TestUpdateMarketDataPoints()
        {
            // Arrange
            var regionName = $"TestMarketRegionName_{Guid.NewGuid()}";
            const double regionLatitude = 53.3498;
            const double regionLongitude = -6.2603;
            const string dummyMarketRegionCurrency = "Euro";
            const string timeZone = "GMT Standard Time";
            const double dummyRegionCurrencyworthPerUSD = 0.95;
            var dateTimeOfRow = DateTime.UtcNow;
            var random = new Random();

            var Price = random.NextDouble();
            var DemandMW = random.NextDouble();
            var RenewablesMW = random.NextDouble();
            var RenewablesPercentage = random.NextDouble();
            var WindMW = random.NextDouble();
            var WindPercentage = random.NextDouble();
            var SolarMW = random.NextDouble();
            var SolarPercentage = random.NextDouble();
            var CarbonPricePerKg = random.NextDouble();
            var IsForecastRow = true;

            //First insert a datapoint that will be updated
            var region = this.objectModel.AddMarketRegion(
                regionName,
                dummyMarketRegionCurrency,
                dummyRegionCurrencyworthPerUSD,
                timeZone,
                regionLatitude,
                regionLongitude);

            this.objectModel.InsertOrUpdateMarketDataPoints(
                region.MarketRegionID,
                dateTimeOfRow,
                Price,
                DemandMW,
                RenewablesMW,
                RenewablesPercentage,
                WindMW,
                WindPercentage,
                SolarMW,
                SolarPercentage,
                CarbonPricePerKg,
                IsForecastRow);

            // Act
            Price = random.NextDouble();
            DemandMW = random.NextDouble();
            RenewablesMW = random.NextDouble();
            RenewablesPercentage = random.NextDouble();
            WindMW = random.NextDouble();
            WindPercentage = random.NextDouble();
            SolarMW = random.NextDouble();
            SolarPercentage = random.NextDouble();
            CarbonPricePerKg = random.NextDouble();

            this.objectModel.InsertOrUpdateMarketDataPoints(
                region.MarketRegionID,
                dateTimeOfRow,
                Price ,
                DemandMW,
                RenewablesMW,
                RenewablesPercentage,
                WindMW,
                WindPercentage,
                SolarMW,
                SolarPercentage,
                CarbonPricePerKg,
                IsForecastRow);

            // Assert
            var retrievedEmissionsRegion =
                this.objectModel.FindMarketDataPoint(region.MarketRegionID, dateTimeOfRow);
            Assert.IsNotNull(retrievedEmissionsRegion);
            Assert.AreEqual(Price, retrievedEmissionsRegion.Price);
            Assert.AreEqual(DemandMW, retrievedEmissionsRegion.DemandMW);
            Assert.AreEqual(RenewablesMW, retrievedEmissionsRegion.RenewablesMW);
            Assert.AreEqual(RenewablesPercentage, retrievedEmissionsRegion.RenewablesPercentage);
            Assert.AreEqual(WindMW, retrievedEmissionsRegion.WindMW);
            Assert.AreEqual(DemandMW, retrievedEmissionsRegion.DemandMW);
            Assert.AreEqual(WindPercentage, retrievedEmissionsRegion.WindPercentage);
            Assert.AreEqual(SolarMW, retrievedEmissionsRegion.SolarMW);
            Assert.AreEqual(SolarPercentage, retrievedEmissionsRegion.SolarPercentage);
            Assert.AreEqual(CarbonPricePerKg, retrievedEmissionsRegion.CarbonPricePerKg);

            Assert.AreEqual(dateTimeOfRow.Date, retrievedEmissionsRegion.DateTimeUTC.Date);
            Assert.AreEqual(dateTimeOfRow.Hour, retrievedEmissionsRegion.DateTimeUTC.Hour);
            Assert.AreEqual(dateTimeOfRow.Second, retrievedEmissionsRegion.DateTimeUTC.Second);

            // Clean Up 
            this.objectModel.DeleteMarketDataPoints(region.MarketRegionID, dateTimeOfRow);
            this.objectModel.DeleteMarketRegion(regionName);
        }       

        [TestMethod]
        public void TestUpdateCarbonEmissionsRelativeMeritDataPoints()
        {
            // Arrange
            var emissionsRegionName = $"TestEmissionsRegionName_{Guid.NewGuid()}";
            const string regionAbbreviation = "PJM";
            const double regionLatitude = 37.7749;
            const double regionLongitude = 122.4194;
            const string timeZone = "Eastern Standard Time";
            var dateTimeOfRow = DateTime.UtcNow;
            var random = new Random();

            var EmissionsRelativeMerit = random.NextDouble();
            var ForecastEmissionsRelativeMerit = random.NextDouble();

            //First insert a datapoint that will be updated
            var emissionsRegion = this.objectModel.AddEmissionsRegion(
                emissionsRegionName,
                timeZone,
                regionLatitude,
                regionLongitude,
                regionAbbreviation);

            this.objectModel.InsertOrUpdateCarbonEmissionsRelativeMeritDataPoints(
                emissionsRegion.EmissionsRegionID,
                dateTimeOfRow,
                EmissionsRelativeMerit,
                ForecastEmissionsRelativeMerit);

            // Act
            EmissionsRelativeMerit = random.NextDouble();
            ForecastEmissionsRelativeMerit = random.NextDouble();

            this.objectModel.InsertOrUpdateCarbonEmissionsRelativeMeritDataPoints(
                emissionsRegion.EmissionsRegionID,
                dateTimeOfRow,
                EmissionsRelativeMerit,
                ForecastEmissionsRelativeMerit);

            // Assert
            var retrievedDataPoints =
                this.objectModel.FindCarbonEmissionsRelativeMeritDataPoints(emissionsRegion.EmissionsRegionID, dateTimeOfRow.AddSeconds(-1), dateTimeOfRow.AddSeconds(1));
            var firstItem = retrievedDataPoints[0];
            Assert.IsNotNull(retrievedDataPoints);
            Assert.AreEqual(EmissionsRelativeMerit, firstItem.EmissionsRelativeMerit);
            Assert.AreEqual(ForecastEmissionsRelativeMerit, firstItem.EmissionsRelativeMerit_Forcast);           

            // Clean Up 
            this.objectModel.DeleteCarbonEmissionsDataPoints(emissionsRegion.EmissionsRegionID, dateTimeOfRow);
            this.objectModel.DeleteEmissionsRegion(emissionsRegionName);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.objectModel.Dispose();
        }
    }
}

