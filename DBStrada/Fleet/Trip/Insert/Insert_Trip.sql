CREATE PROCEDURE [dbo].[Insert_Trip]
	@Id INT OUTPUT,
	@TransactionNo VARCHAR(100),
	@CompanyId INT,
	@TransactionDateTime DATETIME,
	@FinancialYearId INT,
	@ChallanNo VARCHAR(MAX),
	@OMCId INT,
	@VehicleId INT,
	@DriverId INT,
	@RouteId INT,
	@Quantity MONEY,
	@TotalExpense MONEY,
	@VehicleEmpty BIT,
	@BillId INT,
	@GrossAmount MONEY,
	@TDSAmount MONEY,
	@PenaltyAmount MONEY,
	@NetAmount MONEY,
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
		INSERT INTO [dbo].[Trip]
		(
			[TransactionNo],
			[CompanyId],
			[TransactionDateTime],
			[FinancialYearId],
			[ChallanNo],
			[OMCId],
			[VehicleId],
			[DriverId],
			[RouteId],
			[Quantity],
			[TotalExpense],
			[VehicleEmpty],
			[BillId],
			[GrossAmount],
			[TDSAmount],
			[PenaltyAmount],
			[NetAmount],
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
			@ChallanNo,
			@OMCId,
			@VehicleId,
			@DriverId,
			@RouteId,
			@Quantity,
			@TotalExpense,
			@VehicleEmpty,
			@BillId,
			@GrossAmount,
			@TDSAmount,
			@PenaltyAmount,
			@NetAmount,
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
		UPDATE [dbo].[Trip]
		SET
			[TransactionNo] = @TransactionNo,
			[CompanyId] = @CompanyId,
			[TransactionDateTime] = @TransactionDateTime,
			[FinancialYearId] = @FinancialYearId,
			[ChallanNo] = @ChallanNo,
			[OMCId] = @OMCId,
			[VehicleId] = @VehicleId,
			[DriverId] = @DriverId,
			[RouteId] = @RouteId,
			[Quantity] = @Quantity,
			[TotalExpense] = @TotalExpense,
			[VehicleEmpty] = @VehicleEmpty,
			[BillId] = @BillId,
			[GrossAmount] = @GrossAmount,
			[TDSAmount] = @TDSAmount,
			[PenaltyAmount] = @PenaltyAmount,
			[NetAmount] = @NetAmount,
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