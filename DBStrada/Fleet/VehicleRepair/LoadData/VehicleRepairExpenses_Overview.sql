CREATE VIEW [dbo].[VehicleRepairExpenses_Overview]
AS
SELECT
    [tr].[Id],
	[tr].[VehicleExpenseTypeId],
	[er].[Name] AS ExpenseTypeName,
	[er].[Code] AS ExpenseTypeCode,
	[tr].[Amount] AS ExpenseAmount,
	[tr].[IdentificationNo],

	[tr].[MasterId],
    [t].[TransactionNo],
    [t].[CompanyId],
    [c].[Name] AS CompanyName,
    [t].[TransactionDateTime],
    [t].[FinancialYearId],
	CONVERT(VARCHAR(10), fy.StartDate, 103) + ' to ' + CONVERT(VARCHAR(10), fy.EndDate, 103) AS FinancialYear,

	[t].[LedgerId],
	[l].[Name] AS LedgerName,
	[t].[VehicleId],
	[v].[Code] AS VehicleCode,
	[t].[TotalExpense],

    [t].[Remarks],
	[t].[CreatedBy],
	[u].[Name] AS CreatedByName,
	[t].[CreatedAt],
	[t].[CreatedFromPlatform],
	[t].[LastModifiedBy],
	[lm].[Name] AS LastModifiedByUserName,
	[t].[LastModifiedAt],
	[t].[LastModifiedFromPlatform]

FROM
    [dbo].[VehicleRepairExpenses] tr
INNER JOIN
	[dbo].[VehicleRepair] t ON tr.MasterId = t.Id
INNER JOIN
	[dbo].[VehicleExpenseType] er ON tr.[VehicleExpenseTypeId] = er.Id
INNER JOIN
    [dbo].[Company] c ON t.CompanyId = c.Id
INNER JOIN
    [dbo].[FinancialYear] fy ON t.FinancialYearId = fy.Id
LEFT JOIN
	[dbo].[Ledger] l ON t.LedgerId = l.Id
INNER JOIN
	[dbo].[Vehicle] v ON t.VehicleId = v.Id
INNER JOIN
	[dbo].[User] AS u ON t.CreatedBy = u.Id
LEFT JOIN
	[dbo].[User] AS lm ON t.LastModifiedBy = lm.Id

WHERE
	[tr].[Status] = 1 AND
	[t].[Status] = 1;