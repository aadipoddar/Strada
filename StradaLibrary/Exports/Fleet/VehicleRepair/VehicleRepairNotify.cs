using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Mailing;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleRepair;

namespace StradaLibrary.Exports.Fleet.VehicleRepair;

internal static class VehicleRepairNotify
{
    internal static async Task Notify(int repairId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        if (type == NotifyType.Created)
            return;

        await NotifyByMail(repairId, type, previousInvoice);
    }

    private static async Task NotifyByMail(int repairId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        var repair = await CommonData.LoadTableDataById<VehicleRepairOverviewModel>(FleetNames.VehicleRepairOverview, repairId);

        var emailData = new TransactionMailing.TransactionEmailData
        {
            TransactionType = "Vehicle Repair",
            TransactionNo = repair.TransactionNo,
            Action = type,
            LocationName = repair.VehicleCode,
            Details = new Dictionary<string, string>
            {
                ["Transaction Number"] = repair.TransactionNo,
                ["Vehicle"] = $"{repair.VehicleCode}",
                ["Expenses"] = repair.TotalExpense.ToString(),
                [type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = repair.LastModifiedByUserName ?? repair.CreatedByName
            },
            Remarks = repair.Remarks
        };

        // For update emails, include before and after invoices
        if (type == NotifyType.Updated && previousInvoice.HasValue)
        {
            var (afterStream, afterFileName) = await VehicleRepairInvoiceExport.ExportInvoice(repairId, InvoiceExportType.PDF);

            // Rename files to make it clear which is which
            var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
            var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

            emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
            emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
        }
        else
        {
            // For delete/recover, just attach the current invoice
            var (pdfStream, pdfFileName) = await VehicleRepairInvoiceExport.ExportInvoice(repairId, InvoiceExportType.PDF);
            emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
        }

        await TransactionMailing.SendTransactionEmail(emailData);
    }
}
