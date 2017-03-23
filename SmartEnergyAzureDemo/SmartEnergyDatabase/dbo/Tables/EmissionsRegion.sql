CREATE TABLE [dbo].[EmissionsRegion] (
    [EmissionsRegionID]             INT                IDENTITY (0, 1) NOT NULL,
    [FriendlyName]                  NVARCHAR (MAX)     NOT NULL,
    [TimeZoneUTCRelative]           DATETIMEOFFSET (7) NOT NULL,
    [Latitude]                      FLOAT (53)         NULL,
    [Longitude]                     FLOAT (53)         NULL,
    [EmissionsRegionWattTimeSubUrl] NVARCHAR (MAX)     NULL,
    CONSTRAINT [PK_EmissionsRegion] PRIMARY KEY CLUSTERED ([EmissionsRegionID] ASC)
);

