using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Fleet.Route;
using StradaLibrary.Data.Fleet.Trip;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.Trip;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.Route;
using StradaLibrary.Models.Fleet.Trip;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.Trip.Reports;

public partial class TripCardPaymentsReport : IAsyncDisposable
{
	private PeriodicTimer _autoRefreshTimer;
	private CancellationTokenSource _autoRefreshCts;

	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showAllColumns = false;
	private bool _showDeleted = false;

	private DateTime _fromDate = DateTime.Now.Date;
	private DateTime _toDate = DateTime.Now.Date;

	private CompanyModel? _selectedCompany = null;
	private OMCModel? _selectedOMC = null;
	private VehicleModel? _selectedVehicle = null;
	private RouteOverviewModel? _selectedRoute = null;
	private DriverOverviewModel? _selectedDriver = null;
	private int _vehicleEmptyFilter = TripFilterOptions.All;
	private int _pendingBillsFilter = TripFilterOptions.All;

	private List<CompanyModel> _companies = [];
	private List<OMCModel> _omcs = [];
	private List<VehicleModel> _vehicles = [];
	private List<RouteOverviewModel> _routes = [];
	private List<DriverOverviewModel> _drivers = [];
	private List<TripCardPaymentsOverviewModel> _transactionOverviews = [];

	private string _deleteTransactionNo = string.Empty;
	private int _deleteTransactionId = 0;
	private string _recoverTransactionNo = string.Empty;
	private int _recoverTransactionId = 0;

	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private SfGrid<TripCardPaymentsOverviewModel> _sfGrid;
	private ToastNotification _toastNotification;
	private readonly List<ContextMenuItemModel> _gridContextMenuItems =
	[
		new() { Text = "View (Alt + O)", Id = "View", IconCss = "e-icons e-eye", Target = ".e-content" },
		new() { Text = "Export PDF (Alt + P)", Id = "ExportPDF", IconCss = "e-icons e-export-pdf", Target = ".e-content" },
		new() { Text = "Export Excel (Alt + E)", Id = "ExportExcel", IconCss = "e-icons e-export-excel", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecover", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

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
		_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
		_routes = await RouteData.LoadRouteOverview();
		_drivers = await DriverData.LoadDriverOverview();

		_companies = [.. _companies.OrderBy(s => s.Name)];
		_omcs = [.. _omcs.OrderBy(s => s.Name)];
		_vehicles = [.. _vehicles.OrderBy(s => s.ShortCode)];
		_routes = [.. _routes.OrderBy(s => s.Code)];
		_drivers = [.. _drivers.OrderBy(s => s.Name)];
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

			_transactionOverviews = await CommonData.LoadTableDataByDate<TripCardPaymentsOverviewModel>(
				FleetNames.TripCardPaymentsOverview,
				DateOnly.FromDateTime(_fromDate).ToDateTime(TimeOnly.MinValue),
				DateOnly.FromDateTime(_toDate).ToDateTime(TimeOnly.MinValue));

			if (!_showDeleted)
				_transactionOverviews = [.. _transactionOverviews.Where(_ => _.Status)];

			if (_selectedCompany?.Id > 0)
				_transactionOverviews = [.. _transactionOverviews.Where(_ => _.CompanyId == _selectedCompany.Id)];

			if (_selectedOMC?.Id > 0)
				_transactionOverviews = [.. _transactionOverviews.Where(_ => _.OMCId == _selectedOMC.Id)];

			if (_selectedVehicle?.Id > 0)
				_transactionOverviews = [.. _transactionOverviews.Where(_ => _.VehicleId == _selectedVehicle.Id)];

			if (_selectedRoute?.Id > 0)
				_transactionOverviews = [.. _transactionOverviews.Where(_ => _.RouteId == _selectedRoute.Id)];

			if (_selectedDriver?.Id > 0)
				_transactionOverviews = [.. _transactionOverviews.Where(_ => _.DriverId == _selectedDriver.Id)];

			if (_vehicleEmptyFilter == TripFilterOptions.Yes)
				_transactionOverviews = [.. _transactionOverviews.Where(_ => _.VehicleEmpty)];
			else if (_vehicleEmptyFilter == TripFilterOptions.No)
				_transactionOverviews = [.. _transactionOverviews.Where(_ => !_.VehicleEmpty)];

			if (_pendingBillsFilter == TripFilterOptions.Yes)
				_transactionOverviews = [.. _transactionOverviews.Where(_ => _.BillId is null)];
			else if (_pendingBillsFilter == TripFilterOptions.No)
				_transactionOverviews = [.. _transactionOverviews.Where(_ => _.BillId is not null)];

			_transactionOverviews = [.. _transactionOverviews.OrderBy(_ => _.TransactionDateTime)];
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

		_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
		_vehicles = [.. _vehicles.OrderBy(v => v.Code)];
		_selectedVehicle = null;

		if (_selectedCompany?.Id > 0)
			_vehicles = [.. _vehicles.Where(s => s.CompanyId == _selectedCompany.Id)];

		await LoadTransactionOverviews();
	}

	private async Task OnOMCChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<OMCModel, OMCModel> args)
	{
		_selectedOMC = args.Value;
		await LoadTransactionOverviews();
	}

	private async Task OnVehicleChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<VehicleModel, VehicleModel> args)
	{
		_selectedVehicle = args.Value;
		await LoadTransactionOverviews();
	}

	private async Task OnRouteChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<RouteOverviewModel, RouteOverviewModel> args)
	{
		_selectedRoute = args.Value;
		await LoadTransactionOverviews();
	}

	private async Task OnDriverChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<DriverOverviewModel, DriverOverviewModel> args)
	{
		_selectedDriver = args.Value;
		await LoadTransactionOverviews();
	}

	private async Task OnVehicleEmptyFilterChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, YesNoFilterOption> args)
	{
		_vehicleEmptyFilter = args.Value;
		await LoadTransactionOverviews();
	}

	private async Task OnPendingBillsFilterChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, YesNoFilterOption> args)
	{
		_pendingBillsFilter = args.Value;
		await LoadTransactionOverviews();
	}

	private async Task HandleDatesChanged(DateRangeType dateRangeType)
	{
		(_fromDate, _toDate) = await FinancialYearData.GetDateRange(dateRangeType, _fromDate, _toDate);
		await LoadTransactionOverviews();
	}
	#endregion

	#region Exporting
	private async Task ExportExcel()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var (stream, fileName) = await TripReportExport.ExportCardPaymentsReport(
				_transactionOverviews,
				ReportExportType.Excel,
				DateOnly.FromDateTime(_fromDate),
				DateOnly.FromDateTime(_toDate),
				_showAllColumns,
				_showDeleted,
				_selectedCompany?.Id > 0 ? _selectedCompany : null,
				_selectedOMC?.Id > 0 ? _selectedOMC : null,
				_selectedVehicle?.Id > 0 ? _selectedVehicle : null,
				_selectedRoute?.Id > 0 ? _selectedRoute : null,
				_selectedDriver?.Id > 0 ? _selectedDriver : null
			);
			await SaveAndViewService.SaveAndView(fileName, stream);

			await _toastNotification.ShowAsync("Exported", "The export has been downloaded successfully.", ToastType.Success);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Exporting", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task ExportPdf()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var (stream, fileName) = await TripReportExport.ExportCardPaymentsReport(
				_transactionOverviews,
				ReportExportType.PDF,
				DateOnly.FromDateTime(_fromDate),
				DateOnly.FromDateTime(_toDate),
				_showAllColumns,
				_showDeleted,
				_selectedCompany?.Id > 0 ? _selectedCompany : null,
				_selectedOMC?.Id > 0 ? _selectedOMC : null,
				_selectedVehicle?.Id > 0 ? _selectedVehicle : null,
				_selectedRoute?.Id > 0 ? _selectedRoute : null,
				_selectedDriver?.Id > 0 ? _selectedDriver : null
			 );
			await SaveAndViewService.SaveAndView(fileName, stream);

			await _toastNotification.ShowAsync("Exported", "The export has been downloaded successfully.", ToastType.Success);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Exporting", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task ExportSelectedTransactionPdf()
	{
		if (_isProcessing || _sfGrid is null || _sfGrid.SelectedRecords is null || _sfGrid.SelectedRecords.Count == 0)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var decodeTransactionNo = await DecodeCode.DecodeTransactionNo(_sfGrid.SelectedRecords.First().TransactionNo, true, false, CodeType.Trip);
			await SaveAndViewService.SaveAndView(decodeTransactionNo.PDFStream.fileName, decodeTransactionNo.PDFStream.stream);

			await _toastNotification.ShowAsync("Exported", "The export has been downloaded successfully.", ToastType.Success);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Exporting", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task ExportSelectedTransactionExcel()
	{
		if (_isProcessing || _sfGrid is null || _sfGrid.SelectedRecords is null || _sfGrid.SelectedRecords.Count == 0)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var decodeTransactionNo = await DecodeCode.DecodeTransactionNo(_sfGrid.SelectedRecords.First().TransactionNo, false, true, CodeType.Trip);
			await SaveAndViewService.SaveAndView(decodeTransactionNo.ExcelStream.fileName, decodeTransactionNo.ExcelStream.stream);

			await _toastNotification.ShowAsync("Exported", "The export has been downloaded successfully.", ToastType.Success);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Exporting", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}
	#endregion

	#region Actions
	private async Task ViewSelectedTransaction()
	{
		if (_isProcessing || _sfGrid is null || _sfGrid.SelectedRecords is null || _sfGrid.SelectedRecords.Count == 0)
			return;

		if (!_sfGrid.SelectedRecords.First().Status)
		{
			await _toastNotification.ShowAsync("Cannot View", "The selected transaction is deleted. Please recover it or download invoice.", ToastType.Warning);
			return;
		}

		var decodedTransactionNo = await DecodeCode.DecodeTransactionNo(_sfGrid.SelectedRecords.First().TransactionNo, false, false, CodeType.Trip);
		await AuthenticationService.NavigateToRoute(decodedTransactionNo.PageRouteName, FormFactor, JSRuntime, NavigationManager);
	}

	private async Task ConfirmDelete()
	{
		if (_isProcessing || _deleteTransactionId == 0)
			return;

		try
		{
			if (!_user.Admin)
				throw new UnauthorizedAccessException("You do not have permission to delete this transaction.");

			await _deleteConfirmationDialog.HideAsync();
			_isProcessing = true;
			StateHasChanged();

			await _toastNotification.ShowAsync("Processing", "Deleting transaction...", ToastType.Info);

			var trip = await CommonData.LoadTableDataById<TripModel>(FleetNames.Trip, _deleteTransactionId)
				?? throw new Exception("Transaction not found.");
			trip.Status = false;
			trip.LastModifiedBy = _user.Id;
			trip.LastModifiedAt = await CommonData.LoadCurrentDateTime();
			trip.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
			await TripData.DeleteTransaction(trip);

			await _toastNotification.ShowAsync("Success", $"Transaction {_deleteTransactionNo} has been deleted successfully.", ToastType.Success);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"An error occurred while deleting transaction: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_deleteTransactionId = 0;
			_deleteTransactionNo = string.Empty;
			_isProcessing = false;
			StateHasChanged();
			await LoadTransactionOverviews();
		}
	}

	private async Task ConfirmRecover()
	{
		if (_isProcessing || _recoverTransactionId == 0)
			return;

		try
		{
			if (!_user.Admin)
				throw new UnauthorizedAccessException("You do not have permission to recover this transaction.");

			await _recoverConfirmationDialog.HideAsync();
			_isProcessing = true;
			StateHasChanged();

			await _toastNotification.ShowAsync("Processing", "Recovering transaction...", ToastType.Info);

			var trip = await CommonData.LoadTableDataById<TripModel>(FleetNames.Trip, _recoverTransactionId)
				?? throw new Exception("Transaction not found.");
			trip.Status = true;
			trip.LastModifiedBy = _user.Id;
			trip.LastModifiedAt = await CommonData.LoadCurrentDateTime();
			trip.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
			await TripData.RecoverTransaction(trip);

			await _toastNotification.ShowAsync("Success", $"Transaction {_recoverTransactionNo} has been recovered successfully.", ToastType.Success);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"An error occurred while recovering transaction: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_recoverTransactionId = 0;
			_recoverTransactionNo = string.Empty;
			_isProcessing = false;
			StateHasChanged();
			await LoadTransactionOverviews();
		}
	}

	private async Task DeleteRecoverSelectedTransaction()
	{
		if (_sfGrid is null || _sfGrid.SelectedRecords is null || _sfGrid.SelectedRecords.Count == 0)
			return;

		if (_sfGrid.SelectedRecords.First().Status)
			await ShowDeleteConfirmation();
		else
			await ShowRecoverConfirmation();
	}

	private async Task ShowDeleteConfirmation()
	{
		_deleteTransactionId = _sfGrid.SelectedRecords.First().MasterId;
		_deleteTransactionNo = _sfGrid.SelectedRecords.First().TransactionNo;
		StateHasChanged();
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteTransactionId = 0;
		_deleteTransactionNo = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation()
	{
		_recoverTransactionId = _sfGrid.SelectedRecords.First().MasterId;
		_recoverTransactionNo = _sfGrid.SelectedRecords.First().TransactionNo;
		StateHasChanged();
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverTransactionId = 0;
		_recoverTransactionNo = string.Empty;
		await _recoverConfirmationDialog.HideAsync();
	}
	#endregion

	#region Utilities
	private async Task OnMenuSelected(Syncfusion.Blazor.Navigations.MenuEventArgs<Syncfusion.Blazor.Navigations.MenuItem> args)
	{
		switch (args.Item.Id)
		{
			case "NewTransaction":
				await AuthenticationService.NavigateToRoute(PageRouteNames.Trip, FormFactor, JSRuntime, NavigationManager);
				break;
			case "Refresh":
				await LoadTransactionOverviews();
				break;
			case "ToggleDeleted":
				await ToggleDeleted();
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
			case "ViewSelected":
				await ViewSelectedTransaction();
				break;
			case "DownloadSelectedPdf":
				await ExportSelectedTransactionPdf();
				break;
			case "DownloadSelectedExcel":
				await ExportSelectedTransactionExcel();
				break;
			case "DeleteRecoverSelected":
				await DeleteRecoverSelectedTransaction();
				break;
			case "TransactionHistory":
				await AuthenticationService.NavigateToRoute(PageRouteNames.TripReport, FormFactor, JSRuntime, NavigationManager);
				break;
			case "ExpensesReport":
				await AuthenticationService.NavigateToRoute(PageRouteNames.TripExpensesReport, FormFactor, JSRuntime, NavigationManager);
				break;
			case "LedgerPaymentsReport":
				await AuthenticationService.NavigateToRoute(PageRouteNames.TripLedgerPaymentsReport, FormFactor, JSRuntime, NavigationManager);
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

	private async Task OnGridContextMenuItemClicked(ContextMenuClickEventArgs<TripCardPaymentsOverviewModel> args)
	{
		switch (args.Item.Id)
		{
			case "View":
				await ViewSelectedTransaction();
				break;
			case "ExportPDF":
				await ExportSelectedTransactionPdf();
				break;
			case "ExportExcel":
				await ExportSelectedTransactionExcel();
				break;
			case "DeleteRecover":
				await DeleteRecoverSelectedTransaction();
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

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadTransactionOverviews();
		StateHasChanged();
	}

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetReportsDashboard, true);

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