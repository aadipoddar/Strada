CREATE PROCEDURE [dbo].[Insert_OMCCardMoneyTransfer]
	@Id INT OUTPUT,
	@TransactionNo VARCHAR(100),
	@CompanyId INT,
	@TransactionDateTime DATETIME,
	@FinancialYearId INT,
	@LedgerId INT,
	@TotalItems INT,
	@TotalAmount MONEY,
	@Remarks VARCHAR(MAX),
	@FinancialAccountingId INT,
	@CreatedBy INT,
	@CreatedAt DATETIME,
	@CreatedFromPlatform VARCHAR(MAX),
	@Status BIT,
	@LastModifiedBy INT,
	@LastModifiedAt DATETIME,
	@LastModifiedFromPlatform VARCHAR(MAX)
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[OMCCardMoneyTransfer]
		(
			[TransactionNo],
			[CompanyId],
			[TransactionDateTime],
			[FinancialYearId],
			[LedgerId],
			[TotalItems],
			[TotalAmount],
			[Remarks],
			[FinancialAccountingId],
			[CreatedBy],
			[CreatedAt],
			[CreatedFromPlatform],
			[Status]
		) VALUES
		(
			@TransactionNo,
			@CompanyId,
			@TransactionDateTime,
			@FinancialYearId,
			@LedgerId,
			@TotalItems,
			@TotalAmount,
			@Remarks,
			@FinancialAccountingId,
			@CreatedBy,
			@CreatedAt,
			@CreatedFromPlatform,
			@Status
		)

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[OMCCardMoneyTransfer]
		SET
			[TransactionNo] = @TransactionNo,
			[CompanyId] = @CompanyId,
			[TransactionDateTime] = @TransactionDateTime,
			[FinancialYearId] = @FinancialYearId,
			[LedgerId] = @LedgerId,
			[TotalItems] = @TotalItems,
			[TotalAmount] = @TotalAmount,
			[Remarks] = @Remarks,
			[FinancialAccountingId] = @FinancialAccountingId,
			[Status] = @Status,
			[LastModifiedBy] = @LastModifiedBy,
			[LastModifiedAt] = @LastModifiedAt,
			[LastModifiedFromPlatform] = @LastModifiedFromPlatform
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id
END