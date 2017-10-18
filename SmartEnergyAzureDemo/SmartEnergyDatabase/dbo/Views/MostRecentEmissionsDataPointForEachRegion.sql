CREATE VIEW [dbo].[MostRecentEmissionsDataPointForEachRegion] AS
SELECT DataAndRegionInfo.[EmissionsRegionID], DataAndRegionInfo.[FriendlyName], EmissionsRegion.[Latitude],EmissionsRegion.[Longitude], MostRecentDateTime,  [SystemWideCO2Intensity_gCO2kWh],[MarginalCO2Intensity_gCO2kWh] from
  (SELECT Points.[EmissionsRegionID], Mappings.[FriendlyName], MostRecentDateTime,  [SystemWideCO2Intensity_gCO2kWh],[MarginalCO2Intensity_gCO2kWh] from
  (SELECT [DateTimeUTC], LatestEmissionsDataPoint.MostRecentDateTime, DataPoints.[EmissionsRegionID] ,[SystemWideCO2Intensity_gCO2kWh],[MarginalCO2Intensity_gCO2kWh] from [dbo].[CarbonEmissionsDataPoints] as DataPoints
  INNER JOIN 
  ( SELECT [EmissionsRegionID], MAX([DateTimeUTC]) AS MostRecentDateTime
	 FROM [CarbonEmissionsDataPoints]  WHERE [MarginalCO2Intensity_gCO2kWh] IS NOT null 
	 GROUP BY [EmissionsRegionID]) AS LatestEmissionsDataPoint
	 ON DataPoints.[DateTimeUTC] = LatestEmissionsDataPoint.MostRecentDateTime
	 AND DataPoints.[EmissionsRegionID] = LatestEmissionsDataPoint.[EmissionsRegionID]) as Points
INNER JOIN [MarketWeatherEmissionsRegionMapping] as Mappings
     ON points.[EmissionsRegionID] =  Mappings.[EmissionsRegionID]) as DataAndRegionInfo
INNER JOIN [EmissionsRegion] as EmissionsRegion
ON EmissionsRegion.[EmissionsRegionID] =  DataAndRegionInfo.[EmissionsRegionID]