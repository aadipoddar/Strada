CREATE VIEW [dbo].[OMCCardMoneyTransfer_Overview]
AS
SELECT
    [r].[Id],
    [r].[TransactionNo],
    [r].[CompanyId],
    [c].[Name] AS CompanyName,

    [r].[TransactionDateTime],
    [r].[FinancialYearId],
	CONVERT(VARCHAR(10), fy.StartDate, 103) + ' to ' + CONVERT(VARCHAR(10), fy.EndDate, 103) AS FinancialYear,

	[r].[LedgerId],
	[l].[Name] AS LedgerName,
	[r].[TotalItems],
	[r].[TotalAmount],

    [r].[Remarks],
	[r].[FinancialAccountingId],
	[fa].[TransactionNo] AS FinancialAccountingTransactionNo,
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
    [dbo].[OMCCardMoneyTransfer] r
INNER JOIN
    [dbo].[Company] c ON r.CompanyId = c.Id
INNER JOIN
    [dbo].[FinancialYear] fy ON r.FinancialYearId = fy.Id
INNER JOIN
	[dbo].[Ledger] l ON r.LedgerId = l.Id
LEFT JOIN
	[dbo].[FinancialAccounting] fa ON r.FinancialAccountingId = fa.Id
INNER JOIN
	[dbo].[User] AS u ON r.CreatedBy = u.Id
LEFT JOIN
	[dbo].[User] AS lm ON r.LastModifiedBy = lm.Id