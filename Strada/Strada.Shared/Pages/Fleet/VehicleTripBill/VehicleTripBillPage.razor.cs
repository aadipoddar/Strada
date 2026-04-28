using Microsoft.AspNetCore.Components;
using Strada.Shared.Components.Dialog;
using Strada.Shared.Components.Input;
using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Fleet.VehicleTrip;
using StradaLibrary.Data.Fleet.VehicleTripBill;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.VehicleTrip;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleTrip;
using StradaLibrary.Models.Fleet.VehicleTripBill;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.VehicleTripBill;

public partial class VehicleTripBillPage
{
	[Parameter] public int? Id { get; set; }

	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;

	private DateTime _startVehicleTripDate = DateTime.Now.AddMonths(-1);
	private DateTime _endVehicleTripDate = DateTime.Now;

	private CompanyModel _selectedCompany = new();
	private FinancialYearModel _selectedFinancialYear = new();
	private OMCModel _selectedOMC = new();
	private OMCCardModel _selectedOMCCard = null;
	private LedgerModel _selectedLedger = null;
	private VehicleTripOverviewModel _selectedVehicleTrip = new();
	private VehicleTripBillCardPaymentsCartModel _selectedCardPaymentCart = new();
	private VehicleTripBillLedgerPaymentsCartModel _selectedLedgerPaymentCart = new();
	private VehicleTripBillModel _vehicleTripBill = new();

	private List<CompanyModel> _companies = [];
	private List<OMCModel> _omcs = [];
	private List<OMCCardModel> _omcCards = [];
	private List<LedgerModel> _ledgers = [];
	private List<VehicleTripOverviewModel> _pendingVehicleTrips = [];
	private List<VehicleTripOverviewModel> _allPendingVehicleTrips = [];
	private List<VehicleTripOverviewModel> _vehicleTripCart = [];
	private List<VehicleTripBillCardPaymentsCartModel> _cardPaymentsCart = [];
	private List<VehicleTripBillLedgerPaymentsCartModel> _ledgerPaymentsCart = [];

	private AutoCompleteWithAdd<VehicleExpenseTypeModel?, VehicleExpenseTypeModel> _sfExpenseTypeAutoComplete;
	private AutoCompleteWithAdd<OMCCardModel?, OMCCardModel> _sfOMCCardAutoComplete;
	private AutoCompleteWithAdd<LedgerModel?, LedgerModel> _sfLedgerAutoComplete;
	private Syncfusion.Blazor.Inputs.SfTextBox _sfChallanNoTextBox;
	private SfGrid<VehicleTripOverviewModel> _sfPendingVehicleTripGrid;
	private SfGrid<VehicleTripOverviewModel> _sfVehicleTripCartGrid;
	private SfGrid<VehicleTripBillCardPaymentsCartModel> _sfCardPaymentsCartGrid;
	private SfGrid<VehicleTripBillLedgerPaymentsCartModel> _sfLedgerPaymentsCartGrid;
	private ToastNotification _toastNotification;

	private readonly List<ContextMenuItemModel> _cardPaymentsCartGridContextMenuItems =
	[
		new() { Text = "Delete", Id = "DeleteCart", IconCss = "e-icons e-delete" }
	];

	private readonly List<ContextMenuItemModel> _ledgerPaymentsCartGridContextMenuItems =
	[
		new() { Text = "Delete", Id = "DeleteCart", IconCss = "e-icons e-delete" }
	];

	private readonly List<ContextMenuItemModel> _pendingVehicleTripGridContextMenuItems =
	[
		new() { Text = "Insert", Id = "InsertCart", IconCss = "e-icons e-add" },
	];

	private readonly List<ContextMenuItemModel> _vehicleTripCartGridContextMenuItems =
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
		await LoadPendingVehicleTrips();
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

	private async Task LoadPendingVehicleTrips()
	{
		_allPendingVehicleTrips = await VehicleTripData.LoadVehicleTripOverviewByBillIdDate(null, _startVehicleTripDate, _endVehicleTripDate);
		_allPendingVehicleTrips = [.. _allPendingVehicleTrips.Where(s => !s.VehicleEmpty).OrderBy(s => s.TransactionDateTime)];
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

		_vehicleTripBill = await CommonData.LoadTableDataById<VehicleTripBillModel>(FleetNames.VehicleTripBill, Id.Value);
		if (_vehicleTripBill is null || _vehicleTripBill.Id == 0)
		{
			await _toastNotification.ShowAsync("Transaction Not Found", "The requested transaction could not be found.", ToastType.Error);
			NavigationManager.NavigateTo(PageRouteNames.VehicleTripBill, true);
			return false;
		}

		return true;
	}

	private async Task<bool> TryRestoreFromLocalStorage()
	{
		if (!await DataStorageService.LocalExists(StorageFileNames.VehicleTripBillDataFileName))
			return false;

		try
		{
			_vehicleTripBill = System.Text.Json.JsonSerializer.Deserialize<VehicleTripBillModel>(await DataStorageService.LocalGetAsync(StorageFileNames.VehicleTripBillDataFileName));
			if (_vehicleTripBill is null)
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

		_vehicleTripBill = new()
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
		if (_vehicleTripBill.CompanyId > 0)
			_selectedCompany = _companies.FirstOrDefault(s => s.Id == _vehicleTripBill.CompanyId) ?? _companies.FirstOrDefault();
		else
		{
			var mainCompanyId = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
			_selectedCompany = _companies.FirstOrDefault(s => s.Id.ToString() == mainCompanyId.Value) ?? _companies.FirstOrDefault();
		}
		_vehicleTripBill.CompanyId = _selectedCompany.Id;

		if (_vehicleTripBill.OMCId > 0)
			_selectedOMC = _omcs.FirstOrDefault(s => s.Id == _vehicleTripBill.OMCId) ?? _omcs.FirstOrDefault();
		else
			_selectedOMC = _omcs.FirstOrDefault();
		_vehicleTripBill.OMCId = _selectedOMC.Id;

		if (_vehicleTripBill.FinancialYearId > 0)
			_selectedFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, _vehicleTripBill.FinancialYearId);

		if (_selectedFinancialYear is null || _selectedFinancialYear.Id <= 0)
			_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_vehicleTripBill.TransactionDateTime);

		if (_selectedFinancialYear is not null)
			_vehicleTripBill.FinancialYearId = _selectedFinancialYear.Id;
	}

	private async Task ResolvePendingTripsCart()
	{
		try
		{
			_vehicleTripCart.Clear();

			if (await LoadExistingPendingTripsCart())
				return;

			if (await DataStorageService.LocalExists(StorageFileNames.VehicleTripBillPendingTripsCartDataFileName))
				_vehicleTripCart = System.Text.Json.JsonSerializer.Deserialize<List<VehicleTripOverviewModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.VehicleTripBillPendingTripsCartDataFileName));

			if (_vehicleTripCart.Count > 0)
			{
				_startVehicleTripDate = _vehicleTripCart.Min(s => s.TransactionDateTime).AddDays(-1);
				_endVehicleTripDate = _vehicleTripCart.Max(s => s.TransactionDateTime).AddDays(1);
				await LoadPendingVehicleTrips();
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
		if (_vehicleTripBill.Id <= 0)
			return false;

		var existingCart = await VehicleTripData.LoadVehicleTripOverviewByBillIdDate(_vehicleTripBill.Id);

		if (_vehicleTripCart.Count > 0)
		{
			_startVehicleTripDate = existingCart.Min(s => s.TransactionDateTime).AddDays(-1);
			_endVehicleTripDate = existingCart.Max(s => s.TransactionDateTime).AddDays(1);
		}
		await LoadPendingVehicleTrips();

		foreach (var item in existingCart)
		{
			if (_allPendingVehicleTrips.FirstOrDefault(s => s.Id == item.Id) is null)
			{
				var vehicleTrip = await CommonData.LoadTableDataById<VehicleTripModel>(FleetNames.VehicleTrip, item.Id);
				await _toastNotification.ShowAsync("Vehicle Trip Not Found", $"The vehicle trip with transaction date {item.TransactionDateTime:dd MMM yyyy} in the existing transaction cart was not found in the available pending trips list. It may have been deleted or is inaccessible.", ToastType.Error);
				continue;
			}

			_vehicleTripCart.Add(item);
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

			if (await DataStorageService.LocalExists(StorageFileNames.VehicleTripBillCardPaymentsCartDataFileName))
				_cardPaymentsCart = System.Text.Json.JsonSerializer.Deserialize<List<VehicleTripBillCardPaymentsCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.VehicleTripBillCardPaymentsCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Card Payments Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingCardPaymentsCart()
	{
		if (_vehicleTripBill.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<VehicleTripBillCardPaymentsModel>(FleetNames.VehicleTripBillCardPayments, _vehicleTripBill.Id);

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

			if (await DataStorageService.LocalExists(StorageFileNames.VehicleTripBillLedgerPaymentsCartDataFileName))
				_ledgerPaymentsCart = System.Text.Json.JsonSerializer.Deserialize<List<VehicleTripBillLedgerPaymentsCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.VehicleTripBillLedgerPaymentsCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Ledger Payments Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingLedgerPaymentsCart()
	{
		if (_vehicleTripBill.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<VehicleTripBillLedgerPaymentsModel>(FleetNames.VehicleTripBillLedgerPayments, _vehicleTripBill.Id);

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
		_vehicleTripBill.OMCId = _selectedOMC.Id;

		await SaveTransactionFile();
	}

	private async Task OnVehicleTripDateRangeChanged(Syncfusion.Blazor.Calendars.RangePickerEventArgs<DateTime> args)
	{
		if (_isProcessing || _isLoading)
			return;

		_startVehicleTripDate = args.StartDate;
		_endVehicleTripDate = args.EndDate;

		try
		{
			_isProcessing = true;

			await LoadPendingVehicleTrips();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Vehicle Trips Data", ex.Message, ToastType.Error);
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
			_selectedCardPaymentCart.Amount = _vehicleTripBill.TotalNetAmount - _cardPaymentsCart.Sum(s => s.Amount) - _ledgerPaymentsCart.Sum(s => s.Amount);
		}
	}

	private async Task AddCardPaymentsToCart()
	{
		if (_selectedOMCCard is null || _selectedOMCCard.Id <= 0 || _selectedCardPaymentCart.Amount <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		if (_cardPaymentsCart.Sum(s => s.Amount) + _selectedCardPaymentCart.Amount > _vehicleTripBill.TotalNetAmount - _ledgerPaymentsCart.Sum(s => s.Amount))
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
			_selectedLedgerPaymentCart.Amount = _vehicleTripBill.TotalNetAmount - _cardPaymentsCart.Sum(s => s.Amount) - _ledgerPaymentsCart.Sum(s => s.Amount);
		}
	}

	private async Task AddLedgerPaymentsToCart()
	{
		if (_selectedLedger is null || _selectedLedger.Id <= 0 || _selectedLedgerPaymentCart.Amount <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		if (_ledgerPaymentsCart.Sum(s => s.Amount) + _selectedLedgerPaymentCart.Amount > _vehicleTripBill.TotalNetAmount - _cardPaymentsCart.Sum(s => s.Amount))
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

	#region Vehicle Trip Cart
	private async Task EditSelectedPendingVehicleTripItem()
	{
		if (_sfPendingVehicleTripGrid is null || _sfPendingVehicleTripGrid.SelectedRecords is null || _sfPendingVehicleTripGrid.SelectedRecords.Count == 0)
			return;

		_selectedVehicleTrip = _sfPendingVehicleTripGrid.SelectedRecords.First();
		StateHasChanged();
		await _sfChallanNoTextBox.FocusAsync();
	}

	private async void OnPendingVehicleTripDoubleClick(RecordDoubleClickEventArgs<VehicleTripOverviewModel> args)
	{
		if (args.RowData is null || args.RowData.Id <= 0)
			return;

		_selectedVehicleTrip = args.RowData;
		StateHasChanged();
		await _sfChallanNoTextBox.FocusAsync();
	}

	private async Task EditSelectedVehicleTripCartItem()
	{
		if (_sfVehicleTripCartGrid is null || _sfVehicleTripCartGrid.SelectedRecords is null || _sfVehicleTripCartGrid.SelectedRecords.Count == 0)
			return;

		_selectedVehicleTrip = _sfVehicleTripCartGrid.SelectedRecords.First();
		await RemoveSelectedVehicleTripFromCart();
	}

	private async Task AddVehicleTripToCart()
	{
		if (_selectedVehicleTrip is null || _selectedVehicleTrip.Id <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please select a vehicle trip from the pending list first.", ToastType.Error);
			return;
		}

		_selectedVehicleTrip.TDSAmount ??= 0;
		_selectedVehicleTrip.PenaltyAmount ??= 0;
		_selectedVehicleTrip.NetAmount = (_selectedVehicleTrip.GrossAmount ?? 0) - _selectedVehicleTrip.TDSAmount - _selectedVehicleTrip.PenaltyAmount;

		if (string.IsNullOrWhiteSpace(_selectedVehicleTrip.ChallanNo) ||
			_selectedVehicleTrip.Quantity < 0 ||
			_selectedVehicleTrip.GrossAmount is null || _selectedVehicleTrip.GrossAmount <= 0 ||
			_selectedVehicleTrip.TDSAmount < 0 ||
			_selectedVehicleTrip.PenaltyAmount < 0 ||
			_selectedVehicleTrip.NetAmount < 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		var existingItem = _vehicleTripCart.FirstOrDefault(s => s.Id == _selectedVehicleTrip.Id);
		if (existingItem is not null)
			_vehicleTripCart.Remove(existingItem);

		_vehicleTripCart.Add(_selectedVehicleTrip);

		_selectedVehicleTrip = new();
		await SaveTransactionFile();
	}

	private async Task RemoveSelectedVehicleTripFromCart()
	{
		if (_sfVehicleTripCartGrid is null || _sfVehicleTripCartGrid.SelectedRecords is null || _sfVehicleTripCartGrid.SelectedRecords.Count == 0)
			return;

		_vehicleTripCart.Remove(_sfVehicleTripCartGrid.SelectedRecords.First());

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

		foreach (var item in _vehicleTripCart.ToList())
		{
			if (string.IsNullOrWhiteSpace(item.ChallanNo) ||
				item.Quantity < 0 ||
				item.GrossAmount is null || item.GrossAmount < 0 ||
				item.TDSAmount is null || item.TDSAmount < 0 ||
				item.PenaltyAmount is null || item.PenaltyAmount < 0 ||
				item.NetAmount is null || item.NetAmount < 0 ||
				item.GrossAmount - item.TDSAmount - item.PenaltyAmount != item.NetAmount ||
				item.TransactionDateTime < _startVehicleTripDate.AddDays(-1) ||
				item.TransactionDateTime > _endVehicleTripDate.AddDays(1))
			{
				_vehicleTripCart.Remove(item);
				continue;
			}
		}

		_pendingVehicleTrips = [.. _allPendingVehicleTrips
			.Where(s => !_vehicleTripCart.Any(c => c.Id == s.Id))
			.Where(s => s.OMCId == _selectedOMC.Id)
			.OrderBy(s => s.TransactionDateTime)];

		_vehicleTripBill.Remarks = _vehicleTripBill.Remarks?.Trim();
		if (string.IsNullOrWhiteSpace(_vehicleTripBill.Remarks))
			_vehicleTripBill.Remarks = null;

		_vehicleTripBill.CompanyId = _selectedCompany.Id;
		_vehicleTripBill.OMCId = _selectedOMC.Id;
		_vehicleTripBill.TotalGrossAmount = _vehicleTripCart.Sum(s => s.GrossAmount) ?? 0;
		_vehicleTripBill.TotalTDSAmount = _vehicleTripCart.Sum(s => s.TDSAmount) ?? 0;
		_vehicleTripBill.TotalPenaltyAmount = _vehicleTripCart.Sum(s => s.PenaltyAmount) ?? 0;
		_vehicleTripBill.TotalNetAmount = _vehicleTripCart.Sum(s => s.NetAmount) ?? 0;
		_vehicleTripBill.TotalCardPaymentAmount = _cardPaymentsCart.Sum(s => s.Amount);
		_vehicleTripBill.TotalLedgerPaymentAmount = _ledgerPaymentsCart.Sum(s => s.Amount);

		if (_vehicleTripBill.TotalGrossAmount - _vehicleTripBill.TotalTDSAmount - _vehicleTripBill.TotalPenaltyAmount != _vehicleTripBill.TotalNetAmount)
		{
			_vehicleTripCart.Clear();
			_pendingVehicleTrips = await VehicleTripData.LoadVehicleTripOverviewByBillIdDate(null, _startVehicleTripDate, _endVehicleTripDate);
			_pendingVehicleTrips = [.. _pendingVehicleTrips.Where(s => !s.VehicleEmpty).OrderBy(s => s.TransactionDateTime)];

			throw new Exception("Inconsistent trip amounts detected. Transaction data has been reset. Please review the trip details and try saving again.");
		}

		if (_cardPaymentsCart.Sum(s => s.Amount) + _ledgerPaymentsCart.Sum(s => s.Amount) > _vehicleTripBill.TotalNetAmount)
		{
			_cardPaymentsCart.Clear();
			_ledgerPaymentsCart.Clear();
		}

		#region Financial Year
		_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_vehicleTripBill.TransactionDateTime);
		if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
			_vehicleTripBill.FinancialYearId = _selectedFinancialYear.Id;
		else
		{
			await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
			_vehicleTripBill.TransactionDateTime = await CommonData.LoadCurrentDateTime();
			_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_vehicleTripBill.TransactionDateTime);
			_vehicleTripBill.FinancialYearId = _selectedFinancialYear.Id;
		}
		#endregion

		if (Id is null)
			_vehicleTripBill.TransactionNo = await GenerateCodes.GenerateVehicleTripBillTransactionNo(_vehicleTripBill);

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		_vehicleTripBill.Status = true;
		_vehicleTripBill.TransactionDateTime = DateOnly.FromDateTime(_vehicleTripBill.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
		_vehicleTripBill.LastModifiedAt = currentDateTime;
		_vehicleTripBill.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_vehicleTripBill.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_vehicleTripBill.CreatedBy = _user.Id;
		_vehicleTripBill.LastModifiedBy = _user.Id;
	}

	private async Task SaveTransactionFile()
	{
		if (_isProcessing || _isLoading)
			return;

		try
		{
			_isProcessing = true;

			await UpdateFinancialDetails();

			if (_vehicleTripCart.Count == 0 || _vehicleTripBill.Id > 0)
			{
				await DeleteLocalFiles();
				return;
			}

			await DataStorageService.LocalSaveAsync(StorageFileNames.VehicleTripBillDataFileName, System.Text.Json.JsonSerializer.Serialize(_vehicleTripBill));
			await DataStorageService.LocalSaveAsync(StorageFileNames.VehicleTripBillPendingTripsCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_vehicleTripCart));
			await DataStorageService.LocalSaveAsync(StorageFileNames.VehicleTripBillCardPaymentsCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_cardPaymentsCart));
			await DataStorageService.LocalSaveAsync(StorageFileNames.VehicleTripBillLedgerPaymentsCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_ledgerPaymentsCart));
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

			if (_sfPendingVehicleTripGrid is not null)
				await _sfPendingVehicleTripGrid.Refresh();

			if (_sfVehicleTripCartGrid is not null)
				await _sfVehicleTripCartGrid.Refresh();

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

			var cardPayments = VehicleTripBillData.ConvertCardPaymentCartToDetails(_cardPaymentsCart, _vehicleTripBill.Id);
			var ledgerPayments = VehicleTripBillData.ConvertLedgerPaymentCartToDetails(_ledgerPaymentsCart, _vehicleTripBill.Id);
			_vehicleTripBill.Id = await VehicleTripBillData.SaveTransaction(_vehicleTripBill, cardPayments, ledgerPayments, _vehicleTripCart);

			if (savePDF)
			{
				var (pdfStream, pdfFileName) = await VehicleTripInvoiceExport.ExportInvoice(_vehicleTripBill.Id, InvoiceExportType.PDF);
				await SaveAndViewService.SaveAndView(pdfFileName, pdfStream);
			}

			if (saveExcel)
			{
				var (excelStream, excelFileName) = await VehicleTripInvoiceExport.ExportInvoice(_vehicleTripBill.Id, InvoiceExportType.Excel);
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

			var decodeTransactionNo = await GenerateCodes.DecodeTransactionNo(_vehicleTripBill.TransactionNo);
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

			var decodeTransactionNo = await GenerateCodes.DecodeTransactionNo(_vehicleTripBill.TransactionNo);
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
				await AuthenticationService.NavigateToRoute(PageRouteNames.VehicleTripBillReport, FormFactor, JSRuntime, NavigationManager);
				break;
			case "CardPaymentsReport":
				await AuthenticationService.NavigateToRoute(PageRouteNames.VehicleTripBillCardPaymentsReport, FormFactor, JSRuntime, NavigationManager);
				break;
			case "LedgerPaymentsReport":
				await AuthenticationService.NavigateToRoute(PageRouteNames.VehicleTripBillLedgerPaymentsReport, FormFactor, JSRuntime, NavigationManager);
				break;
		}
	}

	private async Task OnCardPaymentsCartGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleTripBillCardPaymentsCartModel> args)
	{
		switch (args.Item.Id)
		{
			case "DeleteCart":
				await RemoveSelectedCardPaymentsCartItem();
				break;
		}
	}

	private async Task OnLedgerPaymentsCartGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleTripBillLedgerPaymentsCartModel> args)
	{
		switch (args.Item.Id)
		{
			case "DeleteCart":
				await RemoveSelectedLedgerPaymentsCartItem();
				break;
		}
	}

	private async Task OnPendingVehicleTripGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleTripOverviewModel> args)
	{
		switch (args.Item.Id)
		{
			case "InsertCart":
				await EditSelectedPendingVehicleTripItem();
				break;
		}
	}

	private async Task OnVehicleTripCartGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleTripOverviewModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditCart":
				await EditSelectedVehicleTripCartItem();
				break;
			case "DeleteCart":
				await RemoveSelectedVehicleTripFromCart();
				break;
		}
	}

	private async Task DeleteLocalFiles()
	{
		await DataStorageService.LocalRemove(StorageFileNames.VehicleTripBillDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.VehicleTripBillCardPaymentsCartDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.VehicleTripBillLedgerPaymentsCartDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.VehicleTripBillPendingTripsCartDataFileName);
	}

	private async Task ResetPage()
	{
		await DeleteLocalFiles();
		NavigationManager.NavigateTo(PageRouteNames.VehicleTripBill, true);
	}

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetTransactionsDashboard, true);
	#endregion
}