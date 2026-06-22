using Strada.Library.Common;
using Strada.Library.DataAccess;

namespace Strada.Library.Fleet.Analysis;

public static class AnalysisData
{
	public static async Task<List<AnalysisMonthlyTrendModel>> LoadDashboardMonthlyTrend(DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<AnalysisMonthlyTrendModel, dynamic>(FleetNames.LoadDashboardMonthlyTrend, new { StartDate, EndDate });

	public static async Task<List<AnalysisVehicleProfitModel>> LoadDashboardTopVehicles(DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<AnalysisVehicleProfitModel, dynamic>(FleetNames.LoadDashboardTopVehicles, new { StartDate, EndDate });

	public static async Task<List<AnalysisExpenseTypeModel>> LoadDashboardExpenseBreakdown(DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<AnalysisExpenseTypeModel, dynamic>(FleetNames.LoadDashboardExpenseBreakdown, new { StartDate, EndDate });
}

public class AnalysisMonthlyTrendModel
{
	public int Year { get; set; }
	public int Month { get; set; }
	public decimal Revenue { get; set; }
	public decimal Expense { get; set; }
}

public class AnalysisVehicleProfitModel
{
	public string VehicleCode { get; set; }
	public decimal Profit { get; set; }
}

public class AnalysisExpenseTypeModel
{
	public string ExpenseTypeName { get; set; }
	public decimal Total { get; set; }
}