using Microsoft.AspNetCore.Components;
using Strada.Shared.Components.Dialog;
using Strada.Shared.Components.Input;
using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Fleet.VehicleExpense;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.VehicleExpense;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleExpense;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.VehicleExpense;

public partial class VehicleExpensePage
{
	[Parameter] public int? Id { get; set; }

	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;

	private CompanyModel _selectedCompany = new();
	private FinancialYearModel _selectedFinancialYear = new();
	private VehicleModel _selectedVehicle = new();
	private VehicleExpenseTypeModel _selectedExpenseType = null;
	private LedgerModel? _selectedLedger = null;
	private VehicleExpenseDetailsCartModel _selectedExpensesCart = new();
	private VehicleExpenseModel _vehicleExpense = new();

	private List<CompanyModel> _companies = [];
	private List<VehicleModel> _vehicles = [];
	private List<VehicleExpenseTypeModel> _expenseTypes = [];
	private List<LedgerModel> _ledgers = [];
	private List<VehicleExpenseDetailsCartModel> _expensesCart = [];

	private AutoCompleteWithAdd<VehicleExpenseTypeModel?, VehicleExpenseTypeModel> _sfExpenseTypeAutoComplete;
	private SfGrid<VehicleExpenseDetailsCartModel> _sfExpensesCartGrid;
	private ToastNotification _toastNotification;

	private readonly List<ContextMenuItemModel> _expensesCartGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditCart", IconCss = "e-icons e-edit" },
		new() { Text = "Delete (Del)", Id = "DeleteCart", IconCss = "e-icons e-delete" }
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

		_isLoading = false;
		StateHasChanged();

		await SaveTransactionFile();
	}

	private async Task LoadData()
	{
		_companies = await CommonData.LoadTableDataByStatus<CompanyModel>(AccountNames.Company);
		_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
		_expenseTypes = await CommonData.LoadTableDataByStatus<VehicleExpenseTypeModel>(FleetNames.VehicleExpenseType);
		_ledgers = await CommonData.LoadTableDataByStatus<LedgerModel>(AccountNames.Ledger);

		_companies = [.. _companies.OrderBy(s => s.Name)];
		var mainCompanyId = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
		_selectedCompany = _companies.FirstOrDefault(s => s.Id.ToString() == mainCompanyId.Value) ?? _companies.FirstOrDefault();

		_vehicles = [.. _vehicles.Where(s => s.CompanyId == _selectedCompany.Id)];
		_vehicles = [.. _vehicles.OrderBy(s => s.ShortCode)];
		_expenseTypes = [.. _expenseTypes.OrderBy(s => s.Name)];
		_ledgers = [.. _ledgers.OrderBy(s => s.Name)];

		_selectedVehicle = _vehicles.FirstOrDefault();
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

		_vehicleExpense = await CommonData.LoadTableDataById<VehicleExpenseModel>(FleetNames.VehicleExpense, Id.Value);
		if (_vehicleExpense is null || _vehicleExpense.Id == 0)
		{
			await _toastNotification.ShowAsync("Transaction Not Found", "The requested transaction could not be found.", ToastType.Error);
			NavigationManager.NavigateTo(PageRouteNames.VehicleExpense, true);
			return false;
		}

		return true;
	}

	private async Task<bool> TryRestoreFromLocalStorage()
	{
		if (!await DataStorageService.LocalExists(StorageFileNames.VehicleExpenseDetailsCartDataFileName))
			return false;

		try
		{
			_vehicleExpense = System.Text.Json.JsonSerializer.Deserialize<VehicleExpenseModel>(await DataStorageService.LocalGetAsync(StorageFileNames.VehicleExpenseDataFileName));
			if (_vehicleExpense is null)
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

		_vehicleExpense = new()
		{
			Id = 0,
			TransactionNo = string.Empty,
			CompanyId = _selectedCompany.Id,
			TransactionDateTime = currentDateTime,
			FinancialYearId = financialYear is null ? 0 : financialYear.Id,
			VehicleId = _selectedVehicle.Id,
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
		if (_vehicleExpense.CompanyId > 0)
			_selectedCompany = _companies.FirstOrDefault(s => s.Id == _vehicleExpense.CompanyId) ?? _companies.FirstOrDefault();
		else
		{
			var mainCompanyId = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
			_selectedCompany = _companies.FirstOrDefault(s => s.Id.ToString() == mainCompanyId.Value) ?? _companies.FirstOrDefault();
		}
		_vehicleExpense.CompanyId = _selectedCompany.Id;

		if (_vehicleExpense.FinancialYearId > 0)
			_selectedFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, _vehicleExpense.FinancialYearId);

		if (_selectedFinancialYear is null || _selectedFinancialYear.Id <= 0)
			_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_vehicleExpense.TransactionDateTime);

		if (_selectedFinancialYear is not null)
			_vehicleExpense.FinancialYearId = _selectedFinancialYear.Id;

		_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
		_vehicles = [.. _vehicles.Where(s => s.CompanyId == _selectedCompany.Id)];
		_vehicles = [.. _vehicles.OrderBy(s => s.ShortCode)];

		if (_vehicleExpense.VehicleId > 0)
			_selectedVehicle = _vehicles.FirstOrDefault(s => s.Id == _vehicleExpense.VehicleId) ?? _vehicles.FirstOrDefault();
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

			if (await DataStorageService.LocalExists(StorageFileNames.VehicleExpenseDetailsCartDataFileName))
				_expensesCart = System.Text.Json.JsonSerializer.Deserialize<List<VehicleExpenseDetailsCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.VehicleExpenseDetailsCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Expenses Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingExpensesCart()
	{
		if (_vehicleExpense.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<VehicleExpenseDetailsModel>(FleetNames.VehicleExpenseDetails, _vehicleExpense.Id);

		foreach (var item in existingCart)
		{
			if (_expenseTypes.FirstOrDefault(s => s.Id == item.VehicleExpenseTypeId) is null)
			{
				var expenseType = await CommonData.LoadTableDataById<VehicleExpenseTypeModel>(FleetNames.VehicleExpenseType, item.VehicleExpenseTypeId);
				await _toastNotification.ShowAsync("Vehicle Expense Type Not Found", $"The vehicle expense type {expenseType?.Name} (ID: {item.VehicleExpenseTypeId}) in the existing transaction cart was not found in the available expense types list. It may have been deleted or is inaccessible.", ToastType.Error);
				continue;
			}

			_expensesCart.Add(new()
			{
				VehicleExpenseTypeId = item.VehicleExpenseTypeId,
				VehicleExpenseTypeName = _expenseTypes.First(s => s.Id == item.VehicleExpenseTypeId).Name,
				LedgerId = item.LedgerId,
				LedgerName = _ledgers.FirstOrDefault(s => s.Id == item.LedgerId)?.Name,
				Amount = item.Amount,
				IdentificationNo = item.IdentificationNo,
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
		_vehicleExpense.CompanyId = _selectedCompany.Id;

		_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
		_vehicles = [.. _vehicles.Where(s => s.CompanyId == _selectedCompany.Id)];
		_selectedVehicle = _vehicles.FirstOrDefault();
		_vehicleExpense.VehicleId = _selectedVehicle.Id;

		await SaveTransactionFile();
	}

	private async Task OnVehicleChanged(ChangeEventArgs<VehicleModel, VehicleModel> args)
	{
		if (args.Value is null || args.Value.Id == 0)
			return;

		_selectedVehicle = args.Value;
		_vehicleExpense.VehicleId = _selectedVehicle.Id;

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
				LedgerId = 0,
				LedgerName = "",
				IdentificationNo = "",
				Amount = 0
			};

		else
		{
			_selectedExpensesCart.VehicleExpenseTypeId = _selectedExpenseType.Id;
			_selectedExpensesCart.VehicleExpenseTypeName = _selectedExpenseType.Name;
			_selectedExpensesCart.LedgerId = _selectedLedger?.Id;
			_selectedExpensesCart.LedgerName = _selectedLedger?.Name;
		}
	}

	private async Task OnLedgerChanged(ChangeEventArgs<LedgerModel?, LedgerModel?> args)
	{
		if (args.Value is null || args.Value.Id == 0)
		{
			_selectedLedger = null;
			_selectedExpensesCart.LedgerId = null;
			_selectedExpensesCart.LedgerName = null;
		}

		else
		{
			_selectedLedger = args.Value;
			_selectedExpensesCart.LedgerId = _selectedLedger.Id;
			_selectedExpensesCart.LedgerName = _selectedLedger.Name;
		}

		await SaveTransactionFile();
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
				LedgerId = _selectedLedger?.Id,
				LedgerName = _selectedLedger?.Name,
				Amount = _selectedExpensesCart.Amount,
				IdentificationNo = _selectedExpensesCart.IdentificationNo,
				Remarks = _selectedExpensesCart.Remarks
			});

		_selectedExpenseType = null;
		_selectedLedger = null;
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

		_selectedLedger = _ledgers.FirstOrDefault(s => s.Id == selectedCartItem.LedgerId);

		_selectedExpensesCart = new()
		{
			VehicleExpenseTypeId = selectedCartItem.VehicleExpenseTypeId,
			VehicleExpenseTypeName = selectedCartItem.VehicleExpenseTypeName,
			LedgerId = selectedCartItem.LedgerId,
			LedgerName = selectedCartItem.LedgerName,
			Amount = selectedCartItem.Amount,
			IdentificationNo = selectedCartItem.IdentificationNo,
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

	#region Saving
	private async Task UpdateFinancialDetails()
	{
		foreach (var item in _expensesCart.ToList())
		{
			if (item.Amount <= 0)
				_expensesCart.Remove(item);

			item.IdentificationNo = item.IdentificationNo?.Trim();
			if (string.IsNullOrWhiteSpace(item.IdentificationNo))
				item.IdentificationNo = null;

			item.Remarks = item.Remarks?.Trim();
			if (string.IsNullOrWhiteSpace(item.Remarks))
				item.Remarks = null;
		}

		_vehicleExpense.Remarks = _vehicleExpense.Remarks?.Trim();
		if (string.IsNullOrWhiteSpace(_vehicleExpense.Remarks))
			_vehicleExpense.Remarks = null;

		_vehicleExpense.CompanyId = _selectedCompany.Id;
		_vehicleExpense.VehicleId = _selectedVehicle.Id;
		_vehicleExpense.TotalExpense = _expensesCart.Sum(s => s.Amount);

		#region Financial Year
		_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_vehicleExpense.TransactionDateTime);
		if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
			_vehicleExpense.FinancialYearId = _selectedFinancialYear.Id;
		else
		{
			await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
			_vehicleExpense.TransactionDateTime = await CommonData.LoadCurrentDateTime();
			_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_vehicleExpense.TransactionDateTime);
			_vehicleExpense.FinancialYearId = _selectedFinancialYear.Id;
		}
		#endregion

		if (Id is null)
			_vehicleExpense.TransactionNo = await GenerateCodes.GenerateVehicleExpenseTransactionNo(_vehicleExpense);

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		_vehicleExpense.Status = true;
		_vehicleExpense.TransactionDateTime = DateOnly.FromDateTime(_vehicleExpense.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
		_vehicleExpense.LastModifiedAt = currentDateTime;
		_vehicleExpense.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_vehicleExpense.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_vehicleExpense.CreatedBy = _user.Id;
		_vehicleExpense.LastModifiedBy = _user.Id;
	}

	private async Task SaveTransactionFile()
	{
		if (_isProcessing || _isLoading)
			return;

		try
		{
			_isProcessing = true;

			await UpdateFinancialDetails();

			if (_expensesCart.Count == 0 || _vehicleExpense.Id > 0)
			{
				await DeleteLocalFiles();
				return;
			}

			await DataStorageService.LocalSaveAsync(StorageFileNames.VehicleExpenseDataFileName, System.Text.Json.JsonSerializer.Serialize(_vehicleExpense));
			await DataStorageService.LocalSaveAsync(StorageFileNames.VehicleExpenseDetailsCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_expensesCart));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Saving Transaction Data", ex.Message, ToastType.Error);
		}
		finally
		{
			if (_sfExpensesCartGrid is not null)
				await _sfExpensesCartGrid.Refresh();

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

			var expenses = VehicleExpenseData.ConvertExpensesCartToDetails(_expensesCart, _vehicleExpense.Id);
			_vehicleExpense.Id = await VehicleExpenseData.SaveTransaction(_vehicleExpense, expenses);

			if (savePDF)
			{
				var (pdfStream, pdfFileName) = await VehicleExpenseInvoiceExport.ExportInvoice(_vehicleExpense.Id, InvoiceExportType.PDF);
				await SaveAndViewService.SaveAndView(pdfFileName, pdfStream);
			}

			if (saveExcel)
			{
				var (excelStream, excelFileName) = await VehicleExpenseInvoiceExport.ExportInvoice(_vehicleExpense.Id, InvoiceExportType.Excel);
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

			var decodeTransactionNo = await GenerateCodes.DecodeTransactionNo(_vehicleExpense.TransactionNo);
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

			var decodeTransactionNo = await GenerateCodes.DecodeTransactionNo(_vehicleExpense.TransactionNo);
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
				await AuthenticationService.NavigateToRoute(PageRouteNames.VehicleExpenseReport, FormFactor, JSRuntime, NavigationManager);
				break;
			case "ExpensesReport":
				await AuthenticationService.NavigateToRoute(PageRouteNames.VehicleExpenseDetailsReport, FormFactor, JSRuntime, NavigationManager);
				break;
		}
	}

	private async Task OnExpensesCartGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleExpenseDetailsCartModel> args)
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

	private async Task DeleteLocalFiles()
	{
		await DataStorageService.LocalRemove(StorageFileNames.VehicleExpenseDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.VehicleExpenseDetailsCartDataFileName);
	}

	private async Task ResetPage()
	{
		await DeleteLocalFiles();
		NavigationManager.NavigateTo(PageRouteNames.VehicleExpense, true);
	}

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard, true);
	#endregion
}