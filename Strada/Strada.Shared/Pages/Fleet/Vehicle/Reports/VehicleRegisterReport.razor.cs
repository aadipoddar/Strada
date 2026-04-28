using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Operations;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleExpense;
using StradaLibrary.Models.Fleet.VehicleTrip;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.Vehicle.Reports;

public partial class VehicleRegisterReport : IAsyncDisposable
{
	private PeriodicTimer _autoRefreshTimer;
	private CancellationTokenSource _autoRefreshCts;

	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showAllColumns = false;

	private DateTime _fromDate = DateTime.Now.Date;
	private DateTime _toDate = DateTime.Now.Date;

	private CompanyModel? _selectedCompany = null;
	private OMCModel? _selectedOMC = null;

	private List<CompanyModel> _companies = [];
	private List<OMCModel> _omcs = [];
	private List<VehicleRegisterModel> _transactionOverviews = [];
	private List<VehicleRegisterExpensesModel> _activeTripExpenseTypes = [];
	private List<VehicleRegisterExpensesModel> _activeVehicleExpenseTypes = [];

	private SfGrid<VehicleRegisterModel> _sfGrid;
	private ToastNotification _toastNotification;

	#region Load Data
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Fleet, UserRoles.Reports]);
		await InitializePage();
	}

	private async Task InitializePage()
	{
		await LoadData();
		await LoadTransactionOverviews();
		await StartAutoRefresh();

		_isLoading = false;
		StateHasChanged();
	}

	private async Task LoadData()
	{
		_fromDate = await CommonData.LoadCurrentDateTime();
		_toDate = _fromDate;

		_companies = await CommonData.LoadTableDataByStatus<CompanyModel>(AccountNames.Company);
		_omcs = await CommonData.LoadTableDataByStatus<OMCModel>(FleetNames.OMC);

		_companies = [.. _companies.OrderBy(s => s.Name)];
		_omcs = [.. _omcs.OrderBy(s => s.Name)];
	}

	private async Task LoadTransactionOverviews()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			// await _toastNotification.ShowAsync("Loading", "Fetching transactions...", ToastType.Info);

			_transactionOverviews = [];

			await LoadVehicles();
			await LoadExpenseTypes();
			await LoadVehicleTrips();
			await LoadVehicleExpenses();
			CalculateProfitLoss();
			await RemoveUnnecessaryTransaction();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to load transactions: {ex.Message}", ToastType.Error);
		}
		finally
		{
			if (_sfGrid is not null)
				await _sfGrid.Refresh();

			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task LoadVehicles()
	{
		var vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);

		if (_selectedCompany?.Id > 0)
			vehicles = [.. vehicles.Where(_ => _.CompanyId == _selectedCompany.Id)];

		foreach (var vehicle in vehicles)
			_transactionOverviews.Add(new VehicleRegisterModel
			{
				VehicleId = vehicle.Id,
				VehicleCode = vehicle.Code,
			});
	}

	private async Task LoadExpenseTypes()
	{
		var expenseTypes = await CommonData.LoadTableData<VehicleExpenseTypeModel>(FleetNames.VehicleExpenseType);

		foreach (var expenseType in expenseTypes)
			foreach (var vehcicle in _transactionOverviews)
			{
				vehcicle.TripExpenses.Add(new()
				{
					ExpenseTypeId = expenseType.Id,
					ExpenseType = expenseType.Name,
					ExpenseAmount = 0
				});

				vehcicle.VehicleExpenses.Add(new()
				{
					ExpenseTypeId = expenseType.Id,
					ExpenseType = expenseType.Name,
					ExpenseAmount = 0
				});
			}
	}

	private async Task LoadVehicleTrips()
	{
		var trips = await CommonData.LoadTableDataByDate<VehicleTripOverviewModel>(FleetNames.VehicleTripOverview, _fromDate, _toDate);
		trips = [.. trips.Where(_ => _.Status)];

		if (_selectedCompany?.Id > 0)
			trips = [.. trips.Where(_ => _.CompanyId == _selectedCompany.Id)];

		if (_selectedOMC?.Id > 0)
			trips = [.. trips.Where(_ => _.OMCId == _selectedOMC.Id)];

		foreach (var vehicle in _transactionOverviews)
		{
			var vehicleTrips = trips.Where(_ => _.VehicleId == vehicle.VehicleId);
			vehicle.TotalTrips = vehicleTrips.Count();
			vehicle.EmptyTrips = vehicleTrips.Count(_ => _.VehicleEmpty);
			vehicle.LoadedTrips = vehicleTrips.Count(_ => !_.VehicleEmpty);
			vehicle.TotalQuantity = vehicleTrips.Sum(_ => _.Quantity);
			vehicle.TotalTripExpenses = vehicleTrips.Sum(_ => _.TotalExpense);

			vehicle.TotalBills = vehicleTrips.Count(_ => _.BillId.HasValue);
			vehicle.TotalGrossAmount = vehicleTrips.Where(_ => _.GrossAmount.HasValue).Sum(_ => _.GrossAmount.Value);
			vehicle.TotalTDSAmount = vehicleTrips.Where(_ => _.TDSAmount.HasValue).Sum(_ => _.TDSAmount.Value);
			vehicle.TotalNetAmount = vehicleTrips.Where(_ => _.NetAmount.HasValue).Sum(_ => _.NetAmount.Value);

			foreach (var trip in vehicleTrips)
			{
				var tripExpenses = await CommonData.LoadTableDataByMasterId<VehicleTripExpensesOverviewModel>(FleetNames.VehicleTripExpensesOverview, trip.Id);
				foreach (var expense in tripExpenses)
					vehicle.TripExpenses.FirstOrDefault(_ => _.ExpenseTypeId == expense.VehicleExpenseTypeId).ExpenseAmount += expense.ExpenseAmount;
			}
		}
	}

	private async Task LoadVehicleExpenses()
	{
		var expenses = await CommonData.LoadTableDataByDate<VehicleExpenseOverviewModel>(FleetNames.VehicleExpenseOverview, _fromDate, _toDate);
		expenses = [.. expenses.Where(_ => _.Status)];

		if (_selectedCompany?.Id > 0)
			expenses = [.. expenses.Where(_ => _.CompanyId == _selectedCompany.Id)];

		foreach (var vehicle in _transactionOverviews)
		{
			var vehicleExpenses = expenses.Where(_ => _.VehicleId == vehicle.VehicleId);
			vehicle.TotalVehicleExpenses = vehicleExpenses.Sum(_ => _.TotalExpense);

			foreach (var expense in vehicleExpenses)
			{
				var expenseDetails = await CommonData.LoadTableDataByMasterId<VehicleExpenseDetailsOverviewModel>(FleetNames.VehicleExpenseDetailsOverview, expense.Id);
				foreach (var expenseDetail in expenseDetails)
					vehicle.VehicleExpenses.FirstOrDefault(_ => _.ExpenseTypeId == expenseDetail.VehicleExpenseTypeId).ExpenseAmount += expenseDetail.ExpenseAmount;
			}
		}
	}

	private void CalculateProfitLoss()
	{
		foreach (var vehicle in _transactionOverviews)
			vehicle.TotalProfitLoss = vehicle.TotalNetAmount - vehicle.TotalTripExpenses - vehicle.TotalVehicleExpenses;
	}

	private async Task RemoveUnnecessaryTransaction()
	{
		// Remove vehicles with zero activity across all fields
		_transactionOverviews = [.. _transactionOverviews.Where(_ =>
			_.TotalTrips > 0 ||
			_.TotalTripExpenses > 0 ||
			_.TotalVehicleExpenses > 0 ||
			_.TotalBills > 0 ||
			_.TotalGrossAmount > 0 ||
			_.TripExpenses.Any(te => te.ExpenseAmount > 0) ||
			_.VehicleExpenses.Any(ve => ve.ExpenseAmount > 0))];

		// Remove trip expense types that are 0 across all vehicles
		var activeTripExpenseTypeIds = _transactionOverviews
			.SelectMany(_ => _.TripExpenses)
			.Where(te => te.ExpenseAmount > 0)
			.Select(te => te.ExpenseTypeId)
			.ToHashSet();

		foreach (var vehicle in _transactionOverviews)
			vehicle.TripExpenses = [.. vehicle.TripExpenses.Where(te => activeTripExpenseTypeIds.Contains(te.ExpenseTypeId))];

		// Remove vehicle expense types that are 0 across all vehicles
		var activeVehicleExpenseTypeIds = _transactionOverviews
			.SelectMany(_ => _.VehicleExpenses)
			.Where(ve => ve.ExpenseAmount > 0)
			.Select(ve => ve.ExpenseTypeId)
			.ToHashSet();

		foreach (var vehicle in _transactionOverviews)
			vehicle.VehicleExpenses = [.. vehicle.VehicleExpenses.Where(ve => activeVehicleExpenseTypeIds.Contains(ve.ExpenseTypeId))];

		// Capture expense type metadata for dynamic column rendering (same set across all vehicles after pruning)
		_activeTripExpenseTypes = _transactionOverviews.FirstOrDefault()?.TripExpenses ?? [];
		_activeVehicleExpenseTypes = _transactionOverviews.FirstOrDefault()?.VehicleExpenses ?? [];
	}
	#endregion

	#region Changed Events
	private async Task OnDateRangeChanged(Syncfusion.Blazor.Calendars.RangePickerEventArgs<DateTime> args)
	{
		_fromDate = args.StartDate;
		_toDate = args.EndDate;
		await LoadTransactionOverviews();
	}

	private async Task OnCompanyChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<CompanyModel, CompanyModel> args)
	{
		_selectedCompany = args.Value;
		await LoadTransactionOverviews();
	}

	private async Task OnOMCChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<OMCModel, OMCModel> args)
	{
		_selectedOMC = args.Value;
		await LoadTransactionOverviews();
	}

	private async Task HandleDatesChanged(DateRangeType dateRangeType)
	{
		(_fromDate, _toDate) = await FinancialYearData.GetDateRange(dateRangeType, _fromDate, _toDate);
		await LoadTransactionOverviews();
	}
	#endregion

	#region Exports
	private async Task ExportExcel()
	{

	}

	private async Task ExportPdf()
	{

	}
	#endregion

	#region Utilities
	private async Task OnMenuSelected(Syncfusion.Blazor.Navigations.MenuEventArgs<Syncfusion.Blazor.Navigations.MenuItem> args)
	{
		switch (args.Item.Id)
		{
			case "VehicleTripReport":
				NavigationManager.NavigateTo(PageRouteNames.VehicleTripReport);
				break;
			case "VehicleExpenseReport":
				NavigationManager.NavigateTo(PageRouteNames.VehicleExpenseReport);
				break;
			case "VehicleTripBillReport":
				NavigationManager.NavigateTo(PageRouteNames.VehicleTripBillReport);
				break;
			case "Refresh":
				await LoadTransactionOverviews();
				break;
			case "ToggleDetailsView":
				await ToggleDetailsView();
				break;
			case "ExportPdf":
				await ExportPdf();
				break;
			case "ExportExcel":
				await ExportExcel();
				break;
			case "PeriodToday":
				await HandleDatesChanged(DateRangeType.Today);
				break;
			case "PeriodPreviousDay":
				await HandleDatesChanged(DateRangeType.Yesterday);
				break;
			case "PeriodNextDay":
				await HandleDatesChanged(DateRangeType.NextDay);
				break;
			case "PeriodCurrentMonth":
				await HandleDatesChanged(DateRangeType.CurrentMonth);
				break;
			case "PeriodPreviousMonth":
				await HandleDatesChanged(DateRangeType.PreviousMonth);
				break;
			case "PeriodNextMonth":
				await HandleDatesChanged(DateRangeType.NextMonth);
				break;
			case "PeriodCurrentFinancialYear":
				await HandleDatesChanged(DateRangeType.CurrentFinancialYear);
				break;
			case "PeriodPreviousFinancialYear":
				await HandleDatesChanged(DateRangeType.PreviousFinancialYear);
				break;
			case "PeriodNextFinancialYear":
				await HandleDatesChanged(DateRangeType.NextFinancialYear);
				break;
			case "PeriodAllTime":
				await HandleDatesChanged(DateRangeType.AllTime);
				break;
		}
	}

	private async Task ToggleDetailsView()
	{
		_showAllColumns = !_showAllColumns;
		StateHasChanged();

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

	private async Task StartAutoRefresh()
	{
		var timerSetting = await SettingsData.LoadSettingsByKey(SettingsKeys.AutoRefreshReportTimer);
		var refreshMinutes = int.TryParse(timerSetting?.Value, out var minutes) ? minutes : 5;

		_autoRefreshCts = new CancellationTokenSource();
		_autoRefreshTimer = new PeriodicTimer(TimeSpan.FromMinutes(refreshMinutes));
		_ = AutoRefreshLoop(_autoRefreshCts.Token);
	}

	private async Task AutoRefreshLoop(CancellationToken cancellationToken)
	{
		try
		{
			while (await _autoRefreshTimer.WaitForNextTickAsync(cancellationToken))
				await LoadTransactionOverviews();
		}
		catch (OperationCanceledException)
		{
			// Timer was cancelled, expected on dispose
		}
	}

	async ValueTask IAsyncDisposable.DisposeAsync()
	{
		if (_autoRefreshCts is not null)
		{
			await _autoRefreshCts.CancelAsync();
			_autoRefreshCts.Dispose();
		}

		_autoRefreshTimer?.Dispose();
		GC.SuppressFinalize(this);
	}
	#endregion
}