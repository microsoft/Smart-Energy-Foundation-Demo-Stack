// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartEnergyOM;

namespace SmartEnergyOM
{
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// A class which allows storing and retrieving data from the underlying database
    /// </summary>
    public class SmartEnergyOM : IDisposable
    {
        private readonly string DatabaseConnectionString;
        #region Constructor and Dispose
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartEnergyOM"/> class.
        /// </summary>
        /// <param name="databaseConnectionString">Entity Framrwork Connection string for the Applications' Database</param>
        public SmartEnergyOM(string databaseConnectionString)
        {
            this.DatabaseConnectionString = databaseConnectionString;
        }

        // Dispose method
        public void Dispose()
        {
        }

        #endregion

        #region Add Methods

        /// <summary>
        /// Add a WeatherRegion
        /// </summary>
        /// <param name="friendlyName">Region's Friendly Name</param>
        /// <param name="utcTimeOffset">The location's UTC DateTime Offset</param>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="weatherRegionWundergroundSubUrl">Sub Url of the region on the Wunderground weather API e.g. CA/San_Francisco</param>
        /// <returns>Object representing row created in the database</returns>
        public WeatherRegion AddWeatherRegion(
            string friendlyName,
            DateTimeOffset utcTimeOffset,
            double? latitude = null,
            double? longitude = null,
            string weatherRegionWundergroundSubUrl = null)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                //First check if the region already exists
                var weatherRegion = this.FindWeatherRegion(friendlyName);
                if (weatherRegion != null)
                {
                    return weatherRegion;
                }

                //If it doesn't exist, add it
                var newRegion = new WeatherRegion()
                {
                    FriendlyName = friendlyName,
                    TimeZoneUTCRelative = utcTimeOffset,
                    Latitude = latitude,
                    Longitude = longitude,
                    WeatherRegionWundergroundSubUrl = weatherRegionWundergroundSubUrl
                };
                dbModel.WeatherRegions.Add(newRegion);
                this.SaveChangeWithContextReloadUponFailure(dbModel);

                //retrieve the new object from the database and return it
                weatherRegion = this.FindWeatherRegion(friendlyName);
                return weatherRegion;
            }
        }

        /// <summary>
        /// Add a WeatherRegion
        /// </summary>
        /// <param name="friendlyName">Region's Friendly Name</param>
        /// <param name="timeZone">.NET Timezone Name (from the list https://msdn.microsoft.com/en-us/library/ms912391(v=winembedded.11).aspx</param>)
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="weatherRegionWundergroundSubUrl">Sub Url of the region on the Wunderground weather API e.g. CA/San_Francisco</param>
        /// <returns>Object representing row created in the database</returns>
        public WeatherRegion AddWeatherRegion(
            string friendlyName,
            string timeZone,
            double? latitude = null,
            double? longitude = null,
            string weatherRegionWundergroundSubUrl = null)
        {
            var utcTimeOffset = ConvertTimezoneNameToUtcDateTimeOffset(timeZone);

            return this.AddWeatherRegion(friendlyName, utcTimeOffset, latitude, longitude, weatherRegionWundergroundSubUrl);
        }

        /// <summary>
        /// Add an EmissionsRegion
        /// </summary>
        /// <param name="friendlyName">Region's Friendly Name</param>
        /// <param name="utcTimeOffset">The location's UTC DateTime Offset</param>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="emissionsRegionWundergroundSubUrl"></param>
        /// <returns>Object representing row created in the database</returns>
        public EmissionsRegion AddEmissionsRegion(
            string friendlyName,
            DateTimeOffset utcTimeOffset,
            double? latitude = null,
            double? longitude = null,
            string emissionsRegionWundergroundSubUrl = null)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                //First check if the region already exists
                var EmissionsRegion = this.FindEmissionsRegion(friendlyName);
                if (EmissionsRegion != null)
                {
                    return EmissionsRegion;
                }

                //If it doesn't exist, add it
                var newRegion = new EmissionsRegion()
                {
                    FriendlyName = friendlyName,
                    TimeZoneUTCRelative = utcTimeOffset,
                    Latitude = latitude,
                    Longitude = longitude,
                    EmissionsRegionWattTimeSubUrl = emissionsRegionWundergroundSubUrl
                };
                dbModel.EmissionsRegions.Add(newRegion);
                this.SaveChangeWithContextReloadUponFailure(dbModel);

                //retrieve the new object from the database and return it
                EmissionsRegion = this.FindEmissionsRegion(friendlyName);
                return EmissionsRegion;
            }
        }

        /// <summary>
        /// Add an EmissionsRegion
        /// </summary>
        /// <param name="friendlyName">Region's Friendly Name</param>
        /// <param name="timeZone">.NET Timezone Name (from the list https://msdn.microsoft.com/en-us/library/ms912391(v=winembedded.11).aspx</param>)
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="emissionsRegionWundergroundSubUrl"></param>
        /// <returns>Object representing row created in the database</returns>
        public EmissionsRegion AddEmissionsRegion(
            string friendlyName,
            string timeZone,
            double? latitude = null,
            double? longitude = null,
            string emissionsRegionWundergroundSubUrl = null)
        {
            var utcTimeOffset = ConvertTimezoneNameToUtcDateTimeOffset(timeZone);

            return this.AddEmissionsRegion(friendlyName, utcTimeOffset, latitude, longitude, emissionsRegionWundergroundSubUrl);
        }

        /// <summary>
        /// Add a MarketRegion
        /// </summary>
        /// <param name="regionName">Region Name</param>
        /// <param name="currencyName">Currency Name</param>
        /// <param name="currencyValuePerUSD">Currency Value per US Dollar</param>
        /// <param name="timeZone">.NET Timezone Name (from the list https://msdn.microsoft.com/en-us/library/ms912391(v=winembedded.11).aspx</param>)
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <returns>Object representing row created in the database</returns>
        public MarketRegion AddMarketRegion(
            string regionName,
            string currencyName,
            double currencyValuePerUSD,
            string timeZone,
            double? latitude = null,
            double? longitude = null)
        {
            var utcTimeOffset = ConvertTimezoneNameToUtcDateTimeOffset(timeZone);

            using (var dbObject = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                // First check if the region already exists
                var regionInfoForGivenRegionName = this.FindMarketRegion(regionName);
                if (regionInfoForGivenRegionName != null)
                {
                    return regionInfoForGivenRegionName;
                }

                // If it doesn't exist, add it
                var newRegion = new MarketRegion()
                {
                    FriendlyName = regionName,
                    CurrencyName = currencyName,
                    CurrencyValuePerUSD = currencyValuePerUSD,
                    TimeZoneUTCRelative = utcTimeOffset,
                    Latitude = latitude,
                    Longitude = longitude
                };
                dbObject.MarketRegions.Add(newRegion);
                dbObject.SaveChanges();

                // Retrieve the new object from the database and return it
                regionInfoForGivenRegionName = this.FindMarketRegion(regionName);
                return regionInfoForGivenRegionName;
            }
        }

        /// <summary>
        /// Add a MarketWeatherEmissionsRegion Mapping to link Weather, Emissions and Market Regions in the database
        /// </summary>
        /// <param name="friendlyName">Region's Friendly Name</param>
        /// <param name="MarketRegionID">MarketRegionID</param>
        /// <param name="WeatherRegionID">WeatherRegionID</param>
        /// <param name="EmissionsRegionID">EmissionsRegionID</param>
        /// <returns>MarketWeatherEmissionsRegionMapping representing the new entry in the database</returns>
        public MarketWeatherEmissionsRegionMapping AddMarketWeatherEmissionsRegionMapping(
            string friendlyName,
            int? MarketRegionID,
            int? WeatherRegionID,
            int? EmissionsRegionID,
            int maxNumberOfRetries = 3)
        {
            var updatedSuccessfully = false;
            var numberOfAttemptsMade = 0;

            while (!updatedSuccessfully)
            {
                using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
                {
                    // First check if the region mapping already exists. If it does and the existing mapping doesn't contain a value that's passed in, add it. 
                    var regionMapping = dbModel.MarketWeatherEmissionsRegionMappings.FirstOrDefault(
                        r => r.FriendlyName.ToLower() == friendlyName.ToLower());

                    if (regionMapping != null)
                    {
                        if (regionMapping.MarketRegionID == null && MarketRegionID != null)
                        {
                            regionMapping.MarketRegionID = MarketRegionID;
                        }
                        if (regionMapping.WeatherRegionID == null && WeatherRegionID != null)
                        {
                            regionMapping.WeatherRegionID = WeatherRegionID;
                        }
                        if (regionMapping.EmissionsRegionID == null && EmissionsRegionID != null)
                        {
                            regionMapping.EmissionsRegionID = EmissionsRegionID;
                        }
                        if (SaveChangesWithRetry(maxNumberOfRetries, dbModel, ref numberOfAttemptsMade)) break;
                    }
                    else
                    {

                        // If it doesn't exist, add it with the specified mappings
                        var newObject = new MarketWeatherEmissionsRegionMapping()
                                            {
                                                FriendlyName = friendlyName,
                                                MarketRegionID = MarketRegionID,
                                                WeatherRegionID = WeatherRegionID,
                                                EmissionsRegionID = EmissionsRegionID
                                            };
                        dbModel.MarketWeatherEmissionsRegionMappings.Add(newObject);
                        if (SaveChangesWithRetry(maxNumberOfRetries, dbModel, ref numberOfAttemptsMade)) break;
                    }
                }
            }

            // Retrieve the final object from the database and return it
            var mapping = this.FindMarketWeatherEmissionsRegionMapping(friendlyName);
            return mapping;
        }

        #endregion

        #region FindMethods

        /// <summary>
        /// Finds a region based on the regions name
        /// </summary>
        /// <param name="friendlyName">Name of the region to find</param>
        /// <returns>MarketRegion for the specified region</returns>
        public MarketRegion FindMarketRegion(string friendlyName)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.MarketRegions.FirstOrDefault(r => r.FriendlyName.ToLower() == friendlyName.ToLower());
                return dbObject;
            }
        }

        /// <summary>
        /// Finds a region based on the marketRegionID
        /// </summary>
        /// <param name="marketRegionID">ID of the object to return</param>
        /// <returns>MarketRegion for the specified region</returns>
        public MarketRegion FindMarketRegion(int marketRegionID)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject = dbModel.MarketRegions.FirstOrDefault(r => r.MarketRegionID == marketRegionID);
                return dbObject;
            }
        }

        /// <summary>
        /// Finds all Market regions available in the database
        /// </summary>
        /// <returns>Friendly names of all available regions</returns>
        public List<string> FindAllMarketRegions()
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.MarketRegions;
                return dbObject.Select(f => f.FriendlyName).ToList();
            }
        }

        /// <summary>
        /// Finds a Weather region based on the friendlyName
        /// </summary>
        /// <param name="friendlyName">Name of the region to find</param>
        /// <returns>WeatherRegion for the specified region</returns>
        public WeatherRegion FindWeatherRegion(string friendlyName)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.WeatherRegions.FirstOrDefault(r => r.FriendlyName.ToLower() == friendlyName.ToLower());
                return dbObject;
            }
        }

        /// <summary>
        /// Finds a Weather region based on the MarketRegionID
        /// </summary>
        /// <param name="WeatherRegionID">ID of the object to return</param>
        /// <returns>WeatherRegion for the specified region</returns>
        public WeatherRegion FindWeatherRegion(int WeatherRegionID)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject = dbModel.WeatherRegions.FirstOrDefault(r => r.WeatherRegionID == WeatherRegionID);
                return dbObject;
            }
        }

        /// <summary>
        /// Finds all Weather regions available in the database
        /// </summary>
        /// <returns>WeatherRegion for the specified region</returns>
        public List<string> FindAllWeatherRegions()
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.WeatherRegions;
                return dbObject.Select(f => f.FriendlyName).ToList();
            }
        }

        /// <summary>
        /// Finds an Emissions region based on the friendlyName
        /// </summary>
        /// <param name="friendlyName">Name of the region to find</param>
        /// <returns>Friendly names of all available regions</returns>
        public EmissionsRegion FindEmissionsRegion(string friendlyName)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.EmissionsRegions.FirstOrDefault(
                        r => r.FriendlyName.ToLower() == friendlyName.ToLower());
                return dbObject;
            }
        }

        /// <summary>
        /// Finds an Emissions region based on the MarketRegionID
        /// </summary>
        /// <param name="EmissionsRegionID">ID of the object to return</param>
        /// <returns>EmissionsRegion for the specified region</returns>
        public EmissionsRegion FindEmissionsRegion(int EmissionsRegionID)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.EmissionsRegions.FirstOrDefault(r => r.EmissionsRegionID == EmissionsRegionID);
                return dbObject;
            }
        }

        /// <summary>
        /// Finds all Emissions regions available in the database
        /// </summary>
        /// <returns>Friendly names of all available regions</returns>
        public List<string> FindAllEmissionsRegions()
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.EmissionsRegions;
                return dbObject.Select(f => f.FriendlyName).ToList();
            }
        }

        /// <summary>
        /// Finds all Emissions regions available in the database and return as EmissionsRegion objects
        /// </summary>
        /// <returns>Emissions database objects of all available regions</returns>
        public List<EmissionsRegion> FindAllEmissionsRegionsAsEmissionsRegionObjects()
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.EmissionsRegions;
                return dbObject.ToList();
            }
        }

        /// <summary>
        /// Finds an MarketWeatherEmissionsRegionMapping based on the friendlyName
        /// </summary>
        /// <param name="friendlyName">ID of the object to return</param>
        /// <returns>MarketWeatherEmissionsRegionMapping for the specified friendlyName</returns>
        public MarketWeatherEmissionsRegionMapping FindMarketWeatherEmissionsRegionMapping(string friendlyName)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.MarketWeatherEmissionsRegionMappings.FirstOrDefault(
                        r => r.FriendlyName.ToLower() == friendlyName.ToLower());
                return dbObject;
            }
        }

        /// <summary>
        /// Finds an MarketWeatherEmissionsRegionMapping based on the RegionMappingID
        /// </summary>
        /// <param name="regionMappingId">ID of the object to return</param>
        /// <returns>MarketWeatherEmissionsRegionMapping for the specified region</returns>
        public MarketWeatherEmissionsRegionMapping FindMarketWeatherEmissionsRegionMapping(int regionMappingId)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.MarketWeatherEmissionsRegionMappings.FirstOrDefault(r => r.EmissionsRegionID == regionMappingId);
                return dbObject;
            }
        }

        /// <summary>
        /// Finds an Emissions datapoint based on the RegionMappingID and datatime
        /// </summary>
        /// <param name="EmissionsRegionID">EmissionsRegionID</param>
        /// <param name="dateTime">dateTime of the entry</param>
        /// <returns>An Emissions datapoint based on the RegionMappingID and datatime</returns>
        public CarbonEmissionsDataPoint FindCarbonEmissionsDataPoint(int EmissionsRegionID, DateTime dateTime)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.CarbonEmissionsDataPoints.FirstOrDefault(
                        r =>
                            r.EmissionsRegionID == EmissionsRegionID && r.DateTimeUTC.Year == dateTime.Year
                            && r.DateTimeUTC.Month == dateTime.Month && r.DateTimeUTC.Day == dateTime.Day
                            && r.DateTimeUTC.Hour == dateTime.Hour && r.DateTimeUTC.Minute == dateTime.Minute
                            && r.DateTimeUTC.Second == dateTime.Second);

                return dbObject;
            }
        }

        /// <summary>
        /// Find Weather Emissions based on the WeatherRegionID between startDateTime and endDateTime
        /// </summary>
        /// <param name="EmissionsRegionID">EmissionsRegionID</param>
        /// <param name="startDateTime">DateTime marking the start of the period to query for</param>
        /// <param name="endDateTime">DateTime marking the end of the period to query for</param>
        /// <returns>Emissions datapoints belonging the given EmissionsRegionID between startDateTime and endDateTime</returns>
        public List<CarbonEmissionsDataPoint> FindCarbonEmissionsDataPoints(int EmissionsRegionID, DateTime startDateTime, DateTime endDateTime)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.CarbonEmissionsDataPoints.Where(
                        r =>
                            r.EmissionsRegionID == EmissionsRegionID && r.DateTimeUTC > startDateTime
                            && r.DateTimeUTC < endDateTime);

                return dbObject.ToList();
            }
        }

        /// <summary>
        /// Finds a Weather datapoint based on the WeatherRegionID and datatime
        /// </summary>
        /// <param name="WeatherRegionID">WeatherRegionID</param>
        /// <param name="dateTime">dateTime</param>
        /// <returns>A Weather datapoint based on the WeatherRegionID and datatime</returns>
        public WeatherDataPoint FindWeatherDataPoint(int WeatherRegionID, DateTime dateTime)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.WeatherDataPoints.FirstOrDefault(
                        r =>
                            r.WeatherRegionID == WeatherRegionID && r.DateTimeUTC.Year == dateTime.Year
                            && r.DateTimeUTC.Month == dateTime.Month && r.DateTimeUTC.Day == dateTime.Day
                            && r.DateTimeUTC.Hour == dateTime.Hour && r.DateTimeUTC.Minute == dateTime.Minute
                            && r.DateTimeUTC.Second == dateTime.Second);

                return dbObject;
            }
        }

        /// <summary>
        /// Find Weather datapoints based on the WeatherRegionID between startDateTime and endDateTime
        /// </summary>
        /// <param name="WeatherRegionID">WeatherRegionID</param>
        /// <param name="startDateTime">DateTime marking the start of the period to query for</param>
        /// <param name="endDateTime">DateTime marking the end of the period to query for</param>
        /// <returns>Weather datapoints belonging the given WeatherRegionID between startDateTime and endDateTime</returns>
        public List<WeatherDataPoint> FindWeatherDataPoints(int WeatherRegionID, DateTime startDateTime, DateTime endDateTime)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.WeatherDataPoints.Where(
                        r =>
                            r.WeatherRegionID == WeatherRegionID && r.DateTimeUTC > startDateTime
                            && r.DateTimeUTC < endDateTime);

                return dbObject.ToList();
            }
        }

        /// <summary>
        /// Finds a Market datapoint based on the MarketRegionID and datatime
        /// </summary>
        /// <param name="MarketRegionID">MarketRegionID</param>
        /// <param name="dateTime">dateTime</param>
        /// <returns>A  Market datapoint based on the MarketRegionID and datatime</returns>
        public MarketDataPoint FindMarketDataPoint(int MarketRegionID, DateTime dateTime)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.MarketDataPoints.FirstOrDefault(
                        r =>
                            r.MarketRegionID == MarketRegionID && r.DateTimeUTC.Year == dateTime.Year
                            && r.DateTimeUTC.Month == dateTime.Month && r.DateTimeUTC.Day == dateTime.Day
                            && r.DateTimeUTC.Hour == dateTime.Hour && r.DateTimeUTC.Minute == dateTime.Minute
                            && r.DateTimeUTC.Second == dateTime.Second);

                return dbObject;
            }
        }

        /// <summary>
        /// Find Market datapoints based on the MarketRegionID between startDateTime and endDateTime
        /// </summary>
        /// <param name="MarketRegionID">MarketRegionID</param>
        /// <param name="startDateTime">DateTime marking the start of the period to query for</param>
        /// <param name="endDateTime">DateTime marking the end of the period to query for</param>
        /// <returns>Weather datapoints belonging the given MarketRegionID between startDateTime and endDateTime</returns>
        public List<MarketDataPoint> FindMarketDataPoints(int MarketRegionID, DateTime startDateTime, DateTime endDateTime)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.MarketDataPoints.Where(
                        r =>
                            r.MarketRegionID == MarketRegionID && r.DateTimeUTC > startDateTime
                            && r.DateTimeUTC < endDateTime);

                return dbObject.ToList();
            }
        }

        #endregion

        #region Insert / Update Methods
        /// <summary>
        /// Insert a new DataPoint. If an existing value exists with the same Id and DateTime, it is updated with any values passed in. 
        /// </summary>
        /// <param name="EmissionsRegionID">EmissionsRegionID</param>
        /// <param name="dateTime">dateTime</param>
        /// <param name="SystemWideCO2Intensity_gCO2kWh"></param>
        /// <param name="ForecastSystemWideCO2Intensity_gCO2kWh"></param>
        /// <param name="MarginalCO2Intensity_gCO2kWh"></param>
        /// <param name="ForecastMarginalCO2Intensity_gCO2kWh"></param>
        /// <param name="maxNumberOfRetries">Maximum number of retries to attempt if exceptions are encountered with the database</param>
        public void InsertOrUpdateCarbonEmissionsDataPoints(
           int EmissionsRegionID,
           DateTime dateTime,
           double? SystemWideCO2Intensity_gCO2kWh = null,
           double? SystemWideCO2Intensity_Forcast_gCO2kWh = null,
           double? MarginalCO2Intensity_gCO2kWh = null,
           double? MarginalCO2Intensity_Forcast_gCO2kWh = null,
           int maxNumberOfRetries = 3)
        {
            var updatedSuccessfully = false;
            var numberOfAttemptsMade = 0;

            while (!updatedSuccessfully)
            {
                using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
                {
                    // Check if there is an entry already
                    var existingEntryForThisDateTime =
                        dbModel.CarbonEmissionsDataPoints.FirstOrDefault(
                            r =>
                                r.EmissionsRegionID == EmissionsRegionID && r.DateTimeUTC.Year == dateTime.Year
                                && r.DateTimeUTC.Month == dateTime.Month && r.DateTimeUTC.Day == dateTime.Day
                                && r.DateTimeUTC.Hour == dateTime.Hour && r.DateTimeUTC.Minute == dateTime.Minute
                                && r.DateTimeUTC.Second == dateTime.Second);

                    var regionEntity = this.FindEmissionsRegion(EmissionsRegionID);

                    if (existingEntryForThisDateTime == null)
                    {
                        // No existing entity exists. Add a new entity.
                        dbModel.CarbonEmissionsDataPoints.Add(
                            new CarbonEmissionsDataPoint
                            {
                                EmissionsRegionID = regionEntity.EmissionsRegionID,
                                DateTimeUTC = dateTime,
                                SystemWideCO2Intensity_gCO2kWh = SystemWideCO2Intensity_gCO2kWh,
                                SystemWideCO2Intensity_Forcast_gCO2kWh = SystemWideCO2Intensity_Forcast_gCO2kWh,
                                MarginalCO2Intensity_gCO2kWh = MarginalCO2Intensity_gCO2kWh,
                                MarginalCO2Intensity_Forcast_gCO2kWh = MarginalCO2Intensity_Forcast_gCO2kWh,
                            });
                    }
                    else
                    {
                        // An existing entity exists. Update it with the new values.
                        if (SystemWideCO2Intensity_gCO2kWh != null)
                        {
                            existingEntryForThisDateTime.SystemWideCO2Intensity_gCO2kWh = SystemWideCO2Intensity_gCO2kWh;
                        }
                        if (SystemWideCO2Intensity_Forcast_gCO2kWh != null)
                        {
                            existingEntryForThisDateTime.SystemWideCO2Intensity_Forcast_gCO2kWh = SystemWideCO2Intensity_Forcast_gCO2kWh;
                        }
                        if (MarginalCO2Intensity_gCO2kWh != null)
                        {
                            existingEntryForThisDateTime.MarginalCO2Intensity_gCO2kWh = MarginalCO2Intensity_gCO2kWh;
                        }
                        if (MarginalCO2Intensity_Forcast_gCO2kWh != null)
                        {
                            existingEntryForThisDateTime.MarginalCO2Intensity_Forcast_gCO2kWh = MarginalCO2Intensity_Forcast_gCO2kWh;
                        }
                    }

                    if (SaveChangesWithRetry(maxNumberOfRetries, dbModel, ref numberOfAttemptsMade)) return;
                }
            }
        }

        /// <summary>
        /// Insert a new DataPoint. If an existing value exists with the same Id and DateTime, it is updated with any values passed in. 
        /// </summary>
        /// <param name="WeatherRegionID"></param>
        /// <param name="dateTime"></param>
        /// <param name="Temperature_Celcius"></param>
        /// <param name="DewPoint_Metric"></param>
        /// <param name="WindSpeed_Metric"></param>
        /// <param name="WindGust_Metric"></param>
        /// <param name="WindDirection_Degrees"></param>
        /// <param name="WindChill_Metric"></param>
        /// <param name="Visibility_Metric"></param>
        /// <param name="UVIndex"></param>
        /// <param name="Precipitation_Metric"></param>
        /// <param name="Snow_Metric"></param>
        /// <param name="Pressure_Metric"></param>
        /// <param name="Humidity_Percent"></param>
        /// <param name="ConditionDescription"></param>
        /// <param name="IsForcastRow"></param>
        /// <param name="maxNumberOfRetries"></param>
        public void InsertOrUpdateWeatherDataPoints(
            int WeatherRegionID,
            DateTime dateTime,
            double? Temperature_Celcius = null,
            double? DewPoint_Metric = null,
            double? WindSpeed_Metric = null,
            double? WindGust_Metric = null,
            double? WindDirection_Degrees = null,
            double? WindChill_Metric = null,
            double? Visibility_Metric = null,
            double? UVIndex = null,
            double? Precipitation_Metric = null,
            double? Snow_Metric = null,
            double? Pressure_Metric = null,
            double? Humidity_Percent = null,
            string ConditionDescription = null,
            bool IsForcastRow = false,
            int maxNumberOfRetries = 3)
        {
            var updatedSuccessfully = false;
            var numberOfAttemptsMade = 0;

            while (!updatedSuccessfully)
            {
                using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
                {
                    // Check if there is an entry already
                    var existingEntryForThisDateTime =
                        dbModel.WeatherDataPoints.FirstOrDefault(
                            r =>
                                r.WeatherRegionID == WeatherRegionID && r.DateTimeUTC.Year == dateTime.Year
                                && r.DateTimeUTC.Month == dateTime.Month && r.DateTimeUTC.Day == dateTime.Day
                                && r.DateTimeUTC.Hour == dateTime.Hour && r.DateTimeUTC.Minute == dateTime.Minute
                                && r.DateTimeUTC.Second == dateTime.Second);

                    var regionEntity = this.FindWeatherRegion(WeatherRegionID);

                    if (existingEntryForThisDateTime == null)
                    {
                        // No existing entity exists. Add a new entity.
                        dbModel.WeatherDataPoints.Add(
                            new WeatherDataPoint()
                                {
                                WeatherRegionID = regionEntity.WeatherRegionID,
                                    DateTimeUTC = dateTime,
                                Temperature_Celcius = Temperature_Celcius,
                                DewPoint_Metric = DewPoint_Metric,
                                WindSpeed_Metric = WindSpeed_Metric,
                                WindGust_Metric = WindGust_Metric,
                                WindDirection_Degrees = WindDirection_Degrees,
                                WindChill_Metric = WindChill_Metric,
                                Visibility_Metric = Visibility_Metric,
                                UVIndex = UVIndex,
                                Precipitation_Metric = Precipitation_Metric,
                                Snow_Metric = Snow_Metric,
                                Pressure_Metric = Pressure_Metric,

                                Humidity_Percent = Humidity_Percent,
                                ConditionDescription = ConditionDescription,
                                IsForcastRow = IsForcastRow,
                                });
                    }
                    else
                    {
                        // An existing entity exists. Update it with any new values.
                        if (Temperature_Celcius != null)
                        {
                            existingEntryForThisDateTime.Temperature_Celcius = Temperature_Celcius;
                        }
                        if (DewPoint_Metric != null)
                        {
                            existingEntryForThisDateTime.DewPoint_Metric =
                                DewPoint_Metric;
                        }
                        if (WindSpeed_Metric != null)
                        {
                            existingEntryForThisDateTime.WindSpeed_Metric = WindSpeed_Metric;
                        }
                        if (WindGust_Metric != null)
                        {
                            existingEntryForThisDateTime.WindGust_Metric =
                                WindGust_Metric;
                        }
                        if (WindDirection_Degrees != null)
                        {
                            existingEntryForThisDateTime.WindDirection_Degrees =
                                WindDirection_Degrees;
                        }
                        if (WindChill_Metric != null)
                        {
                            existingEntryForThisDateTime.WindChill_Metric =
                                WindChill_Metric;
                        }
                        if (Visibility_Metric != null)
                        {
                            existingEntryForThisDateTime.Visibility_Metric =
                                Visibility_Metric;
                        }
                        if (UVIndex != null)
                        {
                            existingEntryForThisDateTime.UVIndex =
                                UVIndex;
                        }
                        if (Precipitation_Metric != null)
                        {
                            existingEntryForThisDateTime.Precipitation_Metric =
                                Precipitation_Metric;
                        }
                        if (Snow_Metric != null)
                        {
                            existingEntryForThisDateTime.Snow_Metric =
                                Snow_Metric;
                        }
                        if (Pressure_Metric != null)
                        {
                            existingEntryForThisDateTime.Pressure_Metric =
                                Pressure_Metric;
                        }
                        if (Humidity_Percent != null)
                        {
                            existingEntryForThisDateTime.Humidity_Percent =
                                Humidity_Percent;
                        }
                        if (ConditionDescription != null)
                        {
                            existingEntryForThisDateTime.ConditionDescription =
                                ConditionDescription;
                        }
                        existingEntryForThisDateTime.IsForcastRow = IsForcastRow;
                    }

                    if (SaveChangesWithRetry(maxNumberOfRetries, dbModel, ref numberOfAttemptsMade)) return;
                }
            }
        }

        /// <summary>
        /// Insert a new DataPoint. If an existing value exists with the same Id and DateTime, it is updated with any values passed in. 
        /// </summary>
        /// <param name="MarketRegionID"></param>
        /// <param name="dateTime"></param>
        /// <param name="Price"></param>
        /// <param name="DemandMW"></param>
        /// <param name="RenewablesMW"></param>
        /// <param name="RenewablesPercentage"></param>
        /// <param name="WindMW"></param>
        /// <param name="WindPercentage"></param>
        /// <param name="SolarMW"></param>
        /// <param name="SolarPercentage"></param>
        /// <param name="CarbonPricePerKg"></param>
        /// <param name="IsForcastRow"></param>
        /// <param name="maxNumberOfRetries"></param>
        public void InsertOrUpdateMarketDataPoints(
           int MarketRegionID,
           DateTime dateTime,
           double? Price = null,
           double? DemandMW = null,
           double? RenewablesMW = null,
           double? RenewablesPercentage = null,
           double? WindMW = null,
           double? WindPercentage = null,
           double? SolarMW = null,
           double? SolarPercentage = null,
           double? CarbonPricePerKg = null,
           bool IsForcastRow = false,
           int maxNumberOfRetries = 3)
        {
            var updatedSuccessfully = false;
            var numberOfAttemptsMade = 0;

            while (!updatedSuccessfully)
            {
                using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
                {
                    // Check if there is an entry already
                    var existingEntryForThisDateTime =
                        dbModel.MarketDataPoints.FirstOrDefault(
                            r =>
                                r.MarketRegionID == MarketRegionID && r.DateTimeUTC.Year == dateTime.Year
                                && r.DateTimeUTC.Month == dateTime.Month && r.DateTimeUTC.Day == dateTime.Day
                                && r.DateTimeUTC.Hour == dateTime.Hour && r.DateTimeUTC.Minute == dateTime.Minute
                                && r.DateTimeUTC.Second == dateTime.Second);

                    var regionEntity = this.FindMarketRegion(MarketRegionID);

                    if (existingEntryForThisDateTime == null)
                    {
                        // No existing entity exists. Add a new entity.
                        dbModel.MarketDataPoints.Add(
                            new MarketDataPoint()
                            {
                                MarketRegionID = regionEntity.MarketRegionID,
                                DateTimeUTC = dateTime,
                                Price = Price,
                                DemandMW = DemandMW,
                                RenewablesMW = RenewablesMW,
                                RenewablesPercentage = RenewablesPercentage,
                                WindMW = WindMW,
                                WindPercentage = WindPercentage,
                                SolarMW = SolarMW,
                                SolarPercentage = SolarPercentage,
                                CarbonPricePerKg = CarbonPricePerKg,
                                IsForcastRow = IsForcastRow,
                            });
                    }
                    else
                    {
                        // An existing entity exists. Update it with any new values.
                        if (Price != null)
                        {
                            existingEntryForThisDateTime.Price = Price;
                        }
                        if (DemandMW != null)
                        {
                            existingEntryForThisDateTime.DemandMW =
                                DemandMW;
                        }
                        if (RenewablesMW != null)
                        {
                            existingEntryForThisDateTime.RenewablesMW = RenewablesMW;
                        }
                        if (RenewablesPercentage != null)
                        {
                            existingEntryForThisDateTime.RenewablesPercentage =
                                RenewablesPercentage;
                        }
                        if (WindMW != null)
                        {
                            existingEntryForThisDateTime.WindMW =
                                WindMW;
                        }
                        if (WindPercentage != null)
                        {
                            existingEntryForThisDateTime.WindPercentage =
                                WindPercentage;
                        }
                        if (SolarMW != null)
                        {
                            existingEntryForThisDateTime.SolarMW =
                                SolarMW;
                        }
                        if (SolarPercentage != null)
                        {
                            existingEntryForThisDateTime.SolarPercentage =
                                SolarPercentage;
                        }
                        if (CarbonPricePerKg != null)
                        {
                            existingEntryForThisDateTime.CarbonPricePerKg =
                                CarbonPricePerKg;
                        }
                       
                        existingEntryForThisDateTime.IsForcastRow = IsForcastRow;
                    }

                    if (SaveChangesWithRetry(maxNumberOfRetries, dbModel, ref numberOfAttemptsMade)) return;
                }
            }
        }

        #endregion

        #region Delete methods

        /// <summary>
        /// Delete object with given Friendly Name from the database
        /// </summary>
        /// <param name="friendlyName">friendlyName</param>
        public void DeleteMarketRegion(string friendlyName)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.MarketRegions.FirstOrDefault(r => r.FriendlyName.ToLower() == friendlyName.ToLower());

                if (dbObject != null)
                {
                    dbModel.MarketRegions.Remove(dbObject);
                    this.SaveChangeWithContextReloadUponFailure(dbModel);
                }
            }
        }

        /// <summary>
        /// Delete object with given Friendly Name from the database
        /// </summary>
        /// <param name="friendlyName">friendlyName</param>
        public void DeleteEmissionsRegion(string friendlyName)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.EmissionsRegions.FirstOrDefault(r => r.FriendlyName.ToLower() == friendlyName.ToLower());

                if (dbObject != null)
                {
                    dbModel.EmissionsRegions.Remove(dbObject);
                    this.SaveChangeWithContextReloadUponFailure(dbModel);
                }
            }
        }

        /// <summary>
        /// Delete object with given Friendly Name from the database
        /// </summary>
        /// <param name="friendlyName">friendlyName</param>
        public void DeleteWeatherRegion(string friendlyName)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.WeatherRegions.FirstOrDefault(r => r.FriendlyName.ToLower() == friendlyName.ToLower());

                if (dbObject != null)
                {
                    dbModel.WeatherRegions.Remove(dbObject);
                    this.SaveChangeWithContextReloadUponFailure(dbModel);
                }
            }
        }

        /// <summary>
        /// Delete object with given MarketRegionID and DateTime from the database
        /// </summary>
        /// <param name="EmissionsRegionID">
        /// The Emissions Region ID.
        /// </param>
        /// <param name="dateTime">
        /// Date Time of entry
        /// </param>
        public void DeleteCarbonEmissionsDataPoints(int EmissionsRegionID, DateTime dateTime)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.CarbonEmissionsDataPoints.FirstOrDefault(
                        r =>
                            r.EmissionsRegionID == EmissionsRegionID
                            && r.DateTimeUTC.Year == dateTime.Year && r.DateTimeUTC.Month == dateTime.Month
                            && r.DateTimeUTC.Day == dateTime.Day && r.DateTimeUTC.Hour == dateTime.Hour
                            && r.DateTimeUTC.Minute == dateTime.Minute && r.DateTimeUTC.Second == dateTime.Second);

                if (dbObject != null)
                {
                    dbModel.CarbonEmissionsDataPoints.Remove(dbObject);
                    this.SaveChangeWithContextReloadUponFailure(dbModel);
                }
            }
        }

        /// <summary>
        /// Delete object with given MarketRegionID and DateTime from the database
        /// </summary>
        /// <param name="WeatherRegionID">
        /// The Weather Region ID.
        /// </param>
        /// <param name="dateTime">
        /// Date Time of entry
        /// </param>
        public void DeleteWeatherDataPoints(int WeatherRegionID, DateTime dateTime)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.WeatherDataPoints.FirstOrDefault(
                        r =>
                            r.WeatherRegionID == WeatherRegionID
                            && r.DateTimeUTC.Year == dateTime.Year && r.DateTimeUTC.Month == dateTime.Month
                            && r.DateTimeUTC.Day == dateTime.Day && r.DateTimeUTC.Hour == dateTime.Hour
                            && r.DateTimeUTC.Minute == dateTime.Minute && r.DateTimeUTC.Second == dateTime.Second);

                if (dbObject != null)
                {
                    dbModel.WeatherDataPoints.Remove(dbObject);
                    this.SaveChangeWithContextReloadUponFailure(dbModel);
                }
            }
        }

        /// <summary>
        /// Delete object with given MarketRegionID and DateTime from the database
        /// </summary>
        /// <param name="MarketRegionID">
        /// The Market Region ID.
        /// </param>
        /// <param name="dateTime">
        /// Date Time of entry
        /// </param>
        public void DeleteMarketDataPoints(int MarketRegionID, DateTime dateTime)
        {
            using (var dbModel = new SmartEnergyDatabase(this.DatabaseConnectionString))
            {
                var dbObject =
                    dbModel.MarketDataPoints.FirstOrDefault(
                        r =>
                            r.MarketRegionID == MarketRegionID 
                            && r.DateTimeUTC.Year == dateTime.Year && r.DateTimeUTC.Month == dateTime.Month
                            && r.DateTimeUTC.Day == dateTime.Day && r.DateTimeUTC.Hour == dateTime.Hour
                            && r.DateTimeUTC.Minute == dateTime.Minute && r.DateTimeUTC.Second == dateTime.Second);

                if (dbObject != null)
                {
                    dbModel.MarketDataPoints.Remove(dbObject);
                    this.SaveChangeWithContextReloadUponFailure(dbModel);
                }
            }
        }
        #endregion

        #region Entity Framrwork helper methods
        /// <summary>
        /// When the save of a delete or addition operation fails, attempt to retry save the operation by refreshing the local objects before retrying the save. 
        /// </summary>
        /// <param name="dbModel"></param>
        private void SaveChangeWithContextReloadUponFailure(SmartEnergyDatabase dbModel)
        {
            try
            {
                dbModel.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                // Update the values of the entity that failed to save from the store 
                ex.Entries.Single().Reload();
                dbModel.SaveChanges();
            }
        }

        /// <summary>
        /// Save changes to the given SmartEnergyDatabase Object Model reference, allowing the calling method to specify the maximum number of retries. If the 
        /// numberOfAttemptsMade exceeds the maxNumberOfRetries allowed, retrying is stopped, and the exception is thrown. 
        /// </summary>
        /// <param name="maxNumberOfRetries"></param>
        /// <param name="dbModel"></param>
        /// <param name="numberOfAttemptsMade"></param>
        /// <returns></returns>
        private static bool SaveChangesWithRetry(
            int maxNumberOfRetries,
            SmartEnergyDatabase dbModel,
            ref int numberOfAttemptsMade)
        {
            Exception exceptionThrown;
            try
            {
                dbModel.SaveChanges();
                return true;
            }
            catch (DataException ex)
            {
                numberOfAttemptsMade++;
                exceptionThrown = ex;
            }

            if (numberOfAttemptsMade >= maxNumberOfRetries)
            {
                throw exceptionThrown;
            }
            return false;
        }

        #endregion

        #region DateTime helper methods   

        /// <summary>
        /// Convert the given .NET TimeZone name, from the list https://msdn.microsoft.com/en-us/library/ms912391(v=winembedded.11).aspx to a UTC DateTimeOffset object
        /// </summary>
        /// <param name="timeZone">
        /// .NET TimeZone name
        /// </param>
        /// <returns>
        /// The <see cref="DateTimeOffset"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        private static DateTimeOffset ConvertTimezoneNameToUtcDateTimeOffset(string timeZone)
        {
            try
            {
                var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                var dtoLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, localTimeZone);
                return dtoLocal;
            }
            catch (Exception e)
            {
                throw new ArgumentException(
                          "Couldn't parse timezone into .NET DateTime Offset. Be sure to supply a string representing a valid .NET Timezone.",
                          timeZone,
                          e);
                throw;
            }
        }
        #endregion

    }
}