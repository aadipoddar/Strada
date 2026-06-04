using StradaLibrary.Common;
using StradaLibrary.Fleet.OMC.Models;
using StradaLibrary.Utils.ExportUtils;
using StradaLibrary.Utils.MailUtils;

namespace StradaLibrary.Fleet.OMC.Exports;

internal static class OMCCardMoneyTransferNotify
{
	internal static async Task Notify(int transactionId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
	{
		if (type != NotifyType.Created)
			await NotifyByMail(transactionId, type, previousInvoice);
	}

	private static async Task NotifyByMail(int transactionId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
	{
		var transaction = await CommonData.LoadTableDataById<OMCCardMoneyTransferOverviewModel>(FleetNames.OMCCardMoneyTransferOverview, transactionId);

		var emailData = new TransactionMailing.TransactionEmailData
		{
			TransactionType = "OMC Card Money Transfer",
			TransactionNo = transaction.TransactionNo,
			Action = type,
			LocationName = transaction.LedgerName,
			Details = new Dictionary<string, string>
			{
				["Transaction Number"] = transaction.TransactionNo,
				["Ledger"] = $"{transaction.LedgerName}",
				["Amount"] = transaction.TotalAmount.FormatIndianCurrency(),
				[type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = transaction.LastModifiedByUserName ?? transaction.CreatedByName
			},
			Remarks = transaction.Remarks
		};

		if (type == NotifyType.Updated && previousInvoice.HasValue)
		{
			var (afterStream, afterFileName) = await OMCCardMoneyTransferInvoiceExport.ExportInvoice(transactionId, InvoiceExportType.PDF);

			var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
			var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

			emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
			emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
		}
		else
		{
			var (pdfStream, pdfFileName) = await OMCCardMoneyTransferInvoiceExport.ExportInvoice(transactionId, InvoiceExportType.PDF);
			emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
		}

		await TransactionMailing.SendTransactionEmail(emailData);
	}
}
