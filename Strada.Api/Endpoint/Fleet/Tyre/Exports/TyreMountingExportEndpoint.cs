using Strada.Data.Fleet.Tyre.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Tyre;

namespace Strada.Api.Endpoint.Fleet.Tyre.Exports;

public class TyreMountingExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(TyreMountingExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(TyreMountingExport.ExportTransaction), async (IEnumerable<TyreMountingModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await TyreMountingExport.ExportTransaction(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
