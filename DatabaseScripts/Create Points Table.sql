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
    [QuadId] CHAR(64) NOT NULL,
	[HoursSpan] INT NULL,
	[IsResident] AS (CASE WHEN [HoursSpan] > 12 THEN 1 ELSE 0 END)
)
GO

ALTER TABLE [dbo].[Person]  
ADD CONSTRAINT [UQ_Person_QuadId] UNIQUE ([QuadId])
GO

ALTER TABLE [dbo].[Point] 
ADD CONSTRAINT [FK_Point_Person] FOREIGN KEY ([PersonId]) 
REFERENCES [dbo].[Person] ([Id]) 
GO

CREATE TABLE [dbo].[Area] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
	[Name] VARCHAR(MAX) NOT NULL,
    [Shape] GEOGRAPHY NOT NULL
)
GO

CREATE TABLE [dbo].[PersonArea] (
	[PersonId] INT NOT NULL,
	[AreaId] INT NOT NULL,
	[Date] [DATE] NOT NULL,
	[Hour] [TINYINT] NOT NULL
)
GO

ALTER TABLE [dbo].[PersonArea]  
ADD CONSTRAINT [UQ_PersonArea] UNIQUE ([PersonId], [AreaId], [Date], [Hour])

ALTER TABLE [dbo].[PersonArea] 
ADD CONSTRAINT [FK_PersonArea_Person] FOREIGN KEY ([PersonId]) 
REFERENCES [dbo].[Person] ([Id]) 

ALTER TABLE [dbo].[PersonArea] 
ADD CONSTRAINT [FK_PersonArea_Area] FOREIGN KEY ([AreaId]) 
REFERENCES [dbo].[Area] ([Id]) 
GO

CREATE VIEW [dbo].[vw_PersonArea_All] AS (
SELECT DISTINCT [PersonId], [AreaId]
FROM [dbo].[PersonArea] PA
)
GO

CREATE VIEW [dbo].[vw_PersonArea_Daily] AS (
SELECT DISTINCT [PersonId], [AreaId], [Date]
FROM [dbo].[PersonArea] PA
)
GO

CREATE VIEW [dbo].[vw_PersonArea_Hourly] AS (
SELECT DISTINCT [PersonId], [AreaId], [Date], [Hour]
FROM [dbo].[PersonArea] PA
)
GO

CREATE VIEW [dbo].[vw_Total_All] AS (
SELECT V.[AreaId], V.[0] AS [Visitors], V.[1] AS [Residents]
FROM (
    SELECT PA.[AreaId], P.[IsResident], COUNT(*) AS [Count]
    FROM [dbo].[vw_PersonArea_All] PA
    JOIN [dbo].[Person] P ON P.[Id] = PA.[PersonId]
    GROUP BY PA.[AreaId], P.[IsResident]
) B
PIVOT (
    SUM(B.[Count])
    FOR B.[IsResident] IN ([0], [1])
) V
)
GO

CREATE VIEW [dbo].[vw_Total_Daily] AS (
SELECT V.[AreaId], V.[Date], V.[0] AS [Visitors], V.[1] AS [Residents]
FROM (
    SELECT PA.[AreaId], PA.[Date], P.[IsResident], COUNT(*) AS [Count]
    FROM [dbo].[vw_PersonArea_Daily] PA
    JOIN [dbo].[Person] P ON P.[Id] = PA.[PersonId]
    GROUP BY PA.[AreaId], PA.[Date], P.[IsResident]
) B
PIVOT (
    SUM(B.[Count])
    FOR B.[IsResident] IN ([0], [1])
) V
)
GO

CREATE VIEW [dbo].[vw_Total_Hourly] AS (
SELECT V.[AreaId], V.[Date], V.[Hour], V.[0] AS [Visitors], V.[1] AS [Residents]
FROM (
    SELECT PA.[AreaId], PA.[Date], PA.[Hour], P.[IsResident], COUNT(*) AS [Count]
    FROM [dbo].[vw_PersonArea_Hourly] PA
    JOIN [dbo].[Person] P ON P.[Id] = PA.[PersonId]
    GROUP BY PA.[AreaId], PA.[Date], PA.[Hour], P.[IsResident]
) B
PIVOT (
    SUM(B.[Count])
    FOR B.[IsResident] IN ([0], [1])
) V
)
GO

