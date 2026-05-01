CREATE TABLE [dbo].[BillLedgerPayments]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[LedgerId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_BillLedgerPayments_ToBill] FOREIGN KEY ([MasterId]) REFERENCES [Bill]([Id]), 
	CONSTRAINT [FK_BillLedgerPayments_ToLedger] FOREIGN KEY ([LedgerId]) REFERENCES [Ledger]([Id])
)
