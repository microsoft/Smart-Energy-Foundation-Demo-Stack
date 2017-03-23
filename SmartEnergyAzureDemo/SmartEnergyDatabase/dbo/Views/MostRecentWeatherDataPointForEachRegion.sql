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