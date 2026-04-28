CREATE PROCEDURE [dbo].[Insert_VehicleExpenseDetails]
	@Id INT OUTPUT,
	@MasterId INT,
	@VehicleExpenseTypeId INT,
	@LedgerId INT,
	@Amount MONEY,
	@IdentificationNo VARCHAR(MAX),
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[VehicleExpenseDetails]
		(
			[MasterId],
			[VehicleExpenseTypeId],
			[LedgerId],
			[Amount],
			[IdentificationNo],
			[Remarks],
			[Status]
		) VALUES
		(
			@MasterId,
			@VehicleExpenseTypeId,
			@LedgerId,
			@Amount,
			@IdentificationNo,
			@Remarks,
			@Status
		)

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[VehicleExpenseDetails]
		SET
			[MasterId] = @MasterId,
			[VehicleExpenseTypeId] = @VehicleExpenseTypeId,
			[LedgerId] = @LedgerId,
			[Amount] = @Amount,
			[IdentificationNo] = @IdentificationNo,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id
END