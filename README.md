# Carbon Emissions Data Platform
Optimising energy consumption based on the real-time Marginal Emissions of an electricity market can substantially reduce the consumer's Carbon Emissions.
This is a demonstration solution to show how data from several Web based APIs can be mined, visualised and acted upon in a Microsoft Azure solution. This solution collects real-time Carbon Emissions data from the WattTime API (https://api.watttime.org/), and global Weather data and weather forecasts from the Wunderground API (https://www.wunderground.com/). It then visualises this data over time to allow the user to understand the relationship between the two. It demonstrates the ability to collect related pieces of data into a single place to allow automation to act upon the conclusions extracted from it. For example, automating devices via the Azure IoT service to minimise net Carbon Emissions.
For more information, and the source code of the solution, see the project's GitHub page at https://github.com/Microsoft/Smart-Energy-Foundation-Demo-Stack .

# Problem and Solution Space
[Video](https://www.youtube.com/watch?v=5VjkwxCsWv4) on the background on this Solution.

# Azure Subscription
To deploy this solution, you'll need an Azure subscription. You can sign up for a free Azure subscription to deploy this solution to at https://azure.microsoft.com/en-us/free . 

# Prerequisites
To enable the solution to mine Carbon Emissions data, you will need to register for a free WattTime API Key, and enter it when deploying the solution. Register for a key at https://api.watttime.org/accounts/register/ . When registering, tick 'I would like request access to Pro features (e.g., marginal carbon emissions data)'">


# Components
This solution automatically provisions the required Azure infrastructure, and kicks off the data miner. The components created are: 
* A SQL Azure Server and Database
* An Azure Storage account
* An Azure Function to create the database tables and initial seeded data
* An Azure Function that runs every hour, mining carbon emissions data from the pre-selected markets
* A PowerBI Report showing your real-time electricity grid carbon emissions data, and whether now is a good, average or bad time to consume power in order to minimise carbon emissions. That dashboard will appear at https://functions-*MyFunctionNamespace*.azurewebsites.net/api/pbiweb 

# Customising the Solution
The code for this solution is available from the project's GitHub page at https://github.com/Microsoft/Smart-Energy-Foundation-Demo-Stack . From here, you can update the locations which the miner pulls in data for and republish the solution. You can also extend the solution as you see fit. For example, by pulling in additional data points which correlate with weather or Carbon Emissions in order to build Machine Learning Models that predict future values. 

# Configuring the Data Miner / Updating  the Regions Mined:
The DataMinerFunction reads where it should mine Weather and Emissions data from in the ApiDataMinerConfigs.xml file in the DataMinerWorkerRole project. The XML file contains a series of <Region> elements, comprised of a \<EmissionsMiningRegion\> and a \<WeatherMiningRegion\>. A Region element can have one or both. Configure the details  of a region as such: 
```xml
<ApiMinerConfigLayout>
  <Regions>
    <Region friendlyName="US_PJM">
      <EmissionsMiningRegion friendlyName="US_PJM">
        <EmissionsWattTimeAbbreviation>PJM</EmissionsWattTimeAbbreviation>
        <Latitude>40.348444276169</Latitude>
        <Longitude>-74.6428556442261</Longitude>
        <TimeZone>Eastern Standard Time</TimeZone>
        <ApiUrl>https://api.watttime.org/api/v1/</ApiUrl>
        <ApiKey>**MyWattTimeAPIKey**</ApiKey>
        <SelfThrottlingMethod>AzureTableStorageCallRecollection</SelfThrottlingMethod>
        <MaxNumberOfCallsPerMinute>200</MaxNumberOfCallsPerMinute>
      </EmissionsMiningRegion>

      <WeatherMiningRegion friendlyName="US_PJM">
        <weatherRegionWundergroundSubUrl>us/nj/princeton</weatherRegionWundergroundSubUrl>
        <Latitude>40.348444276169</Latitude>
        <Longitude>-74.6428556442261</Longitude>
        <MiningMethod>WundergroundPageSubUrl</MiningMethod>
        <TimeZone>Eastern Standard Time</TimeZone>
        <ApiUrl>http://api.wunderground.com/api/</ApiUrl>
        <ApiKey>**MyWundergroundAPIKey**</ApiKey>
        <SelfThrottlingMethod>AzureTableStorageCallRecollection</SelfThrottlingMethod>
        <MaxNumberOfCallsPerMinute>3</MaxNumberOfCallsPerMinute>
      </WeatherMiningRegion>
      </Region>
    </Regions>
</ApiMinerConfigLayout>
```
You can update the regions being mined in two places: 
A) Directly in the running Azure Function: 
	1. Opening the Resource group in the Azure Portal
	2. Hit the Azure Function and hit App Service Editor
	3. You'll see the ApiDataMinerConfigs.xml files listed in the ApiDataMinerConfigs folder under the wwroot
	4. You can update the content of the XML file and the updated contents will be picked up by the miner Azure Function the next time it runs. 

B) In the Visual Studio Solution: by updating the ApiDataMinerConfigs.xml XML file in the Azure Function project, before publishing the function to the Azure Function running on your Azure subscription

### GPS based weather data mining:
There are two options for mining weather data from the  Wunderground Service: 
	1. Using the suburl of the weather station you wise to mine (for example, us/nj/princeton to mine the data for Princeton New Jersey, located at https://www.wunderground.com/weather/us/nj/princeton on Wunderground). For this, set the <MiningMethod> attribute in to "WundergroundPageSubUrl" (as in \<MiningMethod\>WundergroundPageSubUrl\</MiningMethod\> )
	2. Based on GPS coordinates. In this case, the miner will first query Wunderground to locate the closest weather station to the GPS coordinates supplied, and then mine that weather station's weather data. There is the possibility that the weather station closest to the coordinates specified only records a subset of the full set of weather datapoints the major weather stations record. For this, set the <MiningMethod> attribute in to "GPS" (as in \<MiningMethod\>GPS\</MiningMethod\> )


# More Information
[The project's GitHub page](https://github.com/Microsoft/Smart-Energy-Foundation-Demo-Stack)
[WattTime.Org](http://watttime.org/)