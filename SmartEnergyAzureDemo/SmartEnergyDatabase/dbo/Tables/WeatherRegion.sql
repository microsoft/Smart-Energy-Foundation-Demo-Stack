CREATE TABLE [dbo].[WeatherRegion] (
    [WeatherRegionID]                 INT                IDENTITY (0, 1) NOT NULL,
    [FriendlyName]                    NVARCHAR (MAX)     NOT NULL,
    [TimeZoneUTCRelative]             DATETIMEOFFSET (7) NOT NULL,
    [Latitude]                        FLOAT (53)         NULL,
    [Longitude]                       FLOAT (53)         NULL,
    [WeatherRegionWundergroundSubUrl] NVARCHAR (MAX)     NULL,
    CONSTRAINT [PK_WeatherRegion] PRIMARY KEY CLUSTERED ([WeatherRegionID] ASC)
);

