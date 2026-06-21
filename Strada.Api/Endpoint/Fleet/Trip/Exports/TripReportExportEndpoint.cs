using Strada.Data.Fleet.Trip.Exports;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.OMC;
using Strada.Models.Fleet.Route;
using Strada.Models.Fleet.Trip;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Api.Endpoint.Fleet.Trip.Exports;

public class TripReportExportEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(TripReportExportEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(TripReportExport.ExportReport), async (TripReportRequest request) =>
		{
			var (stream, fileName) = await TripReportExport.ExportReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd, request.ShowAllColumns, request.ShowDeleted,
				request.Company, request.Omc, request.Vehicle, request.Route, request.Driver);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});

		group.MapPost(nameof(TripReportExport.ExportExpensesReport), async (TripExpensesReportRequest request) =>
		{
			var (stream, fileName) = await TripReportExport.ExportExpensesReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd, request.ShowAllColumns, request.ShowDeleted,
				request.ExpenseType, request.Company, request.Omc, request.Vehicle, request.Route, request.Driver);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});

		group.MapPost(nameof(TripReportExport.ExportCardPaymentsReport), async (TripCardPaymentsReportRequest request) =>
		{
			var (stream, fileName) = await TripReportExport.ExportCardPaymentsReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd, request.ShowAllColumns, request.ShowDeleted,
				request.OMCCard, request.Company, request.Omc, request.Vehicle, request.Route, request.Driver);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});

		group.MapPost(nameof(TripReportExport.ExportLedgerPaymentsReport), async (TripLedgerPaymentsReportRequest request) =>
		{
			var (stream, fileName) = await TripReportExport.ExportLedgerPaymentsReport(
				request.Data, request.ExportType, request.DateRangeStart, request.DateRangeEnd, request.ShowAllColumns, request.ShowDeleted,
				request.Ledger, request.Company, request.Omc, request.Vehicle, request.Route, request.Driver);
			return TypedResults.File(stream.ToArray(), Helper.ExportContentType, fileName);
		});
	}

	private sealed record TripReportRequest(
		IEnumerable<TripOverviewModel> Data, ReportExportType ExportType, DateOnly? DateRangeStart, DateOnly? DateRangeEnd, bool ShowAllColumns, bool ShowDeleted,
		CompanyModel Company, OMCModel Omc, VehicleModel Vehicle, RouteOverviewModel Route, DriverOverviewModel Driver);

	private sealed record TripExpensesReportRequest(
		IEnumerable<TripExpensesOverviewModel> Data, ReportExportType ExportType, DateOnly? DateRangeStart, DateOnly? DateRangeEnd, bool ShowAllColumns, bool ShowDeleted,
		ExpenseTypeModel ExpenseType, CompanyModel Company, OMCModel Omc, VehicleModel Vehicle, RouteOverviewModel Route, DriverOverviewModel Driver);

	private sealed record TripCardPaymentsReportRequest(
		IEnumerable<TripCardPaymentsOverviewModel> Data, ReportExportType ExportType, DateOnly? DateRangeStart, DateOnly? DateRangeEnd, bool ShowAllColumns, bool ShowDeleted,
		OMCCardModel OMCCard, CompanyModel Company, OMCModel Omc, VehicleModel Vehicle, RouteOverviewModel Route, DriverOverviewModel Driver);

	private sealed record TripLedgerPaymentsReportRequest(
		IEnumerable<TripLedgerPaymentsOverviewModel> Data, ReportExportType ExportType, DateOnly? DateRangeStart, DateOnly? DateRangeEnd, bool ShowAllColumns, bool ShowDeleted,
		LedgerModel Ledger, CompanyModel Company, OMCModel Omc, VehicleModel Vehicle, RouteOverviewModel Route, DriverOverviewModel Driver);
}
