CREATE PROCEDURE [dbo].[Insert_VehicleTripExpenses]
	@Id INT OUTPUT,
	@MasterId INT,
	@VehicleExpenseTypeId INT,
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
			[VehicleExpenseTypeId],
			[Amount],
			[Remarks],
			[Status]
		) VALUES
		(
			@MasterId,
			@VehicleExpenseTypeId,
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
			[VehicleExpenseTypeId] = @VehicleExpenseTypeId,
			[Amount] = @Amount,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id
END