using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Fleet.Expense;

using StradaLibrary.Common;
using StradaLibrary.Utils.ExportUtils;

namespace StradaLibrary.Fleet.Expense.Exports;

public static class ExpenseInvoiceExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType)
	{
		var transaction = await CommonData.LoadTableDataById<ExpenseOverviewModel>(FleetNames.ExpenseOverview, transactionId) ??
			throw new InvalidOperationException("Transaction not found.");

		var expenses = await CommonData.LoadTableDataByMasterId<ExpenseDetailsOverviewModel>(FleetNames.ExpenseDetailsOverview, transaction.Id);
		var company = await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, transaction.CompanyId);

		LedgerModel ledger = new()
		{
			Name = $"Vehicle: {transaction.VehicleCode}"
		};

		var invoiceData = new InvoiceData
		{
			Company = company,
			BillTo = ledger,
			InvoiceType = "EXPENSE",
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
			new(nameof(ExpenseDetailsOverviewModel.ExpenseTypeName), "Expense", exportType, CellAlignment.Left, 0, 30),
			new(nameof(ExpenseDetailsOverviewModel.LedgerName), "Ledger", exportType, CellAlignment.Left, 60, 30),
			new(nameof(ExpenseDetailsOverviewModel.ExpenseAmount), "Amount", exportType, CellAlignment.Right, 55, 15, "#,##0.00"),
			new(nameof(ExpenseDetailsOverviewModel.IdentificationNo), "Identification No", exportType, CellAlignment.Left, 100, 30),
			new(nameof(ExpenseDetailsOverviewModel.ExpenseRemarks), "Remarks", exportType, CellAlignment.Left, 150, 30)
		};

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		string fileName = $"VEHICLE_EXPENSE_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

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
