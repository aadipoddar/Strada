CREATE PROCEDURE [dbo].[Load_Trip_By_SlNo_FinancialYear]
	@SlNo VARCHAR(MAX),
	@FinancialYearId INT
AS
BEGIN 
	SELECT *
	FROM [dbo].[Trip]
	WHERE SlNo = @SlNo
		AND FinancialYearId = @FinancialYearId
END
