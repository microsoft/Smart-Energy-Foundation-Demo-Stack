// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace DataMinerWorkerRole
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using ApiDataMiners;

    using CentralLogger;

    using DataMinerWorkerRole.Helper;

    using Microsoft.Azure;
    using Microsoft.WindowsAzure.ServiceRuntime;

    /// <summary>
    /// All data mining is kicked off from here. This worker role reads the configuration file which tells it what to mine, and it mines that data. 
    /// </summary>
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            Logger.Information("Data Miner Worker Role is Starting", "RunAsync()");

            var databaseConnectionString = CloudConfigurationManager.GetSetting("SQLAzureDatabaseEntityFrameworkConnectionString");
            var azureStorageConnectionString = CloudConfigurationManager.GetSetting("AzureStorageConnectionString");
            var schedulerQueueName = CloudConfigurationManager.GetSetting("SchedulerQueueName");

            var storageQueueManager = new AzureStorageQueueManager(azureStorageConnectionString, schedulerQueueName);

            while (!cancellationToken.IsCancellationRequested)
            {
                // Check if there are any messages on the queue to instruct the data miner what to do
                var message = storageQueueManager.GetMessage();
                if (message != null)
                {
                    // Instruction to run the data mining for the local config file
                    if (message.AsString.Contains("RunMinerForLocalConfigFile"))
                    {
                        // Parse config XML File defining regions, emissions regions, weather regions, etc.
                        using (
                            new TimedOperation(
                                "Beginning Mining of data pointed to by the configuration file",
                                "DataMinerWorkerRole.FullMinerCall"))
                        {
                            try
                            {
                                const string ConfigPath = @".\ApiDataMinerConfigs\ApiDataMinerConfigs.xml";
                                var apiDataMiner = new ApiDataMiner(databaseConnectionString);
                                apiDataMiner.ParseMinerSettingsFileAndMineData(ConfigPath);
                            }
                            catch (Exception exception)
                            {
                                Logger.Error(
                                    "DataMinerWorkerRole encountered an exception mining all data pointed to by the configuration file",
                                    "DataMinerWorkerRole.FullMinerCall",
                                    null,
                                    exception);
                            }
                        }
                    }

                    /* Add logic for any other mining here, and add a corresponding job to the Scheduler to send messages to the queue to kick it off */
                   
                }

                // Sleep for a time before checking the queue again
                var numberOfMinutesToSleep = 1;
                Logger.Information($"Sleping for {numberOfMinutesToSleep} minutes", "RunAsync()");
                await Task.Delay(1000 * 60 * numberOfMinutesToSleep, cancellationToken);
            }
        }

        public override void Run()
        {
            Logger.Information("DataMinerWorkerRole is running", "Run()");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Logger.Information("DataMinerWorkerRole has been started", "OnStart()");

            return result;
        }

        public override void OnStop()
        {
            Logger.Information("DataMinerWorkerRole is stopping", "OnStop()");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Logger.Information("DataMinerWorkerRole has stopped", "OnStop()");
        }
    }
}
