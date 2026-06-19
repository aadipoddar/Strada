namespace Strada.Models.Fleet.Analysis;

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