using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Fleet.Expense.Exports;

public static class ExpenseInvoiceExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(ExpenseInvoiceExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportInvoice)), new { }, new { transactionId, exportType });
}
