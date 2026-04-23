using StradaLibrary.Exports.Mailing;

namespace StradaLibrary.Exports.Fleet.VehicleTrip;

internal static class VehicleTripNotify
{
    internal static async Task Notify(int tripId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        if (type == NotifyType.Created)
            return;

        await NotifyByMail(tripId, type, previousInvoice);
    }

    private static async Task NotifyByMail(int tripId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        //var trip = await CommonData.LoadTableDataById<FinancialAccountingOverviewModel>(AccountNames.FinancialAccountingOverview, tripId);

        //var emailData = new TransactionMailing.TransactionEmailData
        //{
        //    TransactionType = "Accounting",
        //    TransactionNo = trip.TransactionNo,
        //    Action = type,
        //    LocationName = trip.VoucherName,
        //    Details = new Dictionary<string, string>
        //    {
        //        ["Transaction Number"] = trip.TransactionNo,
        //        ["Voucher"] = trip.VoucherName,
        //        ["Transaction Date"] = trip.TransactionDateTime.ToString("dd MMM yyyy, hh:mm tt"),
        //        ["Total Ledgers"] = trip.TotalLedgers.ToString(),
        //        ["Debit Ledgers"] = trip.TotalDebitLedgers.ToString(),
        //        ["Credit Ledgers"] = trip.TotalCreditLedgers.ToString(),
        //        ["Total Debit"] = trip.TotalDebitAmount.FormatIndianCurrency(),
        //        ["Total Credit"] = trip.TotalCreditAmount.FormatIndianCurrency(),
        //        ["Total Amount"] = trip.TotalAmount.FormatIndianCurrency(),
        //        [type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = trip.LastModifiedByUserName ?? trip.CreatedByName
        //    },
        //    Remarks = trip.Remarks
        //};

        //// For update emails, include before and after invoices
        //if (type == NotifyType.Updated && previousInvoice.HasValue)
        //{
        //    var (afterStream, afterFileName) = await FinancialAccountingInvoiceExport.ExportInvoice(accountingId, InvoiceExportType.PDF);

        //    // Rename files to make it clear which is which
        //    var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
        //    var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

        //    emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
        //    emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
        //}
        //else
        //{
        //    // For delete/recover, just attach the current invoice
        //    var (pdfStream, pdfFileName) = await FinancialAccountingInvoiceExport.ExportInvoice(accountingId, InvoiceExportType.PDF);
        //    emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
        //}

        //await TransactionMailing.SendTransactionEmail(emailData);
    }
}
