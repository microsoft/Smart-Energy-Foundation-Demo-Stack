CREATE TABLE [dbo].[CarbonEmissionsDataPoints] (
    [EmissionsRegionID]                INT        NOT NULL,
    [DateTimeUTC]                      DATETIME   NOT NULL,
    [SystemWideCO2Intensity_gCO2kWh]   FLOAT (53) NULL,
    [SystemWideCO2Intensity_IsForcast] BIT        NOT NULL,
    [MarginalCO2Intensity_gCO2kWh]     FLOAT (53) NULL,
    [MarginalCO2Intensity_IsForcast]   BIT        NOT NULL,
    CONSTRAINT [PK_CarbonEmissionsDataPoints] PRIMARY KEY CLUSTERED ([EmissionsRegionID] ASC, [DateTimeUTC] ASC),
    CONSTRAINT [FK_CarbonEmissionsDataPoints_EmissionsRegionID] FOREIGN KEY ([EmissionsRegionID]) REFERENCES [dbo].[EmissionsRegion] ([EmissionsRegionID])
);



