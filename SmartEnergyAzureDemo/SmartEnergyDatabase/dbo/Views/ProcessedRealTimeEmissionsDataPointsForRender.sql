Create view [dbo].[ProcessedRealTimeEmissionsDataPointsForRender] as
SELECT [EmissionsRegionID]
		,[Region]
		,[Emission Type]
		,DateTimeUTC
		,[C02 Intensity] AS [CO2 Intensity (Real Time)],
		[RegTypeKey]
FROM [dbo].[ProcessedEmissionsDataPointsForRender]
WHERE LatestDateFlg = 1