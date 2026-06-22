CREATE PROCEDURE [dbo].[Insert_OMCCard]
	@Id INT OUTPUT,
	@CardNumber VARCHAR(250),
	@Code VARCHAR(10),
	@OMCId INT,
	@LedgerId INT,
	@CurrentBalance MONEY,
	@Remarks VARCHAR(MAX),
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[OMCCard]
		(
			[CardNumber],
			[Code],
			[OMCId],
			[LedgerId],
			[CurrentBalance],
			[Remarks],
			[Status]
		)
		VALUES
		(
			@CardNumber,
			@Code,
			@OMCId,
			@LedgerId,
			@CurrentBalance,
			@Remarks,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[OMCCard]
		SET
			[CardNumber] = @CardNumber,
			[Code] = @Code,
			[OMCId] = @OMCId,
			[LedgerId] = @LedgerId,
			[CurrentBalance] = @CurrentBalance,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id;
END