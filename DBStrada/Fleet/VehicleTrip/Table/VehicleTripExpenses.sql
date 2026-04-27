CREATE TABLE [dbo].[VehicleTripExpenses]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[VehicleExpenseTypeId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_VehicleTripExpenses_ToVehicleTrip] FOREIGN KEY ([MasterId]) REFERENCES [VehicleTrip]([Id]), 
	CONSTRAINT [FK_VehicleTripExpenses_ToVehicleExpenseType] FOREIGN KEY ([VehicleExpenseTypeId]) REFERENCES [VehicleExpenseType]([Id])
)
