CREATE TABLE [dbo].[WeatherDataPoints] (
    [WeatherRegionID]       INT            NOT NULL,
    [DateTimeUTC]           DATETIME       NOT NULL,
    [Temperature_Celcius]   FLOAT (53)     NULL,
    [DewPoint_Metric]       FLOAT (53)     NULL,
    [WindSpeed_Metric]      FLOAT (53)     NULL,
    [WindGust_Metric]       FLOAT (53)     NULL,
    [WindDirection_Degrees] FLOAT (53)     NULL,
    [WindChill_Metric]      FLOAT (53)     NULL,
    [Visibility_Metric]     FLOAT (53)     NULL,
    [UVIndex]               FLOAT (53)     NULL,
    [Precipitation_Metric]  FLOAT (53)     NULL,
    [Snow_Metric]           FLOAT (53)     NULL,
    [Pressure_Metric]       FLOAT (53)     NULL,
    [Humidity_Percent]      FLOAT (53)     NULL,
    [ConditionDescription]  NVARCHAR (MAX) NOT NULL,
    [IsForcastRow]          BIT            NOT NULL,
    CONSTRAINT [PK_WeatherDataPoints] PRIMARY KEY CLUSTERED ([WeatherRegionID] ASC, [DateTimeUTC] ASC),
    CONSTRAINT [FK_WeatherDataPoints_WeatherRegionID] FOREIGN KEY ([WeatherRegionID]) REFERENCES [dbo].[WeatherRegion] ([WeatherRegionID])
);



