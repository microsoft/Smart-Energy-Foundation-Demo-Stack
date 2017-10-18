
CREATE view [dbo].[ProcessedAverageEmissionsDataPointsForRender] as
SELECT a.[EmissionsRegionID]
		,a.[Emission Type]
		,AVG(a.[C02 Intensity]) AS [14dayAvgCO2],
		a.[RegTypeKey]
 from [dbo].[ProcessedEmissionsDataPointsForRender]  as a
GROUP BY a.[EmissionsRegionID], a.[Emission Type], a.[RegTypeKey]