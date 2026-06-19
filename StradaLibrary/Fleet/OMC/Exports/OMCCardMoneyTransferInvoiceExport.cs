using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Fleet.OMC;

using StradaLibrary.Common;
using StradaLibrary.Utils.ExportUtils;
namespace StradaLibrary.Fleet.OMC.Exports;

public static class OMCCardMoneyTransferInvoiceExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType)
	{
		var transaction = await CommonData.LoadTableDataById<OMCCardMoneyTransferOverviewModel>(FleetNames.OMCCardMoneyTransferOverview, transactionId) ??
			throw new InvalidOperationException("Transaction not found.");

		var transfers = await CommonData.LoadTableDataByMasterId<OMCCardMoneyTransferDetailsOverviewModel>(FleetNames.OMCCardMoneyTransferDetailsOverview, transaction.Id);
		var company = await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, transaction.CompanyId);
		var ledger = await CommonData.LoadTableDataById<LedgerModel>(AccountNames.Ledger, transaction.LedgerId);

		var invoiceData = new InvoiceData
		{
			Company = company,
			BillTo = ledger,
			InvoiceType = "OMC Card Money Transfer",
			TransactionNo = transaction.TransactionNo,
			TransactionDateTime = transaction.TransactionDateTime,
			TotalAmount = transaction.TotalAmount,
			Remarks = transaction.Remarks ?? string.Empty,
			Status = transaction.Status
		};

		var summaryFields = new Dictionary<string, string>
		{
			["Total Amount"] = transaction.TotalAmount.FormatIndianCurrency()
		};

		var columnSettings = new List<InvoiceColumnSetting>
		{
			new("#", "#", exportType, CellAlignment.Center, 25, 5),
			new(nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCCardNumber), "Card Number", exportType, CellAlignment.Left, 0, 30),
			new(nameof(OMCCardMoneyTransferDetailsOverviewModel.OMCName), "OMC", exportType, CellAlignment.Left, 50, 15),
			new(nameof(OMCCardMoneyTransferDetailsOverviewModel.TransferAmount), "Amount", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(OMCCardMoneyTransferDetailsOverviewModel.TransferRemarks), "Remarks", exportType, CellAlignment.Left, 150, 30)
		};

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		string fileName = $"OMC_CARD_MONEY_TRANSFER_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == InvoiceExportType.PDF)
		{
			var stream = await PDFInvoiceExportUtil.ExportInvoiceToPdf(
				invoiceData,
				transfers,
				columnSettings,
				null,
				summaryFields
			);

			fileName += ".pdf";
			return (stream, fileName);
		}
		else
		{
			var stream = await ExcelInvoiceExportUtil.ExportInvoiceToExcel(
				invoiceData,
				transfers,
				columnSettings,
				null,
				summaryFields
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}
}
