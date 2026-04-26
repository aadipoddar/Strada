using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Mailing;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.VehicleTrip.TripAdvance;

namespace StradaLibrary.Exports.VehicleTrip.TripAdvance;

internal static class TripAdvanceNotify
{
    internal static async Task Notify(int tripId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        if (type == NotifyType.Created)
            return;

        await NotifyByMail(tripId, type, previousInvoice);
    }

    private static async Task NotifyByMail(int tripId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        var trip = await CommonData.LoadTableDataById<TripAdvanceOverviewModel>(VehicleTripNames.TripAdvanceOverview, tripId);

        var emailData = new TransactionMailing.TransactionEmailData
        {
            TransactionType = "Trip Advance",
            TransactionNo = trip.TransactionNo,
            Action = type,
            LocationName = trip.OMCName,
            Details = new Dictionary<string, string>
            {
                ["Transaction Number"] = trip.TransactionNo,
                ["Challan Number"] = trip.ChallanNo ?? "N/A",
				["Route"] = $"{trip.FromLocation} to {trip.ToLocation}",
                ["Vehicle"] = $"{trip.VehicleCode}",
                ["Driver"] = $"{trip.DriverName} ({trip.DriverMobile})",
                ["Expenses"] = trip.TotalExpense.ToString(),
                [type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = trip.LastModifiedByUserName ?? trip.CreatedByName
            },
            Remarks = trip.Remarks
        };

        // For update emails, include before and after invoices
        if (type == NotifyType.Updated && previousInvoice.HasValue)
        {
            var (afterStream, afterFileName) = await TripAdvanceInvoiceExport.ExportInvoice(tripId, InvoiceExportType.PDF);

            // Rename files to make it clear which is which
            var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
            var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

            emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
            emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
        }
        else
        {
            // For delete/recover, just attach the current invoice
            var (pdfStream, pdfFileName) = await TripAdvanceInvoiceExport.ExportInvoice(tripId, InvoiceExportType.PDF);
            emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
        }

        await TransactionMailing.SendTransactionEmail(emailData);
    }
}
