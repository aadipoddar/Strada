CREATE TABLE [dbo].[VehicleRepairExpenses]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[VehicleExpenseTypeId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[IdentificationNo] VARCHAR(MAX) NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_VehicleRepairExpenses_ToVehicleRepair] FOREIGN KEY ([MasterId]) REFERENCES [VehicleRepair]([Id]), 
	CONSTRAINT [FK_VehicleRepairExpenses_ToVehicleExpenseType] FOREIGN KEY ([VehicleExpenseTypeId]) REFERENCES [VehicleExpenseType]([Id])
)
