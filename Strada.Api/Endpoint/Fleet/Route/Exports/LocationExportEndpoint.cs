using Strada.Data.Fleet.Route.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Route;

namespace Strada.Api.Endpoint.Fleet.Route.Exports;

public class LocationExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(LocationExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(LocationExport.ExportMaster), async (IEnumerable<LocationModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await LocationExport.ExportMaster(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
