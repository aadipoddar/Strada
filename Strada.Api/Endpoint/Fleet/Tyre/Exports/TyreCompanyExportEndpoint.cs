using Strada.Data.Fleet.Tyre.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Tyre;

namespace Strada.Api.Endpoint.Fleet.Tyre.Exports;

public class TyreCompanyExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(TyreCompanyExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(TyreCompanyExport.ExportMaster), async (IEnumerable<TyreCompanyModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await TyreCompanyExport.ExportMaster(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
