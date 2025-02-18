USE [JEDCMobility]
GO

--DROP TABLE [dbo].[PersonArea]

CREATE TABLE [dbo].[PersonArea] (
	[PersonId] INT NOT NULL,
	[AreaId] INT NOT NULL
)
GO

DECLARE @BatchSize INT = 10000
DECLARE @Offset INT = 0
DECLARE @TotalRows INT

SELECT @TotalRows = COUNT(*) FROM [dbo].[Point]

WHILE @Offset < @TotalRows
BEGIN
	WITH [Locations] AS (
		SELECT [PersonId], geography::Point([Latitude], [Longitude], 4326) AS [Location]
		FROM[dbo].[Point]
		ORDER BY (SELECT NULL)
		OFFSET @Offset ROWS FETCH NEXT @BatchSize ROWS ONLY
	)
	INSERT INTO [dbo].[PersonArea] ([PersonId], [AreaId])
	SELECT DISTINCT L.[PersonId], A.[Id]
	FROM [dbo].[Area] A
	CROSS JOIN [Locations] L
	LEFT JOIN [dbo].[PersonArea] P ON P.[PersonId] = L.[PersonId] AND P.[AreaId] = A.[Id]
	WHERE P.[PersonId] IS NULL
	  AND A.[Shape].STIntersects(L.[Location]) = 1;

    SET @Offset = @Offset + @BatchSize;
END
