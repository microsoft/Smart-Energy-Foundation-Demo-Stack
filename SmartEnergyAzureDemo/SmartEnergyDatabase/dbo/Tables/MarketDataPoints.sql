CREATE TABLE [dbo].[MarketDataPoints] (
    [MarketRegionID]       INT        NOT NULL,
    [DateTimeUTC]          DATETIME   NOT NULL,
    [Price]                FLOAT (53) NULL,
    [DemandMW]             FLOAT (53) NULL,
    [RenewablesMW]         FLOAT (53) NULL,
    [RenewablesPercentage] FLOAT (53) NULL,
    [WindMW]               FLOAT (53) NULL,
    [WindPercentage]       FLOAT (53) NULL,
    [SolarMW]              FLOAT (53) NULL,
    [SolarPercentage]      FLOAT (53) NULL,
    [CarbonPricePerKg]     FLOAT (53) NULL,
    [IsForcastRow]         BIT        NOT NULL,
    CONSTRAINT [PK_CurrentRegionsMarketData] PRIMARY KEY CLUSTERED ([MarketRegionID] ASC, [DateTimeUTC] ASC),
    CONSTRAINT [FK_MarketDataPoints_MarketRegionID] FOREIGN KEY ([MarketRegionID]) REFERENCES [dbo].[MarketRegion] ([MarketRegionID])
);



