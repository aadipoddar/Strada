using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleRepair;

namespace StradaLibrary.Exports.Fleet.VehicleRepair;

public static class VehicleRepairReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<VehicleRepairOverviewModel> repairData,
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
			[nameof(VehicleRepairOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleRepairOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleRepairOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleRepairOverviewModel.TransactionNo),
				nameof(VehicleRepairOverviewModel.CompanyName),
				nameof(VehicleRepairOverviewModel.TransactionDateTime),
				nameof(VehicleRepairOverviewModel.FinancialYear),
				nameof(VehicleRepairOverviewModel.LedgerName),
				nameof(VehicleRepairOverviewModel.VehicleCode),
				nameof(VehicleRepairOverviewModel.TotalExpense),
				nameof(VehicleRepairOverviewModel.Remarks),
				nameof(VehicleRepairOverviewModel.CreatedByName),
				nameof(VehicleRepairOverviewModel.CreatedAt),
				nameof(VehicleRepairOverviewModel.CreatedFromPlatform),
				nameof(VehicleRepairOverviewModel.LastModifiedByUserName),
				nameof(VehicleRepairOverviewModel.LastModifiedAt),
				nameof(VehicleRepairOverviewModel.LastModifiedFromPlatform),
				nameof(VehicleRepairOverviewModel.Status)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(VehicleRepairOverviewModel.Status));
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleRepairOverviewModel.TransactionNo),
				nameof(VehicleRepairOverviewModel.CompanyName),
				nameof(VehicleRepairOverviewModel.TransactionDateTime),
				nameof(VehicleRepairOverviewModel.LedgerName),
				nameof(VehicleRepairOverviewModel.VehicleCode),
				nameof(VehicleRepairOverviewModel.TotalExpense),
				nameof(VehicleRepairOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleRepairOverviewModel.CompanyName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(VehicleRepairOverviewModel.VehicleCode));

			if (!showDeleted)
				columnOrder.Remove(nameof(VehicleRepairOverviewModel.Status));
		}

		string fileName = $"VEHICLE_REPAIR_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				repairData,
				"VEHICLE REPAIR REPORT",
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
				repairData,
				"VEHICLE REPAIR REPORT",
				"Vehicle Repair Transactions",
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
		IEnumerable<VehicleRepairExpensesOverviewModel> expensesData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		CompanyModel company = null,
		VehicleModel vehicle = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleRepairExpensesOverviewModel.ExpenseTypeName)] = new() { DisplayName = "Expense Type", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.ExpenseTypeCode)] = new() { DisplayName = "Expense Type Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.ExpenseAmount)] = new() { DisplayName = "Expense Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleRepairExpensesOverviewModel.IdentificationNo)] = new() { DisplayName = "Identification No", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleRepairExpensesOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			
			[nameof(VehicleRepairExpensesOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.TotalExpense)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleRepairExpensesOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleRepairExpensesOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleRepairExpensesOverviewModel.ExpenseTypeName),
				nameof(VehicleRepairExpensesOverviewModel.ExpenseTypeCode),
				nameof(VehicleRepairExpensesOverviewModel.ExpenseAmount),
				nameof(VehicleRepairExpensesOverviewModel.IdentificationNo),
				nameof(VehicleRepairExpensesOverviewModel.TransactionNo),
				nameof(VehicleRepairExpensesOverviewModel.CompanyName),
				nameof(VehicleRepairExpensesOverviewModel.TransactionDateTime),
				nameof(VehicleRepairExpensesOverviewModel.FinancialYear),
				nameof(VehicleRepairExpensesOverviewModel.LedgerName),
				nameof(VehicleRepairExpensesOverviewModel.VehicleCode),
				nameof(VehicleRepairExpensesOverviewModel.TotalExpense),
				nameof(VehicleRepairExpensesOverviewModel.Remarks),
				nameof(VehicleRepairExpensesOverviewModel.CreatedByName),
				nameof(VehicleRepairExpensesOverviewModel.CreatedAt),
				nameof(VehicleRepairExpensesOverviewModel.CreatedFromPlatform),
				nameof(VehicleRepairExpensesOverviewModel.LastModifiedByUserName),
				nameof(VehicleRepairExpensesOverviewModel.LastModifiedAt),
				nameof(VehicleRepairExpensesOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleRepairExpensesOverviewModel.ExpenseTypeName),
				nameof(VehicleRepairExpensesOverviewModel.ExpenseAmount),
				nameof(VehicleRepairExpensesOverviewModel.IdentificationNo),
				nameof(VehicleRepairExpensesOverviewModel.TransactionNo),
				nameof(VehicleRepairExpensesOverviewModel.CompanyName),
				nameof(VehicleRepairExpensesOverviewModel.TransactionDateTime),
				nameof(VehicleRepairExpensesOverviewModel.LedgerName),
				nameof(VehicleRepairExpensesOverviewModel.VehicleCode),
				nameof(VehicleRepairExpensesOverviewModel.TotalExpense),
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleRepairExpensesOverviewModel.CompanyName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(VehicleRepairExpensesOverviewModel.VehicleCode));
		}

		string fileName = $"VEHICLE_REPAIR_EXPENSES_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				expensesData,
				"VEHICLE REPAIR EXPENSES REPORT",
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
				"VEHICLE REPAIR EXPENSES REPORT",
				"Vehicle Repair Expenses Transactions",
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
