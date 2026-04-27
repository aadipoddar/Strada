using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.VehicleTripBill;

namespace StradaLibrary.Exports.Fleet.VehicleTripBill;

public static class VehicleTripBillReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<VehicleTripBillOverviewModel> tripData,
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
			[nameof(VehicleTripBillOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.BillNo)] = new() { DisplayName = "Bill", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			
			[nameof(VehicleTripBillOverviewModel.TotalGrossAmount)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillOverviewModel.TotalTDSAmount)] = new() { DisplayName = "TDS", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillOverviewModel.TotalPenaltyAmount)] = new() { DisplayName = "Penalty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillOverviewModel.TotalNetAmount)] = new() { DisplayName = "Net Amount", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			
			[nameof(VehicleTripBillOverviewModel.TotalCardPaymentAmount)] = new() { DisplayName = "Card Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillOverviewModel.TotalLedgerPaymentAmount)] = new() { DisplayName = "Ledger Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripBillOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripBillOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleTripBillOverviewModel.TransactionNo),
				nameof(VehicleTripBillOverviewModel.CompanyName),
				nameof(VehicleTripBillOverviewModel.TransactionDateTime),
				nameof(VehicleTripBillOverviewModel.FinancialYear),
				nameof(VehicleTripBillOverviewModel.BillNo),
				nameof(VehicleTripBillOverviewModel.OMCName),
				nameof(VehicleTripBillOverviewModel.TotalGrossAmount),
				nameof(VehicleTripBillOverviewModel.TotalTDSAmount),
				nameof(VehicleTripBillOverviewModel.TotalPenaltyAmount),
				nameof(VehicleTripBillOverviewModel.TotalNetAmount),
				nameof(VehicleTripBillOverviewModel.TotalCardPaymentAmount),
				nameof(VehicleTripBillOverviewModel.TotalLedgerPaymentAmount),
				nameof(VehicleTripBillOverviewModel.Remarks),
				nameof(VehicleTripBillOverviewModel.CreatedByName),
				nameof(VehicleTripBillOverviewModel.CreatedAt),
				nameof(VehicleTripBillOverviewModel.CreatedFromPlatform),
				nameof(VehicleTripBillOverviewModel.LastModifiedByUserName),
				nameof(VehicleTripBillOverviewModel.LastModifiedAt),
				nameof(VehicleTripBillOverviewModel.LastModifiedFromPlatform),
				nameof(VehicleTripBillOverviewModel.Status)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(VehicleTripBillOverviewModel.Status));
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleTripBillOverviewModel.TransactionNo),
				nameof(VehicleTripBillOverviewModel.CompanyName),
				nameof(VehicleTripBillOverviewModel.TransactionDateTime),
				nameof(VehicleTripBillOverviewModel.BillNo),
				nameof(VehicleTripBillOverviewModel.OMCName),
				nameof(VehicleTripBillOverviewModel.TotalGrossAmount),
				nameof(VehicleTripBillOverviewModel.TotalTDSAmount),
				nameof(VehicleTripBillOverviewModel.TotalPenaltyAmount),
				nameof(VehicleTripBillOverviewModel.TotalNetAmount),
				nameof(VehicleTripBillOverviewModel.TotalCardPaymentAmount),
				nameof(VehicleTripBillOverviewModel.TotalLedgerPaymentAmount),
				nameof(VehicleTripBillOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleTripBillOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(VehicleTripBillOverviewModel.OMCName));

			if (!showDeleted)
				columnOrder.Remove(nameof(VehicleTripBillOverviewModel.Status));
		}

		string fileName = $"VEHICLE_TRIP_BILL_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				tripData,
				"VEHICLE TRIP BILL REPORT",
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
				"VEHICLE TRIP BILL REPORT",
				"Vehicle Trip Bill Transactions",
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
		IEnumerable<VehicleTripBillCardPaymentsOverviewModel> paymentsData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		CompanyModel company = null,
		OMCModel omc = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleTripBillCardPaymentsOverviewModel.OMCCardNumber)] = new() { DisplayName = "OMC Card", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.OMCCardCode)] = new() { DisplayName = "OMC Card Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.PaymentAmount)] = new() { DisplayName = "Payment Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripBillCardPaymentsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.BillNo)] = new() { DisplayName = "Bill", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleTripBillCardPaymentsOverviewModel.TotalGrossAmount)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.TotalTDSAmount)] = new() { DisplayName = "TDS", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.TotalPenaltyAmount)] = new() { DisplayName = "Penalty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.TotalNetAmount)] = new() { DisplayName = "Net Amount", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripBillCardPaymentsOverviewModel.TotalCardPaymentAmount)] = new() { DisplayName = "Card Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.TotalLedgerPaymentAmount)] = new() { DisplayName = "Ledger Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripBillCardPaymentsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripBillCardPaymentsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleTripBillCardPaymentsOverviewModel.OMCCardNumber),
				nameof(VehicleTripBillCardPaymentsOverviewModel.OMCCardCode),
				nameof(VehicleTripBillCardPaymentsOverviewModel.PaymentAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TransactionNo),
				nameof(VehicleTripBillCardPaymentsOverviewModel.CompanyName),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TransactionDateTime),
				nameof(VehicleTripBillCardPaymentsOverviewModel.FinancialYear),
				nameof(VehicleTripBillCardPaymentsOverviewModel.BillNo),
				nameof(VehicleTripBillCardPaymentsOverviewModel.OMCName),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalGrossAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalTDSAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalPenaltyAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalNetAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalCardPaymentAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalLedgerPaymentAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.Remarks),
				nameof(VehicleTripBillCardPaymentsOverviewModel.CreatedByName),
				nameof(VehicleTripBillCardPaymentsOverviewModel.CreatedAt),
				nameof(VehicleTripBillCardPaymentsOverviewModel.CreatedFromPlatform),
				nameof(VehicleTripBillCardPaymentsOverviewModel.LastModifiedByUserName),
				nameof(VehicleTripBillCardPaymentsOverviewModel.LastModifiedAt),
				nameof(VehicleTripBillCardPaymentsOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleTripBillCardPaymentsOverviewModel.OMCCardNumber),
				nameof(VehicleTripBillCardPaymentsOverviewModel.PaymentAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TransactionNo),
				nameof(VehicleTripBillCardPaymentsOverviewModel.CompanyName),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TransactionDateTime),
				nameof(VehicleTripBillCardPaymentsOverviewModel.BillNo),
				nameof(VehicleTripBillCardPaymentsOverviewModel.OMCName),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalGrossAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalTDSAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalPenaltyAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalNetAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalCardPaymentAmount),
				nameof(VehicleTripBillCardPaymentsOverviewModel.TotalLedgerPaymentAmount)
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleTripBillCardPaymentsOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(VehicleTripBillCardPaymentsOverviewModel.OMCName));
		}

		string fileName = $"VEHICLE_TRIP_BILL_CARD_PAYMENTS_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				paymentsData,
				"VEHICLE TRIP BILL CARD PAYMENTS REPORT",
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
				"VEHICLE TRIP BILL CARD PAYMENTS REPORT",
				"Vehicle Trip Bill Card Payments Transactions",
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
		IEnumerable<VehicleTripBillLedgerPaymentsOverviewModel> paymentsData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		CompanyModel company = null,
		OMCModel omc = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.LedgerCode)] = new() { DisplayName = "Ledger Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.PaymentAmount)] = new() { DisplayName = "Payment Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.BillNo)] = new() { DisplayName = "Bill", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalGrossAmount)] = new() { DisplayName = "Expenses", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalTDSAmount)] = new() { DisplayName = "TDS", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalPenaltyAmount)] = new() { DisplayName = "Penalty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalNetAmount)] = new() { DisplayName = "Net Amount", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalCardPaymentAmount)] = new() { DisplayName = "Card Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalLedgerPaymentAmount)] = new() { DisplayName = "Ledger Payment", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleTripBillLedgerPaymentsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.LedgerName),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.LedgerCode),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.PaymentAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TransactionNo),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.CompanyName),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TransactionDateTime),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.FinancialYear),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.BillNo),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.OMCName),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalGrossAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalTDSAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalPenaltyAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalNetAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalCardPaymentAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalLedgerPaymentAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.Remarks),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.CreatedByName),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.CreatedAt),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.CreatedFromPlatform),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.LastModifiedByUserName),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.LastModifiedAt),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.LastModifiedFromPlatform)
			];
		}
		else
		{
			columnOrder =
			[
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.LedgerName),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.PaymentAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TransactionNo),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.CompanyName),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TransactionDateTime),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.BillNo),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.OMCName),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalGrossAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalTDSAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalPenaltyAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalNetAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalCardPaymentAmount),
				nameof(VehicleTripBillLedgerPaymentsOverviewModel.TotalLedgerPaymentAmount)
			];

			if (company is not null)
				columnOrder.Remove(nameof(VehicleTripBillLedgerPaymentsOverviewModel.CompanyName));

			if (omc is not null)
				columnOrder.Remove(nameof(VehicleTripBillLedgerPaymentsOverviewModel.OMCName));
		}

		string fileName = $"VEHICLE_TRIP_BILL_LEDGER_PAYMENTS_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				paymentsData,
				"VEHICLE TRIP BILL LEDGER PAYMENTS REPORT",
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
				"VEHICLE TRIP BILL LEDGER PAYMENTS REPORT",
				"Vehicle Trip Bill Ledger Payments Transactions",
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
