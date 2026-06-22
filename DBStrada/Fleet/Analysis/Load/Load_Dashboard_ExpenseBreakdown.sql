CREATE PROCEDURE [dbo].[Load_Dashboard_ExpenseBreakdown]
	@StartDate DATETIME,
	@EndDate DATETIME
AS
BEGIN
	SELECT
		ExpenseTypeName,
		SUM(ExpenseAmount) AS Total
	FROM
	(
		SELECT ExpenseTypeName, ExpenseAmount FROM TripExpenses_Overview
		WHERE MasterStatus = 1 AND TransactionDateTime BETWEEN @StartDate AND @EndDate

		UNION ALL

		SELECT ExpenseTypeName, ExpenseAmount FROM ExpenseDetails_Overview
		WHERE MasterStatus = 1 AND TransactionDateTime BETWEEN @StartDate AND @EndDate
	) Data
	GROUP BY ExpenseTypeName
	ORDER BY Total DESC;
END
