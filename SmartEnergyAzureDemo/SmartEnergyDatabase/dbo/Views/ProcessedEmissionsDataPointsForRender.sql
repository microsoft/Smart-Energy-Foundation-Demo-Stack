
CREATE VIEW [dbo].[ProcessedEmissionsDataPointsForRender] AS
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