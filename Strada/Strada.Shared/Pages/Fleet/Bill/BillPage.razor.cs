using Microsoft.AspNetCore.Components;

using Strada.Shared.Components.Dialog;
using Strada.Shared.Components.Input;

using StradaLibrary.Accounts.Masters.Data;
using StradaLibrary.Accounts.Masters.Models;
using StradaLibrary.Fleet.Bill;
using StradaLibrary.Fleet.Bill.Models;
using StradaLibrary.Fleet.OMC.Models;
using StradaLibrary.Fleet.Trip;
using StradaLibrary.Fleet.Trip.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Operations.Models;

using Syncfusion.Blazor.Grids;

using System.Text.Json;

namespace Strada.Shared.Pages.Fleet.Bill;

public partial class BillPage
{
	[Parameter] public int? Id { get; set; }

	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;

	private CompanyModel _selectedCompany = new();
	private FinancialYearModel _selectedFinancialYear = new();
	private OMCModel _selectedOMC = new();
	private LedgerModel _selectedLedger = null;
	private TripOverviewModel _selectedTrip = new();
	private BillLedgerPaymentsCartModel _selectedLedgerPaymentCart = new();
	private BillModel _bill = new();

	private List<CompanyModel> _companies = [];
	private List<OMCModel> _omcs = [];
	private List<LedgerModel> _ledgers = [];
	private List<TripOverviewModel> _pendingTrips = [];
	private List<TripOverviewModel> _allPendingTrips = [];
	private List<TripOverviewModel> _tripCart = [];
	private List<BillLedgerPaymentsCartModel> _ledgerPaymentsCart = [];

	private CustomAutoComplete<LedgerModel> _sfLedgerAutoComplete;
	private SfGrid<TripOverviewModel> _sfPendingTripGrid;
	private SfGrid<TripOverviewModel> _sfTripCartGrid;
	private SfGrid<BillLedgerPaymentsCartModel> _sfLedgerPaymentsCartGrid;
	private CustomTextField _sfChallanNoTextBox;
	private CustomAutoComplete<CompanyModel> _firstFocus;
	private ToastNotification _toastNotification;

	private readonly List<ContextMenuItemModel> _pendingTripGridContextMenuItems =
	[
		new() { Text = "Insert", Id = "InsertCart", IconCss = "e-icons e-add" },
	];

	private readonly List<ContextMenuItemModel> _tripCartGridContextMenuItems =
	[
		new() { Text = "Edit", Id = "EditCart", IconCss = "e-icons e-edit" },
		new() { Text = "Delete", Id = "DeleteCart", IconCss = "e-icons e-delete" },
	];

	private readonly List<ContextMenuItemModel> _ledgerPaymentsCartGridContextMenuItems =
	[
		new() { Text = "Edit", Id = "EditCart", IconCss = "e-icons e-edit" },
		new() { Text = "Delete", Id = "DeleteCart", IconCss = "e-icons e-delete" }
	];

	#region Load Data
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		try
		{
			_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Fleet]);
			await InitializePage();
		}
		catch { await ResetPage(); }
	}

	private async Task InitializePage()
	{
		await LoadData();
		await ResolveTransaction();
		await LoadSelections();
		await ResolvePendingTripsCart();
		await ResolveLedgerPaymentsCart();

		_isLoading = false;
		StateHasChanged();

		await SaveTransactionFile();

		if (_firstFocus is not null)
			await _firstFocus.FocusAsync();
	}

	private async Task LoadData()
	{
		_companies = await CommonData.LoadTableDataByStatus<CompanyModel>(AccountNames.Company);
		_omcs = await CommonData.LoadTableDataByStatus<OMCModel>(FleetNames.OMC);
		_ledgers = await CommonData.LoadTableDataByStatus<LedgerModel>(AccountNames.Ledger);

		_companies = [.. _companies.OrderBy(s => s.Name)];
		_omcs = [.. _omcs.OrderBy(s => s.Name)];
		_ledgers = [.. _ledgers.OrderBy(s => s.Name)];

		var mainCompanyId = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
		_selectedCompany = _companies.FirstOrDefault(s => s.Id.ToString() == mainCompanyId.Value) ?? _companies.FirstOrDefault();
		_selectedOMC = _omcs.FirstOrDefault();

		_allPendingTrips = await TripData.LoadTripOverviewByBillIdDate(null);
		_allPendingTrips = [.. _allPendingTrips.Where(s => !s.VehicleEmpty).OrderBy(s => s.TransactionDateTime)];
	}

	private async Task ResolveTransaction()
	{
		try
		{
			if (await LoadExistingTransaction())
				return;

			if (await TryRestoreFromLocalStorage())
				return;

			await CreateNewTransaction();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Transaction Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingTransaction()
	{
		if (!Id.HasValue)
			return false;

		_bill = await CommonData.LoadTableDataById<BillModel>(FleetNames.Bill, Id.Value);
		if (_bill is null || _bill.Id == 0)
		{
			await _toastNotification.ShowAsync("Transaction Not Found", "The requested transaction could not be found.", ToastType.Error);
			NavigationManager.NavigateTo(PageRouteNames.Bill, true);
		}

		return true;
	}

	private async Task<bool> TryRestoreFromLocalStorage()
	{
		if (!await DataStorageService.LocalExists(StorageFileNames.BillDataFileName))
			return false;

		try
		{
			_bill = JsonSerializer.Deserialize<BillModel>(await DataStorageService.LocalGetAsync(StorageFileNames.BillDataFileName));
			if (_bill is null)
				return false;

			return true;
		}
		catch
		{
			await DeleteLocalFiles();
			return false;
		}
	}

	private async Task CreateNewTransaction()
	{
		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var financialYear = await FinancialYearData.LoadFinancialYearByDateTime(currentDateTime);

		_bill = new()
		{
			Id = 0,
			TransactionNo = string.Empty,
			CompanyId = _selectedCompany.Id,
			TransactionDateTime = currentDateTime,
			FinancialYearId = financialYear is null ? 0 : financialYear.Id,
			OMCId = _selectedOMC.Id,
			BillNo = string.Empty,
			TotalGrossAmount = 0,
			TotalPenaltyAmount = 0,
			TotalNetAmount = 0,
			TotalLedgerPaymentAmount = 0,
			Remarks = string.Empty,
			CreatedBy = _user.Id,
			CreatedAt = DateTime.Now,
			CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform(),
			Status = true,
			LastModifiedAt = null,
			LastModifiedBy = null,
			LastModifiedFromPlatform = null
		};

		var lastTransaction = await CommonData.LoadLastTableData<BillModel>(FleetNames.Bill);
		if (lastTransaction is not null)
			_bill.TransactionDateTime = lastTransaction.TransactionDateTime;

		await DeleteLocalFiles();
	}

	private async Task LoadSelections()
	{
		if (_bill.CompanyId > 0)
			_selectedCompany = _companies.FirstOrDefault(s => s.Id == _bill.CompanyId) ?? _companies.FirstOrDefault();

		if (_bill.OMCId > 0)
			_selectedOMC = _omcs.FirstOrDefault(s => s.Id == _bill.OMCId) ?? _omcs.FirstOrDefault();
		else
			_selectedOMC = _omcs.FirstOrDefault();
	}

	private async Task ResolvePendingTripsCart()
	{
		try
		{
			_tripCart.Clear();

			if (await LoadExistingPendingTripsCart())
				return;

			if (await DataStorageService.LocalExists(StorageFileNames.BillPendingTripsCartDataFileName))
				_tripCart = JsonSerializer.Deserialize<List<TripOverviewModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.BillPendingTripsCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Pending Trips Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingPendingTripsCart()
	{
		if (_bill.Id <= 0)
			return false;

		var existingCart = await TripData.LoadTripOverviewByBillIdDate(_bill.Id);
		foreach (var item in existingCart)
		{
			if (_allPendingTrips.FirstOrDefault(s => s.Id == item.Id) is null)
			{
				var trip = await CommonData.LoadTableDataById<TripModel>(FleetNames.Trip, item.Id);
				await _toastNotification.ShowAsync("Trip Not Found", $"The Trip with transaction date {item.TransactionDateTime:dd MMM yyyy} in the existing transaction cart was not found in the available pending trips list. It may have been deleted or is inaccessible.", ToastType.Error);
				continue;
			}

			_tripCart.Add(item);
		}

		return true;
	}

	private async Task ResolveLedgerPaymentsCart()
	{
		try
		{
			_ledgerPaymentsCart.Clear();

			if (await LoadExistingLedgerPaymentsCart())
				return;

			if (await DataStorageService.LocalExists(StorageFileNames.BillLedgerPaymentsCartDataFileName))
				_ledgerPaymentsCart = JsonSerializer.Deserialize<List<BillLedgerPaymentsCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.BillLedgerPaymentsCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Ledger Payments Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingLedgerPaymentsCart()
	{
		if (_bill.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<BillLedgerPaymentsModel>(FleetNames.BillLedgerPayments, _bill.Id);

		foreach (var item in existingCart)
		{
			if (_ledgers.FirstOrDefault(s => s.Id == item.LedgerId) is null)
			{
				var ledger = await CommonData.LoadTableDataById<LedgerModel>(AccountNames.Ledger, item.LedgerId);
				await _toastNotification.ShowAsync("Ledger Not Found", $"The ledger {ledger?.Name} (ID: {item.LedgerId}) in the existing transaction cart was not found in the available ledgers list. It may have been deleted or is inaccessible.", ToastType.Error);
				continue;
			}

			_ledgerPaymentsCart.Add(new()
			{
				LedgerId = item.LedgerId,
				LedgerName = _ledgers.First(s => s.Id == item.LedgerId).Name,
				Amount = item.Amount,
				Remarks = item.Remarks
			});
		}

		return true;
	}
	#endregion

	#region Changed Events
	private async Task OnCompanyChanged(CompanyModel value)
	{
		if (value is null || value.Id == 0)
			return;

		_selectedCompany = value;
		await SaveTransactionFile();
	}

	private async Task OnOMCChanged(OMCModel value)
	{
		if (value is null || value.Id == 0)
			return;

		_selectedOMC = value;
		_bill.OMCId = _selectedOMC.Id;

		await SaveTransactionFile();
	}
	#endregion

	#region Ledger Payments Cart
	private void OnLedgerPaymentsTypeChanged(LedgerModel value)
	{
		if (value is null || value.Id == 0)
		{
			_selectedLedger = null;
			_selectedLedgerPaymentCart = new();
			return;
		}

		_selectedLedger = value;

		_selectedLedgerPaymentCart.LedgerId = _selectedLedger.Id;
		_selectedLedgerPaymentCart.LedgerName = _selectedLedger.Name;
		_selectedLedgerPaymentCart.Amount = _bill.TotalNetAmount - _ledgerPaymentsCart.Sum(s => s.Amount);
	}

	private async Task AddLedgerPaymentsToCart()
	{
		if (_selectedLedger is null || _selectedLedger.Id <= 0 || _selectedLedgerPaymentCart.Amount <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		var existingItem = _ledgerPaymentsCart.FirstOrDefault(s => s.LedgerId == _selectedLedger.Id);
		if (existingItem is not null)
			existingItem.Amount += _selectedLedgerPaymentCart.Amount;
		else
			_ledgerPaymentsCart.Add(new()
			{
				LedgerId = _selectedLedger.Id,
				LedgerName = _selectedLedger.Name,
				Amount = _selectedLedgerPaymentCart.Amount,
				Remarks = _selectedLedgerPaymentCart.Remarks
			});

		_selectedLedger = null;
		_selectedLedgerPaymentCart = new();
		await _sfLedgerAutoComplete.FocusAsync();
		await SaveTransactionFile();
	}

	private async Task EditSelectedLedgerPaymentsCartItem()
	{
		if (_sfLedgerPaymentsCartGrid is null || _sfLedgerPaymentsCartGrid.SelectedRecords is null || _sfLedgerPaymentsCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartItem = _sfLedgerPaymentsCartGrid.SelectedRecords.First();

		_selectedLedger = _ledgers.FirstOrDefault(s => s.Id == selectedCartItem.LedgerId);
		if (_selectedLedger is null)
			return;

		_selectedLedgerPaymentCart = new()
		{
			LedgerId = selectedCartItem.LedgerId,
			LedgerName = selectedCartItem.LedgerName,
			Amount = selectedCartItem.Amount,
			Remarks = selectedCartItem.Remarks
		};

		await _sfLedgerAutoComplete.FocusAsync();
		await RemoveSelectedLedgerPaymentsCartItem();
	}

	private async Task RemoveSelectedLedgerPaymentsCartItem()
	{
		if (_sfLedgerPaymentsCartGrid is null || _sfLedgerPaymentsCartGrid.SelectedRecords is null || _sfLedgerPaymentsCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartItem = _sfLedgerPaymentsCartGrid.SelectedRecords.First();
		_ledgerPaymentsCart.Remove(selectedCartItem);
		await SaveTransactionFile();
	}
	#endregion

	#region Trip Cart
	private async Task EditSelectedPendingTripItem()
	{
		if (_sfPendingTripGrid is null || _sfPendingTripGrid.SelectedRecords is null || _sfPendingTripGrid.SelectedRecords.Count == 0)
			return;

		_selectedTrip = _sfPendingTripGrid.SelectedRecords.First();
		StateHasChanged();
		await _sfChallanNoTextBox.FocusAsync();
	}

	private async Task OnPendingTripDoubleClick(RecordDoubleClickEventArgs<TripOverviewModel> args)
	{
		if (args.RowData is null || args.RowData.Id <= 0)
			return;

		_selectedTrip = args.RowData;
		StateHasChanged();
		await _sfChallanNoTextBox.FocusAsync();
	}

	private async Task EditSelectedTripCartItem()
	{
		if (_sfTripCartGrid is null || _sfTripCartGrid.SelectedRecords is null || _sfTripCartGrid.SelectedRecords.Count == 0)
			return;

		_selectedTrip = _sfTripCartGrid.SelectedRecords.First();
		await RemoveSelectedTripFromCart();
	}

	private async Task AddTripToCart()
	{
		if (_selectedTrip is null || _selectedTrip.Id <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please select a Trip from the pending list first.", ToastType.Error);
			return;
		}

		_selectedTrip.GrossAmount ??= 0;
		_selectedTrip.PenaltyAmount ??= 0;
		_selectedTrip.NetAmount = _selectedTrip.GrossAmount - _selectedTrip.PenaltyAmount;

		if (string.IsNullOrWhiteSpace(_selectedTrip.ChallanNo) ||
			_selectedTrip.Quantity < 0 ||
			_selectedTrip.GrossAmount < 0 ||
			_selectedTrip.PenaltyAmount < 0 ||
			_selectedTrip.NetAmount < 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		var existingItem = _tripCart.FirstOrDefault(s => s.Id == _selectedTrip.Id);
		if (existingItem is not null)
			_tripCart.Remove(existingItem);

		_tripCart.Add(_selectedTrip);

		_selectedTrip = new();
		await SaveTransactionFile();
	}

	private async Task RemoveSelectedTripFromCart()
	{
		if (_sfTripCartGrid is null || _sfTripCartGrid.SelectedRecords is null || _sfTripCartGrid.SelectedRecords.Count == 0)
			return;

		_tripCart.Remove(_sfTripCartGrid.SelectedRecords.First());

		await SaveTransactionFile();
	}
	#endregion

	#region Saving
	private async Task UpdateFinancialDetails()
	{
		foreach (var item in _ledgerPaymentsCart.ToList())
			if (item.Amount <= 0)
				_ledgerPaymentsCart.Remove(item);

		foreach (var item in _tripCart.ToList())
			if (string.IsNullOrWhiteSpace(item.ChallanNo) ||
				item.Quantity < 0 ||
				item.GrossAmount is null || item.GrossAmount < 0 ||
				item.PenaltyAmount is null || item.PenaltyAmount < 0 ||
				item.NetAmount is null || item.NetAmount < 0 ||
				item.GrossAmount - item.PenaltyAmount != item.NetAmount)
				_tripCart.Remove(item);

		_bill.CompanyId = _selectedCompany.Id;
		_bill.OMCId = _selectedOMC.Id;
		_bill.TotalGrossAmount = _tripCart.Sum(s => s.GrossAmount) ?? 0;
		_bill.TotalPenaltyAmount = _tripCart.Sum(s => s.PenaltyAmount) ?? 0;
		_bill.TotalNetAmount = _tripCart.Sum(s => s.NetAmount) ?? 0;
		_bill.TotalLedgerPaymentAmount = _ledgerPaymentsCart.Sum(s => s.Amount);

		if (_bill.TotalGrossAmount - _bill.TotalPenaltyAmount != _bill.TotalNetAmount)
		{
			_tripCart.Clear();
			await _toastNotification.ShowAsync("Inconsistent trip amounts detected", "Transaction data has been reset. Please review the trip details and try saving again.", ToastType.Error);
		}

		_pendingTrips = [.. _allPendingTrips
			.Where(s => !_tripCart.Any(c => c.Id == s.Id))
			.Where (s => s.CompanyId == _selectedCompany.Id)
			.OrderBy(s => s.TransactionDateTime)];

		#region Financial Year
		_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_bill.TransactionDateTime);
		if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
			_bill.FinancialYearId = _selectedFinancialYear.Id;
		else
			await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
		#endregion

		if (Id is null)
			_bill.TransactionNo = await GenerateCodes.GenerateBillTransactionNo(_bill);

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		_bill.Status = true;
		_bill.TransactionDateTime = DateOnly.FromDateTime(_bill.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
		_bill.CreatedAt = currentDateTime;
		_bill.LastModifiedAt = currentDateTime;
		_bill.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_bill.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_bill.CreatedBy = _user.Id;
		_bill.LastModifiedBy = _user.Id;
	}

	private async Task SaveTransactionFile()
	{
		if (_isProcessing || _isLoading)
			return;

		try
		{
			_isProcessing = true;

			await UpdateFinancialDetails();

			if (_tripCart.Count == 0 || _bill.Id > 0)
			{
				await DeleteLocalFiles();
				return;
			}

			await DataStorageService.LocalSaveAsync(StorageFileNames.BillDataFileName, JsonSerializer.Serialize(_bill));
			await DataStorageService.LocalSaveAsync(StorageFileNames.BillPendingTripsCartDataFileName, JsonSerializer.Serialize(_tripCart));
			await DataStorageService.LocalSaveAsync(StorageFileNames.BillLedgerPaymentsCartDataFileName, JsonSerializer.Serialize(_ledgerPaymentsCart));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Saving Transaction Data", ex.Message, ToastType.Error);
		}
		finally
		{
			if (_sfLedgerPaymentsCartGrid is not null)
				await _sfLedgerPaymentsCartGrid.Refresh();

			if (_sfPendingTripGrid is not null)
				await _sfPendingTripGrid.Refresh();

			if (_sfTripCartGrid is not null)
				await _sfTripCartGrid.Refresh();

			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task SaveTransaction(bool savePDF = false, bool saveExcel = false)
	{
		if (_isProcessing || _isLoading)
			return;

		try
		{
			await SaveTransactionFile();
			_isProcessing = true;

			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			var ledgerPayments = BillData.ConvertLedgerPaymentCartToDetails(_ledgerPaymentsCart, _bill.Id);
			_bill.Id = await BillData.SaveTransaction(_bill, ledgerPayments, _tripCart);
			_bill = await CommonData.LoadTableDataById<BillModel>(FleetNames.Bill, _bill.Id);

			if (savePDF) await ExportSelectedTransaction(false, true);
			if (saveExcel) await ExportSelectedTransaction(true, true);

			await _toastNotification.ShowAsync("Save Transaction", "Transaction saved successfully.", ToastType.Success);

			if (Id.HasValue && Id.Value > 0)
				await AuthenticationService.CloseWindowOrTab(FormFactor, JSRuntime);
			await ResetPage();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Saving Transaction", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
		}
	}
	#endregion

	#region Exporting
	private async Task ExportSelectedTransaction(bool isExcel = false, bool force = false)
	{
		if (_bill.Id <= 0 || (_isProcessing && !force))
			return;

		try
		{
			_isProcessing = true;
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var decodeTransactionNo = await DecodeCode.DecodeTransactionNo(_bill.TransactionNo, !isExcel, isExcel, CodeType.Bill);
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
		}
	}
	#endregion

	#region Utilities
	private async Task OnPendingTripGridContextMenuItemClicked(ContextMenuClickEventArgs<TripOverviewModel> args)
	{
		switch (args.Item.Id)
		{
			case "InsertCart": await EditSelectedPendingTripItem(); break;
		}
	}

	private async Task OnTripCartGridContextMenuItemClicked(ContextMenuClickEventArgs<TripOverviewModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditCart": await EditSelectedTripCartItem(); break;
			case "DeleteCart": await RemoveSelectedTripFromCart(); break;
		}
	}

	private async Task OnLedgerPaymentsCartGridContextMenuItemClicked(ContextMenuClickEventArgs<BillLedgerPaymentsCartModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditCart": await EditSelectedLedgerPaymentsCartItem(); break;
			case "DeleteCart": await RemoveSelectedLedgerPaymentsCartItem(); break;
		}
	}

	private async Task DeleteLocalFiles()
	{
		await DataStorageService.LocalRemove(StorageFileNames.BillDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.BillLedgerPaymentsCartDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.BillPendingTripsCartDataFileName);
	}

	private async Task ResetPage()
	{
		await DeleteLocalFiles();
		PageRefresh.Request();
	}
	#endregion
}
