using Strada.Data.Fleet.OMC.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.OMC;

namespace Strada.Api.Endpoint.Fleet.OMC.Exports;

public class OMCCardExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(OMCCardExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(OMCCardExport.ExportMaster), async (IEnumerable<OMCCardModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await OMCCardExport.ExportMaster(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
