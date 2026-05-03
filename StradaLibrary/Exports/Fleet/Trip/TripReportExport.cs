using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.Route;
using StradaLibrary.Models.Fleet.Trip;
using StradaLibrary.Models.Fleet.Vehicle;

namespace StradaLibrary.Exports.Fleet.Trip;

public static class TripReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<TripOverviewModel> tripData,
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
			[nameof(TripOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripOverviewModel.SlNo)] = new() { DisplayName = "Sl No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripOverviewModel.VehicleEmpty)] = new() { DisplayName = "Vehicle Empty", Alignment = CellAlignment.Center, IncludeInTotal = false },

			[nameof(TripOverviewModel.BillNo)] = new() { DisplayName = "Bill No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.BillDateTime)] = new() { DisplayName = "Bill Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripOverviewModel.GrossAmount)] = new() { DisplayName = "Gross Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripOverviewModel.PenaltyAmount)] = new() { DisplayName = "Penalty Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripOverviewModel.NetAmount)] = new() { DisplayName = "Net Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripOverviewModel.ProfitLoss)] = new() { DisplayName = "P&L", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripOverviewModel.PendingDays)] = new() { DisplayName = "Pending Days", Format = "0", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(TripOverviewModel.TransactionNo),
				nameof(TripOverviewModel.CompanyName),
				nameof(TripOverviewModel.TransactionDateTime),
				nameof(TripOverviewModel.FinancialYear),
				nameof(TripOverviewModel.SlNo),
				nameof(TripOverviewModel.ChallanNo),
				nameof(TripOverviewModel.OMCName),
				nameof(TripOverviewModel.VehicleCode),
				nameof(TripOverviewModel.FromLocation),
				nameof(TripOverviewModel.ToLocation),
				nameof(TripOverviewModel.RouteDisplay),
				nameof(TripOverviewModel.DriverName),
				nameof(TripOverviewModel.DriverMobile),
				nameof(TripOverviewModel.DriverDisplay),
				nameof(TripOverviewModel.Quantity),
				nameof(TripOverviewModel.EstimatedDistance),
				nameof(TripOverviewModel.EstimatedHours),
				nameof(TripOverviewModel.EstimatedFuelConsumption),
				nameof(TripOverviewModel.EstimatedCost),
				nameof(TripOverviewModel.TotalExpense),
				nameof(TripOverviewModel.VehicleEmpty),
				nameof(TripOverviewModel.BillNo),
				nameof(TripOverviewModel.BillDateTime),
				nameof(TripOverviewModel.GrossAmount),
				nameof(TripOverviewModel.PenaltyAmount),
				nameof(TripOverviewModel.NetAmount),
				nameof(TripOverviewModel.ProfitLoss),
				nameof(TripOverviewModel.PendingDays),
				nameof(TripOverviewModel.Remarks),
				nameof(TripOverviewModel.CreatedByName),
				nameof(TripOverviewModel.CreatedAt),
				nameof(TripOverviewModel.CreatedFromPlatform),
				nameof(TripOverviewModel.LastModifiedByUserName),
				nameof(TripOverviewModel.LastModifiedAt),
				nameof(TripOverviewModel.LastModifiedFromPlatform),
				nameof(TripOverviewModel.Status)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(TripOverviewModel.Status));
		}
		else
		{
			columnOrder =
			[
				nameof(TripOverviewModel.TransactionDateTime),
				nameof(TripOverviewModel.SlNo),
				nameof(TripOverviewModel.ChallanNo),
				nameof(TripOverviewModel.VehicleCode),
				nameof(TripOverviewModel.RouteDisplay),
				nameof(TripOverviewModel.DriverDisplay),
				nameof(TripOverviewModel.Quantity),
				nameof(TripOverviewModel.EstimatedDistance),
				nameof(TripOverviewModel.EstimatedCost),
				nameof(TripOverviewModel.TotalExpense),
				nameof(TripOverviewModel.BillNo),
				nameof(TripOverviewModel.BillDateTime),
				nameof(TripOverviewModel.NetAmount),
				nameof(TripOverviewModel.ProfitLoss),
				nameof(TripOverviewModel.PendingDays),
				nameof(TripOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(TripOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(TripOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(TripOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(TripOverviewModel.RouteDisplay));

			if (driver is not null)
				columnOrder.Remove(nameof(TripOverviewModel.DriverDisplay));

			if (!showDeleted)
				columnOrder.Remove(nameof(TripOverviewModel.Status));
		}

		string fileName = $"TRIP_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				tripData,
				"TRIP REPORT",
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
				"TRIP REPORT",
				"Trip Transactions",
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
		IEnumerable<TripExpensesOverviewModel> expensesData,
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
			[nameof(TripExpensesOverviewModel.ExpenseTypeName)] = new() { DisplayName = "Expense Type", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.ExpenseTypeCode)] = new() { DisplayName = "Expense Type Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.ExpenseAmount)] = new() { DisplayName = "Expense Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripExpensesOverviewModel.ExpenseRemarks)] = new() { DisplayName = "Expense Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripExpensesOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripExpensesOverviewModel.SlNo)] = new() { DisplayName = "Sl No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripExpensesOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripExpensesOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripExpensesOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripExpensesOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripExpensesOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripExpensesOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripExpensesOverviewModel.VehicleEmpty)] = new() { DisplayName = "Vehicle Empty", Alignment = CellAlignment.Center, IncludeInTotal = false },

			[nameof(TripExpensesOverviewModel.BillNo)] = new() { DisplayName = "Bill No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.BillDateTime)] = new() { DisplayName = "Bill Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.GrossAmount)] = new() { DisplayName = "Gross Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripExpensesOverviewModel.PenaltyAmount)] = new() { DisplayName = "Penalty Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripExpensesOverviewModel.NetAmount)] = new() { DisplayName = "Net Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripExpensesOverviewModel.ProfitLoss)] = new() { DisplayName = "P&L", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripExpensesOverviewModel.PendingDays)] = new() { DisplayName = "Pending Days", Format = "0", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripExpensesOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripExpensesOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(TripExpensesOverviewModel.ExpenseTypeName),
				nameof(TripExpensesOverviewModel.ExpenseTypeCode),
				nameof(TripExpensesOverviewModel.ExpenseAmount),
				nameof(TripExpensesOverviewModel.ExpenseRemarks),
				nameof(TripExpensesOverviewModel.TransactionNo),
				nameof(TripExpensesOverviewModel.CompanyName),
				nameof(TripExpensesOverviewModel.TransactionDateTime),
				nameof(TripExpensesOverviewModel.FinancialYear),
				nameof(TripExpensesOverviewModel.SlNo),
				nameof(TripExpensesOverviewModel.ChallanNo),
				nameof(TripExpensesOverviewModel.OMCName),
				nameof(TripExpensesOverviewModel.VehicleCode),
				nameof(TripExpensesOverviewModel.FromLocation),
				nameof(TripExpensesOverviewModel.ToLocation),
				nameof(TripExpensesOverviewModel.RouteDisplay),
				nameof(TripExpensesOverviewModel.DriverName),
				nameof(TripExpensesOverviewModel.DriverMobile),
				nameof(TripExpensesOverviewModel.DriverDisplay),
				nameof(TripExpensesOverviewModel.Quantity),
				nameof(TripExpensesOverviewModel.EstimatedDistance),
				nameof(TripExpensesOverviewModel.EstimatedHours),
				nameof(TripExpensesOverviewModel.EstimatedFuelConsumption),
				nameof(TripExpensesOverviewModel.EstimatedCost),
				nameof(TripExpensesOverviewModel.TotalExpense),
				nameof(TripExpensesOverviewModel.VehicleEmpty),
				nameof(TripExpensesOverviewModel.BillNo),
				nameof(TripExpensesOverviewModel.BillDateTime),
				nameof(TripExpensesOverviewModel.GrossAmount),
				nameof(TripExpensesOverviewModel.PenaltyAmount),
				nameof(TripExpensesOverviewModel.NetAmount),
				nameof(TripExpensesOverviewModel.ProfitLoss),
				nameof(TripExpensesOverviewModel.PendingDays),
				nameof(TripExpensesOverviewModel.Remarks),
				nameof(TripExpensesOverviewModel.CreatedByName),
				nameof(TripExpensesOverviewModel.CreatedAt),
				nameof(TripExpensesOverviewModel.CreatedFromPlatform),
				nameof(TripExpensesOverviewModel.LastModifiedByUserName),
				nameof(TripExpensesOverviewModel.LastModifiedAt),
				nameof(TripExpensesOverviewModel.LastModifiedFromPlatform),
				nameof(TripExpensesOverviewModel.Status)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(TripExpensesOverviewModel.ExpenseTypeName),
				nameof(TripExpensesOverviewModel.ExpenseAmount),
				nameof(TripExpensesOverviewModel.TransactionDateTime),
				nameof(TripExpensesOverviewModel.SlNo),
				nameof(TripExpensesOverviewModel.ChallanNo),
				nameof(TripExpensesOverviewModel.VehicleCode),
				nameof(TripExpensesOverviewModel.RouteDisplay),
				nameof(TripExpensesOverviewModel.DriverDisplay),
				nameof(TripExpensesOverviewModel.Quantity),
				nameof(TripExpensesOverviewModel.EstimatedDistance),
				nameof(TripExpensesOverviewModel.EstimatedCost),
				nameof(TripExpensesOverviewModel.TotalExpense),
				nameof(TripExpensesOverviewModel.BillNo),
				nameof(TripExpensesOverviewModel.BillDateTime),
				nameof(TripExpensesOverviewModel.NetAmount),
				nameof(TripExpensesOverviewModel.ProfitLoss),
				nameof(TripExpensesOverviewModel.PendingDays),
				nameof(TripExpensesOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(TripExpensesOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(TripExpensesOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(TripExpensesOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(TripExpensesOverviewModel.RouteDisplay));

			if (driver is not null)
				columnOrder.Remove(nameof(TripExpensesOverviewModel.DriverDisplay));

			if (!showDeleted)
				columnOrder.Remove(nameof(TripExpensesOverviewModel.Status));
		}

		string fileName = $"TRIP_EXPENSES_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				expensesData,
				"TRIP EXPENSES REPORT",
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
				"TRIP EXPENSES REPORT",
				"Trip Expenses Transactions",
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

	public static async Task<(MemoryStream stream, string fileName)> ExportCardPaymentsReport(
		IEnumerable<TripCardPaymentsOverviewModel> paymentsData,
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
			[nameof(TripCardPaymentsOverviewModel.OMCCardNumber)] = new() { DisplayName = "OMC Card", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.OMCCardCode)] = new() { DisplayName = "OMC Card Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.PaymentAmount)] = new() { DisplayName = "Payment Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripCardPaymentsOverviewModel.PaymentRemarks)] = new() { DisplayName = "Payment Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripCardPaymentsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripCardPaymentsOverviewModel.SlNo)] = new() { DisplayName = "Sl No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripCardPaymentsOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripCardPaymentsOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripCardPaymentsOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripCardPaymentsOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripCardPaymentsOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripCardPaymentsOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripCardPaymentsOverviewModel.VehicleEmpty)] = new() { DisplayName = "Vehicle Empty", Alignment = CellAlignment.Center, IncludeInTotal = false },

			[nameof(TripCardPaymentsOverviewModel.BillNo)] = new() { DisplayName = "Bill No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.BillDateTime)] = new() { DisplayName = "Bill Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.GrossAmount)] = new() { DisplayName = "Gross Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripCardPaymentsOverviewModel.PenaltyAmount)] = new() { DisplayName = "Penalty Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripCardPaymentsOverviewModel.NetAmount)] = new() { DisplayName = "Net Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripCardPaymentsOverviewModel.ProfitLoss)] = new() { DisplayName = "P&L", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripCardPaymentsOverviewModel.PendingDays)] = new() { DisplayName = "Pending Days", Format = "0", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripCardPaymentsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripCardPaymentsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(TripCardPaymentsOverviewModel.OMCCardNumber),
				nameof(TripCardPaymentsOverviewModel.OMCCardCode),
				nameof(TripCardPaymentsOverviewModel.PaymentAmount),
				nameof(TripCardPaymentsOverviewModel.PaymentRemarks),
				nameof(TripCardPaymentsOverviewModel.TransactionNo),
				nameof(TripCardPaymentsOverviewModel.CompanyName),
				nameof(TripCardPaymentsOverviewModel.TransactionDateTime),
				nameof(TripCardPaymentsOverviewModel.FinancialYear),
				nameof(TripCardPaymentsOverviewModel.SlNo),
				nameof(TripCardPaymentsOverviewModel.ChallanNo),
				nameof(TripCardPaymentsOverviewModel.OMCName),
				nameof(TripCardPaymentsOverviewModel.VehicleCode),
				nameof(TripCardPaymentsOverviewModel.FromLocation),
				nameof(TripCardPaymentsOverviewModel.ToLocation),
				nameof(TripCardPaymentsOverviewModel.RouteDisplay),
				nameof(TripCardPaymentsOverviewModel.DriverName),
				nameof(TripCardPaymentsOverviewModel.DriverMobile),
				nameof(TripCardPaymentsOverviewModel.DriverDisplay),
				nameof(TripCardPaymentsOverviewModel.Quantity),
				nameof(TripCardPaymentsOverviewModel.EstimatedDistance),
				nameof(TripCardPaymentsOverviewModel.EstimatedHours),
				nameof(TripCardPaymentsOverviewModel.EstimatedFuelConsumption),
				nameof(TripCardPaymentsOverviewModel.EstimatedCost),
				nameof(TripCardPaymentsOverviewModel.TotalExpense),
				nameof(TripCardPaymentsOverviewModel.VehicleEmpty),
				nameof(TripCardPaymentsOverviewModel.BillNo),
				nameof(TripCardPaymentsOverviewModel.BillDateTime),
				nameof(TripCardPaymentsOverviewModel.GrossAmount),
				nameof(TripCardPaymentsOverviewModel.PenaltyAmount),
				nameof(TripCardPaymentsOverviewModel.NetAmount),
				nameof(TripCardPaymentsOverviewModel.ProfitLoss),
				nameof(TripCardPaymentsOverviewModel.PendingDays),
				nameof(TripCardPaymentsOverviewModel.Remarks),
				nameof(TripCardPaymentsOverviewModel.CreatedByName),
				nameof(TripCardPaymentsOverviewModel.CreatedAt),
				nameof(TripCardPaymentsOverviewModel.CreatedFromPlatform),
				nameof(TripCardPaymentsOverviewModel.LastModifiedByUserName),
				nameof(TripCardPaymentsOverviewModel.LastModifiedAt),
				nameof(TripCardPaymentsOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(TripCardPaymentsOverviewModel.OMCCardNumber),
				nameof(TripCardPaymentsOverviewModel.PaymentAmount),
				nameof(TripCardPaymentsOverviewModel.TransactionDateTime),
				nameof(TripCardPaymentsOverviewModel.SlNo),
				nameof(TripCardPaymentsOverviewModel.ChallanNo),
				nameof(TripCardPaymentsOverviewModel.VehicleCode),
				nameof(TripCardPaymentsOverviewModel.RouteDisplay),
				nameof(TripCardPaymentsOverviewModel.DriverDisplay),
				nameof(TripCardPaymentsOverviewModel.Quantity),
				nameof(TripCardPaymentsOverviewModel.EstimatedDistance),
				nameof(TripCardPaymentsOverviewModel.EstimatedCost),
				nameof(TripCardPaymentsOverviewModel.TotalExpense),
				nameof(TripCardPaymentsOverviewModel.BillNo),
				nameof(TripCardPaymentsOverviewModel.BillDateTime),
				nameof(TripCardPaymentsOverviewModel.NetAmount),
				nameof(TripCardPaymentsOverviewModel.ProfitLoss),
				nameof(TripCardPaymentsOverviewModel.PendingDays)
			];

			if (company is not null)
				columnOrder.Remove(nameof(TripCardPaymentsOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(TripCardPaymentsOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(TripCardPaymentsOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(TripCardPaymentsOverviewModel.RouteDisplay));

			if (driver is not null)
				columnOrder.Remove(nameof(TripCardPaymentsOverviewModel.DriverDisplay));
		}

		string fileName = $"TRIP_PAYMENTS_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				paymentsData,
				"TRIP PAYMENTS REPORT",
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
				"TRIP PAYMENTS REPORT",
				"Trip Payments Transactions",
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

	public static async Task<(MemoryStream stream, string fileName)> ExportLedgerPaymentsReport(
		IEnumerable<TripLedgerPaymentsOverviewModel> paymentsData,
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
			[nameof(TripLedgerPaymentsOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.LedgerCode)] = new() { DisplayName = "Ledger Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.PaymentAmount)] = new() { DisplayName = "Payment Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripLedgerPaymentsOverviewModel.PaymentRemarks)] = new() { DisplayName = "Payment Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripLedgerPaymentsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripLedgerPaymentsOverviewModel.SlNo)] = new() { DisplayName = "Sl No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripLedgerPaymentsOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripLedgerPaymentsOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripLedgerPaymentsOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripLedgerPaymentsOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripLedgerPaymentsOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripLedgerPaymentsOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripLedgerPaymentsOverviewModel.VehicleEmpty)] = new() { DisplayName = "Vehicle Empty", Alignment = CellAlignment.Center, IncludeInTotal = false },

			[nameof(TripLedgerPaymentsOverviewModel.BillNo)] = new() { DisplayName = "Bill No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.BillDateTime)] = new() { DisplayName = "Bill Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.GrossAmount)] = new() { DisplayName = "Gross Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripLedgerPaymentsOverviewModel.PenaltyAmount)] = new() { DisplayName = "Penalty Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripLedgerPaymentsOverviewModel.NetAmount)] = new() { DisplayName = "Net Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripLedgerPaymentsOverviewModel.ProfitLoss)] = new() { DisplayName = "P&L", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripLedgerPaymentsOverviewModel.PendingDays)] = new() { DisplayName = "Pending Days", Format = "0", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripLedgerPaymentsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripLedgerPaymentsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(TripLedgerPaymentsOverviewModel.LedgerName),
				nameof(TripLedgerPaymentsOverviewModel.LedgerCode),
				nameof(TripLedgerPaymentsOverviewModel.PaymentAmount),
				nameof(TripLedgerPaymentsOverviewModel.PaymentRemarks),
				nameof(TripLedgerPaymentsOverviewModel.TransactionNo),
				nameof(TripLedgerPaymentsOverviewModel.CompanyName),
				nameof(TripLedgerPaymentsOverviewModel.TransactionDateTime),
				nameof(TripLedgerPaymentsOverviewModel.FinancialYear),
				nameof(TripLedgerPaymentsOverviewModel.SlNo),
				nameof(TripLedgerPaymentsOverviewModel.ChallanNo),
				nameof(TripLedgerPaymentsOverviewModel.OMCName),
				nameof(TripLedgerPaymentsOverviewModel.VehicleCode),
				nameof(TripLedgerPaymentsOverviewModel.FromLocation),
				nameof(TripLedgerPaymentsOverviewModel.ToLocation),
				nameof(TripLedgerPaymentsOverviewModel.RouteDisplay),
				nameof(TripLedgerPaymentsOverviewModel.DriverName),
				nameof(TripLedgerPaymentsOverviewModel.DriverMobile),
				nameof(TripLedgerPaymentsOverviewModel.DriverDisplay),
				nameof(TripLedgerPaymentsOverviewModel.Quantity),
				nameof(TripLedgerPaymentsOverviewModel.EstimatedDistance),
				nameof(TripLedgerPaymentsOverviewModel.EstimatedHours),
				nameof(TripLedgerPaymentsOverviewModel.EstimatedFuelConsumption),
				nameof(TripLedgerPaymentsOverviewModel.EstimatedCost),
				nameof(TripLedgerPaymentsOverviewModel.TotalExpense),
				nameof(TripLedgerPaymentsOverviewModel.VehicleEmpty),
				nameof(TripLedgerPaymentsOverviewModel.BillNo),
				nameof(TripLedgerPaymentsOverviewModel.BillDateTime),
				nameof(TripLedgerPaymentsOverviewModel.GrossAmount),
				nameof(TripLedgerPaymentsOverviewModel.PenaltyAmount),
				nameof(TripLedgerPaymentsOverviewModel.NetAmount),
				nameof(TripLedgerPaymentsOverviewModel.ProfitLoss),
				nameof(TripLedgerPaymentsOverviewModel.PendingDays),
				nameof(TripLedgerPaymentsOverviewModel.Remarks),
				nameof(TripLedgerPaymentsOverviewModel.CreatedByName),
				nameof(TripLedgerPaymentsOverviewModel.CreatedAt),
				nameof(TripLedgerPaymentsOverviewModel.CreatedFromPlatform),
				nameof(TripLedgerPaymentsOverviewModel.LastModifiedByUserName),
				nameof(TripLedgerPaymentsOverviewModel.LastModifiedAt),
				nameof(TripLedgerPaymentsOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(TripLedgerPaymentsOverviewModel.LedgerName),
				nameof(TripLedgerPaymentsOverviewModel.PaymentAmount),
				nameof(TripLedgerPaymentsOverviewModel.TransactionDateTime),
				nameof(TripLedgerPaymentsOverviewModel.SlNo),
				nameof(TripLedgerPaymentsOverviewModel.ChallanNo),
				nameof(TripLedgerPaymentsOverviewModel.VehicleCode),
				nameof(TripLedgerPaymentsOverviewModel.RouteDisplay),
				nameof(TripLedgerPaymentsOverviewModel.DriverDisplay),
				nameof(TripLedgerPaymentsOverviewModel.Quantity),
				nameof(TripLedgerPaymentsOverviewModel.EstimatedDistance),
				nameof(TripLedgerPaymentsOverviewModel.EstimatedCost),
				nameof(TripLedgerPaymentsOverviewModel.TotalExpense),
				nameof(TripLedgerPaymentsOverviewModel.BillNo),
				nameof(TripLedgerPaymentsOverviewModel.BillDateTime),
				nameof(TripLedgerPaymentsOverviewModel.NetAmount),
				nameof(TripLedgerPaymentsOverviewModel.ProfitLoss),
				nameof(TripLedgerPaymentsOverviewModel.PendingDays)
			];

			if (company is not null)
				columnOrder.Remove(nameof(TripLedgerPaymentsOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(TripLedgerPaymentsOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(TripLedgerPaymentsOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(TripLedgerPaymentsOverviewModel.RouteDisplay));

			if (driver is not null)
				columnOrder.Remove(nameof(TripLedgerPaymentsOverviewModel.DriverDisplay));
		}

		string fileName = $"TRIP_LEDGER_PAYMENTS_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				paymentsData,
				"TRIP LEDGER PAYMENTS REPORT",
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
				"TRIP LEDGER PAYMENTS REPORT",
				"Trip Ledger Payments Transactions",
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
