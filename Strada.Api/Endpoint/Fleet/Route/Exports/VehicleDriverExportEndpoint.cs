using Strada.Data.Fleet.Route.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Route;

namespace Strada.Api.Endpoint.Fleet.Route.Exports;

public class VehicleDriverExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VehicleDriverExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(VehicleDriverExport.ExportMaster), async (IEnumerable<VehicleDriverModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await VehicleDriverExport.ExportMaster(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
