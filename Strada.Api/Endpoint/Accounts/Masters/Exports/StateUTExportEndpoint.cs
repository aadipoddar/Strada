using Strada.Data.Accounts.Masters.Exports;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Accounts.Masters.Exports;

public class StateUTExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(StateUTExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(StateUTExport.ExportMaster), async (IEnumerable<StateUTModel> stateUTData, ReportExportType exportType) =>
		{
			var (stream, fileName) = await StateUTExport.ExportMaster(stateUTData, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
