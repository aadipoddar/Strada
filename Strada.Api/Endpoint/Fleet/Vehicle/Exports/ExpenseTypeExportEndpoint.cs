using Strada.Data.Fleet.Vehicle.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Api.Endpoint.Fleet.Vehicle.Exports;

public class ExpenseTypeExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(ExpenseTypeExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(ExpenseTypeExport.ExportMaster), async (IEnumerable<ExpenseTypeModel> data, ReportExportType exportType) =>
		{
			var (stream, fileName) = await ExpenseTypeExport.ExportMaster(data, exportType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
