
INSERT INTO [dbo].[Point] ([TimeStamp],                            [DeviceId], [IdType], [Latitude], [Longitude], [HorizontalAccuracy], [IpAddress], [DeviceOS], [OSVersion], [UserAgent], [Country], [SourceId], [PublisherId], [AppId], [LocationContext], [Geohash], [Consent], [QuadId])
SELECT DATEADD(SECOND, [TimeStamp] / 1000, '1970-01-01 00:00:00'), [DeviceId], [IdType], [Latitude], [Longitude], [HorizontalAccuracy], [IpAddress], [DeviceOS], [OSVersion], [UserAgent], [Country], [SourceId], [PublisherId], [AppId], [LocationContext], [Geohash], [Consent], [QuadId]
FROM OPENJSON(@json) WITH (
	[TimeStamp] BIGINT '$.timestamp',
	[DeviceId] VARCHAR(100) '$.device_id',
	[IdType] VARCHAR(MAX) '$.id_type',
	[Latitude] DECIMAL(8,5)  '$.latitude',
	[Longitude] DECIMAL(8,5)  '$.longitude',
	[HorizontalAccuracy] VARCHAR(MAX)  '$.horizontal_accuracy',
	[IpAddress] VARCHAR(16) '$.ip_address',
	[DeviceOS] VARCHAR(MAX) '$.device_os',
	[OSVersion] VARCHAR(4) '$.os_version',
	[UserAgent] VARCHAR(MAX) '$.user_agent',
	[Country] VARCHAR(2) '$.country',
	[SourceId] VARCHAR(MAX) '$.source_id',
	[PublisherId] VARCHAR(MAX) '$.publisher_id',
	[AppId] VARCHAR(MAX) '$.app_id',
	[LocationContext] VARCHAR(MAX) '$.location_context',
	[Geohash] VARCHAR(MAX) '$.geohash',
	[Consent] VARCHAR(4) '$.consent',
	[QuadId] VARCHAR(MAX) '$.quad_id'
)
