CREATE PROCEDURE [dbo].[Insert_ExpenseDetails]
	@Id INT OUTPUT,
	@MasterId INT,
	@ExpenseTypeId INT,
	@LedgerId INT,
	@Amount MONEY,
	@IdentificationNo VARCHAR(MAX),
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[ExpenseDetails]
		(
			[MasterId],
			[ExpenseTypeId],
			[LedgerId],
			[Amount],
			[IdentificationNo],
			[Remarks],
			[Status]
		) VALUES
		(
			@MasterId,
			@ExpenseTypeId,
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
		UPDATE [dbo].[ExpenseDetails]
		SET
			[MasterId] = @MasterId,
			[ExpenseTypeId] = @ExpenseTypeId,
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