using Microsoft.AspNetCore.Components;
using Strada.Shared.Components.Dialog;
using Strada.Shared.Components.Input;
using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Operations;
using StradaLibrary.Data.VehicleTrip.TripAdvance;
using StradaLibrary.Data.VehicleTrip.VehicleRoute;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Exports.VehicleTrip.TripAdvance;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Operations;
using StradaLibrary.Models.VehicleExpense;
using StradaLibrary.Models.VehicleTrip.OMC;
using StradaLibrary.Models.VehicleTrip.TripAdvance;
using StradaLibrary.Models.VehicleTrip.VehicleRoute;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.VehicleTrip.TripAdvance;

public partial class TripAdvancePage
{
	[Parameter] public int? Id { get; set; }

	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;

	private CompanyModel _selectedCompany = new();
	private FinancialYearModel _selectedFinancialYear = new();
	private OMCModel _selectedOMC = new();
	private VehicleModel _selectedVehicle = new();
	private VehicleDriverOverviewModel _selectedDriver = new();
	private VehicleRouteOverviewModel _selectedRoute = new();
	private VehicleExpenseTypeModel _selectedExpenseType = null;
	private OMCCardModel _selectedOMCCard = null;
	private TripAdvanceExpensesCartModel _selectedExpensesCart = new();
	private TripAdvanceCardPaymentsCartModel _selectedPaymentCart = new();
	private TripAdvanceModel _tripAdvance = new();

	private List<CompanyModel> _companies = [];
	private List<OMCModel> _omcs = [];
	private List<OMCCardModel> _omcCards = [];
	private List<VehicleModel> _vehicles = [];
	private List<VehicleDriverOverviewModel> _vehicleDrivers = [];
	private List<VehicleRouteOverviewModel> _vehicleRoutes = [];
	private List<VehicleExpenseTypeModel> _expenseTypes = [];
	private List<TripAdvanceExpensesCartModel> _expensesCart = [];
	private List<TripAdvanceCardPaymentsCartModel> _paymentsCart = [];

	private AutoCompleteWithAdd<VehicleExpenseTypeModel?, VehicleExpenseTypeModel> _sfExpenseTypeAutoComplete;
	private AutoCompleteWithAdd<OMCCardModel?, OMCCardModel> _sfOMCCardAutoComplete;
	private SfGrid<TripAdvanceExpensesCartModel> _sfExpensesCartGrid;
	private SfGrid<TripAdvanceCardPaymentsCartModel> _sfPaymentsCartGrid;
	private ToastNotification _toastNotification;

	private readonly List<ContextMenuItemModel> _expensesCartGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditCart", IconCss = "e-icons e-edit" },
		new() { Text = "Delete (Del)", Id = "DeleteCart", IconCss = "e-icons e-delete" }
	];

	private readonly List<ContextMenuItemModel> _paymentsCartGridContextMenuItems =
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
		catch
		{
			await ResetPage();
		}
	}

	private async Task InitializePage()
	{
		await LoadData();
		await ResolveTransaction();
		await LoadSelections();
		await ResolveExpensesCart();
		await ResolvePaymentsCart();

		_isLoading = false;
		StateHasChanged();

		await SaveTransactionFile();
	}

	private async Task LoadData()
	{
		_companies = await CommonData.LoadTableDataByStatus<CompanyModel>(AccountNames.Company);
		_omcs = await CommonData.LoadTableDataByStatus<OMCModel>(VehicleTripNames.OMC);
		_omcCards = await CommonData.LoadTableDataByStatus<OMCCardModel>(VehicleTripNames.OMCCard);
		_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
		_vehicleDrivers = await VehicleDriverData.LoadVehicleDriverOverview();
		_vehicleRoutes = await VehicleRouteData.LoadVehicleRouteOverview();
		_expenseTypes = await CommonData.LoadTableDataByStatus<VehicleExpenseTypeModel>(VehicleExpenseNames.VehicleExpenseType);

		_companies = [.. _companies.OrderBy(s => s.Name)];
		var mainCompanyId = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
		_selectedCompany = _companies.FirstOrDefault(s => s.Id.ToString() == mainCompanyId.Value) ?? _companies.FirstOrDefault();

		_omcs = [.. _omcs.OrderBy(s => s.Name)];
		_omcCards = [.. _omcCards.OrderBy(s => s.CardNumber)];
		_vehicles = [.. _vehicles.Where(s => s.CompanyId == _selectedCompany.Id)];
		_vehicles = [.. _vehicles.OrderBy(s => s.ShortCode)];
		_vehicleDrivers = [.. _vehicleDrivers.OrderBy(s => s.Name)];
		_vehicleRoutes = [.. _vehicleRoutes.OrderBy(s => s.Code)];
		_expenseTypes = [.. _expenseTypes.OrderBy(s => s.Name)];

		_selectedOMC = _omcs.FirstOrDefault();
		_selectedVehicle = _vehicles.FirstOrDefault();
		_selectedDriver = _vehicleDrivers.FirstOrDefault();
		_selectedRoute = _vehicleRoutes.FirstOrDefault();
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

		_tripAdvance = await CommonData.LoadTableDataById<TripAdvanceModel>(VehicleTripNames.TripAdvance, Id.Value);
		if (_tripAdvance is null || _tripAdvance.Id == 0)
		{
			await _toastNotification.ShowAsync("Transaction Not Found", "The requested transaction could not be found.", ToastType.Error);
			NavigationManager.NavigateTo(PageRouteNames.TripAdvance, true);
			return false;
		}

		return true;
	}

	private async Task<bool> TryRestoreFromLocalStorage()
	{
		if (!await DataStorageService.LocalExists(StorageFileNames.TripAdvanceExpensesCartDataFileName))
			return false;

		try
		{
			_tripAdvance = System.Text.Json.JsonSerializer.Deserialize<TripAdvanceModel>(await DataStorageService.LocalGetAsync(StorageFileNames.TripAdvanceDataFileName));
			if (_tripAdvance is null)
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

		_tripAdvance = new()
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

		await DeleteLocalFiles();
	}

	private async Task LoadSelections()
	{
		if (_tripAdvance.CompanyId > 0)
			_selectedCompany = _companies.FirstOrDefault(s => s.Id == _tripAdvance.CompanyId) ?? _companies.FirstOrDefault();
		else
		{
			var mainCompanyId = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
			_selectedCompany = _companies.FirstOrDefault(s => s.Id.ToString() == mainCompanyId.Value) ?? _companies.FirstOrDefault();
		}
		_tripAdvance.CompanyId = _selectedCompany.Id;

		if (_tripAdvance.OMCId > 0)
			_selectedOMC = _omcs.FirstOrDefault(s => s.Id == _tripAdvance.OMCId) ?? _omcs.FirstOrDefault();
		else
			_selectedOMC = _omcs.FirstOrDefault();
		_tripAdvance.OMCId = _selectedOMC.Id;

		if (_tripAdvance.DriverId > 0)
			_selectedDriver = _vehicleDrivers.FirstOrDefault(s => s.Id == _tripAdvance.DriverId) ?? _vehicleDrivers.FirstOrDefault();
		else
			_selectedDriver = _vehicleDrivers.FirstOrDefault();

		if (_tripAdvance.RouteId > 0)
			_selectedRoute = _vehicleRoutes.FirstOrDefault(s => s.Id == _tripAdvance.RouteId) ?? _vehicleRoutes.FirstOrDefault();
		else
			_selectedRoute = _vehicleRoutes.FirstOrDefault();

		if (_tripAdvance.FinancialYearId > 0)
			_selectedFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, _tripAdvance.FinancialYearId);

		if (_selectedFinancialYear is null || _selectedFinancialYear.Id <= 0)
			_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_tripAdvance.TransactionDateTime);

		if (_selectedFinancialYear is not null)
			_tripAdvance.FinancialYearId = _selectedFinancialYear.Id;

		_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
		_vehicles = [.. _vehicles.Where(s => s.CompanyId == _selectedCompany.Id)];
		_vehicles = [.. _vehicles.OrderBy(s => s.ShortCode)];

		if (_tripAdvance.VehicleId > 0)
			_selectedVehicle = _vehicles.FirstOrDefault(s => s.Id == _tripAdvance.VehicleId) ?? _vehicles.FirstOrDefault();
		else
			_selectedVehicle = _vehicles.FirstOrDefault();
	}

	private async Task ResolveExpensesCart()
	{
		try
		{
			_expensesCart.Clear();

			if (await LoadExistingExpensesCart())
				return;

			if (await DataStorageService.LocalExists(StorageFileNames.TripAdvanceExpensesCartDataFileName))
				_expensesCart = System.Text.Json.JsonSerializer.Deserialize<List<TripAdvanceExpensesCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.TripAdvanceExpensesCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Expenses Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingExpensesCart()
	{
		if (_tripAdvance.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<TripAdvanceExpensesModel>(VehicleTripNames.TripAdvanceExpenses, _tripAdvance.Id);

		foreach (var item in existingCart)
		{
			if (_expenseTypes.FirstOrDefault(s => s.Id == item.VehicleExpenseTypeId) is null)
			{
				var expenseType = await CommonData.LoadTableDataById<VehicleExpenseTypeModel>(VehicleExpenseNames.VehicleExpenseType, item.VehicleExpenseTypeId);
				await _toastNotification.ShowAsync("Vehicle Expense Type Not Found", $"The vehicle expense type {expenseType?.Name} (ID: {item.VehicleExpenseTypeId}) in the existing transaction cart was not found in the available expense types list. It may have been deleted or is inaccessible.", ToastType.Error);
				continue;
			}

			_expensesCart.Add(new()
			{
				VehicleExpenseTypeId = item.VehicleExpenseTypeId,
				VehicleExpenseTypeName = _expenseTypes.First(s => s.Id == item.VehicleExpenseTypeId).Name,
				Amount = item.Amount,
				Remarks = item.Remarks
			});
		}

		return true;
	}

	private async Task ResolvePaymentsCart()
	{
		try
		{
			_paymentsCart.Clear();

			if (await LoadExistingPaymentsCart())
				return;

			if (await DataStorageService.LocalExists(StorageFileNames.TripAdvancePaymentsCartDataFileName))
				_paymentsCart = System.Text.Json.JsonSerializer.Deserialize<List<TripAdvanceCardPaymentsCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.TripAdvancePaymentsCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Payments Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingPaymentsCart()
	{
		if (_tripAdvance.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<TripAdvanceCardPaymentsModel>(VehicleTripNames.TripAdvanceCardPayments, _tripAdvance.Id);

		foreach (var item in existingCart)
		{
			if (_omcCards.FirstOrDefault(s => s.Id == item.OMCCardId) is null)
			{
				var omcCard = await CommonData.LoadTableDataById<OMCCardModel>(VehicleTripNames.OMCCard, item.OMCCardId);
				await _toastNotification.ShowAsync("OMC Card Not Found", $"The OMC card {omcCard?.CardNumber} (ID: {item.OMCCardId}) in the existing transaction cart was not found in the available cards list. It may have been deleted or is inaccessible.", ToastType.Error);
				continue;
			}

			_paymentsCart.Add(new()
			{
				OMCCardId = item.OMCCardId,
				OMCCardNumber = _omcCards.First(s => s.Id == item.OMCCardId).CardNumber,
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
		_tripAdvance.CompanyId = _selectedCompany.Id;

		_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
		_vehicles = [.. _vehicles.Where(s => s.CompanyId == _selectedCompany.Id)];
		_selectedVehicle = _vehicles.FirstOrDefault();
		_tripAdvance.VehicleId = _selectedVehicle.Id;

		await SaveTransactionFile();
	}

	private async Task OnOMCChanged(ChangeEventArgs<OMCModel, OMCModel> args)
	{
		if (args.Value is null || args.Value.Id == 0)
			return;

		_selectedOMC = args.Value;
		_tripAdvance.OMCId = _selectedOMC.Id;

		await SaveTransactionFile();
	}

	private async Task OnVehicleChanged(ChangeEventArgs<VehicleModel, VehicleModel> args)
	{
		if (args.Value is null || args.Value.Id == 0)
			return;

		_selectedVehicle = args.Value;
		_tripAdvance.VehicleId = _selectedVehicle.Id;

		await SaveTransactionFile();
	}

	private async Task OnVehicleDriverChanged(ChangeEventArgs<VehicleDriverOverviewModel, VehicleDriverOverviewModel> args)
	{
		if (args.Value is null || args.Value.Id == 0)
			return;

		_selectedDriver = args.Value;
		_tripAdvance.DriverId = _selectedDriver.Id;

		await SaveTransactionFile();
	}

	private async Task OnVehicleRouteChanged(ChangeEventArgs<VehicleRouteOverviewModel, VehicleRouteOverviewModel> args)
	{
		if (args.Value is null || args.Value.Id == 0)
			return;

		_selectedRoute = args.Value;
		_tripAdvance.RouteId = _selectedRoute.Id;

		await SaveTransactionFile();
	}
	#endregion

	#region Expenses Cart
	private async Task OnExpensesTypeChanged(ChangeEventArgs<VehicleExpenseTypeModel?, VehicleExpenseTypeModel?> args)
	{
		if (args.Value is null || args.Value.Id == 0)
			return;

		_selectedExpenseType = args.Value;

		if (_selectedExpenseType is null)
			_selectedExpensesCart = new()
			{
				VehicleExpenseTypeId = 0,
				VehicleExpenseTypeName = "",
				Amount = 0
			};

		else
		{
			_selectedExpensesCart.VehicleExpenseTypeId = _selectedExpenseType.Id;
			_selectedExpensesCart.VehicleExpenseTypeName = _selectedExpenseType.Name;
		}
	}

	private async Task AddExpensesToCart()
	{
		if (_selectedExpenseType is null || _selectedExpenseType.Id <= 0 || _selectedExpensesCart.Amount <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		var existingItem = _expensesCart.FirstOrDefault(s => s.VehicleExpenseTypeId == _selectedExpenseType.Id);
		if (existingItem is not null)
			existingItem.Amount += _selectedExpensesCart.Amount;
		else
			_expensesCart.Add(new()
			{
				VehicleExpenseTypeId = _selectedExpenseType.Id,
				VehicleExpenseTypeName = _selectedExpenseType.Name,
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

		_selectedExpenseType = _expenseTypes.FirstOrDefault(s => s.Id == selectedCartItem.VehicleExpenseTypeId);
		if (_selectedExpenseType is null)
			return;

		_selectedExpensesCart = new()
		{
			VehicleExpenseTypeId = selectedCartItem.VehicleExpenseTypeId,
			VehicleExpenseTypeName = selectedCartItem.VehicleExpenseTypeName,
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

	#region Payments Cart
	private async Task OnPaymentsTypeChanged(ChangeEventArgs<OMCCardModel?, OMCCardModel?> args)
	{
		if (args.Value is null || args.Value.Id == 0)
			return;

		_selectedOMCCard = args.Value;

		if (_selectedOMCCard is null)
			_selectedPaymentCart = new()
			{
				OMCCardId = 0,
				OMCCardNumber = "",
				Amount = 0
			};

		else
		{
			_selectedPaymentCart.OMCCardId = _selectedOMCCard.Id;
			_selectedPaymentCart.OMCCardNumber = _selectedOMCCard.CardNumber;
			_selectedPaymentCart.Amount = _tripAdvance.TotalExpense - _paymentsCart.Sum(s => s.Amount);
		}
	}

	private async Task AddPaymentsToCart()
	{
		if (_selectedOMCCard is null || _selectedOMCCard.Id <= 0 || _selectedPaymentCart.Amount <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		if (_paymentsCart.Sum(s => s.Amount) + _selectedPaymentCart.Amount > _tripAdvance.TotalExpense)
		{
			await _toastNotification.ShowAsync("Payment Amount Exceeds Total Expense", "The total payment amount in the cart cannot exceed the total expense of the trip. Please adjust the amount accordingly.", ToastType.Error);
			return;
		}

		var existingItem = _paymentsCart.FirstOrDefault(s => s.OMCCardId == _selectedOMCCard.Id);
		if (existingItem is not null)
			existingItem.Amount += _selectedPaymentCart.Amount;
		else
			_paymentsCart.Add(new()
			{
				OMCCardId = _selectedOMCCard.Id,
				OMCCardNumber = _selectedOMCCard.CardNumber,
				Amount = _selectedPaymentCart.Amount,
				Remarks = _selectedPaymentCart.Remarks
			});

		_selectedOMCCard = null;
		_selectedPaymentCart = new();
		await _sfOMCCardAutoComplete.FocusAsync();
		await SaveTransactionFile();
	}

	private async Task EditSelectedPaymentsCartItem()
	{
		if (_sfPaymentsCartGrid is null || _sfPaymentsCartGrid.SelectedRecords is null || _sfPaymentsCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartItem = _sfPaymentsCartGrid.SelectedRecords.First();

		_selectedOMCCard = _omcCards.FirstOrDefault(s => s.Id == selectedCartItem.OMCCardId);
		if (_selectedOMCCard is null)
			return;

		_selectedPaymentCart = new()
		{
			OMCCardId = selectedCartItem.OMCCardId,
			OMCCardNumber = selectedCartItem.OMCCardNumber,
			Amount = selectedCartItem.Amount,
			Remarks = selectedCartItem.Remarks
		};

		await _sfOMCCardAutoComplete.FocusAsync();
		await RemoveSelectedPaymentsCartItem();
	}

	private async Task RemoveSelectedPaymentsCartItem()
	{
		if (_sfPaymentsCartGrid is null || _sfPaymentsCartGrid.SelectedRecords is null || _sfPaymentsCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartItem = _sfPaymentsCartGrid.SelectedRecords.First();
		_paymentsCart.Remove(selectedCartItem);
		await SaveTransactionFile();
	}
	#endregion

	#region Saving
	private async Task UpdateFinancialDetails()
	{
		foreach (var item in _expensesCart)
		{
			if (item.Amount <= 0)
				_expensesCart.Remove(item);

			item.Remarks = item.Remarks?.Trim();
			if (string.IsNullOrWhiteSpace(item.Remarks))
				item.Remarks = null;
		}

		foreach (var item in _paymentsCart)
		{
			if (item.Amount <= 0)
				_paymentsCart.Remove(item);

			item.Remarks = item.Remarks?.Trim();
			if (string.IsNullOrWhiteSpace(item.Remarks))
				item.Remarks = null;
		}

		_tripAdvance.Remarks = _tripAdvance.Remarks?.Trim();
		if (string.IsNullOrWhiteSpace(_tripAdvance.Remarks))
			_tripAdvance.Remarks = null;

		_tripAdvance.CompanyId = _selectedCompany.Id;
		_tripAdvance.OMCId = _selectedOMC.Id;
		_tripAdvance.VehicleId = _selectedVehicle.Id;
		_tripAdvance.DriverId = _selectedDriver.Id;
		_tripAdvance.RouteId = _selectedRoute.Id;
		_tripAdvance.TotalExpense = _expensesCart.Sum(s => s.Amount);

		if (_paymentsCart.Sum(s => s.Amount) > _tripAdvance.TotalExpense)
			_paymentsCart.Clear();

		#region Financial Year
		_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_tripAdvance.TransactionDateTime);
		if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
			_tripAdvance.FinancialYearId = _selectedFinancialYear.Id;
		else
		{
			await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
			_tripAdvance.TransactionDateTime = await CommonData.LoadCurrentDateTime();
			_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_tripAdvance.TransactionDateTime);
			_tripAdvance.FinancialYearId = _selectedFinancialYear.Id;
		}
		#endregion

		if (Id is null)
			_tripAdvance.TransactionNo = await GenerateCodes.GenerateTripAdvanceTransactionNo(_tripAdvance);

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		_tripAdvance.Status = true;
		_tripAdvance.TransactionDateTime = DateOnly.FromDateTime(_tripAdvance.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
		_tripAdvance.LastModifiedAt = currentDateTime;
		_tripAdvance.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_tripAdvance.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_tripAdvance.CreatedBy = _user.Id;
		_tripAdvance.LastModifiedBy = _user.Id;
	}

	private async Task SaveTransactionFile()
	{
		if (_isProcessing || _isLoading)
			return;

		try
		{
			_isProcessing = true;

			await UpdateFinancialDetails();

			await DataStorageService.LocalSaveAsync(StorageFileNames.TripAdvanceDataFileName, System.Text.Json.JsonSerializer.Serialize(_tripAdvance));
			await DataStorageService.LocalSaveAsync(StorageFileNames.TripAdvanceExpensesCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_expensesCart));
			await DataStorageService.LocalSaveAsync(StorageFileNames.TripAdvancePaymentsCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_paymentsCart));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Saving Transaction Data", ex.Message, ToastType.Error);
		}
		finally
		{
			if (_sfExpensesCartGrid is not null)
				await _sfExpensesCartGrid.Refresh();

			if (_sfPaymentsCartGrid is not null)
				await _sfPaymentsCartGrid.Refresh();

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

			var expenses = TripAdvanceData.ConvertExpensesCartToDetails(_expensesCart, _tripAdvance.Id);
			var payments = TripAdvanceData.ConvertPaymentCartToDetails(_paymentsCart, _tripAdvance.Id);
			_tripAdvance.Id = await TripAdvanceData.SaveTransaction(_tripAdvance, expenses, payments);

			if (savePDF)
			{
				var (pdfStream, pdfFileName) = await TripAdvanceInvoiceExport.ExportInvoice(_tripAdvance.Id, InvoiceExportType.PDF);
				await SaveAndViewService.SaveAndView(pdfFileName, pdfStream);
			}

			if (saveExcel)
			{
				var (excelStream, excelFileName) = await TripAdvanceInvoiceExport.ExportInvoice(_tripAdvance.Id, InvoiceExportType.Excel);
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

			var decodeTransactionNo = await GenerateCodes.DecodeTransactionNo(_tripAdvance.TransactionNo);
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

			var decodeTransactionNo = await GenerateCodes.DecodeTransactionNo(_tripAdvance.TransactionNo);
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
				await AuthenticationService.NavigateToRoute(PageRouteNames.TripAdvanceReport, FormFactor, JSRuntime, NavigationManager);
				break;
			case "ExpensesReport":
				await AuthenticationService.NavigateToRoute(PageRouteNames.TripAdvanceExpensesReport, FormFactor, JSRuntime, NavigationManager);
				break;
			case "PaymentsReport":
				await AuthenticationService.NavigateToRoute(PageRouteNames.TripAdvancePaymentsReport, FormFactor, JSRuntime, NavigationManager);
				break;
		}
	}

	private async Task OnExpensesCartGridContextMenuItemClicked(ContextMenuClickEventArgs<TripAdvanceExpensesCartModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditCart":
				await EditSelectedExpensesCartItem();
				break;
			case "DeleteCart":
				await RemoveSelectedExpensesCartItem();
				break;
		}
	}

	private async Task OnPaymentsCartGridContextMenuItemClicked(ContextMenuClickEventArgs<TripAdvanceCardPaymentsCartModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditCart":
				await EditSelectedPaymentsCartItem();
				break;
			case "DeleteCart":
				await RemoveSelectedPaymentsCartItem();
				break;
		}
	}

	private async Task DeleteLocalFiles()
	{
		await DataStorageService.LocalRemove(StorageFileNames.TripAdvanceDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.TripAdvanceExpensesCartDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.TripAdvancePaymentsCartDataFileName);
	}

	private async Task ResetPage()
	{
		await DeleteLocalFiles();
		NavigationManager.NavigateTo(PageRouteNames.TripAdvance, true);
	}

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard, true);
	#endregion
}