CREATE PROCEDURE [dbo].[Insert_VehicleTripBill]
	@Id INT OUTPUT,
	@TransactionNo VARCHAR(100),
	@CompanyId INT,
	@TransactionDateTime DATETIME,
	@FinancialYearId INT,
	@BillNo VARCHAR(MAX),
	@OMCId INT,
	@TotalGrossAmount MONEY,
	@TotalTDSAmount MONEY,
	@TotalPenaltyAmount MONEY,
	@TotalNetAmount MONEY,
	@TotalCardPaymentAmount MONEY,
	@TotalLedgerPaymentAmount MONEY,
	@Remarks VARCHAR(MAX),
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
		INSERT INTO [dbo].[VehicleTripBill]
		(
			[TransactionNo],
			[CompanyId],
			[TransactionDateTime],
			[FinancialYearId],
			[BillNo],
			[OMCId],
			[TotalGrossAmount],
			[TotalTDSAmount],
			[TotalPenaltyAmount],
			[TotalNetAmount],
			[TotalCardPaymentAmount],
			[TotalLedgerPaymentAmount],
			[Remarks],
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
			@TotalTDSAmount,
			@TotalPenaltyAmount,
			@TotalNetAmount,
			@TotalCardPaymentAmount,
			@TotalLedgerPaymentAmount,
			@Remarks,
			@CreatedBy,
			@CreatedAt,
			@CreatedFromPlatform,
			@Status
		)

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[VehicleTripBill]
		SET
			[TransactionNo] = @TransactionNo,
			[CompanyId] = @CompanyId,
			[TransactionDateTime] = @TransactionDateTime,
			[FinancialYearId] = @FinancialYearId,
			[BillNo] = @BillNo,
			[OMCId] = @OMCId,
			[TotalGrossAmount] = @TotalGrossAmount,
			[TotalTDSAmount] = @TotalTDSAmount,
			[TotalPenaltyAmount] = @TotalPenaltyAmount,
			[TotalNetAmount] = @TotalNetAmount,
			[TotalCardPaymentAmount] = @TotalCardPaymentAmount,
			[TotalLedgerPaymentAmount] = @TotalLedgerPaymentAmount,
			[Remarks] = @Remarks,
			[Status] = @Status,
			[LastModifiedBy] = @LastModifiedBy,
			[LastModifiedAt] = @LastModifiedAt,
			[LastModifiedFromPlatform] = @LastModifiedFromPlatform
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id
END