CREATE TABLE [dbo].[VehicleTripExpenses]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[VehicleRouteExpenseTypeId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_VehicleTripExpenses_ToVehicleTrip] FOREIGN KEY ([MasterId]) REFERENCES [VehicleTrip]([Id]), 
	CONSTRAINT [FK_VehicleTripExpenses_ToVehicleRouteExpenseType] FOREIGN KEY ([VehicleRouteExpenseTypeId]) REFERENCES [VehicleRouteExpenseType]([Id])
)
