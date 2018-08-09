	BEGIN
	CREATE TABLE [dbo].[EmissionsRegion](
		[EmissionsRegionID] [int] IDENTITY(0,1) NOT NULL,
		[FriendlyName] [nvarchar](max) NOT NULL,
		[TimeZoneUTCRelative] [datetimeoffset](7) NOT NULL,
		[Latitude] [float] NULL,
		[Longitude] [float] NULL,
		[EmissionsRegionWattTimeSubUrl] [nvarchar](max) NULL,
	 CONSTRAINT [PK_EmissionsRegion] PRIMARY KEY CLUSTERED 
	(
		[EmissionsRegionID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)
	END
	GO
	
	INSERT INTO [dbo].[EmissionsRegion] VALUES
	('US_PJM','2017-10-05 16:18:32.4821301 -04:00','40.348444276169','-74.6428556442261','PJM'),
	('US_CAISO','2017-10-05 13:42:31.2529296 -07:00','41.7324','-123.409423','CAISO'),
	('US_UpperMidwestISO','2017-10-05 16:25:08.0515083 -05:00','41.9185326985726','-93.5519313787257','MISO')
;
GO
	
	BEGIN
	CREATE TABLE [dbo].[CarbonEmissionsDataPoints](
		[EmissionsRegionID] [int] NOT NULL,
		[DateTimeUTC] [datetime] NOT NULL,
		[SystemWideCO2Intensity_gCO2kWh] [float] NULL,
		[SystemWideCO2Intensity_Forcast_gCO2kWh] [float] NULL,
		[MarginalCO2Intensity_gCO2kWh] [float] NULL,
		[MarginalCO2Intensity_Forcast_gCO2kWh] [float] NULL,
	 CONSTRAINT [PK_CarbonEmissionsDataPoints] PRIMARY KEY CLUSTERED 
	(
		[EmissionsRegionID] ASC,
		[DateTimeUTC] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)
	END
	GO
	
	ALTER TABLE [dbo].[CarbonEmissionsDataPoints]  WITH CHECK ADD  CONSTRAINT [FK_CarbonEmissionsDataPoints_EmissionsRegionID] FOREIGN KEY([EmissionsRegionID])
	REFERENCES [dbo].[EmissionsRegion] ([EmissionsRegionID])
	GO
	
	ALTER TABLE [dbo].[CarbonEmissionsDataPoints] CHECK CONSTRAINT [FK_CarbonEmissionsDataPoints_EmissionsRegionID]
	GO
	
	INSERT INTO [dbo].[CarbonEmissionsDataPoints] VALUES
	('0',dateadd(hour, -1, getdate()),'500',NULL,'750',NULL),
	('1',dateadd(hour, -1, getdate()),'400',NULL,'220',NULL),
	('2',dateadd(hour, -1, getdate()),'550',NULL,'810',NULL)
;
GO

	BEGIN
	CREATE TABLE [dbo].[CarbonEmissionsRelativeMeritDataPoints] (
		[EmissionsRegionID]              [int]        NOT NULL,
		[DateTimeUTC]                    [datetime]   NOT NULL,
		[EmissionsRelativeMerit]         [float]  NULL,
		[EmissionsRelativeMerit_Forcast] [float]  NULL,
		CONSTRAINT [PK_GridEmissionsRelativeMeritDataPoints] PRIMARY KEY CLUSTERED ([EmissionsRegionID] ASC, [DateTimeUTC] ASC),
		CONSTRAINT [FK_CarbonEmissionsRelativeMeritDataPoints_EmissionsRegionID] FOREIGN KEY ([EmissionsRegionID]) REFERENCES [dbo].[EmissionsRegion] ([EmissionsRegionID])
	)
	END
	GO

	
	BEGIN
	CREATE TABLE [dbo].[MarketRegion](
		[MarketRegionID] [int] IDENTITY(0,1) NOT NULL,
		[FriendlyName] [nvarchar](max) NOT NULL,
		[TimeZoneUTCRelative] [datetimeoffset](7) NOT NULL,
		[Latitude] [float] NULL,
		[Longitude] [float] NULL,
		[CurrencyName] [nvarchar](50) NULL,
		[CurrencyValuePerUSD] [float] NULL,
	 CONSTRAINT [PK_MarketRegion] PRIMARY KEY CLUSTERED 
	(	[MarketRegionID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)
	END
	GO
	
	BEGIN
	CREATE TABLE [dbo].[WeatherRegion](
		[WeatherRegionID] [int] IDENTITY(0,1) NOT NULL,
		[FriendlyName] [nvarchar](max) NOT NULL,
		[TimeZoneUTCRelative] [datetimeoffset](7) NOT NULL,
		[Latitude] [float] NULL,
		[Longitude] [float] NULL,
		[WeatherRegionWundergroundSubUrl] [nvarchar](max) NULL,
	 CONSTRAINT [PK_WeatherRegion] PRIMARY KEY CLUSTERED 
	(
		[WeatherRegionID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)
	END
	GO
	
	BEGIN
	CREATE TABLE [dbo].[MarketWeatherEmissionsRegionMapping](
		[RegionMappingID] [int] IDENTITY(0,1) NOT NULL,
		[FriendlyName] [nvarchar](max) NOT NULL,
		[MarketRegionID] [int] NULL,
		[WeatherRegionID] [int] NULL,
		[EmissionsRegionID] [int] NULL,
	 CONSTRAINT [PK_MarketWeatherEmissionsRegionMapping] PRIMARY KEY CLUSTERED 
	(
		[RegionMappingID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)
	END
	GO
	
	ALTER TABLE [dbo].[MarketWeatherEmissionsRegionMapping]  WITH CHECK ADD  CONSTRAINT [FK_MarketWeatherEmissionsRegionMapping_EmissionsRegionID] FOREIGN KEY([EmissionsRegionID])
	REFERENCES [dbo].[EmissionsRegion] ([EmissionsRegionID])
	GO
	
	ALTER TABLE [dbo].[MarketWeatherEmissionsRegionMapping] CHECK CONSTRAINT [FK_MarketWeatherEmissionsRegionMapping_EmissionsRegionID]
	GO
	
	ALTER TABLE [dbo].[MarketWeatherEmissionsRegionMapping]  WITH CHECK ADD  CONSTRAINT [FK_MarketWeatherEmissionsRegionMapping_MarketRegionID] FOREIGN KEY([MarketRegionID])
	REFERENCES [dbo].[MarketRegion] ([MarketRegionID])
	GO
	
	ALTER TABLE [dbo].[MarketWeatherEmissionsRegionMapping] CHECK CONSTRAINT [FK_MarketWeatherEmissionsRegionMapping_MarketRegionID]
	GO
	
	ALTER TABLE [dbo].[MarketWeatherEmissionsRegionMapping]  WITH CHECK ADD  CONSTRAINT [FK_MarketWeatherEmissionsRegionMapping_WeatherRegionID] FOREIGN KEY([WeatherRegionID])
	REFERENCES [dbo].[WeatherRegion] ([WeatherRegionID])
	GO
	
	ALTER TABLE [dbo].[MarketWeatherEmissionsRegionMapping] CHECK CONSTRAINT [FK_MarketWeatherEmissionsRegionMapping_WeatherRegionID]
	GO
	
	BEGIN
	CREATE TABLE [dbo].[MarketDataPoints](
		[MarketRegionID] [int] NOT NULL,
		[DateTimeUTC] [datetime] NOT NULL,
		[Price] [float] NULL,
		[DemandMW] [float] NULL,
		[RenewablesMW] [float] NULL,
		[RenewablesPercentage] [float] NULL,
		[WindMW] [float] NULL,
		[WindPercentage] [float] NULL,
		[SolarMW] [float] NULL,
		[SolarPercentage] [float] NULL,
		[CarbonPricePerKg] [float] NULL,
		[IsForcastRow] [bit] NOT NULL,
	 CONSTRAINT [PK_CurrentRegionsMarketData] PRIMARY KEY CLUSTERED 
	(
		[MarketRegionID] ASC,
		[DateTimeUTC] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)
	END
	GO
	
	BEGIN
	CREATE TABLE [dbo].[WeatherDataPoints](
		[WeatherRegionID] [int] NOT NULL,
		[DateTimeUTC] [datetime] NOT NULL,
		[Temperature_Celcius] [float] NULL,
		[DewPoint_Metric] [float] NULL,
		[WindSpeed_Metric] [float] NULL,
		[WindGust_Metric] [float] NULL,
		[WindDirection_Degrees] [float] NULL,
		[WindChill_Metric] [float] NULL,
		[Visibility_Metric] [float] NULL,
		[UVIndex] [float] NULL,
		[Precipitation_Metric] [float] NULL,
		[Snow_Metric] [float] NULL,
		[Pressure_Metric] [float] NULL,
		[Humidity_Percent] [float] NULL,
		[ConditionDescription] [nvarchar](max) NULL,
		[IsForcastRow] [bit] NOT NULL,
	 CONSTRAINT [PK_WeatherDataPoints] PRIMARY KEY CLUSTERED 
	(
		[WeatherRegionID] ASC,
		[DateTimeUTC] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)
	END
	GO
	
	ALTER TABLE [dbo].[WeatherDataPoints]  WITH CHECK ADD  CONSTRAINT [FK_WeatherDataPoints_WeatherRegionID] FOREIGN KEY([WeatherRegionID])
	REFERENCES [dbo].[WeatherRegion] ([WeatherRegionID])
	GO
	
	ALTER TABLE [dbo].[WeatherDataPoints] CHECK CONSTRAINT [FK_WeatherDataPoints_WeatherRegionID]
	
GO

 CREATE VIEW MostRecentWeatherDataPointForEachRegion AS
  SELECT DataAndRegionInfo.[WeatherRegionID], DataAndRegionInfo.[FriendlyName], WeatherRegion.[Latitude],WeatherRegion.[Longitude], MostRecentDateTime,  [Temperature_Celcius]
		  ,[DewPoint_Metric],[WindSpeed_Metric] ,[WindGust_Metric] ,[WindDirection_Degrees] ,[WindChill_Metric],[Visibility_Metric],[UVIndex],[Precipitation_Metric],[Snow_Metric]
		  ,[Pressure_Metric],[Humidity_Percent],[ConditionDescription],[IsForcastRow] from
		(SELECT Points.[WeatherRegionID], Mappings.[FriendlyName], MostRecentDateTime,  [Temperature_Celcius]
		  ,[DewPoint_Metric],[WindSpeed_Metric] ,[WindGust_Metric] ,[WindDirection_Degrees] ,[WindChill_Metric],[Visibility_Metric],[UVIndex],[Precipitation_Metric],[Snow_Metric]
		  ,[Pressure_Metric],[Humidity_Percent],[ConditionDescription],[IsForcastRow] from
		  (SELECT [DateTimeUTC], LatestDataPoint.MostRecentDateTime, DataPoints.[WeatherRegionID] ,[Temperature_Celcius]
		  ,[DewPoint_Metric],[WindSpeed_Metric],[WindGust_Metric],[WindDirection_Degrees],[WindChill_Metric],[Visibility_Metric],[UVIndex],[Precipitation_Metric],[Snow_Metric],[Pressure_Metric],[Humidity_Percent],[ConditionDescription]
		  ,[IsForcastRow] from [dbo].[WeatherDataPoints] as DataPoints
			  INNER JOIN 
			  ( SELECT [WeatherRegionID], MAX([DateTimeUTC]) AS MostRecentDateTime
				 FROM [WeatherDataPoints] GROUP BY [WeatherRegionID]) AS LatestDataPoint
				 ON DataPoints.[DateTimeUTC] = LatestDataPoint.MostRecentDateTime
				 AND DataPoints.[WeatherRegionID] = LatestDataPoint.[WeatherRegionID]) as Points
		INNER JOIN [MarketWeatherEmissionsRegionMapping] as Mappings
		 ON points.[WeatherRegionID] =  Mappings.[WeatherRegionID]) as DataAndRegionInfo
	INNER JOIN [WeatherRegion] as WeatherRegion
ON WeatherRegion.[WeatherRegionID] =  DataAndRegionInfo.[WeatherRegionID]

GO

 CREATE VIEW MostRecentEmissionsDataPointForEachRegion AS
SELECT DataAndRegionInfo.[EmissionsRegionID], DataAndRegionInfo.[FriendlyName], EmissionsRegion.[Latitude],EmissionsRegion.[Longitude], MostRecentDateTime,  [SystemWideCO2Intensity_gCO2kWh],[MarginalCO2Intensity_gCO2kWh] from
  (SELECT Points.[EmissionsRegionID], Mappings.[FriendlyName], MostRecentDateTime,  [SystemWideCO2Intensity_gCO2kWh],[MarginalCO2Intensity_gCO2kWh] from
  (SELECT [DateTimeUTC], LatestEmissionsDataPoint.MostRecentDateTime, DataPoints.[EmissionsRegionID] ,[SystemWideCO2Intensity_gCO2kWh],[MarginalCO2Intensity_gCO2kWh] from [dbo].[CarbonEmissionsDataPoints] as DataPoints
  INNER JOIN 
  ( SELECT [EmissionsRegionID], MAX([DateTimeUTC]) AS MostRecentDateTime
	 FROM [CarbonEmissionsDataPoints]  WHERE [MarginalCO2Intensity_gCO2kWh] IS NOT null GROUP BY [EmissionsRegionID]) AS LatestEmissionsDataPoint
	 ON DataPoints.[DateTimeUTC] = LatestEmissionsDataPoint.MostRecentDateTime
	 AND DataPoints.[EmissionsRegionID] = LatestEmissionsDataPoint.[EmissionsRegionID]) as Points
INNER JOIN [MarketWeatherEmissionsRegionMapping] as Mappings
     ON points.[EmissionsRegionID] =  Mappings.[EmissionsRegionID]) as DataAndRegionInfo
INNER JOIN [EmissionsRegion] as EmissionsRegion
ON EmissionsRegion.[EmissionsRegionID] =  DataAndRegionInfo.[EmissionsRegionID]

GO

 CREATE VIEW MostRecentMarketDataPointForEachRegion AS
SELECT DataAndRegionInfo.[MarketRegionID], DataAndRegionInfo.[FriendlyName], MarketRegion.[Latitude],MarketRegion.[Longitude],MostRecentDateTime ,[Price],[DemandMW],[RenewablesMW],[RenewablesPercentage],[WindMW],[WindPercentage],[SolarMW],[SolarPercentage],[CarbonPricePerKg],[IsForcastRow] from
    (SELECT Points.[MarketRegionID], Mappings.[FriendlyName], MostRecentDateTime ,[Price],[DemandMW],[RenewablesMW],[RenewablesPercentage],[WindMW],[WindPercentage],[SolarMW],[SolarPercentage],[CarbonPricePerKg],[IsForcastRow] from
  (SELECT [DateTimeUTC], LatestDataPoint.MostRecentDateTime, DataPoints.[MarketRegionID],[Price],[DemandMW],[RenewablesMW],[RenewablesPercentage],[WindMW],[WindPercentage],[SolarMW],[SolarPercentage],[CarbonPricePerKg],[IsForcastRow] from [dbo].[MarketDataPoints] as DataPoints
  INNER JOIN 
  ( SELECT [MarketRegionID], MAX([DateTimeUTC]) AS MostRecentDateTime
	 FROM [MarketDataPoints] GROUP BY [MarketRegionID]) AS LatestDataPoint
	 ON DataPoints.[DateTimeUTC] = LatestDataPoint.MostRecentDateTime
	 AND DataPoints.[MarketRegionID]= LatestDataPoint.[MarketRegionID]) as Points
INNER JOIN [MarketWeatherEmissionsRegionMapping] as Mappings
     ON points.[MarketRegionID]=  Mappings.[MarketRegionID]) as DataAndRegionInfo
INNER JOIN [MarketRegion] as MarketRegion
ON MarketRegion.[MarketRegionID] =  DataAndRegionInfo.[MarketRegionID]

GO

CREATE VIEW ProcessedEmissionsDataPointsForRender AS
SELECT er.[FriendlyName] AS [Region]
		,a.[EmissionsRegionID]
		,a.DateTimeUTC
		--,ce.MaxRegMarginalDate
		--,ce.MaxRegSystemDate
		--,DATEDIFF(dd, a.DateTimeUTC, ce.MaxRegMarginalDate) AS MargDayHistory
		,(CASE WHEN (a.[Emission Type] = 'Marginal' AND a.DateTimeUTC = ce.MaxRegMarginalDate) 
						OR (a.[Emission Type] = 'System' AND a.DateTimeUTC = ce.MaxRegSystemDate) THEN 1 ELSE 0 END) AS LatestDateFlg
		,a.[Emission Type]
		,a.[C02 Intensity]
--		,a.[Forecast C02 Intensity]
                ,CONCAT(a.[EmissionsRegionID],a.[Emission Type]) AS RegTypeKey
	FROM (
	SELECT [EmissionsRegionID]
		  ,[DateTimeUTC]
		  ,'System' AS [Emission Type]
		  ,[SystemWideCO2Intensity_gCO2kWh] AS 'C02 Intensity'
	--      ,[SystemWideCO2Intensity_Forcast_gCO2kWh] AS 'Forecast C02 Intensity'
	  FROM [dbo].[CarbonEmissionsDataPoints]
	  WHERE [SystemWideCO2Intensity_gCO2kWh] IS NOT NULL

	  Union 
	  SELECT [EmissionsRegionID]
		  ,[DateTimeUTC]
		  ,'Marginal' AS [Emission Type]
		  ,[MarginalCO2Intensity_gCO2kWh] AS 'C02 Intensity'
		  --,[MarginalCO2Intensity_Forcast_gCO2kWh] AS 'Forecast C02 Intensity'
	  FROM [dbo].[CarbonEmissionsDataPoints]
	  WHERE [MarginalCO2Intensity_gCO2kWh] IS NOT NULL
	  ) a
JOIN (SELECT [EmissionsRegionID] 
			,MAX(CASE WHEN [MarginalCO2Intensity_gCO2kWh] IS NOT NULL THEN DateTimeUTC END) AS MaxRegMarginalDate 
			,MAX(CASE WHEN [SystemWideCO2Intensity_gCO2kWh] IS NOT NULL THEN DateTimeUTC END) AS MaxRegSystemDate 
			FROM [dbo].[CarbonEmissionsDataPoints] 
			GROUP BY [EmissionsRegionID]
		  ) ce ON ce.[EmissionsRegionID] = a.EmissionsRegionID
JOIN [dbo].[EmissionsRegion] er ON a.[EmissionsRegionID] = er.EmissionsRegionID
WHERE 
	DateDiff(dd,DateTimeUTC, GetDate()) <= 14
GO


CREATE view [dbo].[ProcessedAverageEmissionsDataPointsForRender] as
SELECT a.[EmissionsRegionID]
		,a.[Emission Type]
		,AVG(a.[C02 Intensity]) AS [14dayAvgCO2],
		a.[RegTypeKey]
 from [dbo].[ProcessedEmissionsDataPointsForRender]  as a
GROUP BY a.[EmissionsRegionID], a.[Emission Type], a.[RegTypeKey]

GO

Create view [dbo].[ProcessedRealTimeEmissionsDataPointsForRender] as
SELECT [EmissionsRegionID]
		,[Region]
		,[Emission Type]
		,DateTimeUTC
		,[C02 Intensity] AS [CO2 Intensity (Real Time)],
		[RegTypeKey]
FROM [dbo].[ProcessedEmissionsDataPointsForRender]
WHERE LatestDateFlg = 1

GO