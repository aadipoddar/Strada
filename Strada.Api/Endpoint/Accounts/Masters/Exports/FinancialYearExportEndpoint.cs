using Strada.Data.Accounts.Masters.Exports;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Accounts.Masters.Exports;

public class FinancialYearExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(FinancialYearExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(FinancialYearExport.ExportMaster), async (IEnumerable<FinancialYearModel> financialYearData, ReportExportType exportType) =>
		{
			var (stream, fileName) = await FinancialYearExport.ExportMaster(financialYearData, exportType);
			return TypedResults.File(stream.ToArray(), "application/octet-stream", fileName);
		});
	}
}
