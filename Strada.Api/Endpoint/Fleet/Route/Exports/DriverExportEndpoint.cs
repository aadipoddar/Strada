using Strada.Data.Fleet.Route.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Route;

namespace Strada.Api.Endpoint.Fleet.Route.Exports;

public class DriverExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(DriverExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(DriverExport.ExportMaster), async (IEnumerable<DriverModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await DriverExport.ExportMaster(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
