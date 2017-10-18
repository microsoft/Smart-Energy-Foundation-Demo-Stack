// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.IO;

using ApiDataMiners;
using CentralLogger;

namespace DataMinerFunction
{
    public static class RunMinerForLocalConfigFile
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
                Logger.Error($"RunMinerForLocalConfigFile: Run(): Exception encountered parsing and mining for miner configuration file {ApiDataMinerConfigFilePath}", "RunMinerForLocalConfigFile.Run()", exception:e);
                throw;
            }
        }

        private static void ParseLocalConfigAndAndMineData(string apiDataMinerConfigFileLocation)
        {
            var databaseConnectionString = ConfigurationManager.AppSettings["SQLAzureDatabaseEntityFrameworkConnectionString"];
            var wattTimeApiKey = ConfigurationManager.AppSettings["wattTimeApiKey"];
            var wundergroundApiKey = ConfigurationManager.AppSettings["wundergroundApiKey"];
            var apiDataMiner = new ApiDataMiner(databaseConnectionString);
            apiDataMiner.ParseMinerSettingsFileAndMineData(apiDataMinerConfigFileLocation, wattTimeApiKey, wundergroundApiKey);
        }
    }
}
