using Strada.Data.Operations.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Operations;

namespace Strada.Api.Endpoint.Operations.Exports;

public class AuditTrailExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(AuditTrailExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(AuditTrailExport.ExportReport), async (
			IEnumerable<AuditTrailModel> data,
			ReportExportType exportType,
			DateOnly? dateRangeStart,
			DateOnly? dateRangeEnd,
			bool showAllColumns) =>
		{
			var (stream, fileName) = await AuditTrailExport.ExportReport(data, exportType, dateRangeStart, dateRangeEnd, showAllColumns);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}
}
