CREATE TABLE [dbo].[MarketWeatherEmissionsRegionMapping] (
    [RegionMappingID]   INT            IDENTITY (0, 1) NOT NULL,
    [FriendlyName]      NVARCHAR (MAX) NOT NULL,
    [MarketRegionID]    INT            NULL,
    [WeatherRegionID]   INT            NULL,
    [EmissionsRegionID] INT            NULL,
    CONSTRAINT [PK_MarketWeatherEmissionsRegionMapping] PRIMARY KEY CLUSTERED ([RegionMappingID] ASC),
    CONSTRAINT [FK_MarketWeatherEmissionsRegionMapping_EmissionsRegionID] FOREIGN KEY ([EmissionsRegionID]) REFERENCES [dbo].[EmissionsRegion] ([EmissionsRegionID]),
    CONSTRAINT [FK_MarketWeatherEmissionsRegionMapping_MarketRegionID] FOREIGN KEY ([MarketRegionID]) REFERENCES [dbo].[MarketRegion] ([MarketRegionID]),
    CONSTRAINT [FK_MarketWeatherEmissionsRegionMapping_WeatherRegionID] FOREIGN KEY ([WeatherRegionID]) REFERENCES [dbo].[WeatherRegion] ([WeatherRegionID])
);



