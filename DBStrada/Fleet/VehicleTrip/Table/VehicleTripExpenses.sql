CREATE TABLE [dbo].[VehicleTripExpenses]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[ExpenseTypeId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_VehicleTripExpenses_ToVehicleTrip] FOREIGN KEY ([MasterId]) REFERENCES [VehicleTrip]([Id]), 
	CONSTRAINT [FK_VehicleTripExpenses_ToExpenseType] FOREIGN KEY ([ExpenseTypeId]) REFERENCES [ExpenseType]([Id])
)
