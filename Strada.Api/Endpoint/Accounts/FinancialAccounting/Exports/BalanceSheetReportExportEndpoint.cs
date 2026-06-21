using Strada.Data.Accounts.FinancialAccounting.Exports;
using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Accounts.FinancialAccounting.Exports;

public class BalanceSheetReportExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(BalanceSheetReportExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(BalanceSheetReportExport.ExportAssetsReport), async (BalanceSheetReportRequest request) =>
		{
			var (stream, fileName) = await BalanceSheetReportExport.ExportAssetsReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd, request.ShowAllColumns, request.Company);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});

		group.MapPost(nameof(BalanceSheetReportExport.ExportLiabilitiesReport), async (BalanceSheetReportRequest request) =>
		{
			var (stream, fileName) = await BalanceSheetReportExport.ExportLiabilitiesReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd, request.ShowAllColumns, request.Company);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}

	private sealed record BalanceSheetReportRequest(
		IEnumerable<TrialBalanceModel> Data,
		ReportExportType ExportType,
		DateOnly? DateRangeStart,
		DateOnly? DateRangeEnd,
		bool ShowAllColumns,
		CompanyModel Company);
}
