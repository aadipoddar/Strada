using StradaLibrary.Accounts.Masters.Models;
using StradaLibrary.Common;
using StradaLibrary.Fleet.Trip.Models;
using StradaLibrary.Utils.ExportUtils;

namespace StradaLibrary.Fleet.Trip.Exports;

public static class TripInvoiceExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType)
	{
		var transaction = await CommonData.LoadTableDataById<TripOverviewModel>(FleetNames.TripOverview, transactionId) ??
			throw new InvalidOperationException("Transaction not found.");

		var expenses = await CommonData.LoadTableDataByMasterId<TripExpensesOverviewModel>(FleetNames.TripExpensesOverview, transaction.Id);
		var cardPayments = await CommonData.LoadTableDataByMasterId<TripCardPaymentsOverviewModel>(FleetNames.TripCardPaymentsOverview, transaction.Id);
		var ledgerPayments = await CommonData.LoadTableDataByMasterId<TripLedgerPaymentsOverviewModel>(FleetNames.TripLedgerPaymentsOverview, transaction.Id);
		var company = await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, transaction.CompanyId);

		LedgerModel ledger = new()
		{
			Name = $"Challan: {transaction.ChallanNo ?? "N/A"}",
			Address = $"From: {transaction.FromLocation}" +
			$" \nTo: {transaction.ToLocation}" +
			$" \nVehicle: {transaction.VehicleCode}" +
			$" \nDriver: {transaction.DriverName} ({transaction.DriverMobile})" +
			$" \nQuantity: {transaction.Quantity}" +
			$" \nVehicle: {(transaction.VehicleEmpty ? "Empty" : "Loaded")}"
		};

		Dictionary<string, decimal> paymentModes = [];
		foreach (var payment in cardPayments)
			paymentModes.Add(payment.OMCCardNumber, payment.PaymentAmount);
		foreach (var payment in ledgerPayments)
			paymentModes.Add(payment.LedgerName, payment.PaymentAmount);

		var invoiceData = new InvoiceData
		{
			Company = company,
			BillTo = ledger,
			InvoiceType = "TRIP",
			OCM = transaction.OMCName,
			TransactionNo = transaction.TransactionNo,
			TransactionDateTime = transaction.TransactionDateTime,
			TotalAmount = transaction.TotalExpense,
			Remarks = transaction.Remarks ?? string.Empty,
			Status = transaction.Status,
			PaymentModes = paymentModes
		};

		var summaryFields = new Dictionary<string, string>
		{
			["Total Expenses"] = transaction.TotalExpense.FormatIndianCurrency()
		};

		var columnSettings = new List<InvoiceColumnSetting>
		{
			new("#", "#", exportType, CellAlignment.Center, 25, 5),
			new(nameof(TripExpensesOverviewModel.ExpenseTypeName), "Expense", exportType, CellAlignment.Left, 0, 30),
			new(nameof(TripExpensesOverviewModel.ExpenseAmount), "Amount", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(TripExpensesOverviewModel.ExpenseRemarks), "Remarks", exportType, CellAlignment.Left, 150, 30)
		};

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		string fileName = $"VEHICLE_TRIP_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == InvoiceExportType.PDF)
		{
			var stream = await PDFInvoiceExportUtil.ExportInvoiceToPdf(
				invoiceData,
				expenses,
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
				expenses,
				columnSettings,
				null,
				summaryFields
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}
}
