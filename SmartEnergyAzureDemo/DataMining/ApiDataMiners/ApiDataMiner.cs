// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace ApiDataMiners
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    using CentralLogger;

    using SmartEnergyOM;

    /// <summary>
    /// This class encapsulates the mining of data from several APIs, and the storage of that data in a single database
    /// </summary>
    public class ApiDataMiner
    {
        private readonly string DatabaseConnectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDataMiner"/> class.
        /// </summary>
        /// <param name="databaseConnectionString">Entity Framrwork Connection string for the Applications' Database</param>
        public ApiDataMiner(string databaseConnectionString)
        {
            this.DatabaseConnectionString = databaseConnectionString;
        }

        /// <summary>
        /// Parse a settings file and kick off data miners with those settings
        /// </summary>
        /// <param name="ConfigPath"></param>
        /// <param name="wattTimeApiKeyOverride">Optional wattTimeApiKey to Override what's in the MinerConfigXML File</param>
        /// <param name="wundergroundApiKeyOverride">Optional WundergroundApiKey to Override what's in the MinerConfigXML File</param>
        /// <param name="wattTimeUsernameOverride">Optional wattTimeUsername to Override what's in the MinerConfigXML File</param>
        /// <param name="wattTimePasswordOverride">Optional wattTimePassword to Override what's in the MinerConfigXML File</param>
        /// <param name="wattTimeEmailOverride">Optional wattTimeEmaiL to Override what's in the MinerConfigXML File</param>
        /// <param name="wattTimeOrganizationOverride">Optional wattTimeOrganization to Override what's in the MinerConfigXML File</param>
        public void ParseMinerSettingsFileAndMineData(string ConfigPath, string wattTimeApiKeyOverride = null,
            string wundergroundApiKeyOverride = null, string wattTimeUsernameOverride = null, string wattTimePasswordOverride = null, string wattTimeEmailOverride = null,
            string wattTimeOrganizationOverride = null)
        {
            using (var streamReader = new StreamReader(ConfigPath))
            {
                var xmlSerializer = new XmlSerializer(typeof(ApiMinerConfigLayout));
                var minerConfigs = (ApiMinerConfigLayout)xmlSerializer.Deserialize(streamReader);

                foreach (var minerConfig in minerConfigs.Regions)
                {
                    this.MineRegionData(minerConfig, wattTimeApiKeyOverride, wundergroundApiKeyOverride, wattTimeUsernameOverride, wattTimePasswordOverride, wattTimeEmailOverride,
                        wattTimeOrganizationOverride);
                }
            }
        }

        /// <summary>
        /// Take a config element containing the data for a region to be mined, mine the data and save it in the database
        /// </summary>
        /// <param name="regionConfiguration"></param>
        /// <param name="wattTimeApiKeyOverride">Optional wattTimeApiKey to Override what's in the MinerConfigXML File</param>
        /// <param name="wundergroundApiKeyOverride">Optional WundergroundApiKey to Override what's in the MinerConfigXML File</param>
        /// <param name="wattTimeUsernameOverride">Optional wattTimeUsername to Override what's in the MinerConfigXML File</param>
        /// <param name="wattTimePasswordOverride">Optional wattTimePassword to Override what's in the MinerConfigXML File</param>
        /// <param name="wattTimeEmailOverride">Optional wattTimeEmaiL to Override what's in the MinerConfigXML File</param>
        /// <param name="wattTimeOrganizationOverride">Optional wattTimeOrganization to Override what's in the MinerConfigXML File</param>
        public void MineRegionData(ApiMinerConfigLayoutRegion regionConfiguration, string wattTimeApiKeyOverride = null,
            string wundergroundApiKeyOverride = null, string wattTimeUsernameOverride = null, string wattTimePasswordOverride = null, string wattTimeEmailOverride = null, 
            string wattTimeOrganizationOverride = null)
        {
            var regionGroupingName = regionConfiguration.friendlyName;

            using (new TimedOperation(
                $"Beginning Mining of all data for Region {regionGroupingName}",
                "ApiDataMiner.MineRegionData()"))
            {

                // Mine the regions emissions if an emissions node was supplied
                int? emissionsRegionId = null;
                if (regionConfiguration.EmissionsMiningRegion != null)
                {
                    var friendlyName = regionConfiguration.EmissionsMiningRegion.friendlyName;

                    using (
                        new TimedOperation(
                            $"Beginning Mining of emissions data for Region {friendlyName}",
                            "ApiDataMiner.MineRegionData()"))
                    {
                        var timeZone = regionConfiguration.EmissionsMiningRegion.TimeZone;
                        var regionLat = regionConfiguration.EmissionsMiningRegion.Latitude;
                        var regionLong = regionConfiguration.EmissionsMiningRegion.Longitude;
                        var regionWattTimeName =
                            regionConfiguration.EmissionsMiningRegion.EmissionsWattTimeAbbreviation;
                        var wattTimeApiUrl = regionConfiguration.EmissionsMiningRegion.ApiUrl;
                        var wattTimeApiV2Url = regionConfiguration.EmissionsMiningRegion.WattTimeApiV2Url;

                        string wattTimeApiKey = null;
                        if (string.IsNullOrEmpty(wattTimeApiKeyOverride) || wattTimeApiKeyOverride.Equals("none"))
                        {
                            wattTimeApiKey = regionConfiguration.EmissionsMiningRegion.ApiKey;
                        }
                        else
                        {
                            wattTimeApiKey = wattTimeApiKeyOverride;
                        }

                        string wattTimeUsername = null;
                        if (string.IsNullOrEmpty(wattTimeUsernameOverride) || wattTimeUsernameOverride.Equals("none"))
                        {
                            wattTimeUsername = regionConfiguration.EmissionsMiningRegion.WattTimeUsername;
                        }
                        else
                        {
                            wattTimeUsername = wattTimeUsernameOverride;
                        }

                        string wattTimePassword = null;
                        if (string.IsNullOrEmpty(wattTimePasswordOverride) || wattTimePasswordOverride.Equals("none"))
                        {
                            wattTimePassword = regionConfiguration.EmissionsMiningRegion.WattTimePassword;
                        }
                        else
                        {
                            wattTimePassword = wattTimePasswordOverride;
                        }

                        string wattTimeEmail = null;
                        if (string.IsNullOrEmpty(wattTimeEmailOverride) || wattTimeEmailOverride.Equals("none"))
                        {
                            wattTimeEmail = regionConfiguration.EmissionsMiningRegion.WattTimeEmail;
                        }
                        else
                        {
                            wattTimeEmail = wattTimeEmailOverride;
                        }

                        string wattTimeOrganization = null;
                        if (string.IsNullOrEmpty(wattTimeOrganizationOverride) || wattTimeOrganizationOverride.Equals("none"))
                        {
                            wattTimeOrganization = regionConfiguration.EmissionsMiningRegion.WattTimeOrganization;
                        }
                        else
                        {
                            wattTimeOrganization = wattTimeOrganizationOverride;
                        }

                        var selfThrottlingMethod = regionConfiguration.EmissionsMiningRegion.SelfThrottlingMethod;
                        var maxNumberOfCallsPerMinute =
                            regionConfiguration.EmissionsMiningRegion.MaxNumberOfCallsPerMinute;
                        var relativeMeritDataSource =
                            regionConfiguration.EmissionsMiningRegion.RelativeMeritDataSource;
                        var historicStartDateTime = DateTime.UtcNow.AddDays(-15);
                        var historicEndDateTime = DateTime.UtcNow.AddDays(1);
                        var forecastStartDateTime = DateTime.UtcNow.AddDays(-2);
                        var forecastEndDateTime = DateTime.UtcNow.AddDays(10);

                        if (!string.IsNullOrEmpty(wattTimeApiKey) && !wattTimeApiKey.Equals("none"))
                        {
                            Logger.Information(
                                $"About to add Emissions Region and Mine Carbon Emissions Data for {regionWattTimeName} from WattTime URL {wattTimeApiUrl} from {historicStartDateTime} to {historicEndDateTime} for historic data and insert them into the database",
                                "ApiDataMiner.MineRegionData()");

                            using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                            {
                                emissionsRegionId =
                                    _objectModel.AddEmissionsRegion(friendlyName, timeZone, regionLat, regionLong,
                                            regionWattTimeName)
                                        .EmissionsRegionID;

                                CarbonEmissionsMiner carbonEmissionsMiner = new CarbonEmissionsMiner(
                                    wattTimeApiUrl,
                                    wattTimeApiKey,
                                    wattTimeApiV2Url,
                                    wattTimeUsername,
                                    wattTimePassword,
                                    wattTimeEmail,
                                    wattTimeOrganization,
                                    selfThrottlingMethod,
                                    this.DatabaseConnectionString,
                                    maxNumberOfCallsPerMinute,
                                    null,
                                    relativeMeritDataSource
                                    );

                                // Mine Recent Actual Data
                                carbonEmissionsMiner.MineHistoricCarbonResults(
                                    historicStartDateTime,
                                    historicEndDateTime,
                                    regionWattTimeName,
                                    (int)emissionsRegionId);

                                // Mine Forecast Data
                                carbonEmissionsMiner.MineForecastMarginalCarbonResults(
                                    forecastStartDateTime,
                                    forecastEndDateTime,
                                    regionWattTimeName,
                                    (int)emissionsRegionId);

                                // Calculate or mine relative merit values
                                carbonEmissionsMiner.MineOrCalculateCarbonEmissionsRelativeMerit(
                                    regionWattTimeName,
                                    (int)emissionsRegionId);
                            }
                        }
                        else
                        {
                            Logger.Information(
                                $"No WattTime Api Key was specified. Skipping this region for Emissions.",
                                "RunAsync()");
                        }
                    }
                }

                // Mine the regions weather if a weather node was supplied
                int? weatherRegionId = null;
                if (regionConfiguration.WeatherMiningRegion != null)
                {
                    var friendlyName = regionConfiguration.WeatherMiningRegion.friendlyName;

                    using (
                        new TimedOperation(
                            $"Beginning Mining of weather data for Region {friendlyName}",
                            "ApiDataMiner.MineRegionData()"))
                    {

                        var timeZone = regionConfiguration.WeatherMiningRegion.TimeZone;
                        var regionLat = regionConfiguration.WeatherMiningRegion.Latitude;
                        var regionLong = regionConfiguration.WeatherMiningRegion.Longitude;
                        var weatherRegionWundergroundSubUrl =
                            regionConfiguration.WeatherMiningRegion.weatherRegionWundergroundSubUrl;
                        var wundergroundApiUrl = regionConfiguration.WeatherMiningRegion.ApiUrl;
                        string wundergroundApiKey = null;
                        if (string.IsNullOrEmpty(wundergroundApiKeyOverride) || wundergroundApiKeyOverride.Equals("none"))
                        {
                            wundergroundApiKey = regionConfiguration.WeatherMiningRegion.ApiKey;
                        }
                        else
                        {
                            wundergroundApiKey = wundergroundApiKeyOverride;
                        }
                        var selfThrottlingMethod = regionConfiguration.WeatherMiningRegion.SelfThrottlingMethod;
                        var maxNumberOfCallsPerMinute =
                            regionConfiguration.WeatherMiningRegion.MaxNumberOfCallsPerMinute;
                        var historicStartDateTime = DateTime.UtcNow.AddDays(-1);
                        var historicEndDateTime = DateTime.UtcNow.AddDays(1);

                        if (!string.IsNullOrEmpty(wundergroundApiKey) && !wundergroundApiKey.Equals("none"))
                        {
                            Logger.Information(
                                $"About to add Emissions Region and Mine Carbon Emissions Data for {friendlyName} from Wunderground URL {weatherRegionWundergroundSubUrl} from {historicStartDateTime} to {historicEndDateTime} for historic data and insert them into the database",
                                "ApiDataMiner.MineRegionData()");

                            using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                            {
                                weatherRegionId =
                                    _objectModel.AddWeatherRegion(
                                        friendlyName,
                                        timeZone,
                                        regionLat,
                                        regionLong,
                                        weatherRegionWundergroundSubUrl).WeatherRegionID;


                                WeatherDataMiner weatherDataMiner = new WeatherDataMiner(
                                    wundergroundApiUrl,
                                    wundergroundApiKey,
                                    selfThrottlingMethod,
                                    this.DatabaseConnectionString,
                                    maxNumberOfCallsPerMinute);

                                switch (regionConfiguration.WeatherMiningRegion.MiningMethod)
                                {
                                    case "GPS":
                                        // Mine Recent Actual Data
                                        weatherDataMiner.MineHistoricWeatherValues(
                                            historicStartDateTime,
                                            historicEndDateTime,
                                            regionLat,
                                            regionLong,
                                            (int)weatherRegionId);

                                        // Mine Forecast Data
                                        weatherDataMiner.MineTenDayHourlyForecastWeatherValues(
                                            regionLat,
                                            regionLong,
                                            (int)weatherRegionId);
                                        break;

                                    case "WundergroundPageSubUrl":
                                    default:
                                        // Mine Recent Actual Data
                                        weatherDataMiner.MineHistoricWeatherValues(
                                            historicStartDateTime,
                                            historicEndDateTime,
                                            weatherRegionWundergroundSubUrl,
                                            (int)weatherRegionId);

                                        // Mine Forecast Data
                                        weatherDataMiner.MineTenDayHourlyForecastWeatherValues(
                                            weatherRegionWundergroundSubUrl,
                                            (int)weatherRegionId);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Logger.Information(
                                $"No Wunderground Api Key was specified. Skipping this region for Weather.",
                                "RunAsync()");
                        }
                    }
                }

                // Group the Emissions and Weather regions if they haven't already been grouped
                using (var _objectModel = new SmartEnergyOM(this.DatabaseConnectionString))
                {
                    _objectModel.AddMarketWeatherEmissionsRegionMapping(
                        regionGroupingName,
                        null,
                        weatherRegionId,
                        emissionsRegionId);
                }
            }
        }
    }
}
