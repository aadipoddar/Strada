CREATE PROCEDURE [dbo].[Insert_TripExpenses]
	@Id INT OUTPUT,
	@MasterId INT,
	@ExpenseTypeId INT,
	@Amount MONEY,
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[TripExpenses]
		(
			[MasterId],
			[ExpenseTypeId],
			[Amount],
			[Remarks],
			[Status]
		) VALUES
		(
			@MasterId,
			@ExpenseTypeId,
			@Amount,
			@Remarks,
			@Status
		)

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[TripExpenses]
		SET
			[MasterId] = @MasterId,
			[ExpenseTypeId] = @ExpenseTypeId,
			[Amount] = @Amount,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id
END