
IF OBJECT_ID('tempdb..#Import') IS NOT NULL
    DROP TABLE #Import;

CREATE TABLE #Import (
	[TimeStamp] BIGINT,
	[DeviceId] VARCHAR(100),
	[IdType] CHAR(4),
	[Latitude] DECIMAL(8,5),
	[Longitude] DECIMAL(8,5),
	[HorizontalAccuracy] REAL,
	[IpAddress] VARCHAR(39),
	[DeviceOS] VARCHAR(7),
	[OSVersion] VARCHAR(4),
	[UserAgent] VARCHAR(MAX),
	[Country] CHAR(2),
	[SourceId] CHAR(2),
	[PublisherId] VARCHAR(64),
	[AppId] VARCHAR(64),
	[LocationContext] VARCHAR(1),
	[Geohash] CHAR(12),
	[Consent] CHAR(1),
	[QuadId] CHAR(64)
)
