using Microsoft.Extensions.Caching.Memory;

using StradaLibrary.Fleet.Expense.Models;
using StradaLibrary.Fleet.Trip;
using StradaLibrary.Fleet.Trip.Models;
using StradaLibrary.Fleet.Vehicle.Models;
using StradaLibrary.Fleet.VehicleDocument.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Operations.Models;

namespace Strada.Shared.Components.Dashboard;

public partial class DashboardAnalysis
{
	private List<TripOverviewModel> _trips = [];
	private List<TripOverviewModel> _unBilledTrips = [];
	private List<ExpenseOverviewModel> _expenses = [];
	private List<VehicleModel> _vehicles = [];
	private List<VehicleDocumentRenewalOverviewModel> _dueDocuments = [];

	private int _warningDays = 30;
	private int _cacheHours = 12;

	private int _unbilledTripsCount = 0;

	private int _thisMonthTripsCount = 0;
	private string _thisMonthTripsTrend = "vs last month";

	private string _thisMonthRevenue = "₹0.00";
	private string _thisMonthRevenueTrend = "vs last month";

	private string _thisMonthExpense = "₹0.00";
	private string _thisMonthExpenseTrend = "vs last month";

	private string _thisMonthProfit = "₹0.00";
	private string _thisMonthProfitTrend = "vs last month";

	private int _activeVehiclesCount = 0;
	private string _activeVehiclesNote = "ran a trip this month";

	private int _dueDocumentsCount = 0;
	private string _dueDocumentsNote = "within window or expired";

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		await LoadData();
	}

	private async Task LoadData()
	{
		if (LoadCached())
			return;

		await LoadFresh();

		var expiry = TimeSpan.FromHours(_cacheHours);
		MemoryCache.Set(StorageFileNames.UnBilledTripsDataFileName, _unBilledTrips, expiry);
		MemoryCache.Set(StorageFileNames.TripsOverviewDataFileName, _trips, expiry);
		MemoryCache.Set(StorageFileNames.ExpensesOverviewDataFileName, _expenses, expiry);
		MemoryCache.Set(StorageFileNames.VehiclesDataFileName, _vehicles, expiry);
		MemoryCache.Set(StorageFileNames.DueDocumentsDataFileName, _dueDocuments, expiry);
	}

	private bool LoadCached()
	{
		if (!MemoryCache.TryGetValue(StorageFileNames.TripsOverviewDataFileName, out List<TripOverviewModel> trips))
			return false;

		_trips = trips ?? [];
		_unBilledTrips = MemoryCache.Get<List<TripOverviewModel>>(StorageFileNames.UnBilledTripsDataFileName) ?? [];
		_expenses = MemoryCache.Get<List<ExpenseOverviewModel>>(StorageFileNames.ExpensesOverviewDataFileName) ?? [];
		_vehicles = MemoryCache.Get<List<VehicleModel>>(StorageFileNames.VehiclesDataFileName) ?? [];
		_dueDocuments = MemoryCache.Get<List<VehicleDocumentRenewalOverviewModel>>(StorageFileNames.DueDocumentsDataFileName) ?? [];

		ComputeKpis();
		StateHasChanged();
		return true;
	}

	private async Task LoadFresh()
	{
		try
		{
			var thisMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			var thisMonthEnd = thisMonthStart.AddMonths(1).AddSeconds(-1);
			var lastMonthStart = thisMonthStart.AddMonths(-1);

			var warningSetting = await SettingsData.LoadSettingsByKey(SettingsKeys.ReportWarningDays);
			_warningDays = int.TryParse(warningSetting?.Value, out var days) ? days : 30;

			var cacheSetting = await SettingsData.LoadSettingsByKey(SettingsKeys.AnalysisCacheHours);
			_cacheHours = int.TryParse(cacheSetting?.Value, out var hours) && hours > 0 ? hours : 12;

			_unBilledTrips = await TripData.LoadTripOverviewByBillIdDate();
			_trips = await CommonData.LoadTableDataByDate<TripOverviewModel>(FleetNames.TripOverview, lastMonthStart, thisMonthEnd);
			_expenses = await CommonData.LoadTableDataByDate<ExpenseOverviewModel>(FleetNames.ExpenseOverview, lastMonthStart, thisMonthEnd);
			_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
			_dueDocuments = await CommonData.LoadTableData<VehicleDocumentRenewalOverviewModel>(FleetNames.VehicleDocumentRenewalOverview);

			_unBilledTrips = [.. _unBilledTrips.Where(_ => _.Status).OrderByDescending(_ => _.PendingDays)];
			_trips = [.. _trips.Where(_ => _.Status).OrderByDescending(_ => _.TransactionDateTime)];
			_expenses = [.. _expenses.Where(_ => _.Status).OrderByDescending(_ => _.TransactionDateTime)];
			_dueDocuments = [.. _dueDocuments.Where(_ => _.DaysRemaining < _warningDays).OrderBy(_ => _.DaysRemaining)];

			ComputeKpis();
		}
		catch { }
		finally { StateHasChanged(); }
	}

	private void ComputeKpis()
	{
		var thisMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
		var lastMonthStart = thisMonthStart.AddMonths(-1);

		_unbilledTripsCount = _unBilledTrips.Count;

		var lastMonthTripsCount = _trips.Count(_ => _.TransactionDateTime >= lastMonthStart && _.TransactionDateTime < thisMonthStart);
		_thisMonthTripsCount = _trips.Count(_ => _.TransactionDateTime >= thisMonthStart);
		_thisMonthTripsTrend = Helper.FormatMonthlyTrend(_thisMonthTripsCount, lastMonthTripsCount);

		// Revenue = NetAmount of all Trips
		var thisMonthRevenue = _trips.Where(_ => _.TransactionDateTime >= thisMonthStart).Sum(_ => _.NetAmount ?? 0);
		var lastMonthRevenue = _trips.Where(_ => _.TransactionDateTime >= lastMonthStart && _.TransactionDateTime < thisMonthStart).Sum(_ => _.NetAmount ?? 0);
		_thisMonthRevenue = thisMonthRevenue.FormatIndianCurrency();
		_thisMonthRevenueTrend = Helper.FormatMonthlyTrend(thisMonthRevenue, lastMonthRevenue);

		// Total expense = trip expenses + standalone expenses from the Expense module.
		var thisMonthExpense =
			_trips.Where(_ => _.TransactionDateTime >= thisMonthStart).Sum(_ => _.TotalExpense)
			+ _expenses.Where(_ => _.TransactionDateTime >= thisMonthStart).Sum(_ => _.TotalExpense);
		var lastMonthExpense =
			_trips.Where(_ => _.TransactionDateTime >= lastMonthStart && _.TransactionDateTime < thisMonthStart).Sum(_ => _.TotalExpense)
			+ _expenses.Where(_ => _.TransactionDateTime >= lastMonthStart && _.TransactionDateTime < thisMonthStart).Sum(_ => _.TotalExpense);
		_thisMonthExpense = thisMonthExpense.FormatIndianCurrency();
		_thisMonthExpenseTrend = Helper.FormatMonthlyTrend(thisMonthExpense, lastMonthExpense);

		// Profit = revenue earned minus total expense, for each month.
		var thisMonthProfit = thisMonthRevenue - thisMonthExpense;
		var lastMonthProfit = lastMonthRevenue - lastMonthExpense;
		_thisMonthProfit = thisMonthProfit.FormatIndianCurrency();
		_thisMonthProfitTrend = Helper.FormatMonthlyTrend(thisMonthProfit, lastMonthProfit);

		// Active vehicles in the fleet, and how many actually ran a trip this month.
		_activeVehiclesCount = _vehicles.Count;
		var ranThisMonth = _trips.Where(_ => _.TransactionDateTime >= thisMonthStart).Select(_ => _.VehicleId).Distinct().Count();
		_activeVehiclesNote = $"{ranThisMonth} ran a trip this month";

		// Documents due within the warning window (already filtered).
		_dueDocumentsCount = _dueDocuments.Count;
		_dueDocumentsNote = $"within {_warningDays} days or expired";
	}
}
