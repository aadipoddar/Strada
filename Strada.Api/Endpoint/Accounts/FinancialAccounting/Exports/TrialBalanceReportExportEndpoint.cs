using Strada.Data.Accounts.FinancialAccounting.Exports;
using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Accounts.FinancialAccounting.Exports;

public class TrialBalanceReportExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(TrialBalanceReportExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(TrialBalanceReportExport.ExportReport), async (TrialBalanceReportRequest request) =>
		{
			var (stream, fileName) = await TrialBalanceReportExport.ExportReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd,
				request.ShowAllColumns, request.Company, request.Group, request.AccountType);
			return TypedResults.File(stream.ToArray(), "application/octet-stream", fileName);
		});
	}

	private sealed record TrialBalanceReportRequest(
		IEnumerable<TrialBalanceModel> Data,
		ReportExportType ExportType,
		DateOnly? DateRangeStart,
		DateOnly? DateRangeEnd,
		bool ShowAllColumns,
		CompanyModel Company,
		GroupModel Group,
		AccountTypeModel AccountType);
}
