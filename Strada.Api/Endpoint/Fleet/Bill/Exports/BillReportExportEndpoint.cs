using Strada.Data.Fleet.Bill.Exports;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Bill;
using Strada.Models.Fleet.OMC;

namespace Strada.Api.Endpoint.Fleet.Bill.Exports;

public class BillReportExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(BillReportExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(BillReportExport.ExportReport), async (BillReportRequest request) =>
		{
			var (stream, fileName) = await BillReportExport.ExportReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd,
				request.ShowAllColumns, request.ShowDeleted, request.Company, request.Omc);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});

		group.MapPost(nameof(BillReportExport.ExportLedgerPaymentsReport), async (BillLedgerPaymentsReportRequest request) =>
		{
			var (stream, fileName) = await BillReportExport.ExportLedgerPaymentsReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd,
				request.ShowAllColumns, request.ShowDeleted, request.Ledger, request.Company, request.Omc);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}

	private sealed record BillReportRequest(
		IEnumerable<BillOverviewModel> Data, ReportExportType ExportType, DateOnly? DateRangeStart, DateOnly? DateRangeEnd,
		bool ShowAllColumns, bool ShowDeleted, CompanyModel Company, OMCModel Omc);

	private sealed record BillLedgerPaymentsReportRequest(
		IEnumerable<BillLedgerPaymentsOverviewModel> Data, ReportExportType ExportType, DateOnly? DateRangeStart, DateOnly? DateRangeEnd,
		bool ShowAllColumns, bool ShowDeleted, LedgerModel Ledger, CompanyModel Company, OMCModel Omc);
}
