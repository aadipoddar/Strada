CREATE TABLE [dbo].[VehicleRoute]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [FromLocationId] INT NOT NULL,
    [ToLocationId] INT NOT NULL,
    [Code] VARCHAR(10) NOT NULL UNIQUE, 
    [EstimatedHours] INT NOT NULL,
    [EstimatedDistance] INT NOT NULL,
    [EstimatedFuelConsumption] INT NOT NULL,
    [EstimatedCost] MONEY NOT NULL,
    [Remarks] VARCHAR(MAX) NULL,
    [Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_VehicleRoute_FromVehicleRouteLocation] FOREIGN KEY ([FromLocationId]) REFERENCES [VehicleRouteLocation]([Id]),
    CONSTRAINT [FK_VehicleRoute_ToVehicleRouteLocation] FOREIGN KEY ([ToLocationId]) REFERENCES [VehicleRouteLocation]([Id])
)
