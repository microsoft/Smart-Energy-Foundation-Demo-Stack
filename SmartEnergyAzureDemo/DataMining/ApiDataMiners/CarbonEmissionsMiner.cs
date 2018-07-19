// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDataMiners
{
    using CentralLogger;

    using EmissionsApiInteraction;

    using SmartEnergyOM;

    /// <summary>
    /// This class uses the EmissionsApiInteraction to retrieve data from the WattTime API and store it in the application's database
    /// </summary>
    public class CarbonEmissionsMiner
    {
        private string wattTimeApiUrl;
        private string wattTimeApiKey;
        private string WattTimeUsername;
        private string WattTimePassword;
        private string WattTimeV2ApiUrl;
        private string WattTimeEmail;
        private string WattTimeOrganization;
        private EmissionsApiInteraction wattTimeEmissionsInteraction;
        private readonly string DatabaseConnectionString;
        private readonly string RelativeMeritDataSource;

        /// <summary>
        /// Create an instance of a WeatherDataMiner for the given wattTimeApiUrl and wattTimeApiKey
        /// </summary>
        /// <param name="wattTimeApiUrl">URL of the Watt Time API</param>
        /// <param name="wattTimeApiKey">>WattTime Api Key</param>
        /// <param name="wattTimeV2ApiUrl">URL of the Watt Time V2 API</param>
        /// <param name="wattTimeUsername">Username for the WattTime service</param>
        /// <param name="wattTimePassword">assword for the WattTime service</param>
        /// <param name="wattTimeEmail">Email for the WattTime service</param>
        /// <param name="wattTimeOrganization">Organization for the WattTime service</param>
        /// <param name="selfThrottlingMethod">The method to use to limit calls to the API to below the given threshold. Options: {None, InMemoryCallRecollection, AzureTableStorageCallRecollection}</param>
        /// <param name="databaseConnectionString">Entity Framrwork Connection string for the Applications' Database</param>
        /// <param name="maxNumberOfCallsPerMinute">Maximum number of calls to make to the API per minute</param>
        /// <param name="wattTimeEmissionsInteraction">Optional EmissionsApiInteraction object. If not supplied, it will be created</param>
        /// <param name="relativeMeritDataSource">Optional RelativeMeritDataSource string. If not supplied, no relative data will be miner or calculated</param>
        public CarbonEmissionsMiner(
            string wattTimeApiUrl,
            string wattTimeApiKey,
            string wattTimeV2ApiUrl,
            string wattTimeUsername,
            string wattTimePassword,
            string wattTimeEmail,
            string wattTimeOrganization,
            string selfThrottlingMethod,
            string databaseConnectionString,
            int maxNumberOfCallsPerMinute = -1,
            EmissionsApiInteraction wattTimeEmissionsInteraction = null,
            string relativeMeritDataSource = "WattTime")
        {
            if (wattTimeEmissionsInteraction == null)
            {
                this.wattTimeEmissionsInteraction = new EmissionsApiInteraction(
                                                        selfThrottlingMethod,
                                                        maxNumberOfCallsPerMinute);
            }
            else
            {
                this.wattTimeEmissionsInteraction = wattTimeEmissionsInteraction;
            }
            this.wattTimeApiUrl = wattTimeApiUrl;
            this.wattTimeApiKey = wattTimeApiKey;
            this.WattTimeV2ApiUrl = wattTimeV2ApiUrl;
            this.WattTimeUsername = wattTimeUsername;
            this.WattTimePassword = wattTimePassword;
            this.WattTimeEmail = wattTimeEmail;
            this.WattTimeOrganization = wattTimeOrganization;
            this.DatabaseConnectionString = databaseConnectionString;
            this.RelativeMeritDataSource = relativeMeritDataSource;

           // Perform any service specific actions such as registeration
           if( (!string.IsNullOrEmpty(wattTimeUsername)) && (!string.IsNullOrEmpty(WattTimePassword)) )
            {
                // Register the given user details with the WattTime API in case they haven't already been registered. 
                this.wattTimeEmissionsInteraction.RegisterWithWattTime(this.WattTimeV2ApiUrl, this.WattTimeUsername, this.WattTimePassword, this.WattTimeEmail, this.WattTimeOrganization, true);                    
            }
        }

        /// <summary>
        /// Retrieve historic marginal emissions results between the given dates and store them in the database
        /// </summary>
        /// <param name="startDateTime">Optional StartDateTime. If not supplied, a default value will be used</param>
        /// <param name="endDateTime">Optional endDateTime. If not supplied, a default value will be used</param>
        /// <param name="regionWattTimeName">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="regionId">regionId of this region in the application's database</param>
        public void MineHistoricMarginalCarbonResults(
            DateTime? startDateTime,
            DateTime? endDateTime,
            string regionWattTimeName,
            int regionId)
        {
            var historicStartDateTime = startDateTime ?? DateTime.UtcNow.AddDays(-2);
            var historicEndDateTime = endDateTime ?? DateTime.UtcNow.AddMinutes(15);

            try
            {
                Logger.Information(
                    $"Mining Historic Marginal Carbon Results for {regionWattTimeName} from WattTime URL {this.wattTimeApiUrl}.",
                    "CarbonEmissionsMiner.MineHistoricMarginalCarbonResults()");

                var results = this.wattTimeEmissionsInteraction.GetObservedMarginalCarbonResults(
                    WattTimeV2ApiUrl,
                    regionWattTimeName,
                    WattTimeUsername,
                    WattTimePassword,
                    historicStartDateTime,
                    historicEndDateTime,
                    null);

                Logger.Information(
                   $"Received {results.Count} HistoricMarginalCarbonResults Results for {regionWattTimeName} from WattTime. Inserting them into the database",
                   "CarbonEmissionsMiner.MineHistoricMarginalCarbonResults()");

                // Insert results in the database 
                using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                {
                    foreach (var res in results)
                    {
                        var dateTime = res.point_time;

                        var marginalCarbon = res.value;

                        var marginalCarbonMetric = this.wattTimeEmissionsInteraction
                                                           .ConvertLbsPerMWhTo_GCo2PerkWh((double)marginalCarbon);

                        _objectModel.InsertOrUpdateCarbonEmissionsDataPoints(
                            regionId,
                            dateTime,
                            null,
                            null,
                            marginalCarbonMetric,
                            null);
                    }
                }
            } 
            catch (Exception e)
            {
                Logger.Error(
                    $"CarbonEmissionsMiner: MineHistoricMarginalCarbonResults(): Exception encountered while emissions Data figures for {regionWattTimeName} between {historicStartDateTime} and {historicEndDateTime}.",
                    "CarbonEmissionsMiner.MineHistoricMarginalCarbonResults()",
                    null,
                    e);
            }
        }

        /// <summary>
        /// Retrieve forecast marginal emissions results between the given dates and store them in the database
        /// </summary>
        /// <param name="startDateTime">Optional StartDateTime. If not supplied, a default value will be used</param>
        /// <param name="endDateTime">Optional endDateTime. If not supplied, a default value will be used</param>
        /// <param name="regionWattTimeName">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="regionId">regionId of this region in the application's database</param>
        public void MineForecastMarginalCarbonResults(
            DateTime? startDateTime,
            DateTime? endDateTime,
            string regionWattTimeName,
            int regionId)
        {
            var historicStartDateTime = startDateTime ?? DateTime.UtcNow.AddDays(-2);
            var historicEndDateTime = endDateTime ?? DateTime.UtcNow.AddMinutes(15);

            try
            {
                Logger.Information(
                    $"Mining Forecast Marginal Carbon Results for {regionWattTimeName} from WattTime URL {this.wattTimeApiUrl}.",
                    "CarbonEmissionsMiner.MineForecastMarginalCarbonResults()");

                var results = this.wattTimeEmissionsInteraction.GetForecastMarginalCarbonResults(
                    WattTimeV2ApiUrl,
                    regionWattTimeName,
                    WattTimeUsername,
                    WattTimePassword,
                    historicStartDateTime,
                    historicEndDateTime,
                    null);

                Logger.Information(
                    $"Received {results.Count} ForecastMarginalCarbonResults Results for {regionWattTimeName} from WattTime. Inserting them into the database",
                    "CarbonEmissionsMiner.ForecastMarginalCarbonResults()");

                // Insert results in the database 
                using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                {
                    foreach (var res in results)
                    {
                        var dateTime = res.point_time;

                        var marginalCarbon = res.value;

                        var marginalCarbonMetric = this.wattTimeEmissionsInteraction
                                                           .ConvertLbsPerMWhTo_GCo2PerkWh((double)marginalCarbon);

                        _objectModel.InsertOrUpdateCarbonEmissionsDataPoints(
                            regionId,
                            dateTime,
                            null,
                            null,
                            null,
                            marginalCarbonMetric);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"CarbonEmissionsMiner: MineForecastMarginalCarbonResults(): Exception encountered while emissions Data figures for {regionWattTimeName} between {historicStartDateTime} and {historicEndDateTime}.",
                    "CarbonEmissionsMiner.MineForecastMarginalCarbonResults()",
                    null,
                    e);
            }
        }

        /// <summary>
        /// Mine Or Calculate Relative Merit Data for emissions values between the given dates and store them in the database.
        /// The Relative Merit is a value between 0 (meaning best) and 1 (being worst). 
        /// </summary>
        /// <param name="startDateTime">Optional StartDateTime. If not supplied, a default value will be used</param>
        /// <param name="endDateTime">Optional endDateTime. If not supplied, a default value will be used</param>
        /// <param name="regionWattTimeName">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="regionId">regionId of this region in the application's database</param>
        public void MineOrCalculateHistoricRelativeMeritData(
            double latitude, 
            double longitude,
            DateTime? startDateTime,
            DateTime? endDateTime,
            string regionWattTimeName,
            int regionId)
        {
            var historicStartDateTime = startDateTime ?? DateTime.UtcNow.AddDays(-2);
            var historicEndDateTime = endDateTime ?? DateTime.UtcNow.AddMinutes(15);

            try
            {
                Logger.Information(
                    $"Entering method for {regionWattTimeName} from WattTime URL {this.wattTimeApiUrl} with method of relative data retrieval / calculation of {this.RelativeMeritDataSource}.",
                    "CarbonEmissionsMiner.MineOrCalculateHistoricRelativeMeritData()");

                switch(this.RelativeMeritDataSource)
                {
                    case "WattTime":
                        throw new NotImplementedException("WattTime does not currently offer historic relative merit data querying");
                        break;

                    case "CustomInternalCalculation":
                        var calculatedHistoricRelativeMeritResults = this.CalculateHistoricRelativeMeritDataResults(
                            regionId,
                            historicStartDateTime,
                            historicEndDateTime);

                        // Insert results in the database 
                        using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                        {
                            foreach (var res in calculatedHistoricRelativeMeritResults)
                            {
                                _objectModel.InsertOrUpdateCarbonEmissionsRelativeMeritDataPoints(
                                    regionId,
                                    res.Timestamp,
                                    res.EmissionsRelativeMerit,
                                    res.EmissionsRelativeMerit_Forcast);
                            }
                        }
                        break;

                    default:
                        Logger.Information(
                                $"No known defined method of MineOrCalculateHistoricRelativeMeritData supplied to method. Not mining or calculating Historic Relative Merit Data for this region ({regionWattTimeName})",
                                "CarbonEmissionsMiner.MineOrCalculateHistoricRelativeMeritData()");
                        return;
                }
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"CarbonEmissionsMiner: MineForecastMarginalCarbonResults(): Exception encountered while emissions Data figures for {regionWattTimeName} between {historicStartDateTime} and {historicEndDateTime}.",
                    "CarbonEmissionsMiner.MineForecastMarginalCarbonResults()",
                    null,
                    e);
            }
        }

        /// <summary>
        /// Mine Or Calculate Relative Merit Data for emissions values between the given dates and store them in the database.
        /// The Relative Merit is a value between 0 (meaning best) and 1 (being worst). 
        /// </summary>
        /// <param name="startDateTime">Optional StartDateTime. If not supplied, a default value will be used</param>
        /// <param name="endDateTime">Optional endDateTime. If not supplied, a default value will be used</param>
        /// <param name="regionWattTimeName">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="regionId">regionId of this region in the application's database</param>
        public void MineOrCalculateCarbonEmissionsRelativeMerit(
            string regionWattTimeName,
            int regionId)
        {
            var historicStartDateTime = DateTime.UtcNow.AddMinutes(-1);
            var historicEndDateTime = DateTime.UtcNow.AddMinutes(15);

            try
            {
                Logger.Information(
                    $"Entering method for {regionWattTimeName} from WattTime URL {this.wattTimeApiUrl} with method of relative data retrieval / calculation of {this.RelativeMeritDataSource}.",
                    "CarbonEmissionsMiner.MineOrCalculateHistoricRelativeMeritData()");

                switch (this.RelativeMeritDataSource)
                {
                    case "WattTime":
                        var result = this.wattTimeEmissionsInteraction.GetCarbonEmissionsRelativeMeritResults(
                            WattTimeV2ApiUrl,
                            regionWattTimeName,
                            WattTimeUsername,
                            WattTimePassword
                            );

                        if (result != null)
                        {

                            Logger.Information(
                                $"Received result for RelativeMeritData for {regionWattTimeName} from WattTime. Inserting into the database",
                                "CarbonEmissionsMiner.MineOrCalculateRelativeMeritData()");

                            // Insert results in the database 
                            using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                            {
                                var dateTime = result.validUntil;
                                double emissionsRelativeMerit = result.percent / 100; // Normalise percentage to a value between 0 and 1

                                _objectModel.InsertOrUpdateCarbonEmissionsRelativeMeritDataPoints(
                                    regionId,
                                    dateTime,
                                    emissionsRelativeMerit,
                                    null);
                            }
                        }
                        else
                        {
                            Logger.Information(
                                $"No result found when requesting RelativeMeritData for {regionWattTimeName} from WattTime at UTC: {DateTime.UtcNow}.e",
                                "CarbonEmissionsMiner.MineOrCalculateRelativeMeritData()");
                        }
                        break;

                    case "CustomInternalCalculation":
                        var calculatedHistoricRelativeMeritResults = this.CalculateHistoricRelativeMeritDataResults(
                            regionId,
                            historicStartDateTime,
                            historicEndDateTime);

                        // Insert results in the database 
                        using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                        {
                            foreach (var res in calculatedHistoricRelativeMeritResults)
                            {
                                _objectModel.InsertOrUpdateCarbonEmissionsRelativeMeritDataPoints(
                                    regionId,
                                    res.Timestamp,
                                    res.EmissionsRelativeMerit,
                                    res.EmissionsRelativeMerit_Forcast);
                            }
                        }
                        break;

                    default:
                        Logger.Information(
                                $"No known defined method of MineOrCalculateRelativeMeritData supplied to method. Not mining or calculating Historic Relative Merit Data for this region ({regionWattTimeName})",
                                "CarbonEmissionsMiner.MineOrCalculateRelativeMeritData()");
                        return;
                }
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"CarbonEmissionsMiner: MineOrCalculateRelativeMeritData(): Exception encountered while emissions Data figures for {regionWattTimeName} between {historicStartDateTime} and {historicEndDateTime}.",
                    "CarbonEmissionsMiner.MineOrCalculateRelativeMeritData()",
                    null,
                    e);
            }
        }

        /// <summary>
        /// Retrieve historic system wide emissions results between the given dates and store them in the database
        /// </summary>
        /// <param name="startDateTime">Optional StartDateTime. If not supplied, a default value will be used</param>
        /// <param name="endDateTime">Optional endDateTime. If not supplied, a default value will be used</param>
        /// <param name="regionWattTimeName">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="regionId">regionId of this region in the application's database</param>
        public void MineHistoricSystemWideCarbonResults(
            DateTime? startDateTime,
            DateTime? endDateTime,
            string regionWattTimeName,
            int regionId)
        {
            var historicStartDateTime = startDateTime ?? DateTime.UtcNow.AddDays(-2);
            var historicEndDateTime = endDateTime ?? DateTime.UtcNow.AddMinutes(15);

            try
            {
                Logger.Information(
                    $"Mining Historic SystemWide Carbon Results for {regionWattTimeName} from WattTime URL {this.wattTimeApiUrl}.",
                    "CarbonEmissionsMiner.MineHistoricSystemWideCarbonResults()");

                var results =
                    this.wattTimeEmissionsInteraction.GetGenerationMixAndSystemWideEmissionsResults(
                        this.wattTimeApiUrl,
                        regionWattTimeName,
                        historicStartDateTime,
                        historicEndDateTime,
                        null,
                        this.wattTimeApiKey);

                Logger.Information(
                    $"Received {results.Count} HistoricSystemWideCarbon Results for {regionWattTimeName} from WattTime. Inserting them into the database",
                    "CarbonEmissionsMiner.MineHistoricSystemWideCarbonResults()");

                // Insert results in the database 
                using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                {
                    foreach (var res in results)
                    {
                        var dateTime = res.timestamp;

                        var systemWideCarbon = res.carbon;
                        var units = "lb/MW"; // WattTime System Wide carbon is in lb/MW

                        if (systemWideCarbon != null)
                        {
                            var systemWideCarbonMetric = units == "lb/MW"
                                                             ? this.wattTimeEmissionsInteraction
                                                                 .ConvertLbsPerMWhTo_GCo2PerkWh(
                                                                     (double)systemWideCarbon)
                                                             : systemWideCarbon;

                            _objectModel.InsertOrUpdateCarbonEmissionsDataPoints(
                                regionId,
                                dateTime,
                                systemWideCarbonMetric,
                                null,
                                null,
                                null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"CarbonEmissionsMiner: MineHistoricSystemWideCarbonResults(): Exception encountered while emissions Data figures for {regionWattTimeName} between {historicStartDateTime} and {historicEndDateTime}.",
                    "CarbonEmissionsMiner.MineHistoricSystemWideCarbonResults()",
                    null,
                    e);
            }
        }

        /// <summary>
        /// Take a CSV File containing CarbonEmissionsDataPoint values, and return them as a List of CarbonEmissionsDataPoint.
        /// DateTime is parsed as DD/MM/YYYY HH:MM and should be in this format in the CSV
        /// </summary>
        /// <param name="folderContainingCsv"></param>
        /// <param name="csvFileName"></param>
        /// <param name="regionId">RegionId to which the CarbonEmissionsDataPoint values pertain</param>
        /// <param name="dateTimeFormat">DateTime format to be passed to DateTime.ParseExact(). E.g. "MM/dd/yyyy HH:mm:ss"</param>
        /// <param name="ignoreParsingErrors">True to ignore errors parsing individual rows. False to rethrow the exceptions.</param>
        /// <returns></returns>
        public List<CarbonEmissionsDataPoint> ImportCarbonResultsFromCsv(
            string folderContainingCsv,
            string csvFileName,
            int regionId,
            string dateTimeFormat = "MM/dd/yyyy HH:mm:ss",
            bool ignoreParsingErrors = true)
        {
            var results = new List<CarbonEmissionsDataPoint>();

            try
            {
                Logger.Information(
                    $"Importing Historic Carbon Results for RegionId {regionId} from CSV {csvFileName}.",
                    "CarbonEmissionsMiner.ImportCarbonResultsFromCsv()");
                
                var csvConnectionString = GenerateOleDbConnectionStringToFolder(folderContainingCsv);
                using (var cn = new OleDbConnection(csvConnectionString))
                {
                    cn.Open();
                    using (OleDbCommand cmd = cn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT * FROM [{csvFileName}]";
                        cmd.CommandType = CommandType.Text;
                        using (OleDbDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            foreach (DbDataRecord record in reader)
                            {
                                try
                                {
                                    // DateTime is parsed as DD/MM/YYYY HH:MM and should be in this format in the CSV
                                    var dateTimeUtcString = record.GetValue(reader.GetOrdinal("DateTimeUtc"))
                                        .ToString();
                                    var dateTimeFormatToaPasrseWith = dateTimeFormat;

                                    var dateTimeUtc = DateTime.ParseExact(dateTimeUtcString, dateTimeFormatToaPasrseWith, CultureInfo.InvariantCulture);

                                    decimal systemWideEmissions = -9999;
                                    decimal marginalEmissions = -9999;
                                    string systemWideEmissionsUnit = null;
                                    string marginalEmissionsUnit = null;
                                    
                                    try
                                    {
                                        systemWideEmissions = string.IsNullOrEmpty(
                                            record.GetValue(reader.GetOrdinal("SystemWideEmissions")).ToString())
                                            ? -9999
                                            : decimal.Parse(record.GetValue(reader.GetOrdinal("SystemWideEmissions"))
                                                .ToString());
                                        systemWideEmissionsUnit =
                                            string.IsNullOrEmpty(
                                                record.GetValue(reader.GetOrdinal("SystemWideEmissionsUnit"))
                                                    .ToString())
                                                ? null
                                                : record.GetString(reader.GetOrdinal("SystemWideEmissionsUnit"));
                                    }
                                    catch (Exception)
                                    {
                                    }

                                    try
                                    {
                                        marginalEmissions = string.IsNullOrEmpty(
                                        record.GetValue(reader.GetOrdinal("marginalEmissions")).ToString())
                                        ? -9999
                                        : decimal.Parse(record.GetValue(reader.GetOrdinal("marginalEmissions"))
                                            .ToString());
                                    marginalEmissionsUnit =
                                        string.IsNullOrEmpty(
                                            record.GetValue(reader.GetOrdinal("marginalEmissionsUnit")).ToString())
                                            ? null
                                            : record.GetString(reader.GetOrdinal("marginalEmissionsUnit"));
                                    }
                                    catch (Exception)
                                    {
                                    }

                                    // Convert the units to ensure they are uniform in the database
                                    double? systemWideCarbonMetric = null;
                                    if (systemWideEmissions != -9999)
                                    {
                                        systemWideCarbonMetric = systemWideEmissionsUnit == "lbs/MWh"
                                            ? this.wattTimeEmissionsInteraction
                                                .ConvertLbsPerMWhTo_GCo2PerkWh(
                                                    double.Parse(systemWideEmissions.ToString()))
                                            : double.Parse(systemWideEmissions.ToString());
                                    }

                                    double? marginalCarbonMetric = null;
                                    if (marginalEmissions != -9999)
                                    {
                                        marginalCarbonMetric = marginalEmissionsUnit == "lbs/MWh"
                                            ? this.wattTimeEmissionsInteraction
                                                .ConvertLbsPerMWhTo_GCo2PerkWh(
                                                    double.Parse(marginalEmissions.ToString()))
                                            : double.Parse(marginalEmissions.ToString());
                                    }

                                    var dataPoint = new CarbonEmissionsDataPoint()
                                    {
                                        DateTimeUTC = dateTimeUtc,
                                        EmissionsRegion = null,
                                        EmissionsRegionID = regionId,
                                        MarginalCO2Intensity_Forcast_gCO2kWh = null,
                                        MarginalCO2Intensity_gCO2kWh = marginalCarbonMetric,
                                        SystemWideCO2Intensity_Forcast_gCO2kWh = null,
                                        SystemWideCO2Intensity_gCO2kWh = systemWideCarbonMetric
                                    };

                                    results.Add(dataPoint);
                                }
                                catch (Exception e)
                                {
                                    // Encountered an unreadable row in the CSV file. 
                                    if (!ignoreParsingErrors)
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"CarbonEmissionsMiner: ImportCarbonResultsFromCsv(): Exception encountered importing emissions Data from CSV with name {csvFileName} in folder {folderContainingCsv}.",
                    "CarbonEmissionsMiner.ImportCarbonResultsFromCsv()",
                    null,
                    e);
            }
            
            return results;
        }

        /// <summary>
        /// Calculate the Relative Merit of Marginal Carbon Emissions Values which are present in the database between the given startDateTime and endDateTime. 
        /// The Relative Merit is a value between 0 (meaning best) and 1 (being worst). 
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns>List of MarginalCarbonResult.Result representing the Relative Merit of the corresponding Marginal Carbon Emissions Values</returns>
        public List<EmissionsRelativeMeritDatapoint> CalculateHistoricRelativeMeritDataResults(int regionId, DateTime startDateTime, DateTime endDateTime)
        {
            /*** Implement your custom logic here to calculate relative merit data ***/

            /* Here is a sample method provided here which simply calculates it's comparison to a one week rolling average for the data */
            var results = new List<EmissionsRelativeMeritDatapoint>();
            using (var objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
            {
                // Get datapoints on which to calculate relative merit
                var carbonResultsOnWhichToCalculateRelativeMetit =
                    objectModel.FindCarbonEmissionsDataPoints(regionId, startDateTime, endDateTime);

                foreach (var datapoint in carbonResultsOnWhichToCalculateRelativeMetit)
                {
                    try
                    {
                        // Get the last week of data
                        var startDateOfOneWeekRollingAverage = datapoint.DateTimeUTC.AddDays(-7);
                        var endDateOfOneWeekRollingAverage = datapoint.DateTimeUTC.AddHours(1);
                        var emissionsDataPointsWithinWindow =
                            objectModel.FindCarbonEmissionsDataPoints(regionId, startDateOfOneWeekRollingAverage,
                                endDateOfOneWeekRollingAverage).Where(a => a.MarginalCO2Intensity_gCO2kWh != null);

                        // Calculate where this datapoints falls within the range of the last week
                        if (emissionsDataPointsWithinWindow.Any())
                        {
                            double? relativeMerit = null;
                            double? relativeMerit_Forecast = null;
                            if (datapoint.MarginalCO2Intensity_gCO2kWh != null)
                            {
                                var maxValue = emissionsDataPointsWithinWindow.Max(a => a.MarginalCO2Intensity_gCO2kWh);
                                var minValue = emissionsDataPointsWithinWindow.Min(a => a.MarginalCO2Intensity_gCO2kWh);
                                var range = (double) (maxValue - minValue);
                                relativeMerit =
                                    (datapoint.MarginalCO2Intensity_gCO2kWh - minValue) / range;

                                // One specicial check: 0 should be reserved for zero emissions. Set anything above zero to .2
                                if ((relativeMerit < .2) && (datapoint.MarginalCO2Intensity_gCO2kWh > 0))
                                {
                                    relativeMerit = .2;
                                }
                            }
                            if (datapoint.MarginalCO2Intensity_Forcast_gCO2kWh != null)
                            {
                                var maxValue = emissionsDataPointsWithinWindow.Max(a => a.MarginalCO2Intensity_Forcast_gCO2kWh);
                                var minValue = emissionsDataPointsWithinWindow.Min(a => a.MarginalCO2Intensity_Forcast_gCO2kWh);
                                var range = (double)(maxValue - minValue);
                                relativeMerit_Forecast =
                                    (datapoint.MarginalCO2Intensity_Forcast_gCO2kWh - minValue) / range;

                                // One specicial check: 0 should be reserved for zero emissions. Set anything above zero to .2
                                if ((relativeMerit_Forecast < .2) && (datapoint.MarginalCO2Intensity_Forcast_gCO2kWh > 0))
                                {
                                    relativeMerit_Forecast = .2;
                                }
                            }
                            if ((relativeMerit != null) || (relativeMerit_Forecast != null))
                            {
                                var emissionsRelativeMeritDataResult =
                                    new EmissionsRelativeMeritDatapoint
                                    {
                                        EmissionsRegionID = regionId,
                                        Timestamp = datapoint.DateTimeUTC,
                                        EmissionsRelativeMerit = relativeMerit,
                                        EmissionsRelativeMerit_Forcast = relativeMerit_Forecast
                                    };
                                results.Add(emissionsRelativeMeritDataResult);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(
                            $"CarbonEmissionsMiner: CalculateHistoricRelativeMeritDataResults(): Exception encountered Calculate the Merit of datapoint at {datapoint.DateTimeUTC} for RegionId {datapoint.EmissionsRegionID}.",
                            "CarbonEmissionsMiner.CalculateHistoricRelativeMeritDataResults()",
                            null,
                            e);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Take a CSV File containing CarbonEmissionsDataPoint values, and add them as CarbonEmissionsDataPoint to the database
        /// </summary>
        /// <param name="folderContainingCsv"></param>
        /// <param name="csvFileName"></param>
        /// <param name="regionId">RegionId to which the CarbonEmissionsDataPoint values pertain</param>
        /// <param name="dateTimeFormat">DateTime format to be passed to DateTime.ParseExact(). E.g. "MM/dd/yyyy HH:mm:ss"</param>
        /// <param name="ignoreParsingErrors">True to ignore errors parsing individual rows. False to rethrow the exceptions.</param>
        /// <returns></returns>
        public void ImportCarbonResultsToDatabaseFromCsv(
            string folderContainingCsv,
            string csvFileName,
            int regionId,
            string dateTimeFormat = "MM/dd/yyyy HH:mm:ss",
            bool ignoreParsingErrors = true)
        {
            try
            {
                Logger.Information(
                    $"Importing Historic Carbon Results for RegionId {regionId} from CSV {csvFileName}.",
                    "CarbonEmissionsMiner.ImportCarbonResultsToDatabaseFromCsv()");

                var results =
                    ImportCarbonResultsFromCsv(folderContainingCsv, csvFileName, regionId, dateTimeFormat, ignoreParsingErrors);

                using (var objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                {
                    foreach (var result in results)
                    {
                        // Insert the value into the database
                        objectModel.InsertOrUpdateCarbonEmissionsDataPoints(
                            regionId,
                            result.DateTimeUTC,
                            result.SystemWideCO2Intensity_gCO2kWh,
                            null,
                            result.MarginalCO2Intensity_gCO2kWh,
                            null);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"CarbonEmissionsMiner: ImportCarbonResultsToDatabaseFromCsv(): Exception encountered importing emissions Data from CSV with name {csvFileName} in folder {folderContainingCsv} and inserting them to the database.",
                    "CarbonEmissionsMiner.ImportCarbonResultsToDatabaseFromCsv()",
                    null,
                    e);
            }
        }

        private static string GenerateOleDbConnectionStringToFolder(string folderContainingCSV)
        {
            string csvConnectionString =
                $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source='{folderContainingCSV}';Extended Properties='text;HDR=Yes;FMT=Delimited';";
            return csvConnectionString;
        }

        /// <summary>
        /// Retrieve historic system wide and marginal emissions results between the given dates and store them in the database
        /// </summary>
        /// <param name="startDateTime">Optional StartDateTime. If not supplied, a default value will be used</param>
        /// <param name="endDateTime">Optional endDateTime. If not supplied, a default value will be used</param>
        /// <param name="regionWattTimeName">Abbreviation for the required region (e.g. "PJM"). See https://api.watttime.org/faq/#where </param>
        /// <param name="regionId">regionId of this region in the application's database</param>
        public void MineHistoricCarbonResults(
            DateTime? startDateTime,
            DateTime? endDateTime,
            string regionWattTimeName,
            int regionId)
        {
            this.MineHistoricSystemWideCarbonResults(startDateTime, endDateTime, regionWattTimeName, regionId);
            this.MineHistoricMarginalCarbonResults(startDateTime, endDateTime, regionWattTimeName, regionId);
        }
    }
}


