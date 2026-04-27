using StradaLibrary.Data.Common;
using StradaLibrary.Data.Fleet.VehicleTrip;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.VehicleTrip;
using StradaLibrary.Models.Fleet.VehicleTripBill;

namespace StradaLibrary.Exports.Fleet.VehicleTripBill;

public static class VehicleTripBillInvoiceExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType)
	{
		var transaction = await CommonData.LoadTableDataById<VehicleTripBillOverviewModel>(FleetNames.VehicleTripBillOverview, transactionId) ??
			throw new InvalidOperationException("Transaction not found.");

		var trips = await VehicleTripData.LoadVehicleTripOverviewByBillIdDate(transaction.Id);
		var cardPayments = await CommonData.LoadTableDataByMasterId<VehicleTripBillCardPaymentsModel>(FleetNames.VehicleTripBillCardPayments, transaction.Id);
		var ledgerPayments = await CommonData.LoadTableDataByMasterId<VehicleTripBillLedgerPaymentsModel>(FleetNames.VehicleTripBillLedgerPayments, transaction.Id);
		var company = await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, transaction.CompanyId);

		LedgerModel ledger = new()
		{
			Name = $"Bill: {transaction.BillNo ?? "N/A"}"
		};

		var omcCards = await CommonData.LoadTableData<OMCCardModel>(FleetNames.OMCCard);
		var ledgers = await CommonData.LoadTableData<LedgerModel>(AccountNames.Ledger);
		Dictionary<string, decimal> paymentModes = [];
		foreach (var payment in cardPayments)
			paymentModes.Add(omcCards.FirstOrDefault(c => c.Id == payment.OMCCardId).CardNumber, payment.Amount);
		foreach (var payment in ledgerPayments)
			paymentModes.Add(ledgers.FirstOrDefault(l => l.Id == payment.LedgerId).Name, payment.Amount);

		var invoiceData = new InvoiceData
		{
			Company = company,
			BillTo = ledger,
			InvoiceType = "VEHICLE TRIP BILL",
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
			["TDS"] = transaction.TotalTDSAmount.FormatIndianCurrency(),
			["Penalty"] = transaction.TotalPenaltyAmount.FormatIndianCurrency(),
			["Net"] = transaction.TotalNetAmount.FormatIndianCurrency()
		};

		var columnSettings = new List<InvoiceColumnSetting>
		{
			new("#", "#", exportType, CellAlignment.Center, 25, 5),
			new(nameof(VehicleTripOverviewModel.VehicleCode), "Vehicle", exportType, CellAlignment.Left, 0, 30),
			new(nameof(VehicleTripOverviewModel.ChallanNo), "Challan", exportType, CellAlignment.Left, 60, 30),
			new(nameof(VehicleTripOverviewModel.Quantity), "Qty", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(VehicleTripOverviewModel.GrossAmount), "Gross", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(VehicleTripOverviewModel.TDSAmount), "TDS", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(VehicleTripOverviewModel.PenaltyAmount), "Penalty", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(VehicleTripOverviewModel.NetAmount), "Net", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
		};

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		string fileName = $"VEHICLE_TRIP_BILL_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

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
