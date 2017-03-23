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