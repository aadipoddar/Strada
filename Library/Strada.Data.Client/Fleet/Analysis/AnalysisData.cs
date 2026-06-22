using Strada.Models.Common;
using Strada.Models.Fleet.Analysis;

namespace Strada.Data.Fleet.Analysis;

public static class AnalysisData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(AnalysisData));

	public static Task<List<AnalysisMonthlyTrendModel>> LoadDashboardMonthlyTrend(DateTime StartDate, DateTime EndDate) =>
		Api.Get<List<AnalysisMonthlyTrendModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadDashboardMonthlyTrend)), new { StartDate, EndDate });

	public static Task<List<AnalysisVehicleProfitModel>> LoadDashboardTopVehicles(DateTime StartDate, DateTime EndDate) =>
		Api.Get<List<AnalysisVehicleProfitModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadDashboardTopVehicles)), new { StartDate, EndDate });

	public static Task<List<AnalysisExpenseTypeModel>> LoadDashboardExpenseBreakdown(DateTime StartDate, DateTime EndDate) =>
		Api.Get<List<AnalysisExpenseTypeModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadDashboardExpenseBreakdown)), new { StartDate, EndDate });
}
