using Microsoft.AspNetCore.Components;
using Strada.Shared.Components.Dialog;
using Strada.Shared.Components.Input;
using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Fleet.Bill;
using StradaLibrary.Data.Fleet.Trip;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.Trip;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Bill;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.Trip;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.Bill;

public partial class BillPage
{
	[Parameter] public int? Id { get; set; }

	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;

	private DateTime _startTripDate = DateTime.Now.AddMonths(-1);
	private DateTime _endTripDate = DateTime.Now;

	private CompanyModel _selectedCompany = new();
	private FinancialYearModel _selectedFinancialYear = new();
	private OMCModel _selectedOMC = new();
	private OMCCardModel _selectedOMCCard = null;
	private LedgerModel _selectedLedger = null;
	private TripOverviewModel _selectedTrip = new();
	private BillCardPaymentsCartModel _selectedCardPaymentCart = new();
	private BillLedgerPaymentsCartModel _selectedLedgerPaymentCart = new();
	private BillModel _bill = new();

	private List<CompanyModel> _companies = [];
	private List<OMCModel> _omcs = [];
	private List<OMCCardModel> _omcCards = [];
	private List<LedgerModel> _ledgers = [];
	private List<TripOverviewModel> _pendingTrips = [];
	private List<TripOverviewModel> _allPendingTrips = [];
	private List<TripOverviewModel> _tripCart = [];
	private List<BillCardPaymentsCartModel> _cardPaymentsCart = [];
	private List<BillLedgerPaymentsCartModel> _ledgerPaymentsCart = [];

	private AutoCompleteWithAdd<ExpenseTypeModel?, ExpenseTypeModel> _sfExpenseTypeAutoComplete;
	private AutoCompleteWithAdd<OMCCardModel?, OMCCardModel> _sfOMCCardAutoComplete;
	private AutoCompleteWithAdd<LedgerModel?, LedgerModel> _sfLedgerAutoComplete;
	private Syncfusion.Blazor.Inputs.SfTextBox _sfChallanNoTextBox;
	private SfGrid<TripOverviewModel> _sfPendingTripGrid;
	private SfGrid<TripOverviewModel> _sfTripCartGrid;
	private SfGrid<BillCardPaymentsCartModel> _sfCardPaymentsCartGrid;
	private SfGrid<BillLedgerPaymentsCartModel> _sfLedgerPaymentsCartGrid;
	private ToastNotification _toastNotification;

	private readonly List<ContextMenuItemModel> _cardPaymentsCartGridContextMenuItems =
	[
		new() { Text = "Delete", Id = "DeleteCart", IconCss = "e-icons e-delete" }
	];

	private readonly List<ContextMenuItemModel> _ledgerPaymentsCartGridContextMenuItems =
	[
		new() { Text = "Delete", Id = "DeleteCart", IconCss = "e-icons e-delete" }
	];

	private readonly List<ContextMenuItemModel> _pendingTripGridContextMenuItems =
	[
		new() { Text = "Insert", Id = "InsertCart", IconCss = "e-icons e-add" },
	];

	private readonly List<ContextMenuItemModel> _tripCartGridContextMenuItems =
	[
		new() { Text = "Edit", Id = "EditCart", IconCss = "e-icons e-edit" },
		new() { Text = "Delete", Id = "DeleteCart", IconCss = "e-icons e-delete" },
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
		catch
		{
			await ResetPage();
		}
	}

	private async Task InitializePage()
	{
		await LoadData();
		await LoadPendingTrips();
		await ResolveTransaction();
		await LoadSelections();
		await ResolvePendingTripsCart();
		await ResolveCardPaymentsCart();
		await ResolveLedgerPaymentsCart();

		_isLoading = false;
		StateHasChanged();

		await SaveTransactionFile();
	}

	private async Task LoadData()
	{
		_companies = await CommonData.LoadTableDataByStatus<CompanyModel>(AccountNames.Company);
		_omcs = await CommonData.LoadTableDataByStatus<OMCModel>(FleetNames.OMC);
		_omcCards = await CommonData.LoadTableDataByStatus<OMCCardModel>(FleetNames.OMCCard);
		_ledgers = await CommonData.LoadTableDataByStatus<LedgerModel>(AccountNames.Ledger);

		_companies = [.. _companies.OrderBy(s => s.Name)];
		var mainCompanyId = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
		_selectedCompany = _companies.FirstOrDefault(s => s.Id.ToString() == mainCompanyId.Value) ?? _companies.FirstOrDefault();

		_omcs = [.. _omcs.OrderBy(s => s.Name)];
		_omcCards = [.. _omcCards.OrderBy(s => s.CardNumber)];
		_ledgers = [.. _ledgers.OrderBy(s => s.Name)];

		_selectedOMC = _omcs.FirstOrDefault();
	}

	private async Task LoadPendingTrips()
	{
		_allPendingTrips = await TripData.LoadTripOverviewByBillIdDate(null, _startTripDate, _endTripDate);
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
			return false;
		}

		return true;
	}

	private async Task<bool> TryRestoreFromLocalStorage()
	{
		if (!await DataStorageService.LocalExists(StorageFileNames.BillDataFileName))
			return false;

		try
		{
			_bill = System.Text.Json.JsonSerializer.Deserialize<BillModel>(await DataStorageService.LocalGetAsync(StorageFileNames.BillDataFileName));
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
			TotalTDSAmount = 0,
			TotalPenaltyAmount = 0,
			TotalNetAmount = 0,
			TotalCardPaymentAmount = 0,
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

		await DeleteLocalFiles();
	}

	private async Task LoadSelections()
	{
		if (_bill.CompanyId > 0)
			_selectedCompany = _companies.FirstOrDefault(s => s.Id == _bill.CompanyId) ?? _companies.FirstOrDefault();
		else
		{
			var mainCompanyId = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
			_selectedCompany = _companies.FirstOrDefault(s => s.Id.ToString() == mainCompanyId.Value) ?? _companies.FirstOrDefault();
		}
		_bill.CompanyId = _selectedCompany.Id;

		if (_bill.OMCId > 0)
			_selectedOMC = _omcs.FirstOrDefault(s => s.Id == _bill.OMCId) ?? _omcs.FirstOrDefault();
		else
			_selectedOMC = _omcs.FirstOrDefault();
		_bill.OMCId = _selectedOMC.Id;

		if (_bill.FinancialYearId > 0)
			_selectedFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, _bill.FinancialYearId);

		if (_selectedFinancialYear is null || _selectedFinancialYear.Id <= 0)
			_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_bill.TransactionDateTime);

		if (_selectedFinancialYear is not null)
			_bill.FinancialYearId = _selectedFinancialYear.Id;
	}

	private async Task ResolvePendingTripsCart()
	{
		try
		{
			_tripCart.Clear();

			if (await LoadExistingPendingTripsCart())
				return;

			if (await DataStorageService.LocalExists(StorageFileNames.BillPendingTripsCartDataFileName))
				_tripCart = System.Text.Json.JsonSerializer.Deserialize<List<TripOverviewModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.BillPendingTripsCartDataFileName));

			if (_tripCart.Count > 0)
			{
				_startTripDate = _tripCart.Min(s => s.TransactionDateTime).AddDays(-1);
				_endTripDate = _tripCart.Max(s => s.TransactionDateTime).AddDays(1);
				await LoadPendingTrips();
			}
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

		if (_tripCart.Count > 0)
		{
			_startTripDate = existingCart.Min(s => s.TransactionDateTime).AddDays(-1);
			_endTripDate = existingCart.Max(s => s.TransactionDateTime).AddDays(1);
		}
		await LoadPendingTrips();

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

	private async Task ResolveCardPaymentsCart()
	{
		try
		{
			_cardPaymentsCart.Clear();

			if (await LoadExistingCardPaymentsCart())
				return;

			if (await DataStorageService.LocalExists(StorageFileNames.BillCardPaymentsCartDataFileName))
				_cardPaymentsCart = System.Text.Json.JsonSerializer.Deserialize<List<BillCardPaymentsCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.BillCardPaymentsCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Card Payments Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingCardPaymentsCart()
	{
		if (_bill.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<BillCardPaymentsModel>(FleetNames.BillCardPayments, _bill.Id);

		foreach (var item in existingCart)
		{
			if (_omcCards.FirstOrDefault(s => s.Id == item.OMCCardId) is null)
			{
				var omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, item.OMCCardId);
				await _toastNotification.ShowAsync("OMC Card Not Found", $"The OMC card {omcCard?.CardNumber} (ID: {item.OMCCardId}) in the existing transaction cart was not found in the available cards list. It may have been deleted or is inaccessible.", ToastType.Error);
				continue;
			}

			_cardPaymentsCart.Add(new()
			{
				OMCCardId = item.OMCCardId,
				OMCCardNumber = _omcCards.First(s => s.Id == item.OMCCardId).CardNumber,
				Amount = item.Amount,
				Remarks = item.Remarks
			});
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
				_ledgerPaymentsCart = System.Text.Json.JsonSerializer.Deserialize<List<BillLedgerPaymentsCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.BillLedgerPaymentsCartDataFileName));
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

	#region Change Events
	private async Task OnCompanyChanged(ChangeEventArgs<CompanyModel, CompanyModel> args)
	{
		if (args.Value is null || args.Value.Id == 0)
			return;

		_selectedCompany = args.Value;
		await SaveTransactionFile();
	}

	private async Task OnOMCChanged(ChangeEventArgs<OMCModel, OMCModel> args)
	{
		if (args.Value is null || args.Value.Id == 0)
			return;

		_selectedOMC = args.Value;
		_bill.OMCId = _selectedOMC.Id;

		await SaveTransactionFile();
	}

	private async Task OnTripDateRangeChanged(Syncfusion.Blazor.Calendars.RangePickerEventArgs<DateTime> args)
	{
		if (_isProcessing || _isLoading)
			return;

		_startTripDate = args.StartDate;
		_endTripDate = args.EndDate;

		try
		{
			_isProcessing = true;

			await LoadPendingTrips();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Trips Data", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			await SaveTransactionFile();
		}
	}
	#endregion

	#region Card Payments Cart
	private async Task OnCardPaymentsTypeChanged(ChangeEventArgs<OMCCardModel?, OMCCardModel?> args)
	{
		if (args.Value is null || args.Value.Id == 0)
			return;

		_selectedOMCCard = args.Value;

		if (_selectedOMCCard is null)
			_selectedCardPaymentCart = new()
			{
				OMCCardId = 0,
				OMCCardNumber = "",
				Amount = 0
			};

		else
		{
			_selectedCardPaymentCart.OMCCardId = _selectedOMCCard.Id;
			_selectedCardPaymentCart.OMCCardNumber = _selectedOMCCard.CardNumber;
			_selectedCardPaymentCart.Amount = _bill.TotalNetAmount - _cardPaymentsCart.Sum(s => s.Amount) - _ledgerPaymentsCart.Sum(s => s.Amount);
		}
	}

	private async Task AddCardPaymentsToCart()
	{
		if (_selectedOMCCard is null || _selectedOMCCard.Id <= 0 || _selectedCardPaymentCart.Amount <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		if (_cardPaymentsCart.Sum(s => s.Amount) + _selectedCardPaymentCart.Amount > _bill.TotalNetAmount - _ledgerPaymentsCart.Sum(s => s.Amount))
		{
			await _toastNotification.ShowAsync("Payment Amount Exceeds Total Amount", "The total payment amount in the cart cannot exceed the total amount of the trips. Please adjust the amount accordingly.", ToastType.Error);
			return;
		}

		var existingItem = _cardPaymentsCart.FirstOrDefault(s => s.OMCCardId == _selectedOMCCard.Id);
		if (existingItem is not null)
			existingItem.Amount += _selectedCardPaymentCart.Amount;
		else
			_cardPaymentsCart.Add(new()
			{
				OMCCardId = _selectedOMCCard.Id,
				OMCCardNumber = _selectedOMCCard.CardNumber,
				Amount = _selectedCardPaymentCart.Amount,
				Remarks = _selectedCardPaymentCart.Remarks
			});

		_selectedOMCCard = null;
		_selectedCardPaymentCart = new();
		await _sfOMCCardAutoComplete.FocusAsync();
		await SaveTransactionFile();
	}

	private async Task RemoveSelectedCardPaymentsCartItem()
	{
		if (_sfCardPaymentsCartGrid is null || _sfCardPaymentsCartGrid.SelectedRecords is null || _sfCardPaymentsCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartItem = _sfCardPaymentsCartGrid.SelectedRecords.First();
		_cardPaymentsCart.Remove(selectedCartItem);
		await SaveTransactionFile();
	}
	#endregion

	#region Ledger Payments Cart
	private async Task OnLedgerPaymentsTypeChanged(ChangeEventArgs<LedgerModel?, LedgerModel?> args)
	{
		if (args.Value is null || args.Value.Id == 0)
			return;

		_selectedLedger = args.Value;

		if (_selectedLedger is null)
			_selectedLedgerPaymentCart = new()
			{
				LedgerId = 0,
				LedgerName = "",
				Amount = 0
			};

		else
		{
			_selectedLedgerPaymentCart.LedgerId = _selectedLedger.Id;
			_selectedLedgerPaymentCart.LedgerName = _selectedLedger.Name;
			_selectedLedgerPaymentCart.Amount = _bill.TotalNetAmount - _cardPaymentsCart.Sum(s => s.Amount) - _ledgerPaymentsCart.Sum(s => s.Amount);
		}
	}

	private async Task AddLedgerPaymentsToCart()
	{
		if (_selectedLedger is null || _selectedLedger.Id <= 0 || _selectedLedgerPaymentCart.Amount <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		if (_ledgerPaymentsCart.Sum(s => s.Amount) + _selectedLedgerPaymentCart.Amount > _bill.TotalNetAmount - _cardPaymentsCart.Sum(s => s.Amount))
		{
			await _toastNotification.ShowAsync("Payment Amount Exceeds Total Amount", "The total payment amount in the cart cannot exceed the total amount of the trips. Please adjust the amount accordingly.", ToastType.Error);
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

	private async void OnPendingTripDoubleClick(RecordDoubleClickEventArgs<TripOverviewModel> args)
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

		_selectedTrip.TDSAmount ??= 0;
		_selectedTrip.PenaltyAmount ??= 0;
		_selectedTrip.NetAmount = (_selectedTrip.GrossAmount ?? 0) - _selectedTrip.TDSAmount - _selectedTrip.PenaltyAmount;

		if (string.IsNullOrWhiteSpace(_selectedTrip.ChallanNo) ||
			_selectedTrip.Quantity < 0 ||
			_selectedTrip.GrossAmount is null || _selectedTrip.GrossAmount <= 0 ||
			_selectedTrip.TDSAmount < 0 ||
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
		foreach (var item in _cardPaymentsCart.ToList())
		{
			if (item.Amount <= 0)
				_cardPaymentsCart.Remove(item);

			item.Remarks = item.Remarks?.Trim();
			if (string.IsNullOrWhiteSpace(item.Remarks))
				item.Remarks = null;
		}

		foreach (var item in _ledgerPaymentsCart.ToList())
		{
			if (item.Amount <= 0)
				_ledgerPaymentsCart.Remove(item);

			item.Remarks = item.Remarks?.Trim();
			if (string.IsNullOrWhiteSpace(item.Remarks))
				item.Remarks = null;
		}

		foreach (var item in _tripCart.ToList())
		{
			if (string.IsNullOrWhiteSpace(item.ChallanNo) ||
				item.Quantity < 0 ||
				item.GrossAmount is null || item.GrossAmount < 0 ||
				item.TDSAmount is null || item.TDSAmount < 0 ||
				item.PenaltyAmount is null || item.PenaltyAmount < 0 ||
				item.NetAmount is null || item.NetAmount < 0 ||
				item.GrossAmount - item.TDSAmount - item.PenaltyAmount != item.NetAmount ||
				item.TransactionDateTime < _startTripDate.AddDays(-1) ||
				item.TransactionDateTime > _endTripDate.AddDays(1))
			{
				_tripCart.Remove(item);
				continue;
			}
		}

		_pendingTrips = [.. _allPendingTrips
			.Where(s => !_tripCart.Any(c => c.Id == s.Id))
			.Where(s => s.OMCId == _selectedOMC.Id)
			.OrderBy(s => s.TransactionDateTime)];

		_bill.Remarks = _bill.Remarks?.Trim();
		if (string.IsNullOrWhiteSpace(_bill.Remarks))
			_bill.Remarks = null;

		_bill.CompanyId = _selectedCompany.Id;
		_bill.OMCId = _selectedOMC.Id;
		_bill.TotalGrossAmount = _tripCart.Sum(s => s.GrossAmount) ?? 0;
		_bill.TotalTDSAmount = _tripCart.Sum(s => s.TDSAmount) ?? 0;
		_bill.TotalPenaltyAmount = _tripCart.Sum(s => s.PenaltyAmount) ?? 0;
		_bill.TotalNetAmount = _tripCart.Sum(s => s.NetAmount) ?? 0;
		_bill.TotalCardPaymentAmount = _cardPaymentsCart.Sum(s => s.Amount);
		_bill.TotalLedgerPaymentAmount = _ledgerPaymentsCart.Sum(s => s.Amount);

		if (_bill.TotalGrossAmount - _bill.TotalTDSAmount - _bill.TotalPenaltyAmount != _bill.TotalNetAmount)
		{
			_tripCart.Clear();
			_pendingTrips = await TripData.LoadTripOverviewByBillIdDate(null, _startTripDate, _endTripDate);
			_pendingTrips = [.. _pendingTrips.Where(s => !s.VehicleEmpty).OrderBy(s => s.TransactionDateTime)];

			throw new Exception("Inconsistent trip amounts detected. Transaction data has been reset. Please review the trip details and try saving again.");
		}

		if (_cardPaymentsCart.Sum(s => s.Amount) + _ledgerPaymentsCart.Sum(s => s.Amount) > _bill.TotalNetAmount)
		{
			_cardPaymentsCart.Clear();
			_ledgerPaymentsCart.Clear();
		}

		#region Financial Year
		_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_bill.TransactionDateTime);
		if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
			_bill.FinancialYearId = _selectedFinancialYear.Id;
		else
		{
			await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
			_bill.TransactionDateTime = await CommonData.LoadCurrentDateTime();
			_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_bill.TransactionDateTime);
			_bill.FinancialYearId = _selectedFinancialYear.Id;
		}
		#endregion

		if (Id is null)
			_bill.TransactionNo = await GenerateCodes.GenerateBillTransactionNo(_bill);

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		_bill.Status = true;
		_bill.TransactionDateTime = DateOnly.FromDateTime(_bill.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
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

			await DataStorageService.LocalSaveAsync(StorageFileNames.BillDataFileName, System.Text.Json.JsonSerializer.Serialize(_bill));
			await DataStorageService.LocalSaveAsync(StorageFileNames.BillPendingTripsCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_tripCart));
			await DataStorageService.LocalSaveAsync(StorageFileNames.BillCardPaymentsCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_cardPaymentsCart));
			await DataStorageService.LocalSaveAsync(StorageFileNames.BillLedgerPaymentsCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_ledgerPaymentsCart));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Saving Transaction Data", ex.Message, ToastType.Error);
		}
		finally
		{
			if (_sfCardPaymentsCartGrid is not null)
				await _sfCardPaymentsCartGrid.Refresh();

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

			var cardPayments = BillData.ConvertCardPaymentCartToDetails(_cardPaymentsCart, _bill.Id);
			var ledgerPayments = BillData.ConvertLedgerPaymentCartToDetails(_ledgerPaymentsCart, _bill.Id);
			_bill.Id = await BillData.SaveTransaction(_bill, cardPayments, ledgerPayments, _tripCart);

			if (savePDF)
			{
				var (pdfStream, pdfFileName) = await TripInvoiceExport.ExportInvoice(_bill.Id, InvoiceExportType.PDF);
				await SaveAndViewService.SaveAndView(pdfFileName, pdfStream);
			}

			if (saveExcel)
			{
				var (excelStream, excelFileName) = await TripInvoiceExport.ExportInvoice(_bill.Id, InvoiceExportType.Excel);
				await SaveAndViewService.SaveAndView(excelFileName, excelStream);
			}

			await ResetPage();
			await _toastNotification.ShowAsync("Save Transaction", "Transaction saved successfully.", ToastType.Success);
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
	private async Task ExportPdfInvoice()
	{
		if (!Id.HasValue || Id.Value <= 0)
		{
			await _toastNotification.ShowAsync("Nothing to Export", "There is nothing to export.", ToastType.Error);
			return;
		}

		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var decodeTransactionNo = await GenerateCodes.DecodeTransactionNo(_bill.TransactionNo);
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
		}
	}

	private async Task ExportExcelInvoice()
	{
		if (!Id.HasValue || Id.Value <= 0)
		{
			await _toastNotification.ShowAsync("Nothing to Export", "There is nothing to export.", ToastType.Error);
			return;
		}

		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var decodeTransactionNo = await GenerateCodes.DecodeTransactionNo(_bill.TransactionNo);
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
		}
	}
	#endregion

	#region Utilities
	private async Task OnMenuSelected(Syncfusion.Blazor.Navigations.MenuEventArgs<Syncfusion.Blazor.Navigations.MenuItem> args)
	{
		switch (args.Item.Id)
		{
			case "NewTransaction":
				await ResetPage();
				break;
			case "SaveTransaction":
				await SaveTransaction();
				break;
			case "SavePdfInvoice":
				await SaveTransaction(savePDF: true);
				break;
			case "SaveExcelInvoice":
				await SaveTransaction(saveExcel: true);
				break;
			case "ExportPdfInvoice":
				await ExportPdfInvoice();
				break;
			case "ExportExcelInvoice":
				await ExportExcelInvoice();
				break;
			case "TransactionHistory":
				await AuthenticationService.NavigateToRoute(PageRouteNames.BillReport, FormFactor, JSRuntime, NavigationManager);
				break;
			case "CardPaymentsReport":
				await AuthenticationService.NavigateToRoute(PageRouteNames.BillCardPaymentsReport, FormFactor, JSRuntime, NavigationManager);
				break;
			case "LedgerPaymentsReport":
				await AuthenticationService.NavigateToRoute(PageRouteNames.BillLedgerPaymentsReport, FormFactor, JSRuntime, NavigationManager);
				break;
		}
	}

	private async Task OnCardPaymentsCartGridContextMenuItemClicked(ContextMenuClickEventArgs<BillCardPaymentsCartModel> args)
	{
		switch (args.Item.Id)
		{
			case "DeleteCart":
				await RemoveSelectedCardPaymentsCartItem();
				break;
		}
	}

	private async Task OnLedgerPaymentsCartGridContextMenuItemClicked(ContextMenuClickEventArgs<BillLedgerPaymentsCartModel> args)
	{
		switch (args.Item.Id)
		{
			case "DeleteCart":
				await RemoveSelectedLedgerPaymentsCartItem();
				break;
		}
	}

	private async Task OnPendingTripGridContextMenuItemClicked(ContextMenuClickEventArgs<TripOverviewModel> args)
	{
		switch (args.Item.Id)
		{
			case "InsertCart":
				await EditSelectedPendingTripItem();
				break;
		}
	}

	private async Task OnTripCartGridContextMenuItemClicked(ContextMenuClickEventArgs<TripOverviewModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditCart":
				await EditSelectedTripCartItem();
				break;
			case "DeleteCart":
				await RemoveSelectedTripFromCart();
				break;
		}
	}

	private async Task DeleteLocalFiles()
	{
		await DataStorageService.LocalRemove(StorageFileNames.BillDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.BillCardPaymentsCartDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.BillLedgerPaymentsCartDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.BillPendingTripsCartDataFileName);
	}

	private async Task ResetPage()
	{
		await DeleteLocalFiles();
		NavigationManager.NavigateTo(PageRouteNames.Bill, true);
	}

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetTransactionsDashboard, true);
	#endregion
}