CREATE TABLE [dbo].[VehicleTripBillCardPayments]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
	[OMCCardId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Remarks] VARCHAR(MAX) NULL, 
	[Status] BIT NOT NULL DEFAULT 1, 
	CONSTRAINT [FK_VehicleTripBillCardPayments_ToVehicleTripBill] FOREIGN KEY ([MasterId]) REFERENCES [VehicleTripBill]([Id]), 
	CONSTRAINT [FK_VehicleTripBillCardPayments_ToOMCCard] FOREIGN KEY ([OMCCardId]) REFERENCES [OMCCard]([Id])
)
