CREATE PROCEDURE [dbo].[Insert_TripAdvanceCardPayments]
	@Id INT OUTPUT,
	@MasterId INT,
	@OMCCardId INT,
	@Amount MONEY,
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[TripAdvanceCardPayments]
		(
			[MasterId],
			[OMCCardId],
			[Amount],
			[Remarks],
			[Status]
		) VALUES
		(
			@MasterId,
			@OMCCardId,
			@Amount,
			@Remarks,
			@Status
		)

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[TripAdvanceCardPayments]
		SET
			[MasterId] = @MasterId,
			[OMCCardId] = @OMCCardId,
			[Amount] = @Amount,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id
END