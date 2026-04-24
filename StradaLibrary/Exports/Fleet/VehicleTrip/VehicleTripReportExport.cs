using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.FinancialAccounting;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleRoute;
using StradaLibrary.Models.Fleet.VehicleTrip;

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
		VehicleRouteOverviewModel route = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleTripOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
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
				nameof(VehicleTripOverviewModel.TransactionNo),
				nameof(VehicleTripOverviewModel.CompanyName),
				nameof(VehicleTripOverviewModel.TransactionDateTime),
				nameof(VehicleTripOverviewModel.FinancialYear),
				nameof(VehicleTripOverviewModel.ChallanNo),
				nameof(VehicleTripOverviewModel.OMCName),
				nameof(VehicleTripOverviewModel.VehicleCode),
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
				nameof(VehicleTripOverviewModel.TransactionNo),
				nameof(VehicleTripOverviewModel.CompanyName),
				nameof(VehicleTripOverviewModel.TransactionDateTime),
				nameof(VehicleTripOverviewModel.ChallanNo),
				nameof(VehicleTripOverviewModel.OMCName),
				nameof(VehicleTripOverviewModel.VehicleCode),
				nameof(VehicleTripOverviewModel.RouteDisplay),
				nameof(VehicleTripOverviewModel.DriverDisplay),
				nameof(VehicleTripOverviewModel.Quantity),
				nameof(VehicleTripOverviewModel.TotalExpense),
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
					["Route"] = route?.RouteDisplay ?? null
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
		VehicleRouteOverviewModel route = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleTripExpensesOverviewModel.ExpenseTypeName)] = new() { DisplayName = "Expense Type", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.ExpenseTypeCode)] = new() { DisplayName = "Expense Type Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.ExpenseAmount)] = new() { DisplayName = "Expense Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripExpensesOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripExpensesOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
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
				nameof(VehicleTripExpensesOverviewModel.TransactionNo),
				nameof(VehicleTripExpensesOverviewModel.CompanyName),
				nameof(VehicleTripExpensesOverviewModel.TransactionDateTime),
				nameof(VehicleTripExpensesOverviewModel.FinancialYear),
				nameof(VehicleTripExpensesOverviewModel.ChallanNo),
				nameof(VehicleTripExpensesOverviewModel.OMCName),
				nameof(VehicleTripExpensesOverviewModel.VehicleCode),
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
				nameof(VehicleTripExpensesOverviewModel.TransactionNo),
				nameof(VehicleTripExpensesOverviewModel.CompanyName),
				nameof(VehicleTripExpensesOverviewModel.TransactionDateTime),
				nameof(VehicleTripExpensesOverviewModel.ChallanNo),
				nameof(VehicleTripExpensesOverviewModel.OMCName),
				nameof(VehicleTripExpensesOverviewModel.VehicleCode),
				nameof(VehicleTripExpensesOverviewModel.RouteDisplay),
				nameof(VehicleTripExpensesOverviewModel.DriverDisplay),
				nameof(VehicleTripExpensesOverviewModel.Quantity),
				nameof(VehicleTripExpensesOverviewModel.TotalExpense),
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleTripExpensesOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(VehicleTripExpensesOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(VehicleTripExpensesOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(VehicleTripExpensesOverviewModel.RouteDisplay));
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
					["Route"] = route?.RouteDisplay ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}

	public static async Task<(MemoryStream stream, string fileName)> ExportPaymentsReport(
		IEnumerable<VehicleTripOMCCardPaymentsOverviewModel> paymentsData,
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
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.OMCCardNumber)] = new() { DisplayName = "OMC Card", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.OMCCardCode)] = new() { DisplayName = "OMC Card Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.PaymentAmount)] = new() { DisplayName = "Payment Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripOMCCardPaymentsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.ChallanNo)] = new() { DisplayName = "Challan", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.FromLocation)] = new() { DisplayName = "From Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.ToLocation)] = new() { DisplayName = "To Location", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.RouteDisplay)] = new() { DisplayName = "Route", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.DriverName)] = new() { DisplayName = "Driver Name", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.DriverMobile)] = new() { DisplayName = "Driver Mobile", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.DriverDisplay)] = new() { DisplayName = "Driver", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleTripOMCCardPaymentsOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.EstimatedDistance)] = new() { DisplayName = "Est Dist", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.EstimatedHours)] = new() { DisplayName = "Est Hours", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.EstimatedFuelConsumption)] = new() { DisplayName = "Est Fuel", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.EstimatedCost)] = new() { DisplayName = "Est Cost", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripOMCCardPaymentsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripOMCCardPaymentsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleTripOMCCardPaymentsOverviewModel.OMCCardNumber),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.OMCCardCode),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.PaymentAmount),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.TransactionNo),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.CompanyName),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.TransactionDateTime),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.FinancialYear),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.ChallanNo),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.OMCName),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.VehicleCode),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.FromLocation),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.ToLocation),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.RouteDisplay),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.DriverName),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.DriverMobile),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.DriverDisplay),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.Quantity),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.EstimatedDistance),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.EstimatedHours),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.EstimatedFuelConsumption),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.EstimatedCost),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.TotalExpense),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.Remarks),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.CreatedByName),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.CreatedAt),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.CreatedFromPlatform),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.LastModifiedByUserName),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.LastModifiedAt),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleTripOMCCardPaymentsOverviewModel.OMCCardNumber),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.PaymentAmount),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.TransactionNo),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.CompanyName),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.TransactionDateTime),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.ChallanNo),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.OMCName),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.VehicleCode),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.RouteDisplay),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.DriverDisplay),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.Quantity),
				nameof(VehicleTripOMCCardPaymentsOverviewModel.TotalExpense),
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleTripOMCCardPaymentsOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(VehicleTripOMCCardPaymentsOverviewModel.OMCName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(VehicleTripOMCCardPaymentsOverviewModel.VehicleCode));

			if (route is not null)
				columnOrder.Remove(nameof(VehicleTripOMCCardPaymentsOverviewModel.RouteDisplay));
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
					["Route"] = route?.RouteDisplay ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}
}
