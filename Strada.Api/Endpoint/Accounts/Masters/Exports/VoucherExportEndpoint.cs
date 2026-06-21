using Strada.Data.Accounts.Masters.Exports;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Accounts.Masters.Exports;

public class VoucherExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VoucherExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(VoucherExport.ExportMaster), async (IEnumerable<VoucherModel> voucherData, ReportExportType exportType) =>
		{
			var (stream, fileName) = await VoucherExport.ExportMaster(voucherData, exportType);
			return TypedResults.File(stream.ToArray(), "application/octet-stream", fileName);
		});
	}
}
