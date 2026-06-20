using Strada.Data.Common;
using Strada.Data.Operations.Data;
using Strada.Data.Utils.ExportUtils;
using Strada.Data.Utils.MailUtils;
using Strada.Models.Common;
using Strada.Models.Fleet.Trip;

namespace Strada.Data.Fleet.Trip.Exports;

internal static class TripNotify
{
	internal static async Task Notify(int transactionId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
	{
		if (type != NotifyType.Created)
			await NotifyByMail(transactionId, type, previousInvoice);
	}

	private static async Task NotifyByMail(int transactionId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
	{
		var transaction = await CommonData.LoadTableDataById<TripOverviewModel>(FleetNames.TripOverview, transactionId);

		var emailData = new TransactionMailing.TransactionEmailData
		{
			TransactionType = "Trip",
			TransactionNo = transaction.TransactionNo,
			Action = type,
			LocationName = transaction.OMCName,
			Details = new Dictionary<string, string>
			{
				["Transaction Number"] = transaction.TransactionNo,
				["Sl No"] = transaction.SlNo ?? "N/A",
				["Challan Number"] = transaction.ChallanNo ?? "N/A",
				["Route"] = $"{transaction.FromLocation} to {transaction.ToLocation}",
				["Vehicle"] = $"{transaction.VehicleCode}",
				["Driver"] = $"{transaction.DriverName} ({transaction.DriverMobile})",
				["Expenses"] = transaction.TotalExpense.ToString(),
				[type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = transaction.LastModifiedByUserName ?? transaction.CreatedByName
			},
			Remarks = transaction.Remarks,
			Differences = type == NotifyType.Updated ? (await AuditTrailData.LoadLastAuditTrailByTableRecord(FleetNames.Trip, transaction.TransactionNo)).RecordValue : null
		};

		// For update emails, include before and after invoices
		if (type == NotifyType.Updated && previousInvoice.HasValue)
		{
			var (afterStream, afterFileName) = await TripInvoiceExport.ExportInvoice(transactionId, InvoiceExportType.PDF);

			// Rename files to make it clear which is which
			var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
			var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

			emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
			emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
		}
		else
		{
			// For delete/recover, just attach the current invoice
			var (pdfStream, pdfFileName) = await TripInvoiceExport.ExportInvoice(transactionId, InvoiceExportType.PDF);
			emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
		}

		await TransactionMailing.SendTransactionEmail(emailData);
	}
}
