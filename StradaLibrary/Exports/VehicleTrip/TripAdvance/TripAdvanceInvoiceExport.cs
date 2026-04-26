using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.VehicleExpense;
using StradaLibrary.Models.VehicleTrip.OMC;
using StradaLibrary.Models.VehicleTrip.TripAdvance;

namespace StradaLibrary.Exports.VehicleTrip.TripAdvance;

public static class TripAdvanceInvoiceExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType)
	{
		var transaction = await CommonData.LoadTableDataById<TripAdvanceOverviewModel>(VehicleTripNames.TripAdvanceOverview, transactionId) ??
			throw new InvalidOperationException("Transaction not found.");

		var expenses = await CommonData.LoadTableDataByMasterId<TripAdvanceExpensesModel>(VehicleTripNames.TripAdvanceExpenses, transaction.Id);
		var payments = await CommonData.LoadTableDataByMasterId<TripAdvanceCardPaymentsModel>(VehicleTripNames.TripAdvanceCardPayments, transaction.Id);
		var company = await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, transaction.CompanyId);

		LedgerModel ledger = new()
		{
			Name = $"Challan: {transaction.ChallanNo ?? "NA"}",
			Address = $"From: {transaction.FromLocation}" +
			$" \nTo: {transaction.ToLocation}" +
			$" \nVehicle: {transaction.VehicleCode}" +
			$" \nDriver: {transaction.DriverName} ({transaction.DriverMobile})" +
			$" \nQuantity: {transaction.Quantity}" +
			$" \nVehicle: {(transaction.VehicleEmpty ? "Empty" : "Loaded")}"
		};

		var expensetTypes = await CommonData.LoadTableData<VehicleExpenseTypeModel>(VehicleExpenseNames.VehicleExpenseType);
		var lineItems = expenses.Select(detail =>
		{
			return new TripAdvanceExpensesCartModel
			{
				VehicleExpenseTypeId = detail.VehicleExpenseTypeId,
				VehicleExpenseTypeName = expensetTypes.FirstOrDefault(p => p.Id == detail.VehicleExpenseTypeId).Name,
				Amount = detail.Amount,
				Remarks = detail.Remarks
			};
		}).ToList();

		var omcCards = await CommonData.LoadTableData<OMCCardModel>(VehicleTripNames.OMCCard);
		Dictionary<string, decimal> paymentModes = [];
		foreach (var payment in payments)
			paymentModes.Add(omcCards.FirstOrDefault(c => c.Id == payment.OMCCardId).CardNumber, payment.Amount);

		var invoiceData = new InvoiceData
		{
			Company = company,
			BillTo = ledger,
			InvoiceType = "VEHICLE TRIP",
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
			new(nameof(TripAdvanceExpensesCartModel.VehicleExpenseTypeName), "Expense", exportType, CellAlignment.Left, 0, 30),
			new(nameof(TripAdvanceExpensesCartModel.Amount), "Amount", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(TripAdvanceExpensesCartModel.Remarks), "Remarks", exportType, CellAlignment.Left, 150, 30)
		};

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		string fileName = $"TRIP_ADVANCE_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == InvoiceExportType.PDF)
		{
			var stream = await PDFInvoiceExportUtil.ExportInvoiceToPdf(
				invoiceData,
				lineItems,
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
				lineItems,
				columnSettings,
				null,
				summaryFields
			);

			fileName += ".xlsx";
			return (stream, fileName);
		}
	}
}
