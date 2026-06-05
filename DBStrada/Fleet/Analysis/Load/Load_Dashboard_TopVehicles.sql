CREATE PROCEDURE [dbo].[Load_Dashboard_TopVehicles]
	@StartDate DATETIME,
	@EndDate DATETIME
AS
BEGIN
	SELECT TOP 10
		VehicleCode,
		ISNULL(SUM(Profit), 0) AS Profit
	FROM
	(
		SELECT VehicleCode, NetAmount - TotalExpense AS Profit FROM Trip_Overview
		WHERE Status = 1 AND TransactionDateTime BETWEEN @StartDate AND @EndDate

		UNION ALL

		SELECT VehicleCode, -TotalExpense FROM Expense_Overview
		WHERE Status = 1 AND TransactionDateTime BETWEEN @StartDate AND @EndDate
	) Data
	GROUP BY VehicleCode
	ORDER BY Profit DESC;
END
