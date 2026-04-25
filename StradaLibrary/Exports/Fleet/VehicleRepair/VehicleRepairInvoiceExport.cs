using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleRepair;

namespace StradaLibrary.Exports.Fleet.VehicleRepair;

public static class VehicleRepairInvoiceExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType)
	{
		var transaction = await CommonData.LoadTableDataById<VehicleRepairOverviewModel>(FleetNames.VehicleRepairOverview, transactionId) ??
			throw new InvalidOperationException("Transaction not found.");

		var expenses = await CommonData.LoadTableDataByMasterId<VehicleRepairExpensesModel>(FleetNames.VehicleRepairExpenses, transaction.Id);
		var company = await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, transaction.CompanyId);

		LedgerModel ledger = new();

		if (transaction.LedgerId is not null)
		{
			ledger = await CommonData.LoadTableDataById<LedgerModel>(AccountNames.Ledger, transaction.LedgerId.Value);
			ledger.Address = $"\nVehicle: {transaction.VehicleCode}";
		}

		else
			ledger.Name = $"Vehicle: {transaction.VehicleCode}";

		var expensetTypes = await CommonData.LoadTableData<VehicleExpenseTypeModel>(FleetNames.VehicleExpenseType);
		var lineItems = expenses.Select(detail =>
		{
			return new VehicleRepairExpensesCartModel
			{
				VehicleExpenseTypeId = detail.VehicleExpenseTypeId,
				VehicleExpenseTypeName = expensetTypes.FirstOrDefault(p => p.Id == detail.VehicleExpenseTypeId).Name,
				Amount = detail.Amount,
				IdentificationNo = detail.IdentificationNo,
				Remarks = detail.Remarks
			};
		}).ToList();

		var invoiceData = new InvoiceData
		{
			Company = company,
			BillTo = ledger,
			InvoiceType = "VEHICLE REPAIR",
			TransactionNo = transaction.TransactionNo,
			TransactionDateTime = transaction.TransactionDateTime,
			TotalAmount = transaction.TotalExpense,
			Remarks = transaction.Remarks ?? string.Empty,
			Status = transaction.Status
		};

		var summaryFields = new Dictionary<string, string>
		{
			["Total Expenses"] = transaction.TotalExpense.FormatIndianCurrency()
		};

		var columnSettings = new List<InvoiceColumnSetting>
		{
			new("#", "#", exportType, CellAlignment.Center, 25, 5),
			new(nameof(VehicleRepairExpensesCartModel.VehicleExpenseTypeName), "Expense", exportType, CellAlignment.Left, 0, 30),
			new(nameof(VehicleRepairExpensesCartModel.Amount), "Amount", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(VehicleRepairExpensesCartModel.IdentificationNo), "Identification No", exportType, CellAlignment.Left, 100, 30),
			new(nameof(VehicleRepairExpensesCartModel.Remarks), "Remarks", exportType, CellAlignment.Left, 150, 30)
		};

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		string fileName = $"VEHICLE_REPAIR_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

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
