CREATE VIEW [dbo].[BillLedgerPayments_Overview]
AS
SELECT
    [tp].[Id],
	[tp].[LedgerId],
	[l].[Name] AS LedgerName,
	[l].[Code] AS LedgerCode,
	[tp].[Amount] AS PaymentAmount,
	[tp].[Remarks] AS PaymentRemarks,

	[tp].[MasterId],
	[t].[TransactionNo],
    [t].[CompanyId],
    [c].[Name] AS CompanyName,

    [t].[TransactionDateTime],
    [t].[FinancialYearId],
	CONVERT(VARCHAR(10), fy.StartDate, 103) + ' to ' + CONVERT(VARCHAR(10), fy.EndDate, 103) AS FinancialYear,

    [t].[BillNo],
	[t].[OMCId],
	[o].[Name] AS OMCName,
	
	[t].[TotalGrossAmount],
	[t].[TotalPenaltyAmount],
	[t].[TotalNetAmount],
	[t].[TotalLedgerPaymentAmount],

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
    [dbo].[BillLedgerPayments] tp
INNER JOIN
	[dbo].[Bill] t ON tp.MasterId = t.Id
INNER JOIN
	[dbo].[Ledger] l ON tp.LedgerId = l.Id
INNER JOIN
    [dbo].[Company] c ON t.CompanyId = c.Id
INNER JOIN
    [dbo].[FinancialYear] fy ON t.FinancialYearId = fy.Id
INNER JOIN
	[dbo].[OMC] o ON t.OMCId = o.Id
INNER JOIN
	[dbo].[User] AS u ON t.CreatedBy = u.Id
LEFT JOIN
	[dbo].[User] AS lm ON t.LastModifiedBy = lm.Id

WHERE
	[tp].[Status] = 1 AND
	[t].[Status] = 1;