using Strada.Library.Accounts.Masters.Models;
using Strada.Library.Fleet.Bill.Models;
using Strada.Library.Fleet.OMC.Models;
using Strada.Library.Utils.ExportUtils;

namespace Strada.Library.Fleet.OMC.Exports;

public static class OMCCardMoneyTransferReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<OMCCardMoneyTransferOverviewModel> transferData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showDeleted = false,
		CompanyModel company = null,
		LedgerModel ledger = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(OMCCardMoneyTransferOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.TotalItems)] = new() { DisplayName = "Total Items", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(OMCCardMoneyTransferOverviewModel.TotalAmount)] = new() { DisplayName = "Total Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(OMCCardMoneyTransferOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.FinancialAccountingTransactionNo)] = new() { DisplayName = "Accounting Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false },
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(OMCCardMoneyTransferOverviewModel.TransactionNo),
				nameof(OMCCardMoneyTransferOverviewModel.CompanyName),
				nameof(OMCCardMoneyTransferOverviewModel.TransactionDateTime),
				nameof(OMCCardMoneyTransferOverviewModel.FinancialYear),
				nameof(OMCCardMoneyTransferOverviewModel.LedgerName),
				nameof(OMCCardMoneyTransferOverviewModel.TotalItems),
				nameof(OMCCardMoneyTransferOverviewModel.TotalAmount),
				nameof(OMCCardMoneyTransferOverviewModel.Remarks),
				nameof(OMCCardMoneyTransferOverviewModel.FinancialAccountingTransactionNo),
				nameof(OMCCardMoneyTransferOverviewModel.CreatedByName),
				nameof(OMCCardMoneyTransferOverviewModel.CreatedAt),
				nameof(OMCCardMoneyTransferOverviewModel.CreatedFromPlatform),
				nameof(OMCCardMoneyTransferOverviewModel.LastModifiedByUserName),
				nameof(OMCCardMoneyTransferOverviewModel.LastModifiedAt),
				nameof(OMCCardMoneyTransferOverviewModel.LastModifiedFromPlatform),
				nameof(OMCCardMoneyTransferOverviewModel.Status)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(OMCCardMoneyTransferOverviewModel.Status));
		}
		else
		{
			columnOrder =
			[
				nameof(OMCCardMoneyTransferOverviewModel.TransactionDateTime),
				nameof(OMCCardMoneyTransferOverviewModel.LedgerName),
				nameof(OMCCardMoneyTransferOverviewModel.TotalAmount),
				nameof(OMCCardMoneyTransferOverviewModel.Status)
			];

			if (company is not null)
				columnOrder.Remove(nameof(OMCCardMoneyTransferOverviewModel.CompanyName));

			if (ledger is not null)
				columnOrder.Remove(nameof(OMCCardMoneyTransferOverviewModel.LedgerName));

			if (!showDeleted)
				columnOrder.Remove(nameof(OMCCardMoneyTransferOverviewModel.Status));
		}

		string fileName = "OMC_MONEY_TRANSFER_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				transferData,
				"OMC MONEY TRANSFER REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns,
				new()
				{
					["Company"] = company?.Name ?? null,
					["Ledger"] = ledger?.Code ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				transferData,
				"OMC MONEY TRANSFER REPORT",
				"OMC Money Transfers",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["Company"] = company?.Name ?? null,
					["Ledger"] = ledger?.Code ?? null,
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}

	public static async Task<(MemoryStream stream, string fileName)> ExportTransfersReport(
		IEnumerable<OMCCardMoneyTransferDetailsOverviewModel> transfersData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showDeleted = false,
		OMCCardModel oMCCard = null,
		CompanyModel company = null,
		LedgerModel ledger = null,
		OMCModel omc = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCCardNumber)] = new() { DisplayName = "OMC Card", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCCardCode)] = new() { DisplayName = "OMC Card Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCName)] = new() { DisplayName = "OMC", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.TransferAmount)] = new() { DisplayName = "Transfer Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.TransferRemarks)] = new() { DisplayName = "Transfer Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(OMCCardMoneyTransferDetailsOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Left, IncludeInTotal = false },

			[nameof(OMCCardMoneyTransferDetailsOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.TotalItems)] = new() { DisplayName = "Total Items", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.TotalAmount)] = new() { DisplayName = "Total Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },

			[nameof(OMCCardMoneyTransferDetailsOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(BillOverviewModel.FinancialAccountingTransactionNo)] = new() { DisplayName = "Accounting Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OMCCardMoneyTransferDetailsOverviewModel.MasterStatus)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder;

		if (showAllColumns)
		{
			columnOrder =
			[
				nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCCardNumber),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCCardCode),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCName),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.TransferAmount),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.TransferRemarks),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.CompanyName),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.TransactionDateTime),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.FinancialYear),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.LedgerName),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.TotalItems),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.TotalAmount),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.TransactionNo),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.Remarks),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.FinancialAccountingTransactionNo),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.CreatedByName),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.CreatedAt),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.CreatedFromPlatform),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.LastModifiedByUserName),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.LastModifiedAt),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.LastModifiedFromPlatform),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.MasterStatus)
			];

			if (!showDeleted)
				columnOrder.Remove(nameof(OMCCardMoneyTransferDetailsOverviewModel.MasterStatus));
		}
		else
		{
			columnOrder =
			[
				nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCCardNumber),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.TransferAmount),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCName),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.TransactionDateTime),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.LedgerName),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.TotalAmount),
				nameof(OMCCardMoneyTransferDetailsOverviewModel.MasterStatus)
			];

			if (oMCCard is not null)
				columnOrder.Remove(nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCCardNumber));

			if (company is not null)
				columnOrder.Remove(nameof(OMCCardMoneyTransferDetailsOverviewModel.CompanyName));

			if (ledger is not null)
				columnOrder.Remove(nameof(OMCCardMoneyTransferDetailsOverviewModel.LedgerName));

			if (omc is not null)
				columnOrder.Remove(nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCName));

			if (!showDeleted)
				columnOrder.Remove(nameof(OMCCardMoneyTransferDetailsOverviewModel.MasterStatus));
		}

		string fileName = $"OMC_CARD_MONEY_TRANSFER_DETAILS_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				transfersData,
				"OMC CARD MONEY TRANSFER DETAILS REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns,
				new()
				{
					["OMC Card"] = oMCCard?.CardNumber ?? null,
					["Company"] = company?.Name ?? null,
					["Ledger"] = ledger?.Code ?? null,
					["OMC"] = omc?.Name ?? null
				}
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				transfersData,
				"OMC CARD MONEY TRANSFER DETAILS REPORT",
				"OMC Card Money Transfer Details Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new()
				{
					["OMC Card"] = oMCCard?.CardNumber ?? null,
					["Company"] = company?.Name ?? null,
					["Ledger"] = ledger?.Code ?? null,
					["OMC"] = omc?.Name ?? null
				}
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}
}
