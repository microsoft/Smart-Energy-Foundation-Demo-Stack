CREATE TABLE [dbo].[MarketRegion] (
    [MarketRegionID]      INT                IDENTITY (0, 1) NOT NULL,
    [FriendlyName]        NVARCHAR (MAX)     NOT NULL,
    [TimeZoneUTCRelative] DATETIMEOFFSET (7) NOT NULL,
    [Latitude]            FLOAT (53)         NULL,
    [Longitude]           FLOAT (53)         NULL,
    [CurrencyName]        NVARCHAR (50)      NULL,
    [CurrencyValuePerUSD] FLOAT (53)         NULL,
    CONSTRAINT [PK_MarketRegion] PRIMARY KEY CLUSTERED ([MarketRegionID] ASC)
);



