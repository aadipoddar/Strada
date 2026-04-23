CREATE PROCEDURE [dbo].[Insert_VehicleTripExpenses]
	@Id INT OUTPUT,
	@MasterId INT,
	@VehicleRouteExpenseTypeId INT,
	@Amount MONEY,
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[VehicleTripExpenses]
		(
			[MasterId],
			[VehicleRouteExpenseTypeId],
			[Amount],
			[Remarks],
			[Status]
		) VALUES
		(
			@MasterId,
			@VehicleRouteExpenseTypeId,
			@Amount,
			@Remarks,
			@Status
		)

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[VehicleTripExpenses]
		SET
			[MasterId] = @MasterId,
			[VehicleRouteExpenseTypeId] = @VehicleRouteExpenseTypeId,
			[Amount] = @Amount,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id
END