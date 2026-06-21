using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.FinancialAccounting.Exports;

public static class FinancialAccountingInvoiceExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(FinancialAccountingInvoiceExport));

	public static Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportInvoice)), new { }, new { transactionId, exportType });
}
