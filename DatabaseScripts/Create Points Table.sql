USE [JEDCMobility]
GO

CREATE TABLE [dbo].[Point] (
    [TimeStamp] DATETIME NOT NULL,
    [DeviceId] VARCHAR(100) NOT NULL,
    [IdType] CHAR(4) NOT NULL,
	[Latitude] DECIMAL(8,5) NOT NULL,
	[Longitude] DECIMAL(8,5) NOT NULL,
    [HorizontalAccuracy] REAL NOT NULL,
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
    [PersonId] INT NOT NULL,
)

CREATE CLUSTERED COLUMNSTORE INDEX [CCI_Point]
ON [dbo].[Point]
GO

CREATE TABLE [dbo].[Person] (
	[Id] INT IDENTITY(1,1) PRIMARY KEY,
    [QuadId] CHAR(64) NOT NULL
)
GO

ALTER TABLE [dbo].[Person]  
ADD CONSTRAINT [UQ_Person_QuadId] UNIQUE ([QuadId])
GO

ALTER TABLE [dbo].[Point] 
ADD CONSTRAINT [FK_Point_Person] FOREIGN KEY ([PersonId]) 
REFERENCES [dbo].[Person] ([Id]) 
GO

--ALTER INDEX [CCI_Point] ON [dbo].[Point] REBUILD

CREATE TABLE [dbo].[Area] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
	[Name] VARCHAR(MAX) NOT NULL,
    [Shape] GEOGRAPHY NOT NULL
)
GO
