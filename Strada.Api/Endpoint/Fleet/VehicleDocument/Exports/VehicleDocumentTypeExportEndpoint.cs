using Strada.Data.Fleet.VehicleDocument.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.VehicleDocument;

namespace Strada.Api.Endpoint.Fleet.VehicleDocument.Exports;

public class VehicleDocumentTypeExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VehicleDocumentTypeExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(VehicleDocumentTypeExport.ExportMaster), async (IEnumerable<VehicleDocumentTypeModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await VehicleDocumentTypeExport.ExportMaster(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
