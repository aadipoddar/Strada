using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.VehicleTrip.OMC;
using StradaLibrary.Models.VehicleTrip.TripAdvance;
using StradaLibrary.Models.VehicleTrip.VehicleRoute;

namespace StradaLibrary.Exports.VehicleTrip.TripAdvance;

public static class TripAdvanceReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<TripAdvanceOverviewModel> tripData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showDeleted = false,
		CompanyModel company = null,
		OMCModel omc = null,
		VehicleModel vehicle = null,
		VehicleRouteOverviewModel route = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(TripAdvanceOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripAdvanceOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripAdvanceOverviewModel.VehicleEmpty)] = new() { DisplayName = "Vehicle Empty", Alignment = CellAlignment.Center, IncludeInTotal = false },

			[nameof(TripAdvanceOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripAdvanceOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(TripAdvanceOverviewModel.TransactionNo),
				nameof(TripAdvanceOverviewModel.CompanyName),
				nameof(TripAdvanceOverviewModel.TransactionDateTime),
				nameof(TripAdvanceOverviewModel.FinancialYear),
				nameof(TripAdvanceOverviewModel.ChallanNo),
				nameof(TripAdvanceOverviewModel.OMCName),
				nameof(TripAdvanceOverviewModel.VehicleCode),
				nameof(TripAdvanceOverviewModel.FromLocation),
				nameof(TripAdvanceOverviewModel.ToLocation),
				nameof(TripAdvanceOverviewModel.RouteDisplay),
				nameof(TripAdvanceOverviewModel.DriverName),
				nameof(TripAdvanceOverviewModel.DriverMobile),
				nameof(TripAdvanceOverviewModel.DriverDisplay),
				nameof(TripAdvanceOverviewModel.Quantity),
				nameof(TripAdvanceOverviewModel.EstimatedDistance),
				nameof(TripAdvanceOverviewModel.EstimatedHours),
				nameof(TripAdvanceOverviewModel.EstimatedFuelConsumption),
				nameof(TripAdvanceOverviewModel.EstimatedCost),
				nameof(TripAdvanceOverviewModel.TotalExpense),
				nameof(TripAdvanceOverviewModel.VehicleEmpty),
				nameof(TripAdvanceOverviewModel.Remarks),
				nameof(TripAdvanceOverviewModel.CreatedByName),
				nameof(TripAdvanceOverviewModel.CreatedAt),
				nameof(TripAdvanceOverviewModel.CreatedFromPlatform),
				nameof(TripAdvanceOverviewModel.LastModifiedByUserName),
				nameof(TripAdvanceOverviewModel.LastModifiedAt),
				nameof(TripAdvanceOverviewModel.LastModifiedFromPlatform),
				nameof(TripAdvanceOverviewModel.Status)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(TripAdvanceOverviewModel.Status));
		}
		else
		{
			columnOrder =
			[
				nameof(TripAdvanceOverviewModel.TransactionNo),
				nameof(TripAdvanceOverviewModel.CompanyName),
				nameof(TripAdvanceOverviewModel.TransactionDateTime),
				nameof(TripAdvanceOverviewModel.ChallanNo),
				nameof(TripAdvanceOverviewModel.OMCName),
				nameof(TripAdvanceOverviewModel.VehicleCode),
				nameof(TripAdvanceOverviewModel.RouteDisplay),
				nameof(TripAdvanceOverviewModel.DriverDisplay),
				nameof(TripAdvanceOverviewModel.Quantity),
				nameof(TripAdvanceOverviewModel.TotalExpense),
				nameof(TripAdvanceOverviewModel.VehicleEmpty),
				nameof(TripAdvanceOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(TripAdvanceOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(TripAdvanceOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(TripAdvanceOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(TripAdvanceOverviewModel.RouteDisplay));

			if (!showDeleted)
				columnOrder.Remove(nameof(TripAdvanceOverviewModel.Status));
		}

		string fileName = $"TRIP_ADVANCE_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				tripData,
				"TRIP ADVANCE REPORT",
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
					["Route"] = route?.RouteDisplay ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				tripData,
				"TRIP ADVANCE REPORT",
				"Trip Advance Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
					["Route"] = route?.RouteDisplay ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}

	public static async Task<(MemoryStream stream, string fileName)> ExportExpensesReport(
		IEnumerable<TripAdvanceExpensesOverviewModel> expensesData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		CompanyModel company = null,
		OMCModel omc = null,
		VehicleModel vehicle = null,
		VehicleRouteOverviewModel route = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(TripAdvanceExpensesOverviewModel.ExpenseTypeName)] = new() { DisplayName = "Expense Type", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.ExpenseTypeCode)] = new() { DisplayName = "Expense Type Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.ExpenseAmount)] = new() { DisplayName = "Expense Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripAdvanceExpensesOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripAdvanceExpensesOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceExpensesOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceExpensesOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceExpensesOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceExpensesOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceExpensesOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripAdvanceExpensesOverviewModel.VehicleEmpty)] = new() { DisplayName = "Vehicle Empty", Alignment = CellAlignment.Center, IncludeInTotal = false },

			[nameof(TripAdvanceExpensesOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripAdvanceExpensesOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(TripAdvanceExpensesOverviewModel.ExpenseTypeName),
				nameof(TripAdvanceExpensesOverviewModel.ExpenseTypeCode),
				nameof(TripAdvanceExpensesOverviewModel.ExpenseAmount),
				nameof(TripAdvanceExpensesOverviewModel.TransactionNo),
				nameof(TripAdvanceExpensesOverviewModel.CompanyName),
				nameof(TripAdvanceExpensesOverviewModel.TransactionDateTime),
				nameof(TripAdvanceExpensesOverviewModel.FinancialYear),
				nameof(TripAdvanceExpensesOverviewModel.ChallanNo),
				nameof(TripAdvanceExpensesOverviewModel.OMCName),
				nameof(TripAdvanceExpensesOverviewModel.VehicleCode),
				nameof(TripAdvanceExpensesOverviewModel.FromLocation),
				nameof(TripAdvanceExpensesOverviewModel.ToLocation),
				nameof(TripAdvanceExpensesOverviewModel.RouteDisplay),
				nameof(TripAdvanceExpensesOverviewModel.DriverName),
				nameof(TripAdvanceExpensesOverviewModel.DriverMobile),
				nameof(TripAdvanceExpensesOverviewModel.DriverDisplay),
				nameof(TripAdvanceExpensesOverviewModel.Quantity),
				nameof(TripAdvanceExpensesOverviewModel.EstimatedDistance),
				nameof(TripAdvanceExpensesOverviewModel.EstimatedHours),
				nameof(TripAdvanceExpensesOverviewModel.EstimatedFuelConsumption),
				nameof(TripAdvanceExpensesOverviewModel.EstimatedCost),
				nameof(TripAdvanceExpensesOverviewModel.TotalExpense),
				nameof(TripAdvanceExpensesOverviewModel.VehicleEmpty),
				nameof(TripAdvanceExpensesOverviewModel.Remarks),
				nameof(TripAdvanceExpensesOverviewModel.CreatedByName),
				nameof(TripAdvanceExpensesOverviewModel.CreatedAt),
				nameof(TripAdvanceExpensesOverviewModel.CreatedFromPlatform),
				nameof(TripAdvanceExpensesOverviewModel.LastModifiedByUserName),
				nameof(TripAdvanceExpensesOverviewModel.LastModifiedAt),
				nameof(TripAdvanceExpensesOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(TripAdvanceExpensesOverviewModel.ExpenseTypeName),
				nameof(TripAdvanceExpensesOverviewModel.ExpenseAmount),
				nameof(TripAdvanceExpensesOverviewModel.TransactionNo),
				nameof(TripAdvanceExpensesOverviewModel.CompanyName),
				nameof(TripAdvanceExpensesOverviewModel.TransactionDateTime),
				nameof(TripAdvanceExpensesOverviewModel.ChallanNo),
				nameof(TripAdvanceExpensesOverviewModel.OMCName),
				nameof(TripAdvanceExpensesOverviewModel.VehicleCode),
				nameof(TripAdvanceExpensesOverviewModel.RouteDisplay),
				nameof(TripAdvanceExpensesOverviewModel.DriverDisplay),
				nameof(TripAdvanceExpensesOverviewModel.Quantity),
				nameof(TripAdvanceExpensesOverviewModel.TotalExpense),
				nameof(TripAdvanceExpensesOverviewModel.VehicleEmpty),
			];

			if (company is not null)
				columnOrder.Remove(nameof(TripAdvanceExpensesOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(TripAdvanceExpensesOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(TripAdvanceExpensesOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(TripAdvanceExpensesOverviewModel.RouteDisplay));
		}

		string fileName = $"TRIP_ADVANCE_EXPENSES_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				expensesData,
				"TRIP ADVANCE EXPENSES REPORT",
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
					["Route"] = route?.RouteDisplay ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				expensesData,
				"TRIP ADVANCE EXPENSES REPORT",
				"Trip Advance Expenses Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
					["Route"] = route?.RouteDisplay ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}

	public static async Task<(MemoryStream stream, string fileName)> ExportPaymentsReport(
		IEnumerable<TripAdvanceCardPaymentsOverviewModel> paymentsData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		CompanyModel company = null,
		OMCModel omc = null,
		VehicleModel vehicle = null,
		VehicleRouteOverviewModel route = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(TripAdvanceCardPaymentsOverviewModel.OMCCardNumber)] = new() { DisplayName = "OMC Card", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.OMCCardCode)] = new() { DisplayName = "OMC Card Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.PaymentAmount)] = new() { DisplayName = "Payment Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripAdvanceCardPaymentsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(TripAdvanceCardPaymentsOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceCardPaymentsOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceCardPaymentsOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceCardPaymentsOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceCardPaymentsOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(TripAdvanceCardPaymentsOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(TripAdvanceCardPaymentsOverviewModel.VehicleEmpty)] = new() { DisplayName = "Vehicle Empty", Alignment = CellAlignment.Center, IncludeInTotal = false },

			[nameof(TripAdvanceCardPaymentsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(TripAdvanceCardPaymentsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(TripAdvanceCardPaymentsOverviewModel.OMCCardNumber),
				nameof(TripAdvanceCardPaymentsOverviewModel.OMCCardCode),
				nameof(TripAdvanceCardPaymentsOverviewModel.PaymentAmount),
				nameof(TripAdvanceCardPaymentsOverviewModel.TransactionNo),
				nameof(TripAdvanceCardPaymentsOverviewModel.CompanyName),
				nameof(TripAdvanceCardPaymentsOverviewModel.TransactionDateTime),
				nameof(TripAdvanceCardPaymentsOverviewModel.FinancialYear),
				nameof(TripAdvanceCardPaymentsOverviewModel.ChallanNo),
				nameof(TripAdvanceCardPaymentsOverviewModel.OMCName),
				nameof(TripAdvanceCardPaymentsOverviewModel.VehicleCode),
				nameof(TripAdvanceCardPaymentsOverviewModel.FromLocation),
				nameof(TripAdvanceCardPaymentsOverviewModel.ToLocation),
				nameof(TripAdvanceCardPaymentsOverviewModel.RouteDisplay),
				nameof(TripAdvanceCardPaymentsOverviewModel.DriverName),
				nameof(TripAdvanceCardPaymentsOverviewModel.DriverMobile),
				nameof(TripAdvanceCardPaymentsOverviewModel.DriverDisplay),
				nameof(TripAdvanceCardPaymentsOverviewModel.Quantity),
				nameof(TripAdvanceCardPaymentsOverviewModel.EstimatedDistance),
				nameof(TripAdvanceCardPaymentsOverviewModel.EstimatedHours),
				nameof(TripAdvanceCardPaymentsOverviewModel.EstimatedFuelConsumption),
				nameof(TripAdvanceCardPaymentsOverviewModel.EstimatedCost),
				nameof(TripAdvanceCardPaymentsOverviewModel.TotalExpense),
				nameof(TripAdvanceCardPaymentsOverviewModel.VehicleEmpty),
				nameof(TripAdvanceCardPaymentsOverviewModel.Remarks),
				nameof(TripAdvanceCardPaymentsOverviewModel.CreatedByName),
				nameof(TripAdvanceCardPaymentsOverviewModel.CreatedAt),
				nameof(TripAdvanceCardPaymentsOverviewModel.CreatedFromPlatform),
				nameof(TripAdvanceCardPaymentsOverviewModel.LastModifiedByUserName),
				nameof(TripAdvanceCardPaymentsOverviewModel.LastModifiedAt),
				nameof(TripAdvanceCardPaymentsOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(TripAdvanceCardPaymentsOverviewModel.OMCCardNumber),
				nameof(TripAdvanceCardPaymentsOverviewModel.PaymentAmount),
				nameof(TripAdvanceCardPaymentsOverviewModel.TransactionNo),
				nameof(TripAdvanceCardPaymentsOverviewModel.CompanyName),
				nameof(TripAdvanceCardPaymentsOverviewModel.TransactionDateTime),
				nameof(TripAdvanceCardPaymentsOverviewModel.ChallanNo),
				nameof(TripAdvanceCardPaymentsOverviewModel.OMCName),
				nameof(TripAdvanceCardPaymentsOverviewModel.VehicleCode),
				nameof(TripAdvanceCardPaymentsOverviewModel.RouteDisplay),
				nameof(TripAdvanceCardPaymentsOverviewModel.DriverDisplay),
				nameof(TripAdvanceCardPaymentsOverviewModel.Quantity),
				nameof(TripAdvanceCardPaymentsOverviewModel.TotalExpense),
				nameof(TripAdvanceCardPaymentsOverviewModel.VehicleEmpty)
			];

			if (company is not null)
				columnOrder.Remove(nameof(TripAdvanceCardPaymentsOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(TripAdvanceCardPaymentsOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(TripAdvanceCardPaymentsOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(TripAdvanceCardPaymentsOverviewModel.RouteDisplay));
		}

		string fileName = $"TRIP_ADVANCE_PAYMENTS_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				paymentsData,
				"TRIP ADVANCE PAYMENTS REPORT",
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
					["Route"] = route?.RouteDisplay ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				paymentsData,
				"TRIP ADVANCE PAYMENTS REPORT",
				"Trip Advance Payments Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
					["Route"] = route?.RouteDisplay ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}
}
