CREATE TABLE [dbo].[Vehicle]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Code] VARCHAR(500) NOT NULL UNIQUE, 
    [ShortCode] VARCHAR(500) NOT NULL, 
    [ChasisCode] VARCHAR(500) NOT NULL UNIQUE, 
    [EngineCode] VARCHAR(500) NOT NULL UNIQUE, 
    [VehicleTypeId] INT NOT NULL, 
    [PurchaseDate] DATETIME NOT NULL, 
    [OpeningHour] MONEY NULL, 
    [OpeningKM] MONEY NULL, 
    [Remarks] VARCHAR(MAX) NULL, 
    [Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_Vehicle_ToVehicleType] FOREIGN KEY ([VehicleTypeId]) REFERENCES [VehicleType]([Id])
)
