CREATE TABLE [dbo].[TripAdvanceExpenses]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[VehicleExpenseTypeId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_TripAdvanceExpenses_ToVehicleTrip] FOREIGN KEY ([MasterId]) REFERENCES [TripAdvance]([Id]), 
	CONSTRAINT [FK_TripAdvanceExpenses_ToVehicleExpenseType] FOREIGN KEY ([VehicleExpenseTypeId]) REFERENCES [VehicleExpenseType]([Id])
)
