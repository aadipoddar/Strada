CREATE PROCEDURE [dbo].[Insert_VehicleRoute]
	@Id INT OUTPUT,
	@FromLocationId INT,
	@ToLocationId INT,
	@Code VARCHAR(10),
	@EstimatedHours INT,
	@EstimatedDistance INT,
	@EstimatedFuelConsumption INT,
	@EstimatedCost MONEY,
	@Remarks VARCHAR(MAX),
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[VehicleRoute]
		(
			[FromLocationId],
			[ToLocationId],
			[Code],
			[EstimatedHours],
			[EstimatedDistance],
			[EstimatedFuelConsumption],
			[EstimatedCost],
			[Remarks],
			[Status]
		)
		VALUES
		(
			@FromLocationId,
			@ToLocationId,
			@Code,
			@EstimatedHours,
			@EstimatedDistance,
			@EstimatedFuelConsumption,
			@EstimatedCost,
			@Remarks,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[VehicleRoute]
		SET
			[FromLocationId] = @FromLocationId,
			[ToLocationId] = @ToLocationId,
			[Code] = @Code,
			[EstimatedHours] = @EstimatedHours,
			[EstimatedDistance] = @EstimatedDistance,
			[EstimatedFuelConsumption] = @EstimatedFuelConsumption,
			[EstimatedCost] = @EstimatedCost,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id;
END