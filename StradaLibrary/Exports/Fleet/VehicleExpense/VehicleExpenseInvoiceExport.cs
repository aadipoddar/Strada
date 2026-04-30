using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleExpense;

namespace StradaLibrary.Exports.Fleet.VehicleExpense;

public static class VehicleExpenseInvoiceExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType)
	{
		var transaction = await CommonData.LoadTableDataById<VehicleExpenseOverviewModel>(FleetNames.VehicleExpenseOverview, transactionId) ??
			throw new InvalidOperationException("Transaction not found.");

		var expenses = await CommonData.LoadTableDataByMasterId<VehicleExpenseDetailsOverviewModel>(FleetNames.VehicleExpenseDetailsOverview, transaction.Id);
		var company = await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, transaction.CompanyId);

		LedgerModel ledger = new()
		{
			Name = $"Vehicle: {transaction.VehicleCode}"
		};

		var lineItems = expenses.Select(detail =>
		{
			return new VehicleExpenseDetailsCartModel
			{
				ExpenseTypeId = detail.ExpenseTypeId,
				ExpenseTypeName = detail.ExpenseTypeName,
				LedgerId = detail.LedgerId,
				LedgerName = detail.LedgerName,
				Amount = detail.ExpenseAmount,
				IdentificationNo = detail.IdentificationNo,
				Remarks = detail.Remarks
			};
		}).ToList();

		var invoiceData = new InvoiceData
		{
			Company = company,
			BillTo = ledger,
			InvoiceType = "VEHICLE EXPENSE",
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
			new(nameof(VehicleExpenseDetailsCartModel.ExpenseTypeName), "Expense", exportType, CellAlignment.Left, 0, 30),
			new(nameof(VehicleExpenseDetailsCartModel.LedgerName), "Ledger", exportType, CellAlignment.Left, 60, 30),
			new(nameof(VehicleExpenseDetailsCartModel.Amount), "Amount", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(VehicleExpenseDetailsCartModel.IdentificationNo), "Identification No", exportType, CellAlignment.Left, 100, 30),
			new(nameof(VehicleExpenseDetailsCartModel.Remarks), "Remarks", exportType, CellAlignment.Left, 150, 30)
		};

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		string fileName = $"VEHICLE_EXPENSE_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

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
