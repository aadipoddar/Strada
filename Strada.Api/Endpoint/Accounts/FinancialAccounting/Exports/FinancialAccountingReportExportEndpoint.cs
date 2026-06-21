using Strada.Data.Accounts.FinancialAccounting.Exports;
using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Api.Endpoint.Accounts.FinancialAccounting.Exports;

public class FinancialAccountingReportExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(FinancialAccountingReportExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(FinancialAccountingReportExport.ExportReport), async (FinancialAccountingReportRequest request) =>
		{
			var (stream, fileName) = await FinancialAccountingReportExport.ExportReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd,
				request.ShowAllColumns, request.ShowDeleted, request.Company, request.Voucher);
			return TypedResults.File(stream.ToArray(), "application/octet-stream", fileName);
		});

		group.MapPost(nameof(FinancialAccountingReportExport.ExportLedgerReport), async (FinancialAccountingLedgerReportRequest request) =>
		{
			var (stream, fileName) = await FinancialAccountingReportExport.ExportLedgerReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd,
				request.ShowAllColumns, request.ShowDeleted, request.Company, request.Ledger, request.TrialBalance);
			return TypedResults.File(stream.ToArray(), "application/octet-stream", fileName);
		});
	}

	private sealed record FinancialAccountingReportRequest(
		IEnumerable<FinancialAccountingOverviewModel> Data,
		ReportExportType ExportType,
		DateOnly? DateRangeStart,
		DateOnly? DateRangeEnd,
		bool ShowAllColumns,
		bool ShowDeleted,
		CompanyModel Company,
		VoucherModel Voucher);

	private sealed record FinancialAccountingLedgerReportRequest(
		IEnumerable<FinancialAccountingLedgerOverviewModel> Data,
		ReportExportType ExportType,
		DateOnly? DateRangeStart,
		DateOnly? DateRangeEnd,
		bool ShowAllColumns,
		bool ShowDeleted,
		CompanyModel Company,
		LedgerModel Ledger,
		TrialBalanceModel TrialBalance);
}
