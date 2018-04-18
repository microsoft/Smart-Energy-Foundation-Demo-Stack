CREATE TABLE [dbo].[CarbonEmissionsRelativeMeritDataPoints] (
    [EmissionsRegionID]              INT        NOT NULL,
    [DateTimeUTC]                    DATETIME   NOT NULL,
    [EmissionsRelativeMerit]         FLOAT (53) NULL,
    [EmissionsRelativeMerit_Forcast] FLOAT (53) NULL,
    CONSTRAINT [PK_GridEmissionsRelativeMeritDataPoints] PRIMARY KEY CLUSTERED ([EmissionsRegionID] ASC, [DateTimeUTC] ASC),
    CONSTRAINT [FK_CarbonEmissionsRelativeMeritDataPoints_EmissionsRegionID] FOREIGN KEY ([EmissionsRegionID]) REFERENCES [dbo].[EmissionsRegion] ([EmissionsRegionID])
);

