using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Mailing;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleExpense;

namespace StradaLibrary.Exports.Fleet.VehicleExpense;

internal static class VehicleExpenseNotify
{
    internal static async Task Notify(int expenseId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        if (type == NotifyType.Created)
            return;

        await NotifyByMail(expenseId, type, previousInvoice);
    }

    private static async Task NotifyByMail(int expenseId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        var expense = await CommonData.LoadTableDataById<VehicleExpenseOverviewModel>(FleetNames.VehicleExpenseOverview, expenseId);

        var emailData = new TransactionMailing.TransactionEmailData
        {
            TransactionType = "Vehicle Expense",
            TransactionNo = expense.TransactionNo,
            Action = type,
            LocationName = expense.VehicleCode,
            Details = new Dictionary<string, string>
            {
                ["Transaction Number"] = expense.TransactionNo,
                ["Vehicle"] = $"{expense.VehicleCode}",
                ["Expenses"] = expense.TotalExpense.ToString(),
                [type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = expense.LastModifiedByUserName ?? expense.CreatedByName
            },
            Remarks = expense.Remarks
        };

        if (type == NotifyType.Updated && previousInvoice.HasValue)
        {
            var (afterStream, afterFileName) = await VehicleExpenseInvoiceExport.ExportInvoice(expenseId, InvoiceExportType.PDF);

            var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
            var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

            emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
            emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
        }
        else
        {
            var (pdfStream, pdfFileName) = await VehicleExpenseInvoiceExport.ExportInvoice(expenseId, InvoiceExportType.PDF);
            emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
        }

        await TransactionMailing.SendTransactionEmail(emailData);
    }
}
