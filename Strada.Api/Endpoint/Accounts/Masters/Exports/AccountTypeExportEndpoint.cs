using Strada.Data.Accounts.Masters.Exports;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Accounts.Masters.Exports;

public class AccountTypeExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(AccountTypeExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(AccountTypeExport.ExportMaster), async (IEnumerable<AccountTypeModel> accountTypeData, ReportExportType exportType) =>
		{
			var (stream, fileName) = await AccountTypeExport.ExportMaster(accountTypeData, exportType);
			return TypedResults.File(stream.ToArray(), "application/octet-stream", fileName);
		});
	}
}
