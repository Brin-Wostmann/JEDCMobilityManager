
TRUNCATE TABLE #Import;

INSERT INTO #Import
SELECT *
FROM OPENJSON(@json) WITH (
	[TimeStamp] BIGINT '$.timestamp',
	[DeviceId] VARCHAR(100) '$.device_id',
	[IdType] CHAR(4) '$.id_type',
	[Latitude] DECIMAL(8,5)  '$.latitude',
	[Longitude] DECIMAL(8,5)  '$.longitude',
	[HorizontalAccuracy] REAL  '$.horizontal_accuracy',
	[IpAddress] VARCHAR(39) '$.ip_address',
	[DeviceOS] VARCHAR(7) '$.device_os',
	[OSVersion] VARCHAR(4) '$.os_version',
	[UserAgent] VARCHAR(MAX) '$.user_agent',
	[Country] CHAR(2) '$.country',
	[SourceId] CHAR(2) '$.source_id',
	[PublisherId] VARCHAR(64) '$.publisher_id',
	[AppId] VARCHAR(64) '$.app_id',
	[LocationContext] VARCHAR(1) '$.location_context',
	[Geohash] CHAR(12) '$.geohash',
	[Consent] CHAR(1) '$.consent',
	[QuadId] CHAR(64) '$.quad_id'
);

INSERT INTO [dbo].[Person]
SELECT DISTINCT I.[QuadId]
FROM #Import I
LEFT JOIN [dbo].[Person] P ON P.[QuadId] = I.[QuadId]
WHERE P.[Id] IS NULL;

INSERT INTO [dbo].[Point] ([TimeStamp],                     [DeviceId],     [IdType],   [Latitude],   [Longitude],   [HorizontalAccuracy],   [IpAddress],   [DeviceOS],   [OSVersion],   [UserAgent],   [Country],   [SourceId],   [PublisherId],   [AppId],   [LocationContext],   [Geohash],   [Consent], [PersonId])
SELECT DATEADD(SECOND, I.[TimeStamp] / 1000, '1/1/1970'), I.[DeviceId],   I.[IdType], I.[Latitude], I.[Longitude], I.[HorizontalAccuracy], I.[IpAddress], I.[DeviceOS], I.[OSVersion], I.[UserAgent], I.[Country], I.[SourceId], I.[PublisherId], I.[AppId], I.[LocationContext], I.[Geohash], I.[Consent], P.[Id]
FROM #Import I
JOIN [dbo].[Person] P ON P.[QuadId] = I.[QuadId];

