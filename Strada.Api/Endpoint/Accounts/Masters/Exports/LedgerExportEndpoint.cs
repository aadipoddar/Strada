using Strada.Data.Accounts.Masters.Exports;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Accounts.Masters.Exports;

public class LedgerExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(LedgerExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(LedgerExport.ExportMaster), async (IEnumerable<LedgerModel> ledgerData, ReportExportType exportType) =>
		{
			var (stream, fileName) = await LedgerExport.ExportMaster(ledgerData, exportType);
			return TypedResults.File(stream.ToArray(), "application/octet-stream", fileName);
		});
	}
}
