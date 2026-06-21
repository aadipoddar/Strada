using Strada.Data.Operations.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Operations;

namespace Strada.Api.Endpoint.Operations.Exports;

public class UserExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(UserExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(UserExport.ExportMaster), async (IEnumerable<UserModel> userData, ReportExportType exportType) =>
		{
			var (stream, fileName) = await UserExport.ExportMaster(userData, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
