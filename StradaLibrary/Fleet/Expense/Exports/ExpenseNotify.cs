using StradaLibrary.Common;
using StradaLibrary.Fleet.Expense.Models;
using StradaLibrary.Utils.ExportUtils;
using StradaLibrary.Utils.MailUtils;

namespace StradaLibrary.Fleet.Expense.Exports;

internal static class ExpenseNotify
{
	internal static async Task Notify(int transactionId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
	{
		if (type != NotifyType.Created)
			await NotifyByMail(transactionId, type, previousInvoice);
	}

	private static async Task NotifyByMail(int transactionId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
	{
		var transaction = await CommonData.LoadTableDataById<ExpenseOverviewModel>(FleetNames.ExpenseOverview, transactionId);

		var emailData = new TransactionMailing.TransactionEmailData
		{
			TransactionType = "Expense",
			TransactionNo = transaction.TransactionNo,
			Action = type,
			LocationName = transaction.VehicleCode,
			Details = new Dictionary<string, string>
			{
				["Transaction Number"] = transaction.TransactionNo,
				["Vehicle"] = $"{transaction.VehicleCode}",
				["Expenses"] = transaction.TotalExpense.FormatIndianCurrency(),
				[type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = transaction.LastModifiedByUserName ?? transaction.CreatedByName
			},
			Remarks = transaction.Remarks
		};

		if (type == NotifyType.Updated && previousInvoice.HasValue)
		{
			var (afterStream, afterFileName) = await ExpenseInvoiceExport.ExportInvoice(transactionId, InvoiceExportType.PDF);

			var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
			var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

			emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
			emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
		}
		else
		{
			var (pdfStream, pdfFileName) = await ExpenseInvoiceExport.ExportInvoice(transactionId, InvoiceExportType.PDF);
			emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
		}

		await TransactionMailing.SendTransactionEmail(emailData);
	}
}
