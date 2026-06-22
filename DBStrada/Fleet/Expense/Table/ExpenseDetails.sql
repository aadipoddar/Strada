CREATE TABLE [dbo].[ExpenseDetails]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[ExpenseTypeId] INT NOT NULL,
	[LedgerId] INT NULL,
	[Amount] MONEY NOT NULL,
	[IdentificationNo] VARCHAR(MAX) NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_ExpenseDetails_ToExpense] FOREIGN KEY ([MasterId]) REFERENCES [Expense]([Id]), 
	CONSTRAINT [FK_ExpenseDetails_ToExpenseType] FOREIGN KEY ([ExpenseTypeId]) REFERENCES [ExpenseType]([Id]),
	CONSTRAINT [FK_ExpenseDetails_ToLedger] FOREIGN KEY ([LedgerId]) REFERENCES [Ledger]([Id])
)
