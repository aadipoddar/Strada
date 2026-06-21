using Strada.Data.Accounts.FinancialAccounting.Exports;
using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Accounts.FinancialAccounting.Exports;

public class ProfitAndLossReportExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(ProfitAndLossReportExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(ProfitAndLossReportExport.ExportIncomeReport), async (ProfitAndLossReportRequest request) =>
		{
			var (stream, fileName) = await ProfitAndLossReportExport.ExportIncomeReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd, request.ShowAllColumns, request.Company);
			return TypedResults.File(stream.ToArray(), "application/octet-stream", fileName);
		});

		group.MapPost(nameof(ProfitAndLossReportExport.ExportExpenseReport), async (ProfitAndLossReportRequest request) =>
		{
			var (stream, fileName) = await ProfitAndLossReportExport.ExportExpenseReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd, request.ShowAllColumns, request.Company);
			return TypedResults.File(stream.ToArray(), "application/octet-stream", fileName);
		});
	}

	private sealed record ProfitAndLossReportRequest(
		IEnumerable<TrialBalanceModel> Data,
		ReportExportType ExportType,
		DateOnly? DateRangeStart,
		DateOnly? DateRangeEnd,
		bool ShowAllColumns,
		CompanyModel Company);
}
