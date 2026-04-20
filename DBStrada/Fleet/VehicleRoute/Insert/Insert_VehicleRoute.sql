CREATE PROCEDURE [dbo].[Insert_VehicleRoute]
	@Id INT OUTPUT,
	@FromLocationId INT,
	@ToLocationId INT,
	@Code VARCHAR(10),
	@Remarks VARCHAR(MAX),
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[VehicleRoute]
		(
			[FromLocationId],
			[ToLocationId],
			[Code],
			[Remarks],
			[Status]
		)
		VALUES
		(
			@FromLocationId,
			@ToLocationId,
			@Code,
			@Remarks,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[VehicleRoute]
		SET
			[FromLocationId] = @FromLocationId,
			[ToLocationId] = @ToLocationId,
			[Code] = @Code,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id;
END