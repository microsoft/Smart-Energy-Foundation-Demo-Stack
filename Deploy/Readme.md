# Deployment Instructions
To deploy this solution:
1. Clone this repo.
2. Open powershell and navigate to the root of the Deploy folder.
3. Run Main.ps1. You will be prompted for required inputs.
NOTE -  By default, the script will use certain default parameters (for example for sql username and password). If you wish to change any of these values or the script's behavior, open Main.ps1 and Deploy.ps1. The comments should be helpful in explaining how the script functions.

# Post-Deployment Instructions

## Success! Your solution has been successfully deployed and real-time Carbon Emissions data is now flowing in. 

**Power BI dashboard**

* You can download a sample Power BI desktop **.pbix** file [**here**](https://github.com/bazzdg/Smart-Energy-Foundation-Demo-Stack/tree/master/Deploy/PowerBiDashboards/SmartEnergyDashboardDirectQuery.pbix?raw=true). (*Edge might change the extension to .zip*)
* Instructions on how to connect to data source in Power BI desktop is available [**here**](https://github.com/Azure/Azure-CloudIntelligence-SolutionAuthoringWorkspace/blob/master/docs/powerbi-configurations.md).

The AzureFunction that mines emissions and weather data is scheduled to run every 30 minutes, so it may take up to 30 minutes for the data to appear in the dashboard.


**You can connect to your Azure SQL Database with the outputs the were printed at the end of the powershell deployment script:**

* ***Server***: _{Outputs.sqlServer}.database.windows.net_
* ***Database***: _{Outputs.sqlDatabase}_
* ***Username***: _{Outputs.sqlServerUsername}_
* ***Password***: _\<Password provided at provision time\>_

Feel free to customise and extend the solution (for example, automate your devices to run at times of low Emissions) with the code from **https://github.com/Microsoft/Smart-Energy-Foundation-Demo-Stack**
