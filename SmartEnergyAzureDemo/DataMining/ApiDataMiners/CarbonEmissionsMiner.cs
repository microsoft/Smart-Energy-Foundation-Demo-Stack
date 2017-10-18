// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        private EmissionsApiInteraction wattTimeEmissionsInteraction;
        private readonly string DatabaseConnectionString;

        /// <summary>
        /// Create an instance of a WeatherDataMiner for the given wattTimeApiUrl and wattTimeApiKey
        /// </summary>
        /// <param name="wattTimeApiUrl">URL of the Watt Time API</param>
        /// <param name="wattTimeApiKey">>WattTime Api Key</param>
        /// <param name="selfThrottlingMethod">The method to use to limit calls to the API to below the given threshold. Options: {None, InMemoryCallRecollection, AzureTableStorageCallRecollection}</param>
        /// <param name="databaseConnectionString">Entity Framrwork Connection string for the Applications' Database</param>
        /// <param name="maxNumberOfCallsPerMinute">Maximum number of calls to make to the API per minute</param>
        /// <param name="wattTimeEmissionsInteraction">Optional EmissionsApiInteraction object. If not supplied, it will be created</param>
        public CarbonEmissionsMiner(
            string wattTimeApiUrl,
            string wattTimeApiKey,
            string selfThrottlingMethod,
            string databaseConnectionString,
            int maxNumberOfCallsPerMinute = -1,
            EmissionsApiInteraction wattTimeEmissionsInteraction = null)
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
            this.DatabaseConnectionString = databaseConnectionString;
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
                    wattTimeApiUrl,
                    regionWattTimeName,
                    historicStartDateTime,
                    historicEndDateTime,
                    null,
                    wattTimeApiKey);

                 Logger.Information(
                    $"Received {results.Count} HistoricMarginalCarbonResults Results for {regionWattTimeName} from WattTime. Inserting them into the database",
                    "CarbonEmissionsMiner.MineHistoricMarginalCarbonResults()");

                // Insert results in the database 
                using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                {
                    foreach (var res in results)
                    {
                        var dateTime = res.timestamp;

                        var marginalCarbon = res.marginal_carbon.value;
                        var units = res.marginal_carbon.units;

                        if (marginalCarbon != null)
                        {
                            var marginalCarbonMetric = units == "lb/MW"
                                                           ? this.wattTimeEmissionsInteraction
                                                               .ConvertLbsPerMWhTo_GCo2PerkWh((double)marginalCarbon)
                                                           : marginalCarbon;

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
                    wattTimeApiUrl,
                    regionWattTimeName,
                    historicStartDateTime,
                    historicEndDateTime,
                    null,
                    wattTimeApiKey);

                Logger.Information(
                    $"Received {results.Count} ForecastMarginalCarbonResults Results for {regionWattTimeName} from WattTime. Inserting them into the database",
                    "CarbonEmissionsMiner.ForecastMarginalCarbonResults()");

                // Insert results in the database 
                using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                {
                    foreach (var res in results)
                    {
                        var dateTime = res.timestamp;

                        var marginalCarbon = res.marginal_carbon.value;
                        var units = res.marginal_carbon.units;

                        if (marginalCarbon != null)
                        {
                            var marginalCarbonMetric = units == "lb/MW"
                                                           ? this.wattTimeEmissionsInteraction
                                                               .ConvertLbsPerMWhTo_GCo2PerkWh((double)marginalCarbon)
                                                           : marginalCarbon;

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
