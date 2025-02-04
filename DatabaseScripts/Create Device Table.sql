USE JEDCMobility
GO

CREATE TABLE [dbo].[Device] (
	[Id] VARCHAR(100) PRIMARY KEY
)
GO

INSERT INTO [dbo].[Device]
SELECT DISTINCT [DeviceId]
FROM [dbo].[Point]
GO

ALTER TABLE [dbo].[Point] 
ADD CONSTRAINT [FK_Point_Device]
FOREIGN KEY ([DeviceId]) 
REFERENCES [dbo].[Device] ([Id])
GO

CREATE INDEX IX_Point_DeviceId ON [dbo].[Point] ([DeviceId]) 
INCLUDE ([Timestamp], [Latitude], [Longitude])
GO
