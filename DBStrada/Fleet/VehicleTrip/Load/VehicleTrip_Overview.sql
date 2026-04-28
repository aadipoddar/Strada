CREATE VIEW [dbo].[VehicleTrip_Overview]
AS
SELECT
    [t].[Id],
    [t].[TransactionNo],
    [t].[CompanyId],
    [c].[Name] AS CompanyName,

    [t].[TransactionDateTime],
    [t].[FinancialYearId],
	CONVERT(VARCHAR(10), fy.StartDate, 103) + ' to ' + CONVERT(VARCHAR(10), fy.EndDate, 103) AS FinancialYear,

    [t].[ChallanNo],
	[t].[OMCId],
	[o].[Name] AS OMCName,
	[t].[VehicleId],
	[v].[Code] AS VehicleCode,
	[t].[RouteId],
	[frl].[Name] AS FromLocation,
	[torl].[Name] AS ToLocation,
	[frl].[Name] + ' to ' + [torl].[Name] AS RouteDisplay,
	[t].[DriverId],
	[d].[Name] AS DriverName,
	[d].[Mobile] AS DriverMobile,
	[d].[Name] + ' (' + [d].[Mobile] + ')' AS DriverDisplay,
	
	[t].[Quantity],
	[r].[EstimatedDistance],
	[r].[EstimatedHours],
	[r].[EstimatedFuelConsumption],
	[r].[EstimatedCost],
	[t].[TotalExpense],

	[t].[VehicleEmpty],

	[t].[BillId],
	[b].[BillNo],
	[b].[TransactionDateTime] AS BillDateTime,
	[t].[GrossAmount],
	[t].[TDSAmount],
	[t].[PenaltyAmount],
	[t].[NetAmount],

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
    [dbo].[VehicleTrip] t
INNER JOIN
    [dbo].[Company] c ON t.CompanyId = c.Id
INNER JOIN
    [dbo].[FinancialYear] fy ON t.FinancialYearId = fy.Id
INNER JOIN
	[dbo].[OMC] o ON t.OMCId = o.Id
INNER JOIN
	[dbo].[Vehicle] v ON t.VehicleId = v.Id
INNER JOIN
	[dbo].[VehicleDriver] d ON t.DriverId = d.Id
INNER JOIN
	[dbo].[VehicleRoute] r ON t.RouteId = r.Id
INNER JOIN
	[dbo].[VehicleRouteLocation] frl ON r.FromLocationId = frl.Id
INNER JOIN
	[dbo].[VehicleRouteLocation] torl ON r.ToLocationId = torl.Id
LEFT JOIN
	[dbo].[VehicleTripBill] b ON t.BillId = b.Id
INNER JOIN
	[dbo].[User] AS u ON t.CreatedBy = u.Id
LEFT JOIN
	[dbo].[User] AS lm ON t.LastModifiedBy = lm.Id