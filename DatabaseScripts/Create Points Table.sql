USE [JEDCMobility]
GO

CREATE TABLE [dbo].[Point] (
    [TimeStamp] DATETIME NOT NULL,
    [DeviceId] INT NOT NULL,
    [IdType] VARCHAR(MAX) NOT NULL,
    [Latitude] DECIMAL(8,5) NOT NULL,
    [Longitude] DECIMAL(8,5) NOT NULL,
    [HorizontalAccuracy] VARCHAR(MAX) NOT NULL,
    [IpAddress] VARCHAR(16) NOT NULL,
    [DeviceOS] VARCHAR(MAX) NOT NULL,
    [OSVersion] VARCHAR(4) NOT NULL,
    [UserAgent] VARCHAR(MAX) NOT NULL,
    [Country] VARCHAR(2) NOT NULL,
    [SourceId] VARCHAR(MAX) NOT NULL,
    [PublisherId] VARCHAR(MAX) NOT NULL,
    [AppId] VARCHAR(MAX) NOT NULL,
    [LocationContext] VARCHAR(MAX) NOT NULL,
    [Geohash] VARCHAR(MAX) NOT NULL,
    [Consent] VARCHAR(4) NOT NULL,
    [QuadId] VARCHAR(MAX) NOT NULL
)

CREATE CLUSTERED COLUMNSTORE INDEX CCI_Point
ON [dbo].[Point];
GO

CREATE INDEX IX_Point_DeviceId ON [dbo].[Point] ([DeviceId])
GO

CREATE TABLE [dbo].[Device] (
	[Id] INT IDENTITY(1,1) PRIMARY KEY,
	[DeviceId] VARCHAR(100) NOT NULL
)
GO

ALTER TABLE [dbo].[Point] 
ADD CONSTRAINT [FK_Point_Device] FOREIGN KEY ([DeviceId]) 
REFERENCES [dbo].[Device] ([Id]) 
ON DELETE CASCADE;
