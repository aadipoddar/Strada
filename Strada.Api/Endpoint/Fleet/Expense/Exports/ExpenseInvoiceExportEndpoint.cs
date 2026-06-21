using Strada.Data.Fleet.Expense.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Fleet.Expense.Exports;

public class ExpenseInvoiceExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(ExpenseInvoiceExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(ExpenseInvoiceExport.ExportInvoice), async (int transactionId, InvoiceExportType exportType) =>
		{
			var (stream, fileName) = await ExpenseInvoiceExport.ExportInvoice(transactionId, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
