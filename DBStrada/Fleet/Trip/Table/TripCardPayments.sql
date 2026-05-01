CREATE TABLE [dbo].[TripCardPayments]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[OMCCardId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_TripCardPayments_ToTrip] FOREIGN KEY ([MasterId]) REFERENCES [Trip]([Id]), 
	CONSTRAINT [FK_TripCardPayments_ToOMCCard] FOREIGN KEY ([OMCCardId]) REFERENCES [OMCCard]([Id])
)
