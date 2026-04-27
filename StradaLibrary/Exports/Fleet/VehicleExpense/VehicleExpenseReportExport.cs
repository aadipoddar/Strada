using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleExpense;

namespace StradaLibrary.Exports.Fleet.VehicleExpense;

public static class VehicleExpenseReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<VehicleExpenseOverviewModel> expenseData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showDeleted = false,
		CompanyModel company = null,
		VehicleModel vehicle = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleExpenseOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleExpenseOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleExpenseOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleExpenseOverviewModel.TransactionNo),
				nameof(VehicleExpenseOverviewModel.CompanyName),
				nameof(VehicleExpenseOverviewModel.TransactionDateTime),
				nameof(VehicleExpenseOverviewModel.FinancialYear),
				nameof(VehicleExpenseOverviewModel.LedgerName),
				nameof(VehicleExpenseOverviewModel.VehicleCode),
				nameof(VehicleExpenseOverviewModel.TotalExpense),
				nameof(VehicleExpenseOverviewModel.Remarks),
				nameof(VehicleExpenseOverviewModel.CreatedByName),
				nameof(VehicleExpenseOverviewModel.CreatedAt),
				nameof(VehicleExpenseOverviewModel.CreatedFromPlatform),
				nameof(VehicleExpenseOverviewModel.LastModifiedByUserName),
				nameof(VehicleExpenseOverviewModel.LastModifiedAt),
				nameof(VehicleExpenseOverviewModel.LastModifiedFromPlatform),
				nameof(VehicleExpenseOverviewModel.Status)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(VehicleExpenseOverviewModel.Status));
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleExpenseOverviewModel.TransactionNo),
				nameof(VehicleExpenseOverviewModel.CompanyName),
				nameof(VehicleExpenseOverviewModel.TransactionDateTime),
				nameof(VehicleExpenseOverviewModel.LedgerName),
				nameof(VehicleExpenseOverviewModel.VehicleCode),
				nameof(VehicleExpenseOverviewModel.TotalExpense),
				nameof(VehicleExpenseOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleExpenseOverviewModel.CompanyName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(VehicleExpenseOverviewModel.VehicleCode));

			if (!showDeleted)
				columnOrder.Remove(nameof(VehicleExpenseOverviewModel.Status));
		}

		string fileName = $"VEHICLE_EXPENSE_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				expenseData,
				"VEHICLE EXPENSE REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns,
				new()
				{
					["Company"] = company?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				expenseData,
				"VEHICLE EXPENSE REPORT",
				"Vehicle Expense Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null,
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}

	public static async Task<(MemoryStream stream, string fileName)> ExportExpensesReport(
		IEnumerable<VehicleExpenseDetailsOverviewModel> expensesData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		CompanyModel company = null,
		VehicleModel vehicle = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleExpenseDetailsOverviewModel.ExpenseTypeName)] = new() { DisplayName = "Expense Type", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.ExpenseTypeCode)] = new() { DisplayName = "Expense Type Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.ExpenseAmount)] = new() { DisplayName = "Expense Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleExpenseDetailsOverviewModel.IdentificationNo)] = new() { DisplayName = "Identification No", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleExpenseDetailsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleExpenseDetailsOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleExpenseDetailsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleExpenseDetailsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleExpenseDetailsOverviewModel.ExpenseTypeName),
				nameof(VehicleExpenseDetailsOverviewModel.ExpenseTypeCode),
				nameof(VehicleExpenseDetailsOverviewModel.ExpenseAmount),
				nameof(VehicleExpenseDetailsOverviewModel.IdentificationNo),
				nameof(VehicleExpenseDetailsOverviewModel.TransactionNo),
				nameof(VehicleExpenseDetailsOverviewModel.CompanyName),
				nameof(VehicleExpenseDetailsOverviewModel.TransactionDateTime),
				nameof(VehicleExpenseDetailsOverviewModel.FinancialYear),
				nameof(VehicleExpenseDetailsOverviewModel.LedgerName),
				nameof(VehicleExpenseDetailsOverviewModel.VehicleCode),
				nameof(VehicleExpenseDetailsOverviewModel.TotalExpense),
				nameof(VehicleExpenseDetailsOverviewModel.Remarks),
				nameof(VehicleExpenseDetailsOverviewModel.CreatedByName),
				nameof(VehicleExpenseDetailsOverviewModel.CreatedAt),
				nameof(VehicleExpenseDetailsOverviewModel.CreatedFromPlatform),
				nameof(VehicleExpenseDetailsOverviewModel.LastModifiedByUserName),
				nameof(VehicleExpenseDetailsOverviewModel.LastModifiedAt),
				nameof(VehicleExpenseDetailsOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleExpenseDetailsOverviewModel.ExpenseTypeName),
				nameof(VehicleExpenseDetailsOverviewModel.ExpenseAmount),
				nameof(VehicleExpenseDetailsOverviewModel.IdentificationNo),
				nameof(VehicleExpenseDetailsOverviewModel.TransactionNo),
				nameof(VehicleExpenseDetailsOverviewModel.CompanyName),
				nameof(VehicleExpenseDetailsOverviewModel.TransactionDateTime),
				nameof(VehicleExpenseDetailsOverviewModel.LedgerName),
				nameof(VehicleExpenseDetailsOverviewModel.VehicleCode),
				nameof(VehicleExpenseDetailsOverviewModel.TotalExpense),
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleExpenseDetailsOverviewModel.CompanyName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(VehicleExpenseDetailsOverviewModel.VehicleCode));
		}

		string fileName = $"VEHICLE_EXPENSE_DETAILS_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				expensesData,
				"VEHICLE EXPENSE DETAILS REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns,
				new()
				{
					["Company"] = company?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				expensesData,
				"VEHICLE EXPENSE DETAILS REPORT",
				"Vehicle Expense Details Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["Vehicle"] = vehicle?.Code ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}
}
