using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Fleet.OMC.Exports;

public static class OMCCardMoneyTransferInvoiceExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(OMCCardMoneyTransferInvoiceExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportInvoice)), new { }, new { transactionId, exportType });
}
