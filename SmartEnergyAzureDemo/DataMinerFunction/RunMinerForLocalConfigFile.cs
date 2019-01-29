// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Configuration;
using System.IO;
using ApiDataMiners;
using CentralLogger;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace DataMinerFunction
{
    public static class RunMinerForLocalConfigFile1
    {
        [FunctionName("RunMinerForLocalConfigFile")]
        public static void Run([TimerTrigger("0 */30 * * * *")]TimerInfo myTimer, TraceWriter log, ExecutionContext executionContext)
        {
            const string ApiDataMinerConfigFileLocation = "ApiDataMinerConfigs\\ApiDataMinerConfigs.xml";
            
            // Get Function's Main directory, and the parent of that to access the Configuration file published with the Project
            var directory = executionContext.FunctionDirectory;
            var dirInfo = new DirectoryInfo(directory);

            // Get the Parent of this directory
            var parent = dirInfo.Parent;
            var ApiDataMinerConfigFilePath = $"{parent.FullName}\\{ApiDataMinerConfigFileLocation}";
            try
            {
                using (new TimedOperation($"Beginning Mining of all data in Config File from DataMinerAzureFunctionApp",
                                             "DataMinerAzureFunctionApp.Run()"))
                {
                    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
                    ParseLocalConfigAndAndMineData(ApiDataMinerConfigFilePath);
                }
            }
            catch (Exception e)
            {
                log.Error($"RunMinerForLocalConfigFile: Run(): Exception encountered parsing and mining for miner configuration file {ApiDataMinerConfigFilePath}, {e}", e);
                throw;
            }
        }

        private static void ParseLocalConfigAndAndMineData(string apiDataMinerConfigFileLocation)
        {
            var databaseConnectionString = ConfigurationManager.AppSettings["SQLAzureDatabaseEntityFrameworkConnectionString"];
            var wattTimeApiKey = ConfigurationManager.AppSettings["wattTimeApiKey"];
            var wundergroundApiKey = ConfigurationManager.AppSettings["wundergroundApiKey"];
            var wattTimeApiV2Url = ConfigurationManager.AppSettings["WattTimeApiV2Url"];
            var wattTimeUsername = ConfigurationManager.AppSettings["WattTimeUsername"];
            var wattTimePassword = ConfigurationManager.AppSettings["WattTimePassword"];
            var wattTimeEmail = ConfigurationManager.AppSettings["WattTimeEmail"];
            var wattTimeOrganization = ConfigurationManager.AppSettings["WattTimeOrganization"];
            var darkSkyApiUrl = ConfigurationManager.AppSettings["DarkSkyApiUrl"];
            var darkSkyApiKey = ConfigurationManager.AppSettings["DarkSkyApiKey"];
            var apiDataMiner = new ApiDataMiner(databaseConnectionString);

            apiDataMiner.ParseMinerSettingsFileAndMineData(apiDataMinerConfigFileLocation, wattTimeApiKey, wundergroundApiKey, darkSkyApiKey, wattTimeUsername, wattTimePassword, wattTimeEmail,
                        wattTimeOrganization);
        }
    }
}