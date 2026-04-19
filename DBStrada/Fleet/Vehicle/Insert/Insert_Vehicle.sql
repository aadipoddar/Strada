CREATE PROCEDURE [dbo].[Insert_Vehicle]
	@Id INT OUTPUT,
	@Code VARCHAR(250),
	@ShortCode VARCHAR(250),
	@ChasisCode VARCHAR(250),
	@EngineCode VARCHAR(250),
	@VehicleTypeId INT,
	@PurchaseDate DATETIME,
	@OpeningHour MONEY,
	@OpeningKM MONEY,
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[Vehicle]
		(
			[Code],
			[ShortCode],
			[ChasisCode],
			[EngineCode],
			[VehicleTypeId],
			[PurchaseDate],
			[OpeningHour],
			[OpeningKM],
			[Remarks],
			[Status]
		)
		VALUES
		(
			@Code,
			@ShortCode,
			@ChasisCode,
			@EngineCode,
			@VehicleTypeId,
			@PurchaseDate,
			@OpeningHour,
			@OpeningKM,
			@Remarks,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[Vehicle]
		SET
			[Code] = @Code,
			[ShortCode] = @ShortCode,
			[ChasisCode] = @ChasisCode,
			[EngineCode] = @EngineCode,
			[VehicleTypeId] = @VehicleTypeId,
			[PurchaseDate] = @PurchaseDate,
			[OpeningHour] = @OpeningHour,
			[OpeningKM] = @OpeningKM,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id
END