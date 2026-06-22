CREATE TABLE [dbo].[TripLedgerPayments]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[LedgerId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_TripLedgerPayments_ToTrip] FOREIGN KEY ([MasterId]) REFERENCES [Trip]([Id]), 
	CONSTRAINT [FK_TripLedgerPayments_ToLedger] FOREIGN KEY ([LedgerId]) REFERENCES [Ledger]([Id])
)
