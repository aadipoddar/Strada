using Microsoft.AspNetCore.Components;

using Strada.Shared.Components.Dialog;
using Strada.Shared.Components.Input;

using StradaLibrary.Accounts.Masters.Data;
using StradaLibrary.Accounts.Masters.Models;
using StradaLibrary.Fleet.OMC.Models;
using StradaLibrary.Fleet.Route.Data;
using StradaLibrary.Fleet.Route.Models;
using StradaLibrary.Fleet.Trip;
using StradaLibrary.Fleet.Trip.Models;
using StradaLibrary.Fleet.Vehicle.Models;
using StradaLibrary.Operations.Models;

using Syncfusion.Blazor.Grids;

using System.Text.Json;

namespace Strada.Shared.Pages.Fleet.Trip;

public partial class TripPage
{
	[Parameter] public int? Id { get; set; }

	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;

	private VehicleModel _selectedVehicle = new();
	private CompanyModel _selectedCompany = new();
	private OMCModel _selectedOMC = new();
	private FinancialYearModel _selectedFinancialYear = new();
	private DriverOverviewModel _selectedDriver = new();
	private RouteOverviewModel _selectedRoute = new();
	private ExpenseTypeModel _selectedExpenseType = null;
	private OMCCardModel _selectedOMCCard = null;
	private LedgerModel _selectedLedger = null;
	private TripExpensesCartModel _selectedExpensesCart = new();
	private TripCardPaymentsCartModel _selectedCardPaymentCart = new();
	private TripLedgerPaymentsCartModel _selectedLedgerPaymentCart = new();
	private TripModel _trip = new();

	private List<VehicleModel> _vehicles = [];
	private List<CompanyModel> _companies = [];
	private List<OMCModel> _omcs = [];
	private List<DriverOverviewModel> _drivers = [];
	private List<VehicleDriverOverviewModel> _vehicleDrivers = [];
	private List<RouteOverviewModel> _routes = [];
	private List<ExpenseTypeModel> _expenseTypes = [];
	private List<OMCCardModel> _omcCards = [];
	private List<LedgerModel> _ledgers = [];
	private List<TripExpensesCartModel> _expensesCart = [];
	private List<TripCardPaymentsCartModel> _cardPaymentsCart = [];
	private List<TripLedgerPaymentsCartModel> _ledgerPaymentsCart = [];

	private CustomAutoComplete<ExpenseTypeModel> _sfExpenseTypeAutoComplete;
	private CustomAutoComplete<OMCCardModel> _sfOMCCardAutoComplete;
	private CustomAutoComplete<LedgerModel> _sfLedgerAutoComplete;
	private SfGrid<TripExpensesCartModel> _sfExpensesCartGrid;
	private SfGrid<TripCardPaymentsCartModel> _sfCardPaymentsCartGrid;
	private SfGrid<TripLedgerPaymentsCartModel> _sfLedgerPaymentsCartGrid;
	private CustomAutoComplete<VehicleModel> _sfFirstFocus;
	private ToastNotification _toastNotification;

	private readonly List<ContextMenuItemModel> _expensesCartGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditCart", IconCss = "e-icons e-edit" },
		new() { Text = "Delete (Del)", Id = "DeleteCart", IconCss = "e-icons e-delete" }
	];

	private readonly List<ContextMenuItemModel> _cardPaymentsCartGridContextMenuItems =
	[
		new() { Text = "Edit", Id = "EditCart", IconCss = "e-icons e-edit" },
		new() { Text = "Delete", Id = "DeleteCart", IconCss = "e-icons e-delete" }
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
		await ResolveExpensesCart();
		await ResolveCardPaymentsCart();
		await ResolveLedgerPaymentsCart();

		_isLoading = false;
		StateHasChanged();

		await SaveTransactionFile();

		if (_sfFirstFocus is not null)
			await _sfFirstFocus.FocusAsync();
	}

	private async Task LoadData()
	{
		_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
		_companies = await CommonData.LoadTableDataByStatus<CompanyModel>(AccountNames.Company);
		_omcs = await CommonData.LoadTableDataByStatus<OMCModel>(FleetNames.OMC);
		_drivers = await DriverData.LoadDriverOverview();
		_vehicleDrivers = await VehicleDriverData.LoadVehicleDriverOverview();
		_routes = await StradaLibrary.Fleet.Route.Data.RouteData.LoadRouteOverview();
		_expenseTypes = await CommonData.LoadTableDataByStatus<ExpenseTypeModel>(FleetNames.ExpenseType);
		_omcCards = await CommonData.LoadTableDataByStatus<OMCCardModel>(FleetNames.OMCCard);
		_ledgers = await CommonData.LoadTableDataByStatus<LedgerModel>(AccountNames.Ledger);

		_vehicles = [.. _vehicles.OrderBy(s => s.ShortCode)];
		_companies = [.. _companies.OrderBy(x => x.Name)];
		_omcs = [.. _omcs.OrderBy(x => x.Name)];
		_drivers = [.. _drivers.OrderBy(s => s.Name)];
		_vehicleDrivers = [.. _vehicleDrivers.Where(vd => vd.StartDateTime <= DateTime.Now && (vd.EndDateTime is null || vd.EndDateTime > DateTime.Now))];
		_routes = [.. _routes.OrderBy(s => s.Code)];
		_expenseTypes = [.. _expenseTypes.OrderBy(s => s.Name)];
		_omcCards = [.. _omcCards.OrderBy(s => s.CardNumber)];
		_ledgers = [.. _ledgers.OrderBy(s => s.Name)];

		_selectedVehicle = _vehicles.FirstOrDefault();
		_selectedCompany = _companies.FirstOrDefault(c => c.Id == _selectedVehicle.CompanyId);
		_selectedOMC = _omcs.FirstOrDefault(c => c.Id == _selectedVehicle.OMCId) ?? _omcs.FirstOrDefault();
		_selectedDriver = _vehicleDrivers.FirstOrDefault(vd => vd.VehicleId == _selectedVehicle.Id) is var vehicleDriver && vehicleDriver is not null ? _drivers.FirstOrDefault(d => d.Id == vehicleDriver.DriverId) : _drivers.FirstOrDefault();
		_selectedRoute = _routes.FirstOrDefault();
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

		_trip = await CommonData.LoadTableDataById<TripModel>(FleetNames.Trip, Id.Value);
		if (_trip is null || _trip.Id == 0)
		{
			await _toastNotification.ShowAsync("Transaction Not Found", "The requested transaction could not be found.", ToastType.Error);
			NavigationManager.NavigateTo(PageRouteNames.Trip, true);
		}

		return true;
	}

	private async Task<bool> TryRestoreFromLocalStorage()
	{
		if (!await DataStorageService.LocalExists(StorageFileNames.TripDataFileName))
			return false;

		try
		{
			_trip = JsonSerializer.Deserialize<TripModel>(await DataStorageService.LocalGetAsync(StorageFileNames.TripDataFileName));
			if (_trip is null)
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

		_trip = new()
		{
			Id = 0,
			TransactionNo = string.Empty,
			CompanyId = _selectedCompany.Id,
			TransactionDateTime = currentDateTime,
			FinancialYearId = financialYear is null ? 0 : financialYear.Id,
			OMCId = _selectedOMC.Id,
			VehicleId = _selectedVehicle.Id,
			DriverId = _selectedDriver.Id,
			RouteId = _selectedRoute.Id,
			ChallanNo = string.Empty,
			Quantity = 0,
			TotalExpense = 0,
			Remarks = string.Empty,
			CreatedBy = _user.Id,
			CreatedAt = DateTime.Now,
			CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform(),
			Status = true,
			LastModifiedAt = null,
			LastModifiedBy = null,
			LastModifiedFromPlatform = null
		};

		var lastTransaction = await CommonData.LoadLastTableData<TripModel>(FleetNames.Trip);
		if (lastTransaction is not null)
		{
			_trip.TransactionDateTime = lastTransaction.TransactionDateTime;
			_trip.VehicleId = lastTransaction.VehicleId;

			if (_vehicles.FirstOrDefault(v => v.Id == lastTransaction.VehicleId) is { } lastVehicle)
				_trip.CompanyId = lastVehicle.CompanyId;
		}

		_trip.SlNo = await GenerateCodes.GenerateTripSlNo(_trip);

		await DeleteLocalFiles();
	}

	private async Task LoadSelections()
	{
		if (_trip.VehicleId > 0)
			_selectedVehicle = _vehicles.FirstOrDefault(s => s.Id == _trip.VehicleId) ?? _vehicles.FirstOrDefault();
		else
			_selectedVehicle = _vehicles.FirstOrDefault();

		if (_trip.CompanyId > 0)
			_selectedCompany = _companies.FirstOrDefault(s => s.Id == _trip.CompanyId) ?? _companies.FirstOrDefault();
		else
			_selectedCompany = _companies.FirstOrDefault(s => s.Id == _selectedVehicle.CompanyId) ?? _companies.FirstOrDefault();

		if (_trip.OMCId > 0)
			_selectedOMC = _omcs.FirstOrDefault(s => s.Id == _trip.OMCId) ?? _omcs.FirstOrDefault();
		else
			_selectedOMC = _omcs.FirstOrDefault(s => s.Id == _selectedVehicle.OMCId) ?? _omcs.FirstOrDefault();

		if (_trip.DriverId > 0)
			_selectedDriver = _drivers.FirstOrDefault(s => s.Id == _trip.DriverId) ?? _drivers.FirstOrDefault();
		else
			_selectedDriver = _drivers.FirstOrDefault();

		if (_trip.RouteId > 0)
			_selectedRoute = _routes.FirstOrDefault(s => s.Id == _trip.RouteId) ?? _routes.FirstOrDefault();
		else
			_selectedRoute = _routes.FirstOrDefault();
	}

	private async Task ResolveExpensesCart()
	{
		try
		{
			_expensesCart.Clear();

			if (await LoadExistingExpensesCart())
				return;

			if (await DataStorageService.LocalExists(StorageFileNames.TripExpensesCartDataFileName))
				_expensesCart = JsonSerializer.Deserialize<List<TripExpensesCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.TripExpensesCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Expenses Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingExpensesCart()
	{
		if (_trip.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<TripExpensesModel>(FleetNames.TripExpenses, _trip.Id);

		foreach (var item in existingCart)
		{
			if (_expenseTypes.FirstOrDefault(s => s.Id == item.ExpenseTypeId) is null)
			{
				var expenseType = await CommonData.LoadTableDataById<ExpenseTypeModel>(FleetNames.ExpenseType, item.ExpenseTypeId);
				await _toastNotification.ShowAsync("Expense Type Not Found", $"The expense type {expenseType?.Name} (ID: {item.ExpenseTypeId}) in the existing transaction cart was not found in the available expense types list. It may have been deleted or is inaccessible.", ToastType.Error);
				continue;
			}

			_expensesCart.Add(new()
			{
				ExpenseTypeId = item.ExpenseTypeId,
				ExpenseTypeName = _expenseTypes.First(s => s.Id == item.ExpenseTypeId).Name,
				Amount = item.Amount,
				Remarks = item.Remarks
			});
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

			if (await DataStorageService.LocalExists(StorageFileNames.TripCardPaymentsCartDataFileName))
				_cardPaymentsCart = JsonSerializer.Deserialize<List<TripCardPaymentsCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.TripCardPaymentsCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Payments Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingCardPaymentsCart()
	{
		if (_trip.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<TripCardPaymentsModel>(FleetNames.TripCardPayments, _trip.Id);

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

			if (await DataStorageService.LocalExists(StorageFileNames.TripLedgerPaymentsCartDataFileName))
				_ledgerPaymentsCart = JsonSerializer.Deserialize<List<TripLedgerPaymentsCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.TripLedgerPaymentsCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Payments Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingLedgerPaymentsCart()
	{
		if (_trip.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<TripLedgerPaymentsModel>(FleetNames.TripLedgerPayments, _trip.Id);

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
	private async Task OnTransactionDateChanged(DateTime value)
	{
		_trip.TransactionDateTime = value;

		_vehicleDrivers = await VehicleDriverData.LoadVehicleDriverOverview();
		_vehicleDrivers = [.. _vehicleDrivers.Where(vd => vd.StartDateTime <= DateTime.Now && (vd.EndDateTime is null || vd.EndDateTime > DateTime.Now))];
		_selectedDriver = _vehicleDrivers.FirstOrDefault(vd => vd.VehicleId == _selectedVehicle.Id) is var vehicleDriver && vehicleDriver is not null ? _drivers.FirstOrDefault(d => d.Id == vehicleDriver.DriverId) : _drivers.FirstOrDefault();
	}

	private async Task OnVehicleChanged(VehicleModel value)
	{
		if (value is null || value.Id == 0)
			return;

		_selectedVehicle = value;
		_selectedCompany = _companies.FirstOrDefault(s => s.Id == _selectedVehicle.CompanyId);
		_selectedOMC = _omcs.FirstOrDefault(s => s.Id == _selectedVehicle.OMCId) ?? _omcs.FirstOrDefault();
		_selectedDriver = _vehicleDrivers.FirstOrDefault(vd => vd.VehicleId == _selectedVehicle.Id) is var vehicleDriver && vehicleDriver is not null ? _drivers.FirstOrDefault(d => d.Id == vehicleDriver.DriverId) : _drivers.FirstOrDefault();

		if (_trip.Id == 0)
		{
			_trip.CompanyId = _selectedCompany.Id;
			_trip.SlNo = await GenerateCodes.GenerateTripSlNo(_trip);
		}

		await SaveTransactionFile();
	}

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
		await SaveTransactionFile();
	}

	private async Task OnDriverChanged(DriverOverviewModel value)
	{
		if (value is null || value.Id == 0)
			return;

		_selectedDriver = value;
		await SaveTransactionFile();
	}

	private async Task OnRouteChanged(RouteOverviewModel value)
	{
		if (value is null || value.Id == 0)
			return;

		_selectedRoute = value;
		await SaveTransactionFile();
	}
	#endregion

	#region Expenses Cart
	private void OnExpensesTypeChanged(ExpenseTypeModel value)
	{
		if (value is null || value.Id == 0)
		{
			_selectedExpenseType = null;
			_selectedExpensesCart = new();
			return;
		}

		_selectedExpenseType = value;

		_selectedExpensesCart.ExpenseTypeId = _selectedExpenseType.Id;
		_selectedExpensesCart.ExpenseTypeName = _selectedExpenseType.Name;
	}

	private async Task AddExpensesToCart()
	{
		if (_selectedExpenseType is null || _selectedExpenseType.Id <= 0 || _selectedExpensesCart.Amount <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		var existingItem = _expensesCart.FirstOrDefault(s => s.ExpenseTypeId == _selectedExpenseType.Id);
		if (existingItem is not null)
			existingItem.Amount += _selectedExpensesCart.Amount;
		else
			_expensesCart.Add(new()
			{
				ExpenseTypeId = _selectedExpenseType.Id,
				ExpenseTypeName = _selectedExpenseType.Name,
				Amount = _selectedExpensesCart.Amount,
				Remarks = _selectedExpensesCart.Remarks
			});

		_selectedExpenseType = null;
		_selectedExpensesCart = new();

		await _sfExpenseTypeAutoComplete.FocusAsync();
		await SaveTransactionFile();
	}

	private async Task EditSelectedExpensesCartItem()
	{
		if (_sfExpensesCartGrid is null || _sfExpensesCartGrid.SelectedRecords is null || _sfExpensesCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartItem = _sfExpensesCartGrid.SelectedRecords.First();

		_selectedExpenseType = _expenseTypes.FirstOrDefault(s => s.Id == selectedCartItem.ExpenseTypeId);
		if (_selectedExpenseType is null)
			return;

		_selectedExpensesCart = new()
		{
			ExpenseTypeId = selectedCartItem.ExpenseTypeId,
			ExpenseTypeName = selectedCartItem.ExpenseTypeName,
			Amount = selectedCartItem.Amount,
			Remarks = selectedCartItem.Remarks
		};

		await _sfExpenseTypeAutoComplete.FocusAsync();
		await RemoveSelectedExpensesCartItem();
	}

	private async Task RemoveSelectedExpensesCartItem()
	{
		if (_sfExpensesCartGrid is null || _sfExpensesCartGrid.SelectedRecords is null || _sfExpensesCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartItem = _sfExpensesCartGrid.SelectedRecords.First();
		_expensesCart.Remove(selectedCartItem);
		await SaveTransactionFile();
	}
	#endregion

	#region Card Payments Cart
	private void OnCardPaymentsTypeChanged(OMCCardModel value)
	{
		if (value is null || value.Id == 0)
		{
			_selectedOMCCard = null;
			_selectedCardPaymentCart = new();
			return;
		}

		_selectedOMCCard = value;

		_selectedCardPaymentCart.OMCCardId = _selectedOMCCard.Id;
		_selectedCardPaymentCart.OMCCardNumber = _selectedOMCCard.CardNumber;
		_selectedCardPaymentCart.Amount = _trip.TotalExpense - _cardPaymentsCart.Sum(s => s.Amount) - _ledgerPaymentsCart.Sum(s => s.Amount);
	}

	private async Task AddCardPaymentsToCart()
	{
		if (_selectedOMCCard is null || _selectedOMCCard.Id <= 0 || _selectedCardPaymentCart.Amount <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		if (_cardPaymentsCart.Sum(s => s.Amount) + _ledgerPaymentsCart.Sum(s => s.Amount) + _selectedCardPaymentCart.Amount > _trip.TotalExpense)
		{
			await _toastNotification.ShowAsync("Payment Amount Exceeds Total Expense", "The total payment amount in the cart cannot exceed the total expense of the trip. Please adjust the amount accordingly.", ToastType.Error);
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

	private async Task EditSelectedCardPaymentsCartItem()
	{
		if (_sfCardPaymentsCartGrid is null || _sfCardPaymentsCartGrid.SelectedRecords is null || _sfCardPaymentsCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartItem = _sfCardPaymentsCartGrid.SelectedRecords.First();

		_selectedOMCCard = _omcCards.FirstOrDefault(s => s.Id == selectedCartItem.OMCCardId);
		if (_selectedOMCCard is null)
			return;

		_selectedCardPaymentCart = new()
		{
			OMCCardId = selectedCartItem.OMCCardId,
			OMCCardNumber = selectedCartItem.OMCCardNumber,
			Amount = selectedCartItem.Amount,
			Remarks = selectedCartItem.Remarks
		};

		await _sfOMCCardAutoComplete.FocusAsync();
		await RemoveSelectedCardPaymentsCartItem();
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
		_selectedLedgerPaymentCart.Amount = _trip.TotalExpense - _cardPaymentsCart.Sum(s => s.Amount) - _ledgerPaymentsCart.Sum(s => s.Amount);
	}

	private async Task AddLedgerPaymentsToCart()
	{
		if (_selectedLedger is null || _selectedLedger.Id <= 0 || _selectedLedgerPaymentCart.Amount <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		if (_cardPaymentsCart.Sum(s => s.Amount) + _ledgerPaymentsCart.Sum(s => s.Amount) + _selectedLedgerPaymentCart.Amount > _trip.TotalExpense)
		{
			await _toastNotification.ShowAsync("Payment Amount Exceeds Total Expense", "The total payment amount in the cart cannot exceed the total expense of the trip. Please adjust the amount accordingly.", ToastType.Error);
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

	#region Saving
	private async Task UpdateFinancialDetails()
	{
		foreach (var item in _expensesCart.ToList())
			if (item.Amount <= 0)
				_expensesCart.Remove(item);

		foreach (var item in _cardPaymentsCart.ToList())
			if (item.Amount <= 0)
				_cardPaymentsCart.Remove(item);

		foreach (var item in _ledgerPaymentsCart.ToList())
			if (item.Amount <= 0)
				_ledgerPaymentsCart.Remove(item);

		_trip.CompanyId = _selectedCompany.Id;
		_trip.OMCId = _selectedOMC.Id;
		_trip.VehicleId = _selectedVehicle.Id;
		_trip.DriverId = _selectedDriver.Id;
		_trip.RouteId = _selectedRoute.Id;
		_trip.TotalExpense = _expensesCart.Sum(s => s.Amount);
		_trip.TotalCardPaymentAmount = _cardPaymentsCart.Sum(s => s.Amount);
		_trip.TotalLedgerPaymentAmount = _ledgerPaymentsCart.Sum(s => s.Amount);

		if (_trip.TotalCardPaymentAmount + _trip.TotalLedgerPaymentAmount > _trip.TotalExpense)
		{
			_cardPaymentsCart.Clear();
			_ledgerPaymentsCart.Clear();
		}

		#region Financial Year
		_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_trip.TransactionDateTime);
		if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
			_trip.FinancialYearId = _selectedFinancialYear.Id;
		else
			await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
		#endregion

		if (Id is null)
			_trip.TransactionNo = await GenerateCodes.GenerateTripTransactionNo(_trip);

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		_trip.Status = true;
		_trip.TransactionDateTime = DateOnly.FromDateTime(_trip.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
		_trip.CreatedAt = currentDateTime;
		_trip.LastModifiedAt = currentDateTime;
		_trip.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_trip.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_trip.CreatedBy = _user.Id;
		_trip.LastModifiedBy = _user.Id;
	}

	private async Task SaveTransactionFile()
	{
		if (_isProcessing || _isLoading)
			return;

		try
		{
			_isProcessing = true;

			await UpdateFinancialDetails();

			if (_expensesCart.Count == 0 || _trip.Id > 0)
			{
				await DeleteLocalFiles();
				return;
			}

			await DataStorageService.LocalSaveAsync(StorageFileNames.TripDataFileName, JsonSerializer.Serialize(_trip));
			await DataStorageService.LocalSaveAsync(StorageFileNames.TripExpensesCartDataFileName, JsonSerializer.Serialize(_expensesCart));
			await DataStorageService.LocalSaveAsync(StorageFileNames.TripCardPaymentsCartDataFileName, JsonSerializer.Serialize(_cardPaymentsCart));
			await DataStorageService.LocalSaveAsync(StorageFileNames.TripLedgerPaymentsCartDataFileName, JsonSerializer.Serialize(_ledgerPaymentsCart));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Saving Transaction Data", ex.Message, ToastType.Error);
		}
		finally
		{
			if (_sfExpensesCartGrid is not null)
				await _sfExpensesCartGrid.Refresh();

			if (_sfCardPaymentsCartGrid is not null)
				await _sfCardPaymentsCartGrid.Refresh();

			if (_sfLedgerPaymentsCartGrid is not null)
				await _sfLedgerPaymentsCartGrid.Refresh();

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
			StateHasChanged();

			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			var expenses = TripData.ConvertExpensesCartToDetails(_expensesCart, _trip.Id);
			var cardPayments = TripData.ConvertCardPaymentCartToDetails(_cardPaymentsCart, _trip.Id);
			var ledgerPayments = TripData.ConvertLedgerPaymentCartToDetails(_ledgerPaymentsCart, _trip.Id);
			_trip.Id = await TripData.SaveTransaction(_trip, expenses, cardPayments, ledgerPayments);
			_trip = await CommonData.LoadTableDataById<TripModel>(FleetNames.Trip, _trip.Id);

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
		if (_trip.Id <= 0 || (_isProcessing && !force))
			return;

		try
		{
			_isProcessing = true;
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var decodeTransactionNo = await DecodeCode.DecodeTransactionNo(_trip.TransactionNo, !isExcel, isExcel, CodeType.Trip);
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
	private async Task OnExpensesCartGridContextMenuItemClicked(ContextMenuClickEventArgs<TripExpensesCartModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditCart": await EditSelectedExpensesCartItem(); break;
			case "DeleteCart": await RemoveSelectedExpensesCartItem(); break;
		}
	}

	private async Task OnCardPaymentsCartGridContextMenuItemClicked(ContextMenuClickEventArgs<TripCardPaymentsCartModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditCart": await EditSelectedCardPaymentsCartItem(); break;
			case "DeleteCart": await RemoveSelectedCardPaymentsCartItem(); break;
		}
	}

	private async Task OnLedgerPaymentsCartGridContextMenuItemClicked(ContextMenuClickEventArgs<TripLedgerPaymentsCartModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditCart": await EditSelectedLedgerPaymentsCartItem(); break;
			case "DeleteCart": await RemoveSelectedLedgerPaymentsCartItem(); break;
		}
	}

	private async Task DeleteLocalFiles()
	{
		await DataStorageService.LocalRemove(StorageFileNames.TripDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.TripExpensesCartDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.TripCardPaymentsCartDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.TripLedgerPaymentsCartDataFileName);
	}

	private async Task ResetPage()
	{
		await DeleteLocalFiles();
		PageRefresh.Request();
	}
	#endregion
}
