CREATE PROCEDURE [dbo].[Insert_VehicleRepairExpenses]
	@Id INT OUTPUT,
	@MasterId INT,
	@VehicleExpenseTypeId INT,
	@Amount MONEY,
	@IdentificationNo VARCHAR(MAX),
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[VehicleRepairExpenses]
		(
			[MasterId],
			[VehicleExpenseTypeId],
			[Amount],
			[IdentificationNo],
			[Remarks],
			[Status]
		) VALUES
		(
			@MasterId,
			@VehicleExpenseTypeId,
			@Amount,
			@IdentificationNo,
			@Remarks,
			@Status
		)

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[VehicleRepairExpenses]
		SET
			[MasterId] = @MasterId,
			[VehicleExpenseTypeId] = @VehicleExpenseTypeId,
			[Amount] = @Amount,
			[IdentificationNo] = @IdentificationNo,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id
END