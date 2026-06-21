using Strada.Data.Fleet.Trip.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Fleet.Trip.Exports;

public class TripInvoiceExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(TripInvoiceExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(TripInvoiceExport.ExportInvoice), async (int transactionId, InvoiceExportType exportType) =>
		{
			var (stream, fileName) = await TripInvoiceExport.ExportInvoice(transactionId, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
