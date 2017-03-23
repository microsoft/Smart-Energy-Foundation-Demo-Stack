// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace DataMinerWorkerRole.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// A class with allows interaction with queues on Azure Storage
    /// </summary>
    class AzureStorageQueueManager
    {
        private readonly CloudQueue _queue;

        /// <summary>
        /// Connects to a storage queue, requires a StorageConnectionString and a queue name
        /// </summary>
        /// <param name="storageConnectionString">Connection string of the Azure Storage account</param>
        /// <param name="queueToListenOn">Name of the queue to listen to</param>
        public AzureStorageQueueManager(string storageConnectionString, string queueToListenOn)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            this._queue = queueClient.GetQueueReference(queueToListenOn);

            try
            {
                this._queue.CreateIfNotExists();
            }
            catch (Exception e)
            {
                throw new Exception("Queue names must be lowercase and contain only alphabetical and numeric characters. Exception: ", e);
            }
        }

        /// <summary>
        /// Deletes all messages in the queue
        /// </summary>
        public void CleanQueue()
        {
            this._queue.Clear();
        }

        /// <summary>
        /// Adds a message to the queue
        /// </summary>
        /// <param name="message"></param>
        public void AddAMessage(string message)
        {
            this._queue.AddMessage(new CloudQueueMessage(message));
        }

        /// <summary>
        /// Reads the newest message in the queue and removes it 
        /// </summary>
        /// <returns></returns>
        public CloudQueueMessage GetMessage()
        {
            var newMessage = this._queue.GetMessage();

            if (newMessage != null)
            {
                this._queue.DeleteMessage(newMessage);
            }

            return newMessage;
        }

        /// <summary>
        /// Reads a specific amount of messages up to specific age (in mintues), removes them from the storage queue and returns them in a List
        /// </summary>
        /// <param name="howMany"></param>
        /// <param name="howOld"></param>
        /// <returns></returns>
        public IEnumerable<CloudQueueMessage> GetMessages(int howMany, int howOld)
        {
            var messages = new List<CloudQueueMessage>();

            try
            {
                foreach (var cloudQueueMessage in this._queue.GetMessages(howMany, TimeSpan.FromMinutes(howOld)))
                {
                    messages.Add(cloudQueueMessage);
                    this._queue.DeleteMessage(cloudQueueMessage);
                }
            }
            catch
            {
                throw new Exception($"Could not find {howMany} messages less than {howOld} minutes old");
            }

            return messages;
        }

        /// <summary>
        /// Take an Azure Scheduler message content and extra the text contained in the <Message></Message> node. 
        /// </summary>
        /// <param name="azureSchedulerMessage">The content of the Azure Scheduler message</param>
        /// <returns>The text contained in the <Message></Message> node. </returns>
        public static string ExtractMessageComponentFromSchedulerMessage(string azureSchedulerMessage)
        {
            const string match = @"<Message>(?<MessageContent>.*?)</Message>";
            var r = new Regex(match, RegexOptions.IgnoreCase);
            var results = r.Matches(azureSchedulerMessage);

            return !string.IsNullOrEmpty(results[0].Groups["MessageContent"].Value) ? results[0].Groups["MessageContent"].Value : null;
        }
    }
}
