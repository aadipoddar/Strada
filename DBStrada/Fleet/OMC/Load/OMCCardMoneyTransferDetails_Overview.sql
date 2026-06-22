CREATE VIEW [dbo].[OMCCardMoneyTransferDetails_Overview]
AS
SELECT
    [tr].[Id],
	[tr].[OMCCardId],
	[oc].[CardNumber] AS OMCCardNumber,
	[oc].[Code] AS OMCCardCode,

	[oc].[OMCId],
	[o].[Name] AS OMCName,

	[tr].[Amount] AS TransferAmount,
	[tr].[Remarks] AS TransferRemarks,

	[tr].[MasterId],
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

	[r].[Status] AS MasterStatus

FROM
    [dbo].[OMCCardMoneyTransferDetails] tr
INNER JOIN
	[dbo].[OMCCardMoneyTransfer] r ON tr.MasterId = r.Id
INNER JOIN
	[dbo].[OMCCard] oc ON tr.[OMCCardId] = oc.Id
INNER JOIN
	[dbo].[OMC] o ON oc.OMCId = o.Id
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

WHERE
	[tr].[Status] = 1;