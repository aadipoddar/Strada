using Strada.Data.Fleet.Route.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Route;

namespace Strada.Api.Endpoint.Fleet.Route.Exports;

public class RouteExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(RouteExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(RouteExport.ExportMaster), async (IEnumerable<RouteModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await RouteExport.ExportMaster(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
