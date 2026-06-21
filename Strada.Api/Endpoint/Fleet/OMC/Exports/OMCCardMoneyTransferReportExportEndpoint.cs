using Strada.Data.Fleet.OMC.Exports;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.OMC;

namespace Strada.Api.Endpoint.Fleet.OMC.Exports;

public class OMCCardMoneyTransferReportExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(OMCCardMoneyTransferReportExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(OMCCardMoneyTransferReportExport.ExportReport), async (OMCCardMoneyTransferReportRequest request) =>
		{
			var (stream, fileName) = await OMCCardMoneyTransferReportExport.ExportReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd,
				request.ShowAllColumns, request.ShowDeleted, request.Company, request.Ledger);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});

		group.MapPost(nameof(OMCCardMoneyTransferReportExport.ExportTransfersReport), async (OMCCardMoneyTransferTransfersReportRequest request) =>
		{
			var (stream, fileName) = await OMCCardMoneyTransferReportExport.ExportTransfersReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd,
				request.ShowAllColumns, request.ShowDeleted, request.OMCCard, request.Company, request.Ledger, request.Omc);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}

	private sealed record OMCCardMoneyTransferReportRequest(
		IEnumerable<OMCCardMoneyTransferOverviewModel> Data, ReportExportType ExportType, DateOnly? DateRangeStart, DateOnly? DateRangeEnd,
		bool ShowAllColumns, bool ShowDeleted, CompanyModel Company, LedgerModel Ledger);

	private sealed record OMCCardMoneyTransferTransfersReportRequest(
		IEnumerable<OMCCardMoneyTransferDetailsOverviewModel> Data, ReportExportType ExportType, DateOnly? DateRangeStart, DateOnly? DateRangeEnd,
		bool ShowAllColumns, bool ShowDeleted, OMCCardModel OMCCard, CompanyModel Company, LedgerModel Ledger, OMCModel Omc);
}
