CREATE PROCEDURE [dbo].[Insert_Vehicle]
	@Id INT OUTPUT,
	@Code VARCHAR(500),
	@ShortCode VARCHAR(500),
	@ChasisCode VARCHAR(500),
	@EngineCode VARCHAR(500),
	@VehicleTypeId INT,
	@PurchaseDate DATETIME,
	@OpeningHour MONEY = NULL,
	@OpeningKM MONEY = NULL,
	@Remarks VARCHAR(MAX),
	@Status BIT = 1
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