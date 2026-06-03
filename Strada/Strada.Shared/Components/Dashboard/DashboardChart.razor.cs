using Microsoft.Extensions.Caching.Memory;

using MudBlazor;

using StradaLibrary.Fleet.Expense.Models;
using StradaLibrary.Fleet.Trip.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Operations.Models;

namespace Strada.Shared.Components.Dashboard;

public partial class DashboardChart
{
	private int _cacheHours = 12;

	private List<TripOverviewModel> _trips = [];
	private List<ExpenseOverviewModel> _expenses = [];
	private List<TripExpensesOverviewModel> _tripExpenses = [];
	private List<ExpenseDetailsOverviewModel> _expenseDetails = [];

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
	};

	private readonly PieChartOptions _expenseTypeOptions = new()
	{
		ChartPalette = ["#2563eb", "#16a34a", "#f59e0b", "#dc2626", "#8b5cf6", "#0891b2", "#db2777", "#65a30d"],
		ShowValues = true,
	};

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		// if (!firstRender)
		return;

		await LoadData();
	}

	private async Task LoadData()
	{
		if (LoadCached())
			return;

		await LoadFresh();

		var expiry = TimeSpan.FromHours(_cacheHours);
		MemoryCache.Set(StorageFileNames.TripsYearOverviewDataFileName, _trips, expiry);
		MemoryCache.Set(StorageFileNames.ExpensesYearOverviewDataFileName, _expenses, expiry);
		MemoryCache.Set(StorageFileNames.TripExpensesYearOverviewDataFileName, _tripExpenses, expiry);
		MemoryCache.Set(StorageFileNames.ExpenseDetailsYearOverviewDataFileName, _expenseDetails, expiry);
	}

	private bool LoadCached()
	{
		if (!MemoryCache.TryGetValue(StorageFileNames.TripsYearOverviewDataFileName, out List<TripOverviewModel> trips))
			return false;

		_trips = trips ?? [];
		_expenses = MemoryCache.Get<List<ExpenseOverviewModel>>(StorageFileNames.ExpensesYearOverviewDataFileName) ?? [];
		_tripExpenses = MemoryCache.Get<List<TripExpensesOverviewModel>>(StorageFileNames.TripExpensesYearOverviewDataFileName) ?? [];
		_expenseDetails = MemoryCache.Get<List<ExpenseDetailsOverviewModel>>(StorageFileNames.ExpenseDetailsYearOverviewDataFileName) ?? [];

		BuildAll();
		StateHasChanged();
		return true;
	}

	private async Task LoadFresh()
	{
		try
		{
			var cacheSetting = await SettingsData.LoadSettingsByKey(SettingsKeys.AnalysisCacheHours);
			_cacheHours = int.TryParse(cacheSetting?.Value, out var hours) && hours > 0 ? hours : 12;

			// Window: first day of month 11 months ago → end of current month (12 months total).
			var thisMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			var windowStart = thisMonthStart.AddMonths(-11);
			var windowEnd = thisMonthStart.AddMonths(1).AddSeconds(-1);

			_trips = await CommonData.LoadTableDataByDate<TripOverviewModel>(FleetNames.TripOverview, windowStart, windowEnd);
			_expenses = await CommonData.LoadTableDataByDate<ExpenseOverviewModel>(FleetNames.ExpenseOverview, windowStart, windowEnd);
			_tripExpenses = await CommonData.LoadTableDataByDate<TripExpensesOverviewModel>(FleetNames.TripExpensesOverview, windowStart, windowEnd);
			_expenseDetails = await CommonData.LoadTableDataByDate<ExpenseDetailsOverviewModel>(FleetNames.ExpenseDetailsOverview, windowStart, windowEnd);

			_trips = [.. _trips.Where(_ => _.Status)];
			_expenses = [.. _expenses.Where(_ => _.Status)];
			_tripExpenses = [.. _tripExpenses.Where(_ => _.MasterStatus)];
			_expenseDetails = [.. _expenseDetails.Where(_ => _.MasterStatus)];

			BuildAll();
		}
		catch { }
		finally { StateHasChanged(); }
	}

	private void BuildAll()
	{
		BuildMonthlyTrend();
		BuildTopVehicles();
		BuildExpenseBreakdown();
	}

	private void BuildMonthlyTrend()
	{
		var thisMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

		var buckets = Enumerable.Range(0, 12)
			.Select(i => thisMonthStart.AddMonths(-11 + i))
			.ToList();

		_labels = [.. buckets.Select(b => b.ToString("MMM"))];

		var revenue = new double[12];
		var expense = new double[12];

		for (int i = 0; i < 12; i++)
		{
			var monthStart = buckets[i];
			var monthEnd = monthStart.AddMonths(1);

			revenue[i] = (double)_trips
				.Where(t => t.TransactionDateTime >= monthStart && t.TransactionDateTime < monthEnd)
				.Sum(t => t.NetAmount ?? 0);

			var tripExpense = _trips
				.Where(t => t.TransactionDateTime >= monthStart && t.TransactionDateTime < monthEnd)
				.Sum(t => t.TotalExpense);
			var standaloneExpense = _expenses
				.Where(e => e.TransactionDateTime >= monthStart && e.TransactionDateTime < monthEnd)
				.Sum(e => e.TotalExpense);
			expense[i] = (double)(tripExpense + standaloneExpense);
		}

		_revenueSeries = [new ChartSeries<double> { Name = "Revenue", Data = revenue }];
		_expenseSeries = [new ChartSeries<double> { Name = "Expense", Data = expense }];
	}

	private void BuildTopVehicles()
	{
		// Profit per vehicle = trip revenue − trip expenses − standalone expenses.
		var perVehicle = _trips
			.GroupBy(t => t.VehicleCode)
			.Select(g => new
			{
				Vehicle = g.Key,
				Profit = (double)(g.Sum(t => (t.NetAmount ?? 0) - t.TotalExpense)
					- _expenses.Where(e => e.VehicleCode == g.Key).Sum(e => e.TotalExpense)),
			})
			.OrderByDescending(x => x.Profit)
			.Take(5)
			.ToList();

		_vehicleLabels = [.. perVehicle.Select(v => v.Vehicle)];
		var profitData = perVehicle.Select(v => v.Profit).ToArray();
		_vehicleProfitSeries = perVehicle.Count == 0
			? []
			: [new ChartSeries<double> { Name = "Profit", Data = profitData }];
	}

	private void BuildExpenseBreakdown()
	{
		// Combine trip-level expense lines and standalone expense lines, grouped by ExpenseTypeName.
		var combined = _tripExpenses
			.Select(t => new { t.ExpenseTypeName, Amount = t.ExpenseAmount })
			.Concat(_expenseDetails.Select(e => new { e.ExpenseTypeName, Amount = e.ExpenseAmount }))
			.GroupBy(x => x.ExpenseTypeName ?? "Other")
			.Select(g => new { Type = g.Key, Total = (double)g.Sum(x => x.Amount) })
			.Where(x => x.Total > 0)
			.OrderByDescending(x => x.Total)
			.ToList();

		// Keep top 7 slices, bucket the rest into "Other" so the donut stays readable.
		const int topN = 7;
		var labels = new List<string>();
		var values = new List<double>();

		foreach (var item in combined.Take(topN))
		{
			labels.Add(item.Type);
			values.Add(item.Total);
		}

		if (combined.Count > topN)
		{
			labels.Add("Other");
			values.Add(combined.Skip(topN).Sum(x => x.Total));
		}

		_expenseTypeLabels = [.. labels];
		_expenseTypeSeries = values.Count == 0 ? [] : values.ToArray().AsChartDataSet();
	}
}
