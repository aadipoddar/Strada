using Strada.Data.Common;
using Strada.Data.Operations.Data;
using Strada.Data.Utils.ExportUtils;
using Strada.Data.Utils.MailUtils;
using Strada.Models.Common;
using Strada.Models.Fleet.Bill;

namespace Strada.Data.Fleet.Bill.Exports;

internal static class BillNotify
{
	internal static async Task Notify(int transactionId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
	{
		if (type != NotifyType.Created)
			await NotifyByMail(transactionId, type, previousInvoice);
	}

	private static async Task NotifyByMail(int transactionId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
	{
		var transaction = await CommonData.LoadTableDataById<BillOverviewModel>(FleetNames.BillOverview, transactionId);

		var emailData = new TransactionMailing.TransactionEmailData
		{
			TransactionType = "Bill",
			TransactionNo = transaction.TransactionNo,
			Action = type,
			LocationName = transaction.OMCName,
			Details = new Dictionary<string, string>
			{
				["Transaction Number"] = transaction.TransactionNo,
				["Bill Number"] = transaction.BillNo,
				["Net Amount"] = transaction.TotalNetAmount.FormatIndianCurrency(),
				[type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = transaction.LastModifiedByUserName ?? transaction.CreatedByName
			},
			Remarks = transaction.Remarks,
			Differences = type == NotifyType.Updated ? (await AuditTrailData.LoadLastAuditTrailByTableRecord(FleetNames.Bill, transaction.TransactionNo)).RecordValue : null
		};

		if (type == NotifyType.Updated && previousInvoice.HasValue)
		{
			var (afterStream, afterFileName) = await BillInvoiceExport.ExportInvoice(transactionId, InvoiceExportType.PDF);

			var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
			var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

			emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
			emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
		}
		else
		{
			var (pdfStream, pdfFileName) = await BillInvoiceExport.ExportInvoice(transactionId, InvoiceExportType.PDF);
			emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
		}

		await TransactionMailing.SendTransactionEmail(emailData);
	}
}
