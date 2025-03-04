USE [JEDCMobility]
GO

ALTER INDEX [CCI_Point] ON [dbo].[Point] REBUILD
GO

UPDATE P
SET P.[HoursSpan] = A.[Span]
FROM [dbo].[Person] P
JOIN (
	SELECT *, DATEDIFF(HOUR, [Start], [Stop]) [Span]
	FROM (
		SELECT [PersonId], MIN([TimeStamp]) [Start], MAX([TimeStamp]) [Stop]
		FROM [dbo].[Point]
		GROUP BY [PersonId]
	) _
) A ON A.[PersonId] = P.[Id]
GO
