#-------------------------------------------------------------------------------------
#This code is published under The MIT License(MIT). See LICENSE.TXT for details.
# Copyright(c) Microsoft and Contributors
#-------------------------------------------------------------------------------------

library(shiny)
library(leaflet)
library(shinydashboard)
library(dygraphs)
library(xts)
library(RODBC)
library(RColorBrewer)

# *** Usage Instructions: 
#   1) Deploy the SmartEnergyDatabase in this solution a SQL Azure Server in your Azure Subscription. 
#      If you don't have a SQL Azure Server yet, create one as per https://docs.microsoft.com/en-us/azure/sql-database/sql-database-get-started
#   2) Update the connectionString variable below to replace **MyAzureSQLDatabaseName** with the name of your SQL Azure Server, and fill in the database name, username and password
#   3) Run the solution locally with the "local" connection string uncommented. The solution will launch a ShinyApp application which connects to your SQL Azure database and visualises the data
#   4) To deploy the application to ShinyApps.io, uncomment the connection string commented as "Uncomment this line when deploying to Shiny" and publish. 
#   5) Tailor to your own needs and data. 

ui <- dashboardPage(
  dashboardHeader(title = "Smart Energy Dashboard"),
  dashboardSidebar(
    uiOutput("dropDownMenuSelections")
  ),
  dashboardBody(
  fluidRow(
    fluidRow(
      # The World Map
      leafletOutput("map"),
      # The Status Boxes
      infoBoxOutput("MarginalEmissionsBox"),
      infoBoxOutput("NumberOfActiveRegionsBox"),
      infoBoxOutput("DegreesCelciusBox")
    ),
    fluidRow(
        #The charts
        dygraphOutput("EmissionsDataSeriesChart", width = "98%")
        , dygraphOutput("WeatherDataSeriesChart", width = "98%")
    )
  ),
  p()
)
)

server <- function(input, output, session) {
    
    ##The Connection Stirngs: These need to be different depending on whether you are running the application locally, or deploying to ShinyApps. 
    #Uncomment this line when deploying to Shiny    
    #connectionString <- 'Driver=FreeTDS;TDS_Version=8.0;Server=**MyAzureSQLServerName**.database.windows.net,1433;Database=**MyAzureSQLDatabaseName**;Uid=**MyAzureSQLUserName**@**MyAzureSQLServerName**;Pwd=**MyAzureSQLPassword**;Encrypt=yes;'
    #Uncomment this line when running Locally
    connectionString <- 'Driver=SQL Server Native Client 11.0;Server=**MyAzureSQLServerName**.database.windows.net,1433;Database=**MyAzureSQLDatabaseName**;Uid=**MyAzureSQLUserName**@**MyAzureSQLServerName**;Pwd=**MyAzureSQLPassword**;Encrypt=yes;'
    conn <- odbcDriverConnect(connectionString)

    #Hardcoding some variables to demonstrate data binding from SQL Azure in R. Tailor to your own solution. See the sample SQL Azure database to see the data these relate to. 
    defaultRegionMappingID <- 5
    regionId <- defaultRegionMappingID
    EmissionsRegionId <- 5
    WeatherRegionId <- 1
    regionName <- "US_PJM"

    #Populate the drop down menu with regions available in the database
    RegionDropDownSqlQuery <- sprintf("SELECT [RegionMappingID],[FriendlyName],[MarketRegionID],[WeatherRegionID],[EmissionsRegionID] FROM [dbo].[MarketWeatherEmissionsRegionMapping] ORDER BY [RegionMappingID]")
    dfRegionDropDown <- sqlQuery(conn, RegionDropDownSqlQuery)
    dfRegionDropDownDatasetAsArray <- cbind(dfRegionDropDown)
    DropdownCoices = setNames(dfRegionDropDownDatasetAsArray$RegionMappingID, dfRegionDropDownDatasetAsArray$FriendlyName)
    output$dropDownMenuSelections <-
    renderUI({
        selectInput("DropDownSelectedRegionMappingID", "Select Region", choices = DropdownCoices)
    })    

    ##Generate the Map showing Emissions Regions on their location on the World Map, along with the current marginal emissions
    #Retrieve the Emissions Regions data form the SQL Azure databsae to display a map. This data contains Latitude and Longitude columns.     
    sqlQuery <- sprintf("select * from  [dbo].[MostRecentEmissionsDataPointForEachRegion]", regionId)
    df <- sqlQuery(conn, sqlQuery)
    emissionsRegions <- df
    emissionsRegions$emissionsRegionstatus <- sprintf("Region Name: %s. \n Current Marginal Emissions: %s gCO2 / kWh", emissionsRegions$FriendlyName, emissionsRegions$MarginalCO2Intensity_gCO2kWh)
    emissionsRegions
    numberOfemissionsRegions <- nrow(emissionsRegions)

    #Render the Customer geo-spacial data onto the Leaflet World Map
    output$map <- renderLeaflet({
        leaflet() %>%
        addTiles() %>%
        addMarkers(data = emissionsRegions, ~ Longitude, ~ Latitude, popup = ~emissionsRegionstatus)
    })
  

    ##Create the three infoBoxOutput "Status Boxes" - Reactive Select
    GetMarketWeatherEmissionsRegionMapping <- reactive({
        if (is.null(input$DropDownSelectedRegionMappingID)) {
            IdOfRegionInTheSelectedDropDown = defaultRegionMappingID
        } else {
            IdOfRegionInTheSelectedDropDown = as.numeric(input$DropDownSelectedRegionMappingID)
        }
  
        MarketWeatherEmissionsRegionMappingQuery <- sprintf("SELECT [MarketRegionID],[WeatherRegionID],[EmissionsRegionID] FROM [dbo].[MarketWeatherEmissionsRegionMapping] WHERE [RegionMappingID] = '%s'", IdOfRegionInTheSelectedDropDown)        
        conn <- odbcDriverConnect(connectionString)
        dfMarketWeatherEmissionsRegionMapping <- sqlQuery(conn, MarketWeatherEmissionsRegionMappingQuery)

        return(dfMarketWeatherEmissionsRegionMapping)
    })

    GetCurrentSelectedEmissionsRegionId <- reactive({
        dfMarketWeatherEmissionsRegionMapping = GetMarketWeatherEmissionsRegionMapping()
        IdOfSelectedRegionInTheDropDown = dfMarketWeatherEmissionsRegionMapping['EmissionsRegionID'] 
        id <- IdOfSelectedRegionInTheDropDown[1,1]
        return(id)
    })

    GetCurrentSelectedWeatherRegionId <- reactive({
        dfMarketWeatherEmissionsRegionMapping = GetMarketWeatherEmissionsRegionMapping()
        IdOfSelectedRegionInTheDropDown = dfMarketWeatherEmissionsRegionMapping['WeatherRegionID']
        id <- IdOfSelectedRegionInTheDropDown[1, 1]
        return(id)
    })

    GetCurrentSelectedmarketRegionId <- reactive({
        dfMarketWeatherEmissionsRegionMapping = GetMarketWeatherEmissionsRegionMapping()
        IdOfSelectedRegionInTheDropDown = dfMarketWeatherEmissionsRegionMapping['MarketRegionID']
        id <- IdOfSelectedRegionInTheDropDown[1, 1]
        return(id)
    })

    GetCurrentMarginalEmissionsForSelectedRegion <- reactive({
        CurrentSelectedEmissionsRegionId = GetCurrentSelectedEmissionsRegionId()
        sqlQuery <- sprintf("select * from  [dbo].[MostRecentEmissionsDataPointForEachRegion] WHERE [EmissionsRegionID] = '%d'", CurrentSelectedEmissionsRegionId)
        conn <- odbcDriverConnect(connectionString)
        df <- sqlQuery(conn, sqlQuery)
        CurrentmarginalEmissions <- df['MarginalCO2Intensity_gCO2kWh']
        return(CurrentmarginalEmissions)
    })

    GetCurrentWeatherForSelectedRegion <- reactive({
        CurrentSelectedWeatherRegionId = GetCurrentSelectedWeatherRegionId()
        sqlQuery <- sprintf("select * from  [dbo].[MostRecentWeatherDataPointForEachRegion] WHERE [WeatherRegionID] = '%d'", CurrentSelectedWeatherRegionId)
        conn <- odbcDriverConnect(connectionString)
        df <- sqlQuery(conn, sqlQuery)
        CurrentValue <- df['Temperature_Celcius']
        return(CurrentValue)
    })

    # Marginal CO2 Box
    output$MarginalEmissionsBox <- renderInfoBox({
        infoBox(
        "Marginal Emissions", paste0(GetCurrentMarginalEmissionsForSelectedRegion(), " gCO2 / kWh"), icon = icon("list"),
        color = "green", fill = TRUE
      )
    })

    # Number of Emissions Regions Box
    output$NumberOfActiveRegionsBox <- renderInfoBox({
        infoBox(
        "Number Of Active Regions", paste0(numberOfemissionsRegions), icon = icon("credit-card"),
        color = "purple", fill = TRUE
      )
    })

    # Current Temperature
    output$DegreesCelciusBox <- renderInfoBox({
        infoBox(
        "Current Temperature", paste0(GetCurrentWeatherForSelectedRegion(), " Degrees Celcius"), icon = icon("thumbs-up"),
        color = "yellow", fill = TRUE
      )
    })

    ##Generate the data charts displaying time series data
    #Time Series Price Data
    emissionsSqlQuery <- sprintf("SELECT TOP (1000) [DateTimeUTC],[SystemWideCO2Intensity_gCO2kWh],[MarginalCO2Intensity_gCO2kWh] FROM [dbo].[CarbonEmissionsDataPoints] WHERE [EmissionsRegionID] = '%d' ORDER BY [DateTimeUTC] DESC", EmissionsRegionId)
    dbResults <- sqlQuery(conn, emissionsSqlQuery)
    dataset <- dbResults
    datasetAsArray <- cbind(dataset)

    emissionsArray <- datasetAsArray[, c("DateTimeUTC", "SystemWideCO2Intensity_gCO2kWh", "MarginalCO2Intensity_gCO2kWh")]
    emissionsArrayAsXts <- xts(emissionsArray[, -1], order.by = emissionsArray[, 1])
    output$EmissionsDataSeriesChart <- renderDygraph({
    dygraph(emissionsArrayAsXts, main = "Marginal Carbon Emissions") %>%
        dySeries("SystemWideCO2Intensity_gCO2kWh", label = "SystemWideCO2Intensity_gCO2kWh", fillGraph = TRUE) %>%
        dySeries("MarginalCO2Intensity_gCO2kWh", label = "MarginalCO2Intensity_gCO2kWh", drawPoints = TRUE, strokePattern = "dashed") %>%
        dyOptions(stackedGraph = FALSE) %>%
    dyRangeSelector(height = 20)
})

    ##Time Series Weather Data
    weatherSqlQuery <- sprintf("SELECT TOP (1000) [DateTimeUTC],[Temperature_Celcius] ,[DewPoint_Metric],[WindSpeed_Metric] FROM [dbo].[WeatherDataPoints] WHERE [WeatherRegionID] = '%d' ORDER BY [DateTimeUTC] DESC", WeatherRegionId)
    dbweatherResults <- sqlQuery(conn, weatherSqlQuery)
    weatherDataset <- dbweatherResults
    weatherDatasetAsArray <- cbind(weatherDataset)

    weatherArray <- weatherDatasetAsArray[, c("DateTimeUTC", "Temperature_Celcius", "WindSpeed_Metric")]
    weatherArrayAsXts <- xts(weatherArray[, -1], order.by = weatherArray[, 1])
    output$WeatherDataSeriesChart <- renderDygraph({
    dygraph(weatherArrayAsXts, main = "Weather: Wind Speeds and Temperature") %>%
        dySeries("Temperature_Celcius", label = "Temperature (Celcius)", fillGraph = TRUE) %>%
        dySeries("WindSpeed_Metric", label = "WindSpeed Kmph", drawPoints = TRUE, strokePattern = "dashed") %>%
        dyOptions(stackedGraph = FALSE) %>%
        dyRangeSelector(height = 20)
    })

    close(conn) # Close the connection
}

shinyApp(ui, server)

