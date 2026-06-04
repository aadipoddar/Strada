CREATE TABLE [dbo].[OMCCardMoneyTransferDetails]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[OMCCardId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_OMCCardMoneyTransferDetails_ToMaster] FOREIGN KEY ([MasterId]) REFERENCES [OMCCardMoneyTransfer]([Id]),
	CONSTRAINT [FK_OMCCardMoneyTransferDetails_ToOMCCard] FOREIGN KEY ([OMCCardId]) REFERENCES [OMCCard]([Id])
)
