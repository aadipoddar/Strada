CREATE TABLE [dbo].[TripAdvanceCardPayments]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[OMCCardId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_TripAdvanceCardPayments_ToVehicleTrip] FOREIGN KEY ([MasterId]) REFERENCES [TripAdvance]([Id]), 
	CONSTRAINT [FK_TripAdvanceCardPayments_ToOMCCard] FOREIGN KEY ([OMCCardId]) REFERENCES [OMCCard]([Id])
)
