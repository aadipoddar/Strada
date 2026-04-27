CREATE VIEW [dbo].[VehicleTripBill_Overview]
AS
SELECT
    [t].[Id],
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
	[t].[TotalTDSAmount],
	[t].[TotalPenaltyAmount],
	[t].[TotalNetAmount],
	[t].[TotalCardPaymentAmount],
	[t].[TotalLedgerPaymentAmount],

    [t].[Remarks],
	[t].[CreatedBy],
	[u].[Name] AS CreatedByName,
	[t].[CreatedAt],
	[t].[CreatedFromPlatform],
	[t].[LastModifiedBy],
	[lm].[Name] AS LastModifiedByUserName,
	[t].[LastModifiedAt],
	[t].[LastModifiedFromPlatform],

	[t].[Status]

FROM
    [dbo].[VehicleTripBill] t
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