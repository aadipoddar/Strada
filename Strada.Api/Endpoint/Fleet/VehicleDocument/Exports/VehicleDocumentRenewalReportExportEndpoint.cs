using Strada.Data.Fleet.VehicleDocument.Exports;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Vehicle;
using Strada.Models.Fleet.VehicleDocument;

namespace Strada.Api.Endpoint.Fleet.VehicleDocument.Exports;

public class VehicleDocumentRenewalReportExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VehicleDocumentRenewalReportExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(VehicleDocumentRenewalReportExport.ExportReport), async (VehicleDocumentRenewalReportRequest request) =>
		{
			var (stream, fileName) = await VehicleDocumentRenewalReportExport.ExportReport(
				request.Data, request.ExportType, request.ShowAllColumns, request.Vehicle, request.DocumentType);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}

	private sealed record VehicleDocumentRenewalReportRequest(
		IEnumerable<VehicleDocumentRenewalOverviewModel> Data,
		ReportExportType ExportType,
		bool ShowAllColumns,
		VehicleModel Vehicle,
		VehicleDocumentTypeModel DocumentType);
}
