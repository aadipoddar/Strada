using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Expense;
using StradaLibrary.Models.Fleet.Vehicle;

namespace StradaLibrary.Exports.Fleet.Expense;

public static class ExpenseReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<ExpenseOverviewModel> expenseData,
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
			[nameof(ExpenseOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(ExpenseOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(ExpenseOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false },
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(ExpenseOverviewModel.TransactionNo),
				nameof(ExpenseOverviewModel.CompanyName),
				nameof(ExpenseOverviewModel.TransactionDateTime),
				nameof(ExpenseOverviewModel.FinancialYear),
				nameof(ExpenseOverviewModel.VehicleCode),
				nameof(ExpenseOverviewModel.TotalExpense),
				nameof(ExpenseOverviewModel.Remarks),
				nameof(ExpenseOverviewModel.CreatedByName),
				nameof(ExpenseOverviewModel.CreatedAt),
				nameof(ExpenseOverviewModel.CreatedFromPlatform),
				nameof(ExpenseOverviewModel.LastModifiedByUserName),
				nameof(ExpenseOverviewModel.LastModifiedAt),
				nameof(ExpenseOverviewModel.LastModifiedFromPlatform),
				nameof(ExpenseOverviewModel.Status)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(ExpenseOverviewModel.Status));
		}
		else
		{
			columnOrder =
			[
				nameof(ExpenseOverviewModel.TransactionDateTime),
				nameof(ExpenseOverviewModel.VehicleCode),
				nameof(ExpenseOverviewModel.TotalExpense),
				nameof(ExpenseOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(ExpenseOverviewModel.CompanyName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(ExpenseOverviewModel.VehicleCode));

			if (!showDeleted)
				columnOrder.Remove(nameof(ExpenseOverviewModel.Status));
		}

		string fileName = $"EXPENSE_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				expenseData,
				"EXPENSE REPORT",
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
				"EXPENSE REPORT",
				"Expense Transactions",
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
		IEnumerable<ExpenseDetailsOverviewModel> expensesData,
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
			[nameof(ExpenseDetailsOverviewModel.ExpenseTypeName)] = new() { DisplayName = "Expense Type", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.ExpenseTypeCode)] = new() { DisplayName = "Expense Type Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.ExpenseAmount)] = new() { DisplayName = "Expense Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(ExpenseDetailsOverviewModel.IdentificationNo)] = new() { DisplayName = "Identification No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.ExpenseRemarks)] = new() { DisplayName = "Expense Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(ExpenseDetailsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(ExpenseDetailsOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(ExpenseDetailsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(ExpenseDetailsOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(ExpenseDetailsOverviewModel.ExpenseTypeName),
				nameof(ExpenseDetailsOverviewModel.ExpenseTypeCode),
				nameof(ExpenseDetailsOverviewModel.LedgerName),
				nameof(ExpenseDetailsOverviewModel.ExpenseAmount),
				nameof(ExpenseDetailsOverviewModel.IdentificationNo),
				nameof(ExpenseDetailsOverviewModel.ExpenseRemarks),
				nameof(ExpenseDetailsOverviewModel.CompanyName),
				nameof(ExpenseDetailsOverviewModel.TransactionDateTime),
				nameof(ExpenseDetailsOverviewModel.FinancialYear),
				nameof(ExpenseDetailsOverviewModel.VehicleCode),
				nameof(ExpenseDetailsOverviewModel.TotalExpense),
				nameof(ExpenseDetailsOverviewModel.TransactionNo),
				nameof(ExpenseDetailsOverviewModel.Remarks),
				nameof(ExpenseDetailsOverviewModel.CreatedByName),
				nameof(ExpenseDetailsOverviewModel.CreatedAt),
				nameof(ExpenseDetailsOverviewModel.CreatedFromPlatform),
				nameof(ExpenseDetailsOverviewModel.LastModifiedByUserName),
				nameof(ExpenseDetailsOverviewModel.LastModifiedAt),
				nameof(ExpenseDetailsOverviewModel.LastModifiedFromPlatform),
				nameof(ExpenseDetailsOverviewModel.Status)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(ExpenseDetailsOverviewModel.Status));
		}
		else
		{
			columnOrder =
			[
				nameof(ExpenseDetailsOverviewModel.ExpenseTypeName),
				nameof(ExpenseDetailsOverviewModel.LedgerName),
				nameof(ExpenseDetailsOverviewModel.ExpenseAmount),
				nameof(ExpenseDetailsOverviewModel.IdentificationNo),
				nameof(ExpenseDetailsOverviewModel.TransactionDateTime),
				nameof(ExpenseDetailsOverviewModel.VehicleCode),
				nameof(ExpenseDetailsOverviewModel.TotalExpense),
				nameof(ExpenseDetailsOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(ExpenseDetailsOverviewModel.CompanyName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(ExpenseDetailsOverviewModel.VehicleCode));

			if (!showDeleted)
				columnOrder.Remove(nameof(ExpenseDetailsOverviewModel.Status));
		}

		string fileName = $"EXPENSE_DETAILS_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				expensesData,
				"EXPENSE DETAILS REPORT",
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
				"EXPENSE DETAILS REPORT",
				"Expense Details Transactions",
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
