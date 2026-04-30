CREATE TABLE [dbo].[VehicleExpenseDetails]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[ExpenseTypeId] INT NOT NULL,
	[LedgerId] INT NULL,
	[Amount] MONEY NOT NULL,
	[IdentificationNo] VARCHAR(MAX) NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_VehicleExpenseDetails_ToVehicleExpense] FOREIGN KEY ([MasterId]) REFERENCES [VehicleExpense]([Id]), 
	CONSTRAINT [FK_VehicleExpenseDetails_ToExpenseType] FOREIGN KEY ([ExpenseTypeId]) REFERENCES [ExpenseType]([Id]),
	CONSTRAINT [FK_VehicleExpenseDetails_ToLedger] FOREIGN KEY ([LedgerId]) REFERENCES [Ledger]([Id])
)
