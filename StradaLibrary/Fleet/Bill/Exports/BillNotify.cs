using StradaLibrary.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Fleet.Bill.Models;
using StradaLibrary.Utils.ExportUtils;
using StradaLibrary.Utils.MailUtils;

namespace StradaLibrary.Fleet.Bill.Exports;

internal static class BillNotify
{
    internal static async Task Notify(int billId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        if (type == NotifyType.Created)
            return;

        await NotifyByMail(billId, type, previousInvoice);
    }

    private static async Task NotifyByMail(int billId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        var bill = await CommonData.LoadTableDataById<BillOverviewModel>(FleetNames.BillOverview, billId);

        var emailData = new TransactionMailing.TransactionEmailData
        {
            TransactionType = "Bill",
            TransactionNo = bill.TransactionNo,
            Action = type,
            LocationName = bill.OMCName,
            Details = new Dictionary<string, string>
            {
                ["Transaction Number"] = bill.TransactionNo,
                ["Bill Number"] = bill.BillNo,
                ["Net Amount"] = bill.TotalNetAmount.FormatIndianCurrency(),
                [type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = bill.LastModifiedByUserName ?? bill.CreatedByName
            },
            Remarks = bill.Remarks
        };

        if (type == NotifyType.Updated && previousInvoice.HasValue)
        {
            var (afterStream, afterFileName) = await BillInvoiceExport.ExportInvoice(billId, InvoiceExportType.PDF);

            var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
            var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

            emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
            emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
        }
        else
        {
            var (pdfStream, pdfFileName) = await BillInvoiceExport.ExportInvoice(billId, InvoiceExportType.PDF);
            emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
        }

        await TransactionMailing.SendTransactionEmail(emailData);
    }
}
