CREATE TABLE [dbo].[VehicleTripBillLedgerPayments]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[LedgerId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_VehicleTripBillLedgerPayments_ToVehicleTripBill] FOREIGN KEY ([MasterId]) REFERENCES [VehicleTripBill]([Id]), 
	CONSTRAINT [FK_VehicleTripBillLedgerPayments_ToLedger] FOREIGN KEY ([LedgerId]) REFERENCES [Ledger]([Id])
)
