// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLogger
{
    /// <summary>
    /// A class which logs the time it was created and disposed to allow the timing of any block of code
    /// </summary>
    public class TimedOperation : IDisposable
    {
        private readonly string _correlationId;
        private readonly DateTime _startDateTime;
        private readonly string _message;
        private readonly string _caller;
        private readonly Exception _exception;
        private DateTime _endDateTime;

        private readonly List<Tuple<string, object>> _propertyValues;

        /// <summary>
        /// Create an instance of TimedOperation, which will log it's start time, and the time it was disposed of. 
        /// </summary>
        /// <param name="Level">Level of the Log Message. E.g. "Inforation", "Error", etc.</param>
        /// <param name="Message">The message to log</param>
        /// <param name="Caller">The identifier of the calling code</param>
        /// <param name="CorrelationId">Optional CorrelationId to group related log messages. Often a Guid. </param>
        /// <param name="exception">Optional exception</param>
        /// <param name="PropertyValues">Optional List of key value pairs representing properties to log</param>
        /// <param name="exception"></param>
        public TimedOperation(string message, string caller, string correlationId = null, Exception exception = null, List<Tuple<string, object>> PropertyValues = null)
        {
            this._caller = caller;
            this._exception = exception;
            this._message = message;
            this._message = message;
            this._propertyValues = PropertyValues;
            this._correlationId = string.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId;

            this._startDateTime = DateTime.UtcNow;
            var loggingMessage = $"Starting Timed Operation: {message}";
            var logMessage = new LogMessage("TimedOperation", loggingMessage, caller, this._correlationId, this._exception, this._propertyValues);
            Logger.LogMessageToTableStorage(logMessage);
        }

        public void Dispose()
        {
            this._endDateTime = DateTime.UtcNow;
            var timeDeltaSinceOperationStart = this._endDateTime - this._startDateTime;
            var loggingMessage = $"Timed Operation Completed in {timeDeltaSinceOperationStart} for {this._message}";
            var logMessage = new LogMessage("TimedOperation", loggingMessage, this._caller, this._correlationId, this._exception, this._propertyValues, timeDeltaSinceOperationStart);
            Logger.LogMessageToTableStorage(logMessage);
        }
    }
}


