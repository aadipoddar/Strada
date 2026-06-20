using Strada.Data.Accounts.Masters.Data;
using Strada.Data.Common;
using Strada.Data.Operations.Data;
using Strada.Models.Accounts.Masters;
using Strada.Models.Fleet.Expense;
using Strada.Models.Fleet.OMC;
using Strada.Models.Fleet.Trip;
using Strada.Models.Fleet.Vehicle;
using Strada.Models.Operations;
using Strada.Shared.Components.Dialog;
using Strada.Shared.Components.Input;

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
	private List<VehicleRegisterExpensesModel> _activeExpenseTypes = [];

	private SfGrid<VehicleRegisterModel> _sfGrid;
	private CustomDateRangePicker _firstFocus;
	private ToastNotification _toastNotification;

	#region Load Data
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		try
		{
			_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Fleet, UserRoles.Reports]);
			await InitializePage();
		}
		catch { NavigationManager.NavigateTo(PageRouteNames.Dashboard); }
	}

	private async Task InitializePage()
	{
		await LoadData();
		await LoadTransactionOverviews();
		await StartAutoRefresh();

		_isLoading = false;
		StateHasChanged();

		if (_firstFocus is not null)
			await _firstFocus.FocusAsync();
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
			await _toastNotification.ShowAsync("Loading", "Fetching transactions...", ToastType.Info);

			_transactionOverviews = [];

			await LoadVehicles();
			await LoadExpenseTypes();
			await LoadTrips();
			await LoadExpenses();
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
			await _toastNotification.HideAllInfoAsync();
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
		var expenseTypes = await CommonData.LoadTableData<ExpenseTypeModel>(FleetNames.ExpenseType);

		foreach (var expenseType in expenseTypes)
			foreach (var vehcicle in _transactionOverviews)
			{
				vehcicle.TripExpenses.Add(new()
				{
					ExpenseTypeId = expenseType.Id,
					ExpenseType = expenseType.Name,
					ExpenseAmount = 0
				});

				vehcicle.Expenses.Add(new()
				{
					ExpenseTypeId = expenseType.Id,
					ExpenseType = expenseType.Name,
					ExpenseAmount = 0
				});
			}
	}

	private async Task LoadTrips()
	{
		var trips = await CommonData.LoadTableDataByDate<TripOverviewModel>(FleetNames.TripOverview, _fromDate, _toDate);
		trips = [.. trips.Where(_ => _.Status)];

		if (_selectedCompany?.Id > 0)
			trips = [.. trips.Where(_ => _.CompanyId == _selectedCompany.Id)];

		if (_selectedOMC?.Id > 0)
			trips = [.. trips.Where(_ => _.OMCId == _selectedOMC.Id)];

		foreach (var vehicle in _transactionOverviews)
		{
			var vehicleTrips = trips.Where(_ => _.VehicleId == vehicle.VehicleId);
			vehicle.TotalTrips = Enumerable.Count(vehicleTrips);
			vehicle.EmptyTrips = Enumerable.Count(vehicleTrips, _ => _.VehicleEmpty);
			vehicle.LoadedTrips = Enumerable.Count(vehicleTrips, _ => !_.VehicleEmpty);
			vehicle.TotalQuantity = Enumerable.Sum(vehicleTrips, _ => _.Quantity);
			vehicle.TotalTripExpenses = Enumerable.Sum(vehicleTrips, _ => _.TotalExpense);

			vehicle.TotalBills = Enumerable.Count(vehicleTrips, _ => _.BillId.HasValue);
			vehicle.TotalGrossAmount = vehicleTrips.Where(_ => _.GrossAmount.HasValue).Sum(_ => _.GrossAmount.Value);
			vehicle.TotalNetAmount = vehicleTrips.Where(_ => _.NetAmount.HasValue).Sum(_ => _.NetAmount.Value);

			vehicle.TripOverviews = [.. vehicleTrips];

			foreach (var trip in vehicleTrips)
			{
				var tripExpenses = await CommonData.LoadTableDataByMasterId<TripExpensesOverviewModel>(FleetNames.TripExpensesOverview, trip.Id);
				foreach (var expense in tripExpenses)
					vehicle.TripExpenses.FirstOrDefault(_ => _.ExpenseTypeId == expense.ExpenseTypeId).ExpenseAmount += expense.ExpenseAmount;
			}
		}
	}

	private async Task LoadExpenses()
	{
		var expenses = await CommonData.LoadTableDataByDate<ExpenseOverviewModel>(FleetNames.ExpenseOverview, _fromDate, _toDate);
		expenses = [.. expenses.Where(_ => _.Status)];

		if (_selectedCompany?.Id > 0)
			expenses = [.. expenses.Where(_ => _.CompanyId == _selectedCompany.Id)];

		foreach (var vehicle in _transactionOverviews)
		{
			var vehicleExpenses = expenses.Where(_ => _.VehicleId == vehicle.VehicleId);
			vehicle.TotalExpenses = vehicleExpenses.Sum(_ => _.TotalExpense);

			foreach (var expense in vehicleExpenses)
			{
				var expenseDetails = await CommonData.LoadTableDataByMasterId<ExpenseDetailsOverviewModel>(FleetNames.ExpenseDetailsOverview, expense.Id);

				foreach (var expenseDetail in expenseDetails)
				{
					vehicle.Expenses.FirstOrDefault(_ => _.ExpenseTypeId == expenseDetail.ExpenseTypeId).ExpenseAmount += expenseDetail.ExpenseAmount;
					vehicle.ExpenseDetailsOverviews.Add(expenseDetail);
				}
			}
		}
	}

	private void CalculateProfitLoss()
	{
		foreach (var vehicle in _transactionOverviews)
			vehicle.TotalProfitLoss = vehicle.TotalNetAmount - vehicle.TotalTripExpenses - vehicle.TotalExpenses;
	}

	private async Task RemoveUnnecessaryTransaction()
	{
		// Remove vehicles with zero activity across all fields
		_transactionOverviews = [.. _transactionOverviews.Where(_ =>
			_.TotalTrips > 0 ||
			_.TotalTripExpenses > 0 ||
			_.TotalExpenses > 0 ||
			_.TotalBills > 0 ||
			_.TotalGrossAmount > 0 ||
			_.TripExpenses.Any(te => te.ExpenseAmount > 0) ||
			_.Expenses.Any(ve => ve.ExpenseAmount > 0))];

		// Remove trip expense types that are 0 across all vehicles
		var activeTripExpenseTypeIds = _transactionOverviews
			.SelectMany(_ => _.TripExpenses)
			.Where(te => te.ExpenseAmount > 0)
			.Select(te => te.ExpenseTypeId)
			.ToHashSet();

		foreach (var vehicle in _transactionOverviews)
			vehicle.TripExpenses = [.. vehicle.TripExpenses.Where(te => activeTripExpenseTypeIds.Contains(te.ExpenseTypeId))];

		// Remove expense types that are 0 across all vehicles
		var activeExpenseTypeIds = _transactionOverviews
			.SelectMany(_ => _.Expenses)
			.Where(ve => ve.ExpenseAmount > 0)
			.Select(ve => ve.ExpenseTypeId)
			.ToHashSet();

		foreach (var vehicle in _transactionOverviews)
			vehicle.Expenses = [.. vehicle.Expenses.Where(ve => activeExpenseTypeIds.Contains(ve.ExpenseTypeId))];

		// Capture expense type metadata for dynamic column rendering (same set across all vehicles after pruning)
		_activeTripExpenseTypes = _transactionOverviews.FirstOrDefault()?.TripExpenses ?? [];
		_activeExpenseTypes = _transactionOverviews.FirstOrDefault()?.Expenses ?? [];
	}
	#endregion

	#region Changed Events
	private async Task OnDateRangeChanged(MudBlazor.DateRange range)
	{
		_fromDate = range?.Start ?? _fromDate;
		_toDate = range?.End ?? _toDate;
		await LoadTransactionOverviews();
	}

	private async Task OnCompanyChanged(CompanyModel value)
	{
		_selectedCompany = value;
		await LoadTransactionOverviews();
	}

	private async Task OnOMCChanged(OMCModel value)
	{
		_selectedOMC = value;
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
		var props = new ExcelExportProperties
		{
			FileName = $"VehicleRegister_{_fromDate:dd-MMM-yyyy}_to_{_toDate:dd-MMM-yyyy}.xlsx",
			IncludeTemplateColumn = true,
			ExcelDetailRowMode = _showAllColumns ? ExcelDetailRowMode.Expand : ExcelDetailRowMode.None
		};
		await _sfGrid.ExportToExcelAsync(props);
	}

	public void OnExcelDetailTemplateExporting(ExcelDetailTemplateEventArgs<VehicleRegisterModel> args)
	{
		var vehicle = args.ParentRow.Data;
		var rows = new List<ExcelDetailTemplateRow>();

		if (vehicle.TripOverviews.Count > 0)
		{
			// Section heading
			rows.Add(ExcelRow(["TRIPS", .. Enumerable.Repeat("", 13)], bold: true, backColor: "#BDD7EE"));

			// Column headers
			rows.Add(ExcelRow(["Date", "Sl No", "Challan", "OMC", "Route", "Driver", "Qty", "Est Dist", "Est Hours", "Est Fuel", "Est Cost", "Trip Exp", "Empty", "Bill No", "Bill Date", "Gross Amt", "Penalty Amt", "Net Amt", "Profit/Loss", "Pending Days", "Remarks"], bold: true, backColor: "#DEEAF1"));

			foreach (var trip in vehicle.TripOverviews)
			{
				rows.Add(ExcelRow([
					trip.TransactionDateTime.ToString("dd/MM/yy"),
					trip.SlNo?? "",
					trip.ChallanNo ?? "",
					trip.OMCName ?? "",
					trip.RouteDisplay ?? "",
					trip.DriverDisplay ?? "",
					trip.Quantity.ToString("N2"),
					trip.EstimatedDistance.ToString("N2"),
					trip.EstimatedHours.ToString("N2"),
					trip.EstimatedFuelConsumption.ToString("N2"),
					trip.EstimatedCost.ToString("N2"),
					trip.TotalExpense.ToString("N2"),
					trip.VehicleEmpty ? "Empty" : "Loaded",
					trip.BillNo ?? "",
					trip.BillDateTime?.ToString("dd/MM/yy") ?? "",
					trip.GrossAmount?.ToString("N2") ?? "",
					trip.PenaltyAmount?.ToString("N2") ?? "",
					trip.NetAmount?.ToString("N2") ?? "",
					trip.ProfitLoss?.ToString("N2") ?? "",
					trip.PendingDays.HasValue ? trip.PendingDays.Value.ToString() : "",
					trip.Remarks ?? ""
				]));
			}

			// Blank separator
			rows.Add(ExcelRow([""]));
		}

		if (vehicle.ExpenseDetailsOverviews.Count > 0)
		{
			// Section heading
			rows.Add(ExcelRow(["EXPENSES", .. Enumerable.Repeat("", 3)], bold: true, backColor: "#BDD7EE"));

			// Column headers
			rows.Add(ExcelRow(["Date", "Expense Type", "Ledger", "Amount", "Identification No", "Expense Remarks", "Total Expense", "Remarks"], bold: true, backColor: "#DEEAF1"));

			foreach (var expense in vehicle.ExpenseDetailsOverviews)
			{
				rows.Add(ExcelRow([
					expense.TransactionDateTime.ToString("dd/MM/yy"),
					expense.ExpenseTypeName ?? "",
					expense.LedgerName ?? "",
					expense.ExpenseAmount.ToString("N2"),
					expense.IdentificationNo ?? "",
					expense.ExpenseRemarks ?? "",
					expense.TotalExpense.ToString("N2"),
					expense.Remarks ?? ""
				]));
			}
		}

		args.RowInfo.Rows = rows;
	}

	private static ExcelDetailTemplateRow ExcelRow(List<string> values, bool bold = false, string backColor = null)
	{
		var cells = new List<ExcelDetailTemplateCell>();
		for (var i = 0; i < values.Count; i++)
		{
			var cell = new ExcelDetailTemplateCell { Index = i, CellValue = values[i] };
			if (bold || backColor is not null)
				cell.Style = new ExcelStyle { Bold = bold, BackColor = backColor };
			cells.Add(cell);
		}
		return new ExcelDetailTemplateRow { Cells = cells };
	}

	private async Task ExportPdf()
	{
	}
	#endregion

	#region Utilities
	private async Task ToggleDetailsView()
	{
		_showAllColumns = !_showAllColumns;
		StateHasChanged();

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}

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
		catch (OperationCanceledException) { }
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
