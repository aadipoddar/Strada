using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Fleet.Bill;
using Strada.Models.Fleet.Trip;

using StradaLibrary.Common;
using StradaLibrary.Fleet.Trip;
using StradaLibrary.Utils.ExportUtils;

namespace StradaLibrary.Fleet.Bill.Exports;

public static class BillInvoiceExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType)
	{
		var transaction = await CommonData.LoadTableDataById<BillOverviewModel>(FleetNames.BillOverview, transactionId) ??
			throw new InvalidOperationException("Transaction not found.");

		var trips = await TripData.LoadTripOverviewByBillIdDate(transaction.Id);
		var ledgerPayments = await CommonData.LoadTableDataByMasterId<BillLedgerPaymentsOverviewModel>(FleetNames.BillLedgerPaymentsOverview, transaction.Id);
		var company = await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, transaction.CompanyId);

		LedgerModel ledger = new()
		{
			Name = $"Bill: {transaction.BillNo ?? "N/A"}"
		};

		Dictionary<string, decimal> paymentModes = [];
		foreach (var payment in ledgerPayments)
			paymentModes.Add(payment.LedgerName, payment.PaymentAmount);

		var invoiceData = new InvoiceData
		{
			Company = company,
			BillTo = ledger,
			InvoiceType = "BILL",
			OCM = transaction.OMCName,
			TransactionNo = transaction.TransactionNo,
			TransactionDateTime = transaction.TransactionDateTime,
			TotalAmount = transaction.TotalNetAmount,
			Remarks = transaction.Remarks ?? string.Empty,
			Status = transaction.Status,
			PaymentModes = paymentModes
		};

		var summaryFields = new Dictionary<string, string>
		{
			["Gross"] = transaction.TotalGrossAmount.FormatIndianCurrency(),
			["Penalty"] = transaction.TotalPenaltyAmount.FormatIndianCurrency(),
			["Net"] = transaction.TotalNetAmount.FormatIndianCurrency()
		};

		var columnSettings = new List<InvoiceColumnSetting>
		{
			new("#", "#", exportType, CellAlignment.Center, 25, 5),
			new(nameof(TripOverviewModel.VehicleCode), "Vehicle", exportType, CellAlignment.Left, 60, 30),
			new(nameof(TripOverviewModel.ChallanNo), "Challan", exportType, CellAlignment.Left, 60, 30),
			new(nameof(TripOverviewModel.RouteDisplay), "Route", exportType, CellAlignment.Left, 0, 30),
			new(nameof(TripOverviewModel.Quantity), "Qty", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(TripOverviewModel.GrossAmount), "Gross", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(TripOverviewModel.PenaltyAmount), "Penalty", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(TripOverviewModel.NetAmount), "Net", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
		};

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		string fileName = $"BILL_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == InvoiceExportType.PDF)
		{
			var stream = await PDFInvoiceExportUtil.ExportInvoiceToPdf(
				invoiceData,
				trips,
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
				trips,
				columnSettings,
				null,
				summaryFields
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}
}
