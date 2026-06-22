using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.OMC;
using Strada.Models.Fleet.Route;
using Strada.Models.Fleet.Trip;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Data.Fleet.Trip.Exports;

public static class TripReportExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(TripReportExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportReport(IEnumerable<TripOverviewModel> tripData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = false, CompanyModel company = null, OMCModel omc = null, VehicleModel vehicle = null, RouteOverviewModel route = null, DriverOverviewModel driver = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportReport)),
			new { data = tripData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, company, omc, vehicle, route, driver });

	public static async Task<(MemoryStream stream, string fileName)> ExportExpensesReport(IEnumerable<TripExpensesOverviewModel> expensesData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = false, ExpenseTypeModel expenseType = null, CompanyModel company = null, OMCModel omc = null, VehicleModel vehicle = null, RouteOverviewModel route = null, DriverOverviewModel driver = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportExpensesReport)),
			new { data = expensesData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, expenseType, company, omc, vehicle, route, driver });

	public static async Task<(MemoryStream stream, string fileName)> ExportCardPaymentsReport(IEnumerable<TripCardPaymentsOverviewModel> paymentsData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = false, OMCCardModel omcCard = null, CompanyModel company = null, OMCModel omc = null, VehicleModel vehicle = null, RouteOverviewModel route = null, DriverOverviewModel driver = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportCardPaymentsReport)),
			new { data = paymentsData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, omcCard, company, omc, vehicle, route, driver });

	public static async Task<(MemoryStream stream, string fileName)> ExportLedgerPaymentsReport(IEnumerable<TripLedgerPaymentsOverviewModel> paymentsData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = false, LedgerModel ledger = null, CompanyModel company = null, OMCModel omc = null, VehicleModel vehicle = null, RouteOverviewModel route = null, DriverOverviewModel driver = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportLedgerPaymentsReport)),
			new { data = paymentsData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, ledger, company, omc, vehicle, route, driver });
}
