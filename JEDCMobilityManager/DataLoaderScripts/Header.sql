
IF OBJECT_ID('tempdb..#Import') IS NOT NULL
    DROP TABLE #Import;

CREATE TABLE #Import (
	[TimeStamp] BIGINT,
	[DeviceId] VARCHAR(100),
	[IdType] VARCHAR(MAX),
	[Latitude] DECIMAL(8,5),
	[Longitude] DECIMAL(8,5),
	[HorizontalAccuracy] VARCHAR(MAX),
	[IpAddress] VARCHAR(16),
	[DeviceOS] VARCHAR(MAX),
	[OSVersion] VARCHAR(4),
	[UserAgent] VARCHAR(MAX),
	[Country] VARCHAR(2),
	[SourceId] VARCHAR(MAX),
	[PublisherId] VARCHAR(MAX),
	[AppId] VARCHAR(MAX),
	[LocationContext] VARCHAR(MAX),
	[Geohash] VARCHAR(MAX),
	[Consent] VARCHAR(4),
	[QuadId] VARCHAR(MAX)
)
