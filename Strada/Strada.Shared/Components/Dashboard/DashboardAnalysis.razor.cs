using StradaLibrary.Accounts.Masters.Models;
using StradaLibrary.Fleet.Expense.Models;
using StradaLibrary.Fleet.OMC.Models;
using StradaLibrary.Fleet.Trip;
using StradaLibrary.Fleet.Trip.Models;
using StradaLibrary.Fleet.Vehicle.Models;
using StradaLibrary.Fleet.VehicleDocument.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Operations.Models;

using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Components.Dashboard;

public partial class DashboardAnalysis
{
	private int _warningDays = 30;

	private List<TripOverviewModel> _trips = [];
	private List<TripOverviewModel> _unBilledTrips = [];
	private List<ExpenseOverviewModel> _expenses = [];
	private List<VehicleModel> _vehicles = [];
	private List<VehicleDocumentRenewalOverviewModel> _dueDocuments = [];

	#region KPI
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
	#endregion

	#region Attention
	private List<TripOverviewModel> _lossTrips = [];
	private List<VehicleModel> _idleVehicles = [];
	private List<OMCCardModel> _omcCards = [];
	private List<CompanyModel> _companies = [];
	private List<OMCModel> _omcs = [];

	private SfGrid<VehicleDocumentRenewalOverviewModel> _sfGrid;
	private SfGrid<TripOverviewModel> _tripsGrid;
	private SfGrid<TripOverviewModel> _lossGrid;
	private SfGrid<VehicleModel> _idleGrid;
	private SfGrid<OMCCardModel> _cardGrid;

	private readonly List<ContextMenuItemModel> _gridContextMenuItems =
	[
		new() { Text = "Renew", Id = "Renew", IconCss = "e-icons e-refresh", Target = ".e-content" }
	];

	private readonly List<ContextMenuItemModel> _tripContextMenuItems =
	[
		new() { Text = "Bill", Id = "Bill", IconCss = "e-icons e-description", Target = ".e-content" }
	];

	private readonly List<ContextMenuItemModel> _lossContextMenuItems =
	[
		new() { Text = "View Trip", Id = "ViewTrip", IconCss = "e-icons e-eye", Target = ".e-content" }
	];

	private readonly List<ContextMenuItemModel> _idleContextMenuItems =
	[
		new() { Text = "View Vehicle", Id = "ViewVehicle", IconCss = "e-icons e-eye", Target = ".e-content" }
	];
	#endregion

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		await LoadData();
		LoadKPI();
		await LoadAttentions();
	}

	private async Task LoadData()
	{
		try
		{
			var thisMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			var thisMonthEnd = thisMonthStart.AddMonths(1).AddSeconds(-1);
			var lastMonthStart = thisMonthStart.AddMonths(-1);

			var warningSetting = await SettingsData.LoadSettingsByKey(SettingsKeys.ReportWarningDays);
			_warningDays = int.TryParse(warningSetting?.Value, out var days) ? days : 30;

			_unBilledTrips = await TripData.LoadTripOverviewByBillIdDate();
			_trips = await CommonData.LoadTableDataByDate<TripOverviewModel>(FleetNames.TripOverview, lastMonthStart, thisMonthEnd);
			_expenses = await CommonData.LoadTableDataByDate<ExpenseOverviewModel>(FleetNames.ExpenseOverview, lastMonthStart, thisMonthEnd);
			_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
			_dueDocuments = await CommonData.LoadTableData<VehicleDocumentRenewalOverviewModel>(FleetNames.VehicleDocumentRenewalOverview);

			_unBilledTrips = [.. _unBilledTrips.Where(_ => _.Status).OrderByDescending(_ => _.PendingDays)];
			_trips = [.. _trips.Where(_ => _.Status).OrderByDescending(_ => _.TransactionDateTime)];
			_expenses = [.. _expenses.Where(_ => _.Status).OrderByDescending(_ => _.TransactionDateTime)];
			_dueDocuments = [.. _dueDocuments.Where(_ => _.DaysRemaining < _warningDays).OrderBy(_ => _.DaysRemaining)];
		}
		catch { }
		finally { StateHasChanged(); }
	}

	private void LoadKPI()
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

		StateHasChanged();
	}

	private async Task LoadAttentions()
	{
		try
		{
			var currentDateTime = await CommonData.LoadCurrentDateTime();
			var warningWindowStart = currentDateTime.AddDays(-_warningDays);

			// Trips within the warning window — used for both losses and vehicle activity.
			var windowTrips = _trips.Where(_ => _.TransactionDateTime >= warningWindowStart && _.TransactionDateTime <= currentDateTime).ToList();

			// Loss-making trips within the warning window (biggest loss first).
			_lossTrips = [.. windowTrips.Where(_ => _.ProfitLoss < 0).OrderBy(_ => _.ProfitLoss)];

			// Active vehicles that ran no trip in the warning window.
			var activeVehicleIds = windowTrips.Select(_ => _.VehicleId).ToHashSet();
			var vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
			_idleVehicles = [.. vehicles.Where(_ => !activeVehicleIds.Contains(_.Id)).OrderBy(_ => _.Code)];
			_companies = await CommonData.LoadTableData<CompanyModel>(AccountNames.Company);
			_omcs = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC);

			// All active OMC cards, lowest balance first so low ones surface at the top.
			var cards = await CommonData.LoadTableDataByStatus<OMCCardModel>(FleetNames.OMCCard);
			_omcCards = [.. cards.OrderBy(_ => _.CurrentBalance)];
		}
		catch { }
		finally { StateHasChanged(); }
	}

	#region Utilities
	private void OnGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleDocumentRenewalOverviewModel> args)
	{
		if (args.Item.Id == "Renew")
			NavigationManager.NavigateTo(PageRouteNames.VehicleDocument);
	}

	private void OnTripsContextMenuItemClicked(ContextMenuClickEventArgs<TripOverviewModel> args)
	{
		if (args.Item.Id == "Bill")
			NavigationManager.NavigateTo(PageRouteNames.Bill);
	}

	private void OnLossContextMenuItemClicked(ContextMenuClickEventArgs<TripOverviewModel> args)
	{
		if (args.Item.Id == "ViewTrip")
			NavigationManager.NavigateTo(PageRouteNames.Trip);
	}

	private void OnIdleContextMenuItemClicked(ContextMenuClickEventArgs<VehicleModel> args)
	{
		if (args.Item.Id == "ViewVehicle")
			NavigationManager.NavigateTo(PageRouteNames.VehicleMaster);
	}
	#endregion
}