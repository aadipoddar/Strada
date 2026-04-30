using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.Route;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleTrip;
using StradaLibrary.Models.Fleet.VehicleTripBill;

namespace StradaLibrary.Exports.Fleet.VehicleTrip;

public static class VehicleTripReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<VehicleTripOverviewModel> tripData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showDeleted = false,
		CompanyModel company = null,
		OMCModel omc = null,
		VehicleModel vehicle = null,
		RouteOverviewModel route = null,
		DriverOverviewModel driver = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleTripOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleTripOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripOverviewModel.VehicleEmpty)] = new() { DisplayName = "Vehicle Empty", Alignment = CellAlignment.Center, IncludeInTotal = false },

			[nameof(VehicleTripOverviewModel.BillNo)] = new() { DisplayName = "Bill No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.BillDateTime)] = new() { DisplayName = "Bill Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.GrossAmount)] = new() { DisplayName = "Gross Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.TDSAmount)] = new() { DisplayName = "TDS Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.PenaltyAmount)] = new() { DisplayName = "Penalty Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.NetAmount)] = new() { DisplayName = "Net Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripOverviewModel.ProfitLoss)] = new() { DisplayName = "P&L", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOverviewModel.PendingDays)] = new() { DisplayName = "Pending Days", Format = "0", Alignment = CellAlignment.Right, IncludeInTotal = false },

			[nameof(VehicleTripOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleTripOverviewModel.VehicleCode),
				nameof(VehicleTripOverviewModel.CompanyName),
				nameof(VehicleTripOverviewModel.TransactionDateTime),
				nameof(VehicleTripOverviewModel.FinancialYear),
				nameof(VehicleTripOverviewModel.ChallanNo),
				nameof(VehicleTripOverviewModel.OMCName),
				nameof(VehicleTripOverviewModel.FromLocation),
				nameof(VehicleTripOverviewModel.ToLocation),
				nameof(VehicleTripOverviewModel.RouteDisplay),
				nameof(VehicleTripOverviewModel.DriverName),
				nameof(VehicleTripOverviewModel.DriverMobile),
				nameof(VehicleTripOverviewModel.DriverDisplay),
				nameof(VehicleTripOverviewModel.Quantity),
				nameof(VehicleTripOverviewModel.EstimatedDistance),
				nameof(VehicleTripOverviewModel.EstimatedHours),
				nameof(VehicleTripOverviewModel.EstimatedFuelConsumption),
				nameof(VehicleTripOverviewModel.EstimatedCost),
				nameof(VehicleTripOverviewModel.TotalExpense),
				nameof(VehicleTripOverviewModel.VehicleEmpty),
				nameof(VehicleTripOverviewModel.BillNo),
				nameof(VehicleTripOverviewModel.BillDateTime),
				nameof(VehicleTripOverviewModel.GrossAmount),
				nameof(VehicleTripOverviewModel.TDSAmount),
				nameof(VehicleTripOverviewModel.PenaltyAmount),
				nameof(VehicleTripOverviewModel.NetAmount),
				nameof(VehicleTripOverviewModel.ProfitLoss),
				nameof(VehicleTripOverviewModel.PendingDays),
				nameof(VehicleTripOverviewModel.TransactionNo),
				nameof(VehicleTripOverviewModel.Remarks),
				nameof(VehicleTripOverviewModel.CreatedByName),
				nameof(VehicleTripOverviewModel.CreatedAt),
				nameof(VehicleTripOverviewModel.CreatedFromPlatform),
				nameof(VehicleTripOverviewModel.LastModifiedByUserName),
				nameof(VehicleTripOverviewModel.LastModifiedAt),
				nameof(VehicleTripOverviewModel.LastModifiedFromPlatform),
				nameof(VehicleTripOverviewModel.Status)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.Status));
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleTripOverviewModel.VehicleCode),
				nameof(VehicleTripOverviewModel.CompanyName),
				nameof(VehicleTripOverviewModel.TransactionDateTime),
				nameof(VehicleTripOverviewModel.ChallanNo),
				nameof(VehicleTripOverviewModel.OMCName),
				nameof(VehicleTripOverviewModel.RouteDisplay),
				nameof(VehicleTripOverviewModel.DriverDisplay),
				nameof(VehicleTripOverviewModel.Quantity),
				nameof(VehicleTripOverviewModel.TotalExpense),
				nameof(VehicleTripOverviewModel.VehicleEmpty),
				nameof(VehicleTripOverviewModel.BillNo),
				nameof(VehicleTripOverviewModel.BillDateTime),
				nameof(VehicleTripOverviewModel.GrossAmount),
				nameof(VehicleTripOverviewModel.TDSAmount),
				nameof(VehicleTripOverviewModel.PenaltyAmount),
				nameof(VehicleTripOverviewModel.NetAmount),
				nameof(VehicleTripOverviewModel.ProfitLoss),
				nameof(VehicleTripOverviewModel.PendingDays),
				nameof(VehicleTripOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.RouteDisplay));

			if (driver is not null)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.DriverDisplay));

			if (!showDeleted)
				columnOrder.Remove(nameof(VehicleTripOverviewModel.Status));
		}

		string fileName = $"VEHICLE_TRIP_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				tripData,
				"VEHICLE TRIP REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
					["Route"] = route?.RouteDisplay ?? null,
					["Driver"] = driver?.DisplayName ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				tripData,
				"VEHICLE TRIP REPORT",
				"Vehicle Trip Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
					["Route"] = route?.RouteDisplay ?? null,
					["Driver"] = driver?.DisplayName ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}

	public static async Task<(MemoryStream stream, string fileName)> ExportExpensesReport(
		IEnumerable<VehicleTripExpensesOverviewModel> expensesData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		CompanyModel company = null,
		OMCModel omc = null,
		VehicleModel vehicle = null,
		RouteOverviewModel route = null,
		DriverOverviewModel driver = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleTripExpensesOverviewModel.ExpenseTypeName)] = new() { DisplayName = "Expense Type", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.ExpenseTypeCode)] = new() { DisplayName = "Expense Type Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.ExpenseAmount)] = new() { DisplayName = "Expense Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripExpensesOverviewModel.ExpenseRemarks)] = new() { DisplayName = "Expense Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleTripExpensesOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleTripExpensesOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripExpensesOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripExpensesOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripExpensesOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripExpensesOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripExpensesOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripExpensesOverviewModel.VehicleEmpty)] = new() { DisplayName = "Vehicle Empty", Alignment = CellAlignment.Center, IncludeInTotal = false },

			[nameof(VehicleTripExpensesOverviewModel.BillNo)] = new() { DisplayName = "Bill No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.BillDateTime)] = new() { DisplayName = "Bill Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.GrossAmount)] = new() { DisplayName = "Gross Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripExpensesOverviewModel.TDSAmount)] = new() { DisplayName = "TDS Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripExpensesOverviewModel.PenaltyAmount)] = new() { DisplayName = "Penalty Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripExpensesOverviewModel.NetAmount)] = new() { DisplayName = "Net Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripExpensesOverviewModel.ProfitLoss)] = new() { DisplayName = "P&L", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripExpensesOverviewModel.PendingDays)] = new() { DisplayName = "Pending Days", Format = "0", Alignment = CellAlignment.Right, IncludeInTotal = false },

			[nameof(VehicleTripExpensesOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleTripExpensesOverviewModel.ExpenseTypeName),
				nameof(VehicleTripExpensesOverviewModel.ExpenseTypeCode),
				nameof(VehicleTripExpensesOverviewModel.ExpenseAmount),
				nameof(VehicleTripExpensesOverviewModel.ExpenseRemarks),
				nameof(VehicleTripExpensesOverviewModel.VehicleCode),
				nameof(VehicleTripExpensesOverviewModel.CompanyName),
				nameof(VehicleTripExpensesOverviewModel.TransactionDateTime),
				nameof(VehicleTripExpensesOverviewModel.FinancialYear),
				nameof(VehicleTripExpensesOverviewModel.ChallanNo),
				nameof(VehicleTripExpensesOverviewModel.OMCName),
				nameof(VehicleTripExpensesOverviewModel.FromLocation),
				nameof(VehicleTripExpensesOverviewModel.ToLocation),
				nameof(VehicleTripExpensesOverviewModel.RouteDisplay),
				nameof(VehicleTripExpensesOverviewModel.DriverName),
				nameof(VehicleTripExpensesOverviewModel.DriverMobile),
				nameof(VehicleTripExpensesOverviewModel.DriverDisplay),
				nameof(VehicleTripExpensesOverviewModel.Quantity),
				nameof(VehicleTripExpensesOverviewModel.EstimatedDistance),
				nameof(VehicleTripExpensesOverviewModel.EstimatedHours),
				nameof(VehicleTripExpensesOverviewModel.EstimatedFuelConsumption),
				nameof(VehicleTripExpensesOverviewModel.EstimatedCost),
				nameof(VehicleTripExpensesOverviewModel.TotalExpense),
				nameof(VehicleTripExpensesOverviewModel.VehicleEmpty),
				nameof(VehicleTripExpensesOverviewModel.BillNo),
				nameof(VehicleTripExpensesOverviewModel.BillDateTime),
				nameof(VehicleTripExpensesOverviewModel.GrossAmount),
				nameof(VehicleTripExpensesOverviewModel.TDSAmount),
				nameof(VehicleTripExpensesOverviewModel.PenaltyAmount),
				nameof(VehicleTripExpensesOverviewModel.NetAmount),
				nameof(VehicleTripExpensesOverviewModel.ProfitLoss),
				nameof(VehicleTripExpensesOverviewModel.PendingDays),
				nameof(VehicleTripExpensesOverviewModel.TransactionNo),
				nameof(VehicleTripExpensesOverviewModel.Remarks),
				nameof(VehicleTripExpensesOverviewModel.CreatedByName),
				nameof(VehicleTripExpensesOverviewModel.CreatedAt),
				nameof(VehicleTripExpensesOverviewModel.CreatedFromPlatform),
				nameof(VehicleTripExpensesOverviewModel.LastModifiedByUserName),
				nameof(VehicleTripExpensesOverviewModel.LastModifiedAt),
				nameof(VehicleTripExpensesOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleTripExpensesOverviewModel.ExpenseTypeName),
				nameof(VehicleTripExpensesOverviewModel.ExpenseAmount),
				nameof(VehicleTripExpensesOverviewModel.VehicleCode),
				nameof(VehicleTripExpensesOverviewModel.CompanyName),
				nameof(VehicleTripExpensesOverviewModel.TransactionDateTime),
				nameof(VehicleTripExpensesOverviewModel.ChallanNo),
				nameof(VehicleTripExpensesOverviewModel.OMCName),
				nameof(VehicleTripExpensesOverviewModel.RouteDisplay),
				nameof(VehicleTripExpensesOverviewModel.DriverDisplay),
				nameof(VehicleTripExpensesOverviewModel.Quantity),
				nameof(VehicleTripExpensesOverviewModel.TotalExpense),
				nameof(VehicleTripExpensesOverviewModel.VehicleEmpty),
				nameof(VehicleTripExpensesOverviewModel.BillNo),
				nameof(VehicleTripExpensesOverviewModel.BillDateTime),
				nameof(VehicleTripExpensesOverviewModel.GrossAmount),
				nameof(VehicleTripExpensesOverviewModel.TDSAmount),
				nameof(VehicleTripExpensesOverviewModel.PenaltyAmount),
				nameof(VehicleTripExpensesOverviewModel.NetAmount),
				nameof(VehicleTripExpensesOverviewModel.ProfitLoss),
				nameof(VehicleTripExpensesOverviewModel.PendingDays)
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleTripExpensesOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(VehicleTripExpensesOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(VehicleTripExpensesOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(VehicleTripExpensesOverviewModel.RouteDisplay));

			if (driver is not null)
				columnOrder.Remove(nameof(VehicleTripExpensesOverviewModel.DriverDisplay));
		}

		string fileName = $"VEHICLE_TRIP_EXPENSES_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				expensesData,
				"VEHICLE TRIP EXPENSES REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
					["Route"] = route?.RouteDisplay ?? null,
					["Driver"] = driver?.DisplayName ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				expensesData,
				"VEHICLE TRIP EXPENSES REPORT",
				"Vehicle Trip Expenses Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
					["Route"] = route?.RouteDisplay ?? null,
					["Driver"] = driver?.DisplayName ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}

	public static async Task<(MemoryStream stream, string fileName)> ExportPaymentsReport(
		IEnumerable<VehicleTripCardPaymentsOverviewModel> paymentsData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		CompanyModel company = null,
		OMCModel omc = null,
		VehicleModel vehicle = null,
		RouteOverviewModel route = null,
		DriverOverviewModel driver = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleTripCardPaymentsOverviewModel.OMCCardNumber)] = new() { DisplayName = "OMC Card", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.OMCCardCode)] = new() { DisplayName = "OMC Card Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.PaymentAmount)] = new() { DisplayName = "Payment Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripCardPaymentsOverviewModel.PaymentRemarks)] = new() { DisplayName = "Payment Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleTripCardPaymentsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleTripCardPaymentsOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripCardPaymentsOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripCardPaymentsOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripCardPaymentsOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripCardPaymentsOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripCardPaymentsOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripCardPaymentsOverviewModel.VehicleEmpty)] = new() { DisplayName = "Vehicle Empty", Alignment = CellAlignment.Center, IncludeInTotal = false },

			[nameof(VehicleTripCardPaymentsOverviewModel.BillNo)] = new() { DisplayName = "Bill No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.BillDateTime)] = new() { DisplayName = "Bill Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.GrossAmount)] = new() { DisplayName = "Gross Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripCardPaymentsOverviewModel.TDSAmount)] = new() { DisplayName = "TDS Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripCardPaymentsOverviewModel.PenaltyAmount)] = new() { DisplayName = "Penalty Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripCardPaymentsOverviewModel.NetAmount)] = new() { DisplayName = "Net Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripCardPaymentsOverviewModel.ProfitLoss)] = new() { DisplayName = "P&L", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripCardPaymentsOverviewModel.PendingDays)] = new() { DisplayName = "Pending Days", Format = "0", Alignment = CellAlignment.Right, IncludeInTotal = false },

			[nameof(VehicleTripCardPaymentsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripCardPaymentsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleTripCardPaymentsOverviewModel.OMCCardNumber),
				nameof(VehicleTripCardPaymentsOverviewModel.OMCCardCode),
				nameof(VehicleTripCardPaymentsOverviewModel.PaymentAmount),
				nameof(VehicleTripCardPaymentsOverviewModel.PaymentRemarks),
				nameof(VehicleTripCardPaymentsOverviewModel.VehicleCode),
				nameof(VehicleTripCardPaymentsOverviewModel.CompanyName),
				nameof(VehicleTripCardPaymentsOverviewModel.TransactionDateTime),
				nameof(VehicleTripCardPaymentsOverviewModel.FinancialYear),
				nameof(VehicleTripCardPaymentsOverviewModel.ChallanNo),
				nameof(VehicleTripCardPaymentsOverviewModel.OMCName),
				nameof(VehicleTripCardPaymentsOverviewModel.FromLocation),
				nameof(VehicleTripCardPaymentsOverviewModel.ToLocation),
				nameof(VehicleTripCardPaymentsOverviewModel.RouteDisplay),
				nameof(VehicleTripCardPaymentsOverviewModel.DriverName),
				nameof(VehicleTripCardPaymentsOverviewModel.DriverMobile),
				nameof(VehicleTripCardPaymentsOverviewModel.DriverDisplay),
				nameof(VehicleTripCardPaymentsOverviewModel.Quantity),
				nameof(VehicleTripCardPaymentsOverviewModel.EstimatedDistance),
				nameof(VehicleTripCardPaymentsOverviewModel.EstimatedHours),
				nameof(VehicleTripCardPaymentsOverviewModel.EstimatedFuelConsumption),
				nameof(VehicleTripCardPaymentsOverviewModel.EstimatedCost),
				nameof(VehicleTripCardPaymentsOverviewModel.TotalExpense),
				nameof(VehicleTripCardPaymentsOverviewModel.VehicleEmpty),
				nameof(VehicleTripCardPaymentsOverviewModel.BillNo),
				nameof(VehicleTripCardPaymentsOverviewModel.BillDateTime),
				nameof(VehicleTripCardPaymentsOverviewModel.GrossAmount),
				nameof(VehicleTripCardPaymentsOverviewModel.TDSAmount),
				nameof(VehicleTripCardPaymentsOverviewModel.PenaltyAmount),
				nameof(VehicleTripCardPaymentsOverviewModel.NetAmount),
				nameof(VehicleTripCardPaymentsOverviewModel.ProfitLoss),
				nameof(VehicleTripCardPaymentsOverviewModel.PendingDays),
				nameof(VehicleTripCardPaymentsOverviewModel.TransactionNo),
				nameof(VehicleTripCardPaymentsOverviewModel.Remarks),
				nameof(VehicleTripCardPaymentsOverviewModel.CreatedByName),
				nameof(VehicleTripCardPaymentsOverviewModel.CreatedAt),
				nameof(VehicleTripCardPaymentsOverviewModel.CreatedFromPlatform),
				nameof(VehicleTripCardPaymentsOverviewModel.LastModifiedByUserName),
				nameof(VehicleTripCardPaymentsOverviewModel.LastModifiedAt),
				nameof(VehicleTripCardPaymentsOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleTripCardPaymentsOverviewModel.OMCCardNumber),
				nameof(VehicleTripCardPaymentsOverviewModel.PaymentAmount),
				nameof(VehicleTripCardPaymentsOverviewModel.VehicleCode),
				nameof(VehicleTripCardPaymentsOverviewModel.CompanyName),
				nameof(VehicleTripCardPaymentsOverviewModel.TransactionDateTime),
				nameof(VehicleTripCardPaymentsOverviewModel.ChallanNo),
				nameof(VehicleTripCardPaymentsOverviewModel.OMCName),
				nameof(VehicleTripCardPaymentsOverviewModel.RouteDisplay),
				nameof(VehicleTripCardPaymentsOverviewModel.DriverDisplay),
				nameof(VehicleTripCardPaymentsOverviewModel.Quantity),
				nameof(VehicleTripCardPaymentsOverviewModel.TotalExpense),
				nameof(VehicleTripCardPaymentsOverviewModel.VehicleEmpty),
				nameof(VehicleTripCardPaymentsOverviewModel.BillNo),
				nameof(VehicleTripCardPaymentsOverviewModel.BillDateTime),
				nameof(VehicleTripCardPaymentsOverviewModel.GrossAmount),
				nameof(VehicleTripCardPaymentsOverviewModel.TDSAmount),
				nameof(VehicleTripCardPaymentsOverviewModel.PenaltyAmount),
				nameof(VehicleTripCardPaymentsOverviewModel.NetAmount),
				nameof(VehicleTripCardPaymentsOverviewModel.ProfitLoss),
				nameof(VehicleTripCardPaymentsOverviewModel.PendingDays)
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleTripCardPaymentsOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(VehicleTripCardPaymentsOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(VehicleTripCardPaymentsOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(VehicleTripCardPaymentsOverviewModel.RouteDisplay));

			if (driver is not null)
				columnOrder.Remove(nameof(VehicleTripCardPaymentsOverviewModel.DriverDisplay));
		}

		string fileName = $"VEHICLE_TRIP_PAYMENTS_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				paymentsData,
				"VEHICLE TRIP PAYMENTS REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
					["Route"] = route?.RouteDisplay ?? null,
					["Driver"] = driver?.DisplayName ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				paymentsData,
				"VEHICLE TRIP PAYMENTS REPORT",
				"Vehicle Trip Payments Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
					["Route"] = route?.RouteDisplay ?? null,
					["Driver"] = driver?.DisplayName ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}
}
