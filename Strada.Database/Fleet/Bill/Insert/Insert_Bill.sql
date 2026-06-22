CREATE PROCEDURE [dbo].[Insert_Bill]
	@Id INT OUTPUT,
	@TransactionNo VARCHAR(100),
	@CompanyId INT,
	@TransactionDateTime DATETIME,
	@FinancialYearId INT,
	@BillNo VARCHAR(MAX),
	@OMCId INT,
	@TotalGrossAmount MONEY,
	@TotalPenaltyAmount MONEY,
	@TotalNetAmount MONEY,
	@TotalLedgerPaymentAmount MONEY,
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
		INSERT INTO [dbo].[Bill]
		(
			[TransactionNo],
			[CompanyId],
			[TransactionDateTime],
			[FinancialYearId],
			[BillNo],
			[OMCId],
			[TotalGrossAmount],
			[TotalPenaltyAmount],
			[TotalNetAmount],
			[TotalLedgerPaymentAmount],
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
			@BillNo,
			@OMCId,
			@TotalGrossAmount,
			@TotalPenaltyAmount,
			@TotalNetAmount,
			@TotalLedgerPaymentAmount,
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
		UPDATE [dbo].[Bill]
		SET
			[TransactionNo] = @TransactionNo,
			[CompanyId] = @CompanyId,
			[TransactionDateTime] = @TransactionDateTime,
			[FinancialYearId] = @FinancialYearId,
			[BillNo] = @BillNo,
			[OMCId] = @OMCId,
			[TotalGrossAmount] = @TotalGrossAmount,
			[TotalPenaltyAmount] = @TotalPenaltyAmount,
			[TotalNetAmount] = @TotalNetAmount,
			[TotalLedgerPaymentAmount] = @TotalLedgerPaymentAmount,
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