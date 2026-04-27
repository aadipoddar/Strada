CREATE PROCEDURE [dbo].[Load_VehicleTrip_Overview_By_BillId_Date]
	@BillId INT = NULL,
	@StartDate DATETIME = NULL,
	@EndDate DATETIME = NULL
AS
BEGIN
	SELECT *
	FROM [dbo].[VehicleTrip_Overview]
	WHERE
		((@BillId IS NULL AND BillId IS NULL) OR BillId = @BillId)
		AND (@StartDate IS NULL OR TransactionDateTime >= CAST(@StartDate AS DATE))
		AND (@EndDate IS NULL OR TransactionDateTime < DATEADD(DAY, 1, CAST(@EndDate AS DATE)))
		AND Status = 1
END