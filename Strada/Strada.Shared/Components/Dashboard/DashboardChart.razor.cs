using MudBlazor;

using Strada.Library.Fleet.Analysis;

namespace Strada.Shared.Components.Dashboard;

public partial class DashboardChart
{
	// Row 1 - Revenue line / Expense bar
	private List<ChartSeries<double>> _revenueSeries = [];
	private List<ChartSeries<double>> _expenseSeries = [];
	private string[] _labels = [];

	// Row 2 - Top vehicles bar / Expense type donut
	private List<ChartSeries<double>> _vehicleProfitSeries = [];
	private string[] _vehicleLabels = [];

	private List<ChartSeries<double>> _expenseTypeSeries = [];
	private string[] _expenseTypeLabels = [];

	private readonly LineChartOptions _revenueOptions = new()
	{
		ChartPalette = ["#16a34a"],
		YAxisFormat = "N0",
		ShowLegend = false,
		ShowDataMarkers = true,
		LineStrokeWidth = 2.5,
		InterpolationOption = InterpolationOption.NaturalSpline,
	};

	private readonly BarChartOptions _expenseOptions = new()
	{
		ChartPalette = ["#dc2626"],
		YAxisFormat = "N0",
		ShowLegend = false,
		YAxisLines = true,
		XAxisLines = false,
	};

	private readonly BarChartOptions _vehicleProfitOptions = new()
	{
		ChartPalette = ["#2563eb"],
		YAxisFormat = "N0",
		ShowLegend = false,
		YAxisLines = true,
		XAxisLines = false,
		XAxisLabelRotation = 45
	};

	private readonly PieChartOptions _expenseTypeOptions = new()
	{
		ChartPalette = ["#2563eb", "#16a34a", "#f59e0b", "#dc2626", "#8b5cf6", "#0891b2", "#db2777", "#65a30d"],
		ShowValues = true,
	};

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		await LoadData();
	}

	private async Task LoadData()
	{
		try
		{
			// Window: first day of month 11 months ago → end of current month (12 months total).
			var thisMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			var windowStart = thisMonthStart.AddMonths(-11);
			var windowEnd = thisMonthStart.AddMonths(1).AddSeconds(-1);

			// SQL does all the grouping and summing; we just read the finished numbers.
			BuildMonthlyTrend(await AnalysisData.LoadDashboardMonthlyTrend(windowStart, windowEnd), thisMonthStart);
			BuildTopVehicles(await AnalysisData.LoadDashboardTopVehicles(windowStart, windowEnd));
			BuildExpenseBreakdown(await AnalysisData.LoadDashboardExpenseBreakdown(windowStart, windowEnd));
		}
		catch { }
		finally { StateHasChanged(); }
	}

	private void BuildMonthlyTrend(List<AnalysisMonthlyTrendModel> rows, DateTime thisMonthStart)
	{
		var buckets = Enumerable.Range(0, 12)
			.Select(i => thisMonthStart.AddMonths(-11 + i))
			.ToList();

		_labels = [.. buckets.Select(b => b.ToString("MMM"))];

		var revenue = new double[12];
		var expense = new double[12];

		for (int i = 0; i < 12; i++)
		{
			var row = rows.FirstOrDefault(r => r.Year == buckets[i].Year && r.Month == buckets[i].Month);
			revenue[i] = (double)(row?.Revenue ?? 0);
			expense[i] = (double)(row?.Expense ?? 0);
		}

		_revenueSeries = [new ChartSeries<double> { Name = "Revenue", Data = revenue }];
		_expenseSeries = [new ChartSeries<double> { Name = "Expense", Data = expense }];
	}

	private void BuildTopVehicles(List<AnalysisVehicleProfitModel> rows)
	{
		_vehicleLabels = [.. rows.Select(v => v.VehicleCode)];
		var profitData = rows.Select(v => (double)v.Profit).ToArray();
		_vehicleProfitSeries = rows.Count == 0
			? []
			: [new ChartSeries<double> { Name = "Profit", Data = profitData }];
	}

	private void BuildExpenseBreakdown(List<AnalysisExpenseTypeModel> rows)
	{
		// Keep top 7 slices, bucket the rest into "Other" so the donut stays readable.
		const int topN = 7;
		var labels = new List<string>();
		var values = new List<double>();

		foreach (var item in rows.Take(topN))
		{
			labels.Add(item.ExpenseTypeName);
			values.Add((double)item.Total);
		}

		if (rows.Count > topN)
		{
			labels.Add("Other");
			values.Add(rows.Skip(topN).Sum(x => (double)x.Total));
		}

		_expenseTypeLabels = [.. labels];
		_expenseTypeSeries = values.Count == 0 ? [] : values.ToArray().AsChartDataSet();
	}
}
