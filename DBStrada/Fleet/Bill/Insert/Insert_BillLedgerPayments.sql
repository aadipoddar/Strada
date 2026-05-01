CREATE PROCEDURE [dbo].[Insert_BillLedgerPayments]
	@Id INT OUTPUT,
	@MasterId INT,
	@LedgerId INT,
	@Amount MONEY,
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[BillLedgerPayments]
		(
			[MasterId],
			[LedgerId],
			[Amount],
			[Remarks],
			[Status]
		) VALUES
		(
			@MasterId,
			@LedgerId,
			@Amount,
			@Remarks,
			@Status
		)

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[BillLedgerPayments]
		SET
			[MasterId] = @MasterId,
			[LedgerId] = @LedgerId,
			[Amount] = @Amount,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id
END