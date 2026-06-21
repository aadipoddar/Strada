using Strada.Data.Accounts.FinancialAccounting.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Accounts.FinancialAccounting.Exports;

public class FinancialAccountingInvoiceExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(FinancialAccountingInvoiceExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(FinancialAccountingInvoiceExport.ExportInvoice), async (int transactionId, InvoiceExportType exportType) =>
		{
			var (stream, fileName) = await FinancialAccountingInvoiceExport.ExportInvoice(transactionId, exportType);
			return TypedResults.File(stream.ToArray(), "application/octet-stream", fileName);
		});
	}
}
