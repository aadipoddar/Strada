using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleRoute;
using StradaLibrary.Models.Fleet.VehicleTrip;

namespace StradaLibrary.Exports.Fleet.VehicleTrip;

public static class VehicleTripInvoiceExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType)
	{
		var transaction = await CommonData.LoadTableDataById<VehicleTripOverviewModel>(FleetNames.VehicleTripOverview, transactionId) ??
			throw new InvalidOperationException("Transaction not found.");

		var expenses = await CommonData.LoadTableDataByMasterId<VehicleTripExpensesModel>(FleetNames.VehicleTripExpenses, transaction.Id);
		var payments = await CommonData.LoadTableDataByMasterId<VehicleTripOMCCardPaymentsModel>(FleetNames.VehicleTripOMCCardPayments, transaction.Id);
		var company = await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, transaction.CompanyId);

		LedgerModel ledger = new()
		{
			Name = $"Challan: {transaction.ChallanNo}",
			Address = $" \nFrom: {transaction.FromLocation}" +
			$" \nTo: {transaction.ToLocation}" +
			$" \nVehicle: {transaction.VehicleCode}" +
			$" \nDriver: {transaction.DriverName} ({transaction.DriverMobile})" +
			$" \nQuantity: {transaction.Quantity}",
		};

		var expensetTypes = await CommonData.LoadTableData<VehicleRouteExpenseTypeModel>(FleetNames.VehicleRouteExpenseType);
		var lineItems = expenses.Select(detail =>
		{
			var expense = expensetTypes.FirstOrDefault(p => p.Id == detail.VehicleRouteExpenseTypeId);
			return new VehicleTripExpensesCartModel
			{
				VehicleRouteExpenseTypeId = detail.VehicleRouteExpenseTypeId,
				VehicleRouteExpenseTypeName = expense?.Name ?? $"Expense #{detail.VehicleRouteExpenseTypeId}",
				Amount = detail.Amount,
				Remarks = detail.Remarks
			};
		}).ToList();

		var omcCards = await CommonData.LoadTableData<OMCCardModel>(FleetNames.OMCCard);
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
			new(nameof(VehicleTripExpensesCartModel.VehicleRouteExpenseTypeName), "Expense", exportType, CellAlignment.Left, 0, 30),
			new(nameof(VehicleTripExpensesCartModel.Amount), "Amount", exportType, CellAlignment.Right, 55, 15, "#,##0.00")
		};

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		string fileName = $"VEHICLE_TRIP_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

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
