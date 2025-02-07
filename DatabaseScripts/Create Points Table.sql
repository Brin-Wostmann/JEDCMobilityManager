USE [JEDCMobility]
GO

CREATE TABLE [dbo].[Point] (
    [TimeStamp] DATETIME NOT NULL,
    [DeviceId] INT NOT NULL,
    [IdType] CHAR(4) NOT NULL,
    [Latitude] DECIMAL(8,5) NOT NULL,
    [Longitude] DECIMAL(8,5) NOT NULL,
    [HorizontalAccuracy] DECIMAL(10,5) NOT NULL,
    [IpAddress] VARCHAR(39) NOT NULL,
    [DeviceOS] VARCHAR(7) NOT NULL,
    [OSVersion] VARCHAR(4) NOT NULL,
    [UserAgent] VARCHAR(MAX) NOT NULL,
    [Country] CHAR(2) NOT NULL,
    [SourceId] CHAR(2) NOT NULL,
    [PublisherId] VARCHAR(64) NOT NULL,
    [AppId] VARCHAR(64) NOT NULL,
    [LocationContext] VARCHAR(1) NOT NULL,
    [Geohash] CHAR(12) NOT NULL,
    [Consent] CHAR(1) NOT NULL,
    [QuadId] CHAR(64) NOT NULL
)

CREATE CLUSTERED COLUMNSTORE INDEX [CCI_Point]
ON [dbo].[Point]
GO

CREATE TABLE [dbo].[Device] (
	[Id] INT IDENTITY(1,1) PRIMARY KEY,
	[DeviceId] VARCHAR(100) NOT NULL
)
GO

ALTER TABLE [dbo].[Device]  
ADD CONSTRAINT UQ_Device_DeviceId UNIQUE ([DeviceId])
GO

ALTER TABLE [dbo].[Point] 
ADD CONSTRAINT [FK_Point_Device] FOREIGN KEY ([DeviceId]) 
REFERENCES [dbo].[Device] ([Id]) 
GO
