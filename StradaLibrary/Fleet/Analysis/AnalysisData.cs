using Strada.Models.Common;
using Strada.Models.Fleet.Analysis;

using StradaLibrary.DataAccess;

namespace StradaLibrary.Fleet.Analysis;

public static class AnalysisData
{
	public static async Task<List<AnalysisMonthlyTrendModel>> LoadDashboardMonthlyTrend(DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<AnalysisMonthlyTrendModel, dynamic>(FleetNames.LoadDashboardMonthlyTrend, new { StartDate, EndDate });

	public static async Task<List<AnalysisVehicleProfitModel>> LoadDashboardTopVehicles(DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<AnalysisVehicleProfitModel, dynamic>(FleetNames.LoadDashboardTopVehicles, new { StartDate, EndDate });

	public static async Task<List<AnalysisExpenseTypeModel>> LoadDashboardExpenseBreakdown(DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<AnalysisExpenseTypeModel, dynamic>(FleetNames.LoadDashboardExpenseBreakdown, new { StartDate, EndDate });
}