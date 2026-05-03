CREATE VIEW [dbo].[ExpenseDetails_Overview]
AS
SELECT
    [tr].[Id],
	[tr].[ExpenseTypeId],
	[er].[Name] AS ExpenseTypeName,
	[er].[Code] AS ExpenseTypeCode,

	[tr].[LedgerId],
	[l].[Name] AS LedgerName,

	[tr].[Amount] AS ExpenseAmount,
	[tr].[IdentificationNo],
	[tr].[Remarks] AS ExpenseRemarks,

	[tr].[MasterId],
    [r].[TransactionNo],
    [r].[CompanyId],
    [c].[Name] AS CompanyName,

    [r].[TransactionDateTime],
    [r].[FinancialYearId],
	CONVERT(VARCHAR(10), fy.StartDate, 103) + ' to ' + CONVERT(VARCHAR(10), fy.EndDate, 103) AS FinancialYear,

	[r].[VehicleId],
	[v].[Code] AS VehicleCode,
	[r].[TotalExpense],

    [r].[Remarks],
	[r].[CreatedBy],
	[u].[Name] AS CreatedByName,
	[r].[CreatedAt],
	[r].[CreatedFromPlatform],
	[r].[LastModifiedBy],
	[lm].[Name] AS LastModifiedByUserName,
	[r].[LastModifiedAt],
	[r].[LastModifiedFromPlatform],

	[r].[Status]

FROM
    [dbo].[ExpenseDetails] tr
INNER JOIN
	[dbo].[Expense] r ON tr.MasterId = r.Id
INNER JOIN
	[dbo].[ExpenseType] er ON tr.[ExpenseTypeId] = er.Id
LEFT JOIN
	[dbo].[Ledger] l ON tr.LedgerId = l.Id
INNER JOIN
    [dbo].[Company] c ON r.CompanyId = c.Id
INNER JOIN
    [dbo].[FinancialYear] fy ON r.FinancialYearId = fy.Id
INNER JOIN
	[dbo].[Vehicle] v ON r.VehicleId = v.Id
INNER JOIN
	[dbo].[User] AS u ON r.CreatedBy = u.Id
LEFT JOIN
	[dbo].[User] AS lm ON r.LastModifiedBy = lm.Id

WHERE
	[tr].[Status] = 1;