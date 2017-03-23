// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace CentralLogger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CentralLogger.Helper;

    using Microsoft.Azure;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A simple class that logs telemetry messages to an Azure Table Storage table called "SystemLogs" in the Azure Storage account pointed to by the "AzureStorageConnectionString"
    /// setting of the calling executable.
    /// </summary>
    public class Logger
    {
        public const string AzureTableStorageTableName = "SystemLogs";

        public const string telemetryStorageConnectionStringSettingName = "AzureStorageConnectionString";

        public static void Debug(string message, string caller, string correlationId = null, Exception exception = null)
        {
            var logMessage = new LogMessage("Debug", message, caller, correlationId, exception);
            LogMessageToTableStorage(logMessage);
        }

        /// <summary>
        /// Log a message with a Level of "Information"
        /// </summary>
        /// <param name="message">The text of the message to log</param>
        /// <param name="caller">A string to identify the calling code</param>
        /// <param name="correlationId">An optional correlation Id to allow searching of several related messages</param>
        /// <param name="exception">An optional exception object to include in the logged message</param>
        public static void Information(
            string message,
            string caller,
            string correlationId = null,
            Exception exception = null)
        {
            var logMessage = new LogMessage("Information", message, caller, correlationId, exception);
            LogMessageToTableStorage(logMessage);
        }

        /// <summary>
        /// Log a message with a Level of "Warning"
        /// </summary>
        /// <param name="message">The text of the message to log</param>
        /// <param name="caller">A string to identify the calling code</param>
        /// <param name="correlationId">An optional correlation Id to allow searching of several related messages</param>
        /// <param name="exception">An optional exception object to include in the logged message</param>
        public static void Warning(
            string message,
            string caller,
            string correlationId = null,
            Exception exception = null)
        {
            var logMessage = new LogMessage("Warning", message, caller, correlationId, exception);
            LogMessageToTableStorage(logMessage);
        }

        /// <summary>
        /// Log a message with a Level of "Error"
        /// </summary>
        /// <param name="message">The text of the message to log</param>
        /// <param name="caller">A string to identify the calling code</param>
        /// <param name="correlationId">An optional correlation Id to allow searching of several related messages</param>
        /// <param name="exception">An optional exception object to include in the logged message</param>
        public static void Error(string message, string caller, string correlationId = null, Exception exception = null)
        {
            var logMessage = new LogMessage("Error", message, caller, correlationId, exception);
            LogMessageToTableStorage(logMessage);
        }

        /// <summary>
        /// Log a message with a Level of "Fatal"
        /// </summary>
        /// <param name="message">The text of the message to log</param>
        /// <param name="caller">A string to identify the calling code</param>
        /// <param name="correlationId">An optional correlation Id to allow searching of several related messages</param>
        /// <param name="exception">An optional exception object to include in the logged message</param>
        public static void Fatal(string message, string caller, string correlationId = null, Exception exception = null)
        {
            var logMessage = new LogMessage("Fatal", message, caller, correlationId, exception);
            LogMessageToTableStorage(logMessage);
        }

        /// <summary>
        /// Store the given LogMessage object an Azure Table Storage table called "SystemLogs" in the Azure Storage account pointed to by the "AzureStorageConnectionString"
        /// </summary>
        /// <param name="logMessage">Message object to log</param>
        public static void LogMessageToTableStorage(LogMessage logMessage)
        {
            try
            {
                // Convert message to Azure Table Entity
                var logAzureTableEntity = new LogAzureTableEntity(logMessage);

                var connectionString = CloudConfigurationManager.GetSetting(telemetryStorageConnectionStringSettingName);
                var table = AzureTablesHelper.GetAzureTablesTableObject(connectionString, AzureTableStorageTableName);
                table.CreateIfNotExists();

                // Create the TableOperation object that inserts the entity.
                var insertOperation = TableOperation.Insert(logAzureTableEntity);

                // Execute the insert operation.
                table.Execute(insertOperation);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        /// <summary>
        /// Retrieve all messages logged by the given caller. Pass in optional start and end time params. 
        /// </summary>
        /// <param name="caller">caller</param>
        /// <param name="dateFrom">Start datetime of search</param>
        /// <param name="dateTo">End datetime of search</param>
        /// <returns>All messages logged by the given caller, between optional start and end time params.</returns>
        public static IEnumerable<DynamicTableEntity> RetrieveLogMessagesFromTableStorage(
            string caller,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            try
            {
                var queryDictionary = new Dictionary<string, string> { { "Caller", caller } };
                return RetrieveLogMessagesFromTableStorage(queryDictionary, dateFrom, dateTo);
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        /// <summary>
        /// Retrieve all messages logged matching the filter param dictionary given as (Column Name, Coumn Value). Pass in optional start and end time params. 
        /// </summary>
        /// <param name="queryDictionary">Dictionary of (Column Name, Coumn Value) to run query for</param>
        /// <param name="dateFrom">dateFrom</param>
        /// <param name="dateTo">dateFrom</param>
        /// <returns>All messages logged by the given caller, between optional start and end time params.</returns>
        public static IEnumerable<DynamicTableEntity> RetrieveLogMessagesFromTableStorage(
            Dictionary<string, string> queryDictionary,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            try
            {
                var filter = AzureTablesHelper.CreateTableStorageQueryFilter<DynamicTableEntity>(queryDictionary);

                if (dateFrom != null)
                {
                    if (dateTo == null)
                    {
                        dateTo = DateTime.UtcNow;
                    }
                    filter = AzureTablesHelper.AppendOptionalTimeBasedFilters(
                        dateFrom.Value.AddSeconds(-1),
                        dateTo.Value.AddSeconds(1),
                        filter);
                }

                var entities = ExecuteAzureTableStorageQuery(filter);

                return entities;
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        /// <summary>
        /// Retrieve the Operation Running Time as a TimeSpan from the last logging statement in the Azure Storage Account with the given correlationId
        /// </summary>
        /// <param name="correlationId">correlationId of the Timed Operation Logging statement to retrieve</param>
        /// <returns>The Operation Running Time as a TimeSpan from the last logging statement in the Azure Storage Account with the given correlationId</returns>
        public static TimeSpan? RetrieveRunningTimeOfOperation(string correlationId)
        {
            try
            {
                var queryDictionary = new Dictionary<string, string> { { "CorrelationId", correlationId } };
                var retrievedEntries = Logger.RetrieveLogMessagesFromTableStorage(queryDictionary);
                var timedOperationRunningTimeLogMessage =
                    retrievedEntries.OrderByDescending(x => x.Properties["OperationTimeElapsed"].StringValue.ToString())
                        .FirstOrDefault();
                TimeSpan runningTime;
                TimeSpan.TryParse(
                    timedOperationRunningTimeLogMessage.Properties["OperationTimeElapsed"].StringValue,
                    out runningTime);
                return runningTime;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Execute an Azure Table Query Filter filter against the Azure Storage account and Table name specified in the settings file of the calling executable
        /// </summary>
        /// <param name="filter">Azure Table Query Filter</param>
        /// <returns>Entities matching the query in the target table</returns>
        private static List<DynamicTableEntity> ExecuteAzureTableStorageQuery(string filter)
        {
            var tableQuery = new TableQuery<DynamicTableEntity>().Where(filter);
            var table =
                AzureTablesHelper.GetAzureTablesTableObject(
                    CloudConfigurationManager.GetSetting(telemetryStorageConnectionStringSettingName),
                    AzureTableStorageTableName);

            TableContinuationToken token = null;
            var entities = new List<DynamicTableEntity>();
            do
            {
                var queryResult = table.ExecuteQuerySegmented(tableQuery, token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            }
            while (token != null);
            return entities;
        }

        /// <summary>
        /// The log azure table entity.
        /// </summary>
        public class LogAzureTableEntity : TableEntity
        {
            public LogAzureTableEntity(LogMessage logMessage)
            {
                this.PartitionKey = logMessage.caller;
                this.RowKey = $"{DateTime.UtcNow.Ticks}-{Guid.NewGuid()}";
                this.Exception = logMessage.exception != null ? logMessage.exception.ToString() : null;
                this.Message = logMessage.message;
                this.Level = logMessage.level;
                this.Caller = logMessage.caller;
                this.CorrelationId = logMessage.correlationId;
                this.PropertyValues = logMessage.propertyValues;
                this.OperationTimeElapsed = logMessage.operationTimeElapsed.ToString();
            }

            public string Level { get; set; }

            public string Message { get; set; }

            public string Caller { get; set; }

            public string CorrelationId { get; set; }

            public string Exception { get; set; }

            public string OperationTimeElapsed { get; set; }

            public List<Tuple<string, object>> PropertyValues { get; set; }
        }
    }
}

