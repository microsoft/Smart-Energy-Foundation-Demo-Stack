// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace CentralLogger
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class to hold details of a message to be logged to the underlying logging system
    /// </summary>
    public class LogMessage
    {
        public string level;

        public string message;

        public string caller;

        public string correlationId;

        public Exception exception;

        public List<Tuple<string,object>> propertyValues;

        public TimeSpan? operationTimeElapsed;

        public LogMessage()
        { }

        /// <summary>
        /// Declare an object of type LogMessage
        /// </summary>
        /// <param name="Level">Level of the Log Message. E.g. "Inforation", "Error", etc.</param>
        /// <param name="Message">The message to log</param>
        /// <param name="Caller">The identifier of the calling code</param>
        /// <param name="CorrelationId">Optional CorrelationId to group related log messages. Often a Guid. </param>
        /// <param name="Exception">Optional Exception</param>
        /// <param name="PropertyValues">Optional List of key value pairs representing properties to log</param>
        /// <param name="OperationTimeElapsed">Optional TimeSpan representing the running time of a block of code</param>
        public LogMessage(string Level, string Message, string Caller, string CorrelationId = null, Exception Exception = null, List<Tuple<string, object>> PropertyValues = null, TimeSpan ? OperationTimeElapsed = null)
        {
            this.level = Level;
            this.message = Message;
            this.caller = Caller;
            this.correlationId = CorrelationId;
            this.exception = Exception;
            this.propertyValues = PropertyValues;
            this.operationTimeElapsed = OperationTimeElapsed;
        }
    }
}