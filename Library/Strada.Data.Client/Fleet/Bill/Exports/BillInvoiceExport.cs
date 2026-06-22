using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Fleet.Bill.Exports;

public static class BillInvoiceExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(BillInvoiceExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportInvoice)), new { }, new { transactionId, exportType });
}
