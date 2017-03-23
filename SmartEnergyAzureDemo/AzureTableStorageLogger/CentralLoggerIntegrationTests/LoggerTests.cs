// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace CentralLoggerIntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using CentralLogger;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void TestLoggingToAzureTableStorage()
        {
            //Arrange
            DateTime beforeLoggingDateTime = DateTime.UtcNow;
            var messageToLog = "Test Message";
            var caller = "LoggerTests.TestLoggingToAzureTableStorage";
            var correlationId = Guid.NewGuid().ToString();
            Exception testException = new Exception("Test Outer exception From Test Method");

            //Act
            Logger.Error(messageToLog, caller, correlationId, testException);

            //Assert
            DateTime afteroggingDateTime = DateTime.UtcNow;
            var retrievedEntries = Logger.RetrieveLogMessagesFromTableStorage(caller, beforeLoggingDateTime, afteroggingDateTime);

            Assert.IsTrue(retrievedEntries.Any());

        }

        [TestMethod]
        public void TestLoggingToAzureTableStorageForCorrelationId()
        {
            //Arrange
            var messageToLog = "Test Message Testing CorrelationId";
            var caller = "LoggerTests.TestLoggingToAzureTableStorageForCorrelationId";
            var correlationId = Guid.NewGuid().ToString();

            //Act
            Logger.Warning(messageToLog, caller, correlationId, exception: null);

            //Assert
            var queryDictionary = new Dictionary<string, string> { { "CorrelationId", correlationId } };
            var retrievedEntries = Logger.RetrieveLogMessagesFromTableStorage(queryDictionary);

            Assert.IsTrue(retrievedEntries.Any());
            Assert.IsTrue(retrievedEntries.FirstOrDefault().Properties["CorrelationId"].StringValue == correlationId);
        }

        [TestMethod]
        public void TestLoggingToAzureTableStorageAllLoggingLevels_DemonstratesUsage()
        {
            //Arrange
            var correlationId = Guid.NewGuid().ToString();

            //Act
            Logger.Information("Logging Message", "Name of Project and Method", correlationId, null);
            Logger.Debug("Logging Message", "Name of Project and Method", correlationId, null);
            Logger.Warning("Logging Message", "Name of Project and Method", correlationId, null);
            Logger.Error("Logging Message", "Name of Project and Method", correlationId, new Exception("Pass in an optional exception here"));
            Logger.Fatal("Logging Message", "Name of Project and Method", correlationId, new Exception("Pass in an optional exception here"));
            
            //Assert
            var queryDictionary = new Dictionary<string, string> { { "CorrelationId", correlationId } };
            var retrievedEntries = Logger.RetrieveLogMessagesFromTableStorage(queryDictionary);
            Assert.IsTrue(retrievedEntries.Count() == 5);
        }

        [TestMethod]
        public void TestLoggingToAzureTableStorageTimedOperationObject()
        {
            //Arrange
            var messageToLog = "Test Timing Message";
            var caller = "LoggerTests.TestLoggingToAzureTableStorageTimedOperationObject";
            var correlationId = Guid.NewGuid().ToString();

            //Act
            using (var timedOperation = new TimedOperation(messageToLog, caller, correlationId))
            {
                //Wrap any piece of code in a timing block to log it's start time, end time and running duration
                Thread.Sleep(2000); /* This represents the code being timed */
            }

            //Assert
            var queryDictionary = new Dictionary<string, string> { { "CorrelationId", correlationId } };
            var retrievedEntries = Logger.RetrieveLogMessagesFromTableStorage(queryDictionary);
            Assert.IsTrue(retrievedEntries.Count() == 2);

            //Verify the logged time was greater than the time we slept for above
            var runningTime = Logger.RetrieveRunningTimeOfOperation(correlationId);
            Assert.IsTrue(runningTime > new TimeSpan(0, 0, 2));
        }

        [TestMethod]
        public void TestRetrieveRunningTimeOfOperation_WhereNoLoggingStatementExists()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();

            // Act - request a timed operation which hasn't been logged. We should get NULL back.
            var runningTime = Logger.RetrieveRunningTimeOfOperation(correlationId);

            // Assert
            Assert.IsNull(runningTime);
        }
    }
}
