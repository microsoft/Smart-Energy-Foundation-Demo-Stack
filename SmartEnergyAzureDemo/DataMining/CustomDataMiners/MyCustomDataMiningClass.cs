namespace CustomDataMiners
{
    public class MyCustomDataMiningClass
    {
        /* 
         * Place code for any extra data mining jobs here to pull data from any source and send it to the database. To call it: 
         * (1) Write the method(s) to retrieve the data and insert it into the database here
         * (2) Add a job to the Azure Scheduler to run however often you want to mine this data
         * (3) Add a section to the switch statement in the RunAsync() method of DataMinerWorkerRole \ WorkerRole.cs to catch this new scheduler message text
         * (4) Inside that switch statement, call the data mining method here
         * From then onwards, each time the scheduler sends the message you created to the queue, your custom data mining code here will be run. 
         */
    }
}
