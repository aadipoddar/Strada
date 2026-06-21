using Strada.Data.Fleet.OMC.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Fleet.OMC.Exports;

public class OMCCardMoneyTransferInvoiceExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(OMCCardMoneyTransferInvoiceExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(OMCCardMoneyTransferInvoiceExport.ExportInvoice), async (int transactionId, InvoiceExportType exportType) =>
		{
			var (stream, fileName) = await OMCCardMoneyTransferInvoiceExport.ExportInvoice(transactionId, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
