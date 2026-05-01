using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Bill;
using StradaLibrary.Models.Fleet.OMC;

namespace StradaLibrary.Exports.Fleet.Bill;

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
			[nameof(BillOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.BillNo)] = new() { DisplayName = "Bill", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(BillOverviewModel.TotalGrossAmount)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillOverviewModel.TotalTDSAmount)] = new() { DisplayName = "TDS", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillOverviewModel.TotalPenaltyAmount)] = new() { DisplayName = "Penalty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillOverviewModel.TotalNetAmount)] = new() { DisplayName = "Net Amount", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(BillOverviewModel.TotalCardPaymentAmount)] = new() { DisplayName = "Card Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillOverviewModel.TotalLedgerPaymentAmount)] = new() { DisplayName = "Ledger Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(BillOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(BillOverviewModel.CompanyName),
				nameof(BillOverviewModel.TransactionDateTime),
				nameof(BillOverviewModel.FinancialYear),
				nameof(BillOverviewModel.BillNo),
				nameof(BillOverviewModel.OMCName),
				nameof(BillOverviewModel.TotalGrossAmount),
				nameof(BillOverviewModel.TotalTDSAmount),
				nameof(BillOverviewModel.TotalPenaltyAmount),
				nameof(BillOverviewModel.TotalNetAmount),
				nameof(BillOverviewModel.TotalCardPaymentAmount),
				nameof(BillOverviewModel.TotalLedgerPaymentAmount),
				nameof(BillOverviewModel.TransactionNo),
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
				nameof(BillOverviewModel.CompanyName),
				nameof(BillOverviewModel.TransactionDateTime),
				nameof(BillOverviewModel.BillNo),
				nameof(BillOverviewModel.OMCName),
				nameof(BillOverviewModel.TotalGrossAmount),
				nameof(BillOverviewModel.TotalTDSAmount),
				nameof(BillOverviewModel.TotalPenaltyAmount),
				nameof(BillOverviewModel.TotalNetAmount),
				nameof(BillOverviewModel.TotalCardPaymentAmount),
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

		string fileName = $"VEHICLE_TRIP_BILL_REPORT";
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

	public static async Task<(MemoryStream stream, string fileName)> ExportCardPaymentsReport(
		IEnumerable<BillCardPaymentsOverviewModel> paymentsData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		CompanyModel company = null,
		OMCModel omc = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(BillCardPaymentsOverviewModel.OMCCardNumber)] = new() { DisplayName = "OMC Card", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.OMCCardCode)] = new() { DisplayName = "OMC Card Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.PaymentAmount)] = new() { DisplayName = "Payment Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillCardPaymentsOverviewModel.PaymentRemarks)] = new() { DisplayName = "Payment Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(BillCardPaymentsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.BillNo)] = new() { DisplayName = "Bill", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(BillCardPaymentsOverviewModel.TotalGrossAmount)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillCardPaymentsOverviewModel.TotalTDSAmount)] = new() { DisplayName = "TDS", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillCardPaymentsOverviewModel.TotalPenaltyAmount)] = new() { DisplayName = "Penalty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillCardPaymentsOverviewModel.TotalNetAmount)] = new() { DisplayName = "Net Amount", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(BillCardPaymentsOverviewModel.TotalCardPaymentAmount)] = new() { DisplayName = "Card Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillCardPaymentsOverviewModel.TotalLedgerPaymentAmount)] = new() { DisplayName = "Ledger Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(BillCardPaymentsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillCardPaymentsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(BillCardPaymentsOverviewModel.OMCCardNumber),
				nameof(BillCardPaymentsOverviewModel.OMCCardCode),
				nameof(BillCardPaymentsOverviewModel.PaymentAmount),
				nameof(BillCardPaymentsOverviewModel.PaymentRemarks),
				nameof(BillCardPaymentsOverviewModel.CompanyName),
				nameof(BillCardPaymentsOverviewModel.TransactionDateTime),
				nameof(BillCardPaymentsOverviewModel.FinancialYear),
				nameof(BillCardPaymentsOverviewModel.BillNo),
				nameof(BillCardPaymentsOverviewModel.OMCName),
				nameof(BillCardPaymentsOverviewModel.TotalGrossAmount),
				nameof(BillCardPaymentsOverviewModel.TotalTDSAmount),
				nameof(BillCardPaymentsOverviewModel.TotalPenaltyAmount),
				nameof(BillCardPaymentsOverviewModel.TotalNetAmount),
				nameof(BillCardPaymentsOverviewModel.TotalCardPaymentAmount),
				nameof(BillCardPaymentsOverviewModel.TotalLedgerPaymentAmount),
				nameof(BillCardPaymentsOverviewModel.TransactionNo),
				nameof(BillCardPaymentsOverviewModel.Remarks),
				nameof(BillCardPaymentsOverviewModel.CreatedByName),
				nameof(BillCardPaymentsOverviewModel.CreatedAt),
				nameof(BillCardPaymentsOverviewModel.CreatedFromPlatform),
				nameof(BillCardPaymentsOverviewModel.LastModifiedByUserName),
				nameof(BillCardPaymentsOverviewModel.LastModifiedAt),
				nameof(BillCardPaymentsOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(BillCardPaymentsOverviewModel.OMCCardNumber),
				nameof(BillCardPaymentsOverviewModel.PaymentAmount),
				nameof(BillCardPaymentsOverviewModel.CompanyName),
				nameof(BillCardPaymentsOverviewModel.TransactionDateTime),
				nameof(BillCardPaymentsOverviewModel.BillNo),
				nameof(BillCardPaymentsOverviewModel.OMCName),
				nameof(BillCardPaymentsOverviewModel.TotalGrossAmount),
				nameof(BillCardPaymentsOverviewModel.TotalTDSAmount),
				nameof(BillCardPaymentsOverviewModel.TotalPenaltyAmount),
				nameof(BillCardPaymentsOverviewModel.TotalNetAmount),
				nameof(BillCardPaymentsOverviewModel.TotalCardPaymentAmount),
				nameof(BillCardPaymentsOverviewModel.TotalLedgerPaymentAmount)
			];

			if (company is not null)
				columnOrder.Remove(nameof(BillCardPaymentsOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(BillCardPaymentsOverviewModel.OMCName));
		}

		string fileName = $"BILL_CARD_PAYMENTS_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				paymentsData,
				"BILL CARD PAYMENTS REPORT",
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
				"BILL CARD PAYMENTS REPORT",
				"Bill Card Payments Transactions",
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
		CompanyModel company = null,
		OMCModel omc = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(BillLedgerPaymentsOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.LedgerCode)] = new() { DisplayName = "Ledger Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.PaymentAmount)] = new() { DisplayName = "Payment Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillLedgerPaymentsOverviewModel.PaymentRemarks)] = new() { DisplayName = "Payment Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(BillLedgerPaymentsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.BillNo)] = new() { DisplayName = "Bill", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(BillLedgerPaymentsOverviewModel.TotalGrossAmount)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillLedgerPaymentsOverviewModel.TotalTDSAmount)] = new() { DisplayName = "TDS", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillLedgerPaymentsOverviewModel.TotalPenaltyAmount)] = new() { DisplayName = "Penalty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillLedgerPaymentsOverviewModel.TotalNetAmount)] = new() { DisplayName = "Net Amount", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(BillLedgerPaymentsOverviewModel.TotalCardPaymentAmount)] = new() { DisplayName = "Card Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(BillLedgerPaymentsOverviewModel.TotalLedgerPaymentAmount)] = new() { DisplayName = "Ledger Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(BillLedgerPaymentsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(BillLedgerPaymentsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
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
				nameof(BillLedgerPaymentsOverviewModel.CompanyName),
				nameof(BillLedgerPaymentsOverviewModel.TransactionDateTime),
				nameof(BillLedgerPaymentsOverviewModel.FinancialYear),
				nameof(BillLedgerPaymentsOverviewModel.BillNo),
				nameof(BillLedgerPaymentsOverviewModel.OMCName),
				nameof(BillLedgerPaymentsOverviewModel.TotalGrossAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalTDSAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalPenaltyAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalNetAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalCardPaymentAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalLedgerPaymentAmount),
				nameof(BillLedgerPaymentsOverviewModel.TransactionNo),
				nameof(BillLedgerPaymentsOverviewModel.Remarks),
				nameof(BillLedgerPaymentsOverviewModel.CreatedByName),
				nameof(BillLedgerPaymentsOverviewModel.CreatedAt),
				nameof(BillLedgerPaymentsOverviewModel.CreatedFromPlatform),
				nameof(BillLedgerPaymentsOverviewModel.LastModifiedByUserName),
				nameof(BillLedgerPaymentsOverviewModel.LastModifiedAt),
				nameof(BillLedgerPaymentsOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(BillLedgerPaymentsOverviewModel.LedgerName),
				nameof(BillLedgerPaymentsOverviewModel.PaymentAmount),
				nameof(BillLedgerPaymentsOverviewModel.CompanyName),
				nameof(BillLedgerPaymentsOverviewModel.TransactionDateTime),
				nameof(BillLedgerPaymentsOverviewModel.BillNo),
				nameof(BillLedgerPaymentsOverviewModel.OMCName),
				nameof(BillLedgerPaymentsOverviewModel.TotalGrossAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalTDSAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalPenaltyAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalNetAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalCardPaymentAmount),
				nameof(BillLedgerPaymentsOverviewModel.TotalLedgerPaymentAmount)
			];

			if (company is not null)
				columnOrder.Remove(nameof(BillLedgerPaymentsOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(BillLedgerPaymentsOverviewModel.OMCName));
		}

		string fileName = $"VEHICLE_TRIP_BILL_LEDGER_PAYMENTS_REPORT";
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
