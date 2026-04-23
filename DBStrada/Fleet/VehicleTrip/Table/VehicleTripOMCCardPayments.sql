CREATE TABLE [dbo].[VehicleTripOMCCardPayments]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[OMCCardId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_VehicleTripOMCCardPayments_ToVehicleTrip] FOREIGN KEY ([MasterId]) REFERENCES [VehicleTrip]([Id]), 
	CONSTRAINT [FK_VehicleTripOMCCardPayments_ToOMCCard] FOREIGN KEY ([OMCCardId]) REFERENCES [OMCCard]([Id])
)
