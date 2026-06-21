using Strada.Data.Fleet.OMC.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.OMC;

namespace Strada.Api.Endpoint.Fleet.OMC.Exports;

public class OMCExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(OMCExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(OMCExport.ExportMaster), async (IEnumerable<OMCModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await OMCExport.ExportMaster(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
