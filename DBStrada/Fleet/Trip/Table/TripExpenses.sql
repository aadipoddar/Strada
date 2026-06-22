CREATE TABLE [dbo].[TripExpenses]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[ExpenseTypeId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_TripExpenses_ToTrip] FOREIGN KEY ([MasterId]) REFERENCES [Trip]([Id]), 
	CONSTRAINT [FK_TripExpenses_ToExpenseType] FOREIGN KEY ([ExpenseTypeId]) REFERENCES [ExpenseType]([Id])
)
