using Strada.Shared.Components.Dialog;
using Strada.Shared.Components.Input;

using StradaLibrary.Accounts.Masters.Data;
using StradaLibrary.Accounts.Masters.Models;
using StradaLibrary.Fleet.Bill;
using StradaLibrary.Fleet.Bill.Exports;
using StradaLibrary.Fleet.Bill.Models;
using StradaLibrary.Fleet.OMC.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Operations.Models;
using StradaLibrary.Utils.ExportUtils;

using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.Bill.Reports;

public partial class BillReport : IAsyncDisposable
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

	private List<CompanyModel> _companies = [];
	private List<OMCModel> _omcs = [];
	private List<BillOverviewModel> _transactionOverviews = [];
	private List<BillOverviewModel> _allTransactionOverviews = [];

	private readonly List<ContextMenuItemModel> _gridContextMenuItems =
	[
		new() { Text = "View (Alt + O)", Id = "View", IconCss = "e-icons e-eye", Target = ".e-content" },
		new() { Text = "Export PDF (Alt + P)", Id = "ExportPDF", IconCss = "e-icons e-export-pdf", Target = ".e-content" },
		new() { Text = "Export Excel (Alt + E)", Id = "ExportExcel", IconCss = "e-icons e-export-excel", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecover", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<BillOverviewModel> _sfGrid;
	private CustomDateRangePicker _sfFirstFocus;
	private ToastNotification _toastNotification;
	private ConfirmationDialog _confirmationDialog;

	private string _confirmTitle = string.Empty;
	private string _confirmMessage = string.Empty;
	private Func<Task> _confirmAction;

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

		if (_sfFirstFocus is not null)
			await _sfFirstFocus.FocusAsync();
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

			_allTransactionOverviews = await CommonData.LoadTableDataByDate<BillOverviewModel>(
				FleetNames.BillOverview,
				DateOnly.FromDateTime(_fromDate).ToDateTime(TimeOnly.MinValue),
				DateOnly.FromDateTime(_toDate).ToDateTime(TimeOnly.MinValue));

			await ApplyFilters();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to load transactions: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task ApplyFilters()
	{
		var query = _allTransactionOverviews.AsEnumerable();

		if (!_showDeleted) query = query.Where(t => t.Status);
		if (_selectedCompany?.Id > 0) query = query.Where(t => t.CompanyId == _selectedCompany.Id);
		if (_selectedOMC?.Id > 0) query = query.Where(t => t.OMCId == _selectedOMC.Id);

		_transactionOverviews = [.. query.OrderBy(t => t.TransactionDateTime)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
		StateHasChanged();
	}
	#endregion

	#region Changed Events
	private async Task OnDateRangeChanged(MudBlazor.DateRange range)
	{
		_fromDate = range?.Start ?? _fromDate;
		_toDate = range?.End ?? _toDate;
		await LoadTransactionOverviews();
	}

	private async Task HandleDatesChanged(DateRangeType dateRangeType)
	{
		(_fromDate, _toDate) = await FinancialYearData.GetDateRange(dateRangeType, _fromDate, _toDate);
		await LoadTransactionOverviews();
	}

	private async Task OnCompanyChanged(CompanyModel value)
	{
		_selectedCompany = value;
		await ApplyFilters();
	}

	private async Task OnOMCChanged(OMCModel value)
	{
		_selectedOMC = value;
		await ApplyFilters();
	}
	#endregion

	#region Exporting
	private async Task ExportReport(bool isExcel = false)
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var (stream, fileName) = await BillReportExport.ExportReport(
				_transactionOverviews,
				isExcel ? ReportExportType.Excel : ReportExportType.PDF,
				DateOnly.FromDateTime(_fromDate),
				DateOnly.FromDateTime(_toDate),
				_showAllColumns,
				_showDeleted,
				_selectedCompany?.Id > 0 ? _selectedCompany : null,
				_selectedOMC?.Id > 0 ? _selectedOMC : null
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

	private async Task ExportSelectedTransaction(bool isExcel = false)
	{
		if (_isProcessing || _sfGrid is null || _sfGrid.SelectedRecords is null || _sfGrid.SelectedRecords.Count == 0)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var decodeTransactionNo = await DecodeCode.DecodeTransactionNo(_sfGrid.SelectedRecords.First().TransactionNo, !isExcel, isExcel, CodeType.Bill);
			await SaveAndViewService.SaveAndView(isExcel ? decodeTransactionNo.ExcelStream.fileName : decodeTransactionNo.PDFStream.fileName,
				isExcel ? decodeTransactionNo.ExcelStream.stream : decodeTransactionNo.PDFStream.stream);

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

		await _toastNotification.ShowAsync("Not Allowed", "Transaction viewing will come soon in a future update.", ToastType.Info);
	}

	private async Task DeleteTransaction(int id, string transactionNo)
	{
		if (_isProcessing || id == 0)
			return;

		try
		{
			if (!_user.Admin)
				throw new UnauthorizedAccessException("You do not have permission to delete this transaction.");

			_isProcessing = true;
			StateHasChanged();

			await _toastNotification.ShowAsync("Processing", "Deleting transaction...", ToastType.Info);

			var bill = await CommonData.LoadTableDataById<BillModel>(FleetNames.Bill, id)
				?? throw new Exception("Transaction not found.");
			bill.LastModifiedBy = _user.Id;
			bill.LastModifiedAt = await CommonData.LoadCurrentDateTime();
			bill.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
			await BillData.DeleteTransaction(bill);

			await _toastNotification.ShowAsync("Success", $"Transaction {transactionNo} has been deleted successfully.", ToastType.Success);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"An error occurred while deleting transaction: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
			await LoadTransactionOverviews();
		}
	}

	private async Task DeleteRecoverSelectedTransaction()
	{
		if (_sfGrid is null || _sfGrid.SelectedRecords is null || _sfGrid.SelectedRecords.Count == 0)
			return;

		var record = _sfGrid.SelectedRecords.First();
		if (!record.Status)
			return;

		await ShowConfirmation("Delete", $"Are you sure you want to delete transaction {record.TransactionNo}", () => DeleteTransaction(record.Id, record.TransactionNo));
	}

	private async Task ShowConfirmation(string title, string message, Func<Task> action)
	{
		_confirmTitle = title;
		_confirmMessage = message;
		_confirmAction = action;
		StateHasChanged();
		await _confirmationDialog.ShowAsync();
	}

	private async Task OnConfirmed()
	{
		await _confirmationDialog.HideAsync();
		if (_confirmAction is not null)
			await _confirmAction();
		_confirmAction = null;
	}

	private async Task OnCancelled()
	{
		_confirmAction = null;
		await _confirmationDialog.HideAsync();
	}
	#endregion

	#region Utilities
	private async Task OnMenuSelected(Syncfusion.Blazor.Navigations.MenuEventArgs<Syncfusion.Blazor.Navigations.MenuItem> args)
	{
		switch (args.Item.Id)
		{
			case "NewTransaction": await AuthenticationService.NavigateToRoute(PageRouteNames.Bill, FormFactor, JSRuntime, NavigationManager); break;
			case "Refresh": await LoadTransactionOverviews(); break;
			case "ToggleDeleted": await ToggleDeleted(); break;
			case "ToggleDetailsView": await ToggleDetailsView(); break;
			case "ExportPdf": await ExportReport(); break;
			case "ExportExcel": await ExportReport(true); break;
			case "ViewSelected": await ViewSelectedTransaction(); break;
			case "DownloadSelectedPdf": await ExportSelectedTransaction(); break;
			case "DownloadSelectedExcel": await ExportSelectedTransaction(true); break;
			case "DeleteRecoverSelected": await DeleteRecoverSelectedTransaction(); break;
			case "LedgerPaymentsReport": await AuthenticationService.NavigateToRoute(PageRouteNames.BillLedgerPaymentsReport, FormFactor, JSRuntime, NavigationManager); break;
			case "PeriodToday": await HandleDatesChanged(DateRangeType.Today); break;
			case "PeriodPreviousDay": await HandleDatesChanged(DateRangeType.Yesterday); break;
			case "PeriodNextDay": await HandleDatesChanged(DateRangeType.NextDay); break;
			case "PeriodCurrentMonth": await HandleDatesChanged(DateRangeType.CurrentMonth); break;
			case "PeriodPreviousMonth": await HandleDatesChanged(DateRangeType.PreviousMonth); break;
			case "PeriodNextMonth": await HandleDatesChanged(DateRangeType.NextMonth); break;
			case "PeriodCurrentFinancialYear": await HandleDatesChanged(DateRangeType.CurrentFinancialYear); break;
			case "PeriodPreviousFinancialYear": await HandleDatesChanged(DateRangeType.PreviousFinancialYear); break;
			case "PeriodNextFinancialYear": await HandleDatesChanged(DateRangeType.NextFinancialYear); break;
			case "PeriodAllTime": await HandleDatesChanged(DateRangeType.AllTime); break;
		}
	}

	private async Task OnGridContextMenuItemClicked(ContextMenuClickEventArgs<BillOverviewModel> args)
	{
		switch (args.Item.Id)
		{
			case "View": await ViewSelectedTransaction(); break;
			case "ExportPDF": await ExportSelectedTransaction(); break;
			case "ExportExcel": await ExportSelectedTransaction(true); break;
			case "DeleteRecover": await DeleteRecoverSelectedTransaction(); break;
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
		await ApplyFilters();
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
