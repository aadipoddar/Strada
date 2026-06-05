CREATE PROCEDURE [dbo].[Load_Dashboard_MonthlyTrend]
	@StartDate DATETIME,
	@EndDate DATETIME
AS
BEGIN
	SELECT
		YEAR(TransactionDateTime) AS Year,
		MONTH(TransactionDateTime) AS Month,
		ISNULL(SUM(Revenue), 0) AS Revenue,
		ISNULL(SUM(Expense), 0) AS Expense
	FROM
	(
		SELECT TransactionDateTime, NetAmount AS Revenue, TotalExpense AS Expense FROM Trip_Overview
		WHERE Status = 1 AND TransactionDateTime BETWEEN @StartDate AND @EndDate

		UNION ALL

		SELECT TransactionDateTime, 0, TotalExpense FROM Expense_Overview
		WHERE Status = 1 AND TransactionDateTime BETWEEN @StartDate AND @EndDate
	) Data
	GROUP BY YEAR(TransactionDateTime), MONTH(TransactionDateTime);
END
