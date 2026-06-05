using StradaLibrary.Common;
using StradaLibrary.DataAccess;

namespace StradaLibrary.Fleet.Analysis;

public static class AnalysisData
{
	public static async Task<List<MonthlyTrendModel>> LoadDashboardMonthlyTrend(DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<MonthlyTrendModel, dynamic>(FleetNames.LoadDashboardMonthlyTrend, new { StartDate, EndDate });

	public static async Task<List<VehicleProfitModel>> LoadDashboardTopVehicles(DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<VehicleProfitModel, dynamic>(FleetNames.LoadDashboardTopVehicles, new { StartDate, EndDate });

	public static async Task<List<ExpenseTypeModel>> LoadDashboardExpenseBreakdown(DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<ExpenseTypeModel, dynamic>(FleetNames.LoadDashboardExpenseBreakdown, new { StartDate, EndDate });
}

public class MonthlyTrendModel
{
	public int Year { get; set; }
	public int Month { get; set; }
	public decimal Revenue { get; set; }
	public decimal Expense { get; set; }
}

public class VehicleProfitModel
{
	public string VehicleCode { get; set; }
	public decimal Profit { get; set; }
}

public class ExpenseTypeModel
{
	public string ExpenseTypeName { get; set; }
	public decimal Total { get; set; }
}