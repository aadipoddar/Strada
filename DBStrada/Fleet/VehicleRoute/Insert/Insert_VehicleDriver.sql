CREATE PROCEDURE [dbo].[Insert_VehicleDriver]
	@Id INT OUTPUT,
	@Name VARCHAR(MAX),
	@Mobile VARCHAR(10),
	@Code VARCHAR(10),
	@Remarks VARCHAR(MAX),
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[VehicleDriver]
		(
			[Name],
			[Mobile],
			[Code],
			[Remarks],
			[Status]
		)
		VALUES
		(
			@Name,
			@Mobile,
			@Code,
			@Remarks,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE [dbo].[VehicleDriver]
		SET
			[Name] = @Name,
			[Mobile] = @Mobile,
			[Code] = @Code,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id;
END