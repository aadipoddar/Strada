using Strada.Data.Fleet.Vehicle.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Api.Endpoint.Fleet.Vehicle.Exports;

public class VehicleExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VehicleExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(VehicleExport.ExportMaster), async (IEnumerable<VehicleModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await VehicleExport.ExportMaster(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
