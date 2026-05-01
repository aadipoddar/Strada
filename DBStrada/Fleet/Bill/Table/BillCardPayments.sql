CREATE TABLE [dbo].[BillCardPayments]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[OMCCardId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_BillCardPayments_ToBill] FOREIGN KEY ([MasterId]) REFERENCES [Bill]([Id]), 
	CONSTRAINT [FK_BillCardPayments_ToOMCCard] FOREIGN KEY ([OMCCardId]) REFERENCES [OMCCard]([Id])
)
