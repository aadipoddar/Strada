using Strada.Data.Fleet.Vehicle.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Api.Endpoint.Fleet.Vehicle.Exports;

public class VehicleTypeExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VehicleTypeExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(VehicleTypeExport.ExportMaster), async (IEnumerable<VehicleTypeModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await VehicleTypeExport.ExportMaster(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
