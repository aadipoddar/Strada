using Strada.Data.Fleet.Expense.Exports;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Expense;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Api.Endpoint.Fleet.Expense.Exports;

public class ExpenseReportExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(ExpenseReportExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(ExpenseReportExport.ExportReport), async (ExpenseReportRequest request) =>
		{
			var (stream, fileName) = await ExpenseReportExport.ExportReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd,
				request.ShowAllColumns, request.ShowDeleted, request.Company, request.Vehicle);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});

		group.MapPost(nameof(ExpenseReportExport.ExportExpensesReport), async (ExpenseDetailsReportRequest request) =>
		{
			var (stream, fileName) = await ExpenseReportExport.ExportExpensesReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd,
				request.ShowAllColumns, request.ShowDeleted, request.ExpenseType, request.Company, request.Vehicle);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}

	private sealed record ExpenseReportRequest(
		IEnumerable<ExpenseOverviewModel> Data, ReportExportType ExportType, DateOnly? DateRangeStart, DateOnly? DateRangeEnd,
		bool ShowAllColumns, bool ShowDeleted, CompanyModel Company, VehicleModel Vehicle);

	private sealed record ExpenseDetailsReportRequest(
		IEnumerable<ExpenseDetailsOverviewModel> Data, ReportExportType ExportType, DateOnly? DateRangeStart, DateOnly? DateRangeEnd,
		bool ShowAllColumns, bool ShowDeleted, ExpenseTypeModel ExpenseType, CompanyModel Company, VehicleModel Vehicle);
}
