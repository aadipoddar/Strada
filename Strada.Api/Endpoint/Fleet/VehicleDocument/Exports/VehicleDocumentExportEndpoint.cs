using Strada.Data.Fleet.VehicleDocument.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.VehicleDocument;

namespace Strada.Api.Endpoint.Fleet.VehicleDocument.Exports;

public class VehicleDocumentExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VehicleDocumentExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(VehicleDocumentExport.ExportTransaction), async (IEnumerable<VehicleDocumentModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await VehicleDocumentExport.ExportTransaction(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
