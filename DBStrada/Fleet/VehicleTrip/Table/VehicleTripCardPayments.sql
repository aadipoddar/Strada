CREATE TABLE [dbo].[VehicleTripCardPayments]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[OMCCardId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_VehicleTripCardPayments_ToVehicleTrip] FOREIGN KEY ([MasterId]) REFERENCES [VehicleTrip]([Id]), 
	CONSTRAINT [FK_VehicleTripCardPayments_ToOMCCard] FOREIGN KEY ([OMCCardId]) REFERENCES [OMCCard]([Id])
)
