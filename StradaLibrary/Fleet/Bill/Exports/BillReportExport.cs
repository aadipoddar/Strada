using StradaLibrary.Accounts.Masters.Models;
using StradaLibrary.Fleet.Bill.Models;
using StradaLibrary.Fleet.OMC.Models;
using StradaLibrary.Utils.ExportUtils;

namespace StradaLibrary.Fleet.Bill.Exports;

public static class BillReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<BillOverviewModel> tripData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showDeleted = false,
		CompanyModel company = null,
		OMCModel omc = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(BillOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.BillNo)] = new() { DisplayName = "Bill", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(BillOverviewModel.TotalGrossAmount)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillOverviewModel.TotalPenaltyAmount)] = new() { DisplayName = "Penalty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillOverviewModel.TotalNetAmount)] = new() { DisplayName = "Net Amount", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(BillOverviewModel.TotalLedgerPaymentAmount)] = new() { DisplayName = "Ledger Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(BillOverviewModel.FinancialAccountingTransactionNo)] = new() { DisplayName = "Accounting Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(BillOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(BillOverviewModel.TransactionNo),
				nameof(BillOverviewModel.CompanyName),
				nameof(BillOverviewModel.TransactionDateTime),
				nameof(BillOverviewModel.FinancialYear),
				nameof(BillOverviewModel.BillNo),
				nameof(BillOverviewModel.OMCName),
				nameof(BillOverviewModel.TotalGrossAmount),
				nameof(BillOverviewModel.TotalPenaltyAmount),
				nameof(BillOverviewModel.TotalNetAmount),
				nameof(BillOverviewModel.TotalLedgerPaymentAmount),
				nameof(BillOverviewModel.FinancialAccountingTransactionNo),
				nameof(BillOverviewModel.Remarks),
				nameof(BillOverviewModel.CreatedByName),
				nameof(BillOverviewModel.CreatedAt),
				nameof(BillOverviewModel.CreatedFromPlatform),
				nameof(BillOverviewModel.LastModifiedByUserName),
				nameof(BillOverviewModel.LastModifiedAt),
				nameof(BillOverviewModel.LastModifiedFromPlatform),
				nameof(BillOverviewModel.Status)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(BillOverviewModel.Status));
		}
		else
		{
			columnOrder =
			[
				nameof(BillOverviewModel.TransactionDateTime),
				nameof(BillOverviewModel.BillNo),
				nameof(BillOverviewModel.OMCName),
				nameof(BillOverviewModel.TotalGrossAmount),
				nameof(BillOverviewModel.TotalPenaltyAmount),
				nameof(BillOverviewModel.TotalNetAmount),
				nameof(BillOverviewModel.TotalLedgerPaymentAmount),
				nameof(BillOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(BillOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(BillOverviewModel.OMCName));

			if (!showDeleted)
				columnOrder.Remove(nameof(BillOverviewModel.Status));
		}

		string fileName = $"BILL_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				tripData,
				"BILL REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				tripData,
				"BILL REPORT",
				"Bill Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}

	public static async Task<(MemoryStream stream, string fileName)> ExportLedgerPaymentsReport(
		IEnumerable<BillLedgerPaymentsOverviewModel> paymentsData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showDeleted = false,
		CompanyModel company = null,
		OMCModel omc = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(BillLedgerPaymentsOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.LedgerCode)] = new() { DisplayName = "Ledger Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.PaymentAmount)] = new() { DisplayName = "Payment Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillLedgerPaymentsOverviewModel.PaymentRemarks)] = new() { DisplayName = "Payment Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(BillLedgerPaymentsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.BillNo)] = new() { DisplayName = "Bill", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(BillLedgerPaymentsOverviewModel.TotalGrossAmount)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillLedgerPaymentsOverviewModel.TotalPenaltyAmount)] = new() { DisplayName = "Penalty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillLedgerPaymentsOverviewModel.TotalNetAmount)] = new() { DisplayName = "Net Amount", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(BillLedgerPaymentsOverviewModel.TotalLedgerPaymentAmount)] = new() { DisplayName = "Ledger Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(BillLedgerPaymentsOverviewModel.FinancialAccountingTransactionNo)] = new() { DisplayName = "Accounting Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(BillLedgerPaymentsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.MasterStatus)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(BillLedgerPaymentsOverviewModel.LedgerName),
				nameof(BillLedgerPaymentsOverviewModel.LedgerCode),
				nameof(BillLedgerPaymentsOverviewModel.PaymentAmount),
				nameof(BillLedgerPaymentsOverviewModel.PaymentRemarks),
				nameof(BillLedgerPaymentsOverviewModel.TransactionNo),
				nameof(BillLedgerPaymentsOverviewModel.CompanyName),
				nameof(BillLedgerPaymentsOverviewModel.TransactionDateTime),
				nameof(BillLedgerPaymentsOverviewModel.FinancialYear),
				nameof(BillLedgerPaymentsOverviewModel.BillNo),
				nameof(BillLedgerPaymentsOverviewModel.OMCName),
				nameof(BillLedgerPaymentsOverviewModel.TotalGrossAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalPenaltyAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalNetAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalLedgerPaymentAmount),
				nameof(BillLedgerPaymentsOverviewModel.FinancialAccountingTransactionNo),
				nameof(BillLedgerPaymentsOverviewModel.Remarks),
				nameof(BillLedgerPaymentsOverviewModel.CreatedByName),
				nameof(BillLedgerPaymentsOverviewModel.CreatedAt),
				nameof(BillLedgerPaymentsOverviewModel.CreatedFromPlatform),
				nameof(BillLedgerPaymentsOverviewModel.LastModifiedByUserName),
				nameof(BillLedgerPaymentsOverviewModel.LastModifiedAt),
				nameof(BillLedgerPaymentsOverviewModel.LastModifiedFromPlatform),
				nameof(BillLedgerPaymentsOverviewModel.MasterStatus)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(BillLedgerPaymentsOverviewModel.MasterStatus));
		}
		else
		{
			columnOrder =
			[
				nameof(BillLedgerPaymentsOverviewModel.LedgerName),
				nameof(BillLedgerPaymentsOverviewModel.PaymentAmount),
				nameof(BillLedgerPaymentsOverviewModel.TransactionDateTime),
				nameof(BillLedgerPaymentsOverviewModel.BillNo),
				nameof(BillLedgerPaymentsOverviewModel.OMCName),
				nameof(BillLedgerPaymentsOverviewModel.TotalGrossAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalPenaltyAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalNetAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalLedgerPaymentAmount),
				nameof(BillLedgerPaymentsOverviewModel.MasterStatus)
			];

			if (company is not null)
				columnOrder.Remove(nameof(BillLedgerPaymentsOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(BillLedgerPaymentsOverviewModel.OMCName));

			if (!showDeleted)
				columnOrder.Remove(nameof(BillLedgerPaymentsOverviewModel.MasterStatus));
		}

		string fileName = $"BILL_LEDGER_PAYMENTS_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				paymentsData,
				"BILL LEDGER PAYMENTS REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				paymentsData,
				"BILL LEDGER PAYMENTS REPORT",
				"Bill Ledger Payments Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["OMC"] = omc?.Name ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}
}
