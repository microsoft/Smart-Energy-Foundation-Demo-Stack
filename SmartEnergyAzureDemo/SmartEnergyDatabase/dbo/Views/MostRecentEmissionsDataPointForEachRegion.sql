 CREATE VIEW MostRecentEmissionsDataPointForEachRegion AS
SELECT DataAndRegionInfo.[EmissionsRegionID], DataAndRegionInfo.[FriendlyName], EmissionsRegion.[Latitude],EmissionsRegion.[Longitude], MostRecentDateTime,  [SystemWideCO2Intensity_gCO2kWh],[SystemWideCO2Intensity_IsForcast],[MarginalCO2Intensity_gCO2kWh],[MarginalCO2Intensity_IsForcast] from
  (SELECT Points.[EmissionsRegionID], Mappings.[FriendlyName], MostRecentDateTime,  [SystemWideCO2Intensity_gCO2kWh],[SystemWideCO2Intensity_IsForcast],[MarginalCO2Intensity_gCO2kWh],[MarginalCO2Intensity_IsForcast] from
  (SELECT [DateTimeUTC], LatestEmissionsDataPoint.MostRecentDateTime, DataPoints.[EmissionsRegionID] ,[SystemWideCO2Intensity_gCO2kWh],[SystemWideCO2Intensity_IsForcast],[MarginalCO2Intensity_gCO2kWh],[MarginalCO2Intensity_IsForcast] from [dbo].[CarbonEmissionsDataPoints] as DataPoints
  INNER JOIN 
  ( SELECT [EmissionsRegionID], MAX([DateTimeUTC]) AS MostRecentDateTime
	 FROM [CarbonEmissionsDataPoints] GROUP BY [EmissionsRegionID]) AS LatestEmissionsDataPoint
	 ON DataPoints.[DateTimeUTC] = LatestEmissionsDataPoint.MostRecentDateTime
	 AND DataPoints.[EmissionsRegionID] = LatestEmissionsDataPoint.[EmissionsRegionID]) as Points
INNER JOIN [MarketWeatherEmissionsRegionMapping] as Mappings
     ON points.[EmissionsRegionID] =  Mappings.[EmissionsRegionID]) as DataAndRegionInfo
INNER JOIN [EmissionsRegion] as EmissionsRegion
ON EmissionsRegion.[EmissionsRegionID] =  DataAndRegionInfo.[EmissionsRegionID]