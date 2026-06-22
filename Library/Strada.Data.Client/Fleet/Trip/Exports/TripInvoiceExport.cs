using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Fleet.Trip.Exports;

public static class TripInvoiceExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(TripInvoiceExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportInvoice)), new { }, new { transactionId, exportType });
}
