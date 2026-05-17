using Microsoft.AspNetCore.Components;

using Strada.Shared.Components.Dialog;
using Strada.Shared.Components.Input;

using StradaLibrary.Accounts.Masters.Data;
using StradaLibrary.Fleet.Expense;
using StradaLibrary.Fleet.Expense.Exports;
using StradaLibrary.Utils.ExportUtils;
using StradaLibrary.Accounts.Masters.Models;
using StradaLibrary.Fleet.Expense.Models;
using StradaLibrary.Fleet.Vehicle.Models;
using StradaLibrary.Operations.Models;

using Syncfusion.Blazor.Grids;

using System.Text.Json;

namespace Strada.Shared.Pages.Fleet.Expense;

public partial class ExpensePage
{
	[Parameter] public int? Id { get; set; }

	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;

	private CompanyModel _selectedCompany = new();
	private FinancialYearModel _selectedFinancialYear = new();
	private VehicleModel _selectedVehicle = null;
	private ExpenseTypeModel _selectedExpenseType = null;
	private LedgerModel _selectedLedger = null;
	private ExpenseDetailsCartModel _selectedExpensesCart = new();
	private ExpenseModel _expense = new();

	private List<CompanyModel> _companies = [];
	private List<VehicleModel> _vehicles = [];
	private List<ExpenseTypeModel> _expenseTypes = [];
	private List<LedgerModel> _ledgers = [];
	private List<ExpenseDetailsCartModel> _expensesCart = [];

	private CustomAutoComplete<ExpenseTypeModel> _sfExpenseTypeAutoComplete;
	private SfGrid<ExpenseDetailsCartModel> _sfExpensesCartGrid;
	private CustomAutoComplete<VehicleModel> _sfFirstFocus;
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

		if (_sfFirstFocus is not null)
			await _sfFirstFocus.FocusAsync();
	}

	private async Task LoadData()
	{
		_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
		_companies = await CommonData.LoadTableDataByStatus<CompanyModel>(AccountNames.Company);
		_expenseTypes = await CommonData.LoadTableDataByStatus<ExpenseTypeModel>(FleetNames.ExpenseType);
		_ledgers = await CommonData.LoadTableDataByStatus<LedgerModel>(AccountNames.Ledger);

		_vehicles = [.. _vehicles.OrderBy(s => s.ShortCode)];
		_companies = [.. _companies.OrderBy(s => s.Name)];
		_expenseTypes = [.. _expenseTypes.OrderBy(s => s.Name)];
		_ledgers = [.. _ledgers.OrderBy(s => s.Name)];

		_selectedVehicle = _vehicles.FirstOrDefault();
		_selectedCompany = _companies.FirstOrDefault(c => c.Id == _selectedVehicle.CompanyId);
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

		_expense = await CommonData.LoadTableDataById<ExpenseModel>(FleetNames.Expense, Id.Value);
		if (_expense is null || _expense.Id == 0)
		{
			await _toastNotification.ShowAsync("Transaction Not Found", "The requested transaction could not be found.", ToastType.Error);
			NavigationManager.NavigateTo(PageRouteNames.Expense, true);
		}

		return true;
	}

	private async Task<bool> TryRestoreFromLocalStorage()
	{
		if (!await DataStorageService.LocalExists(StorageFileNames.ExpenseDataFileName))
			return false;

		try
		{
			_expense = JsonSerializer.Deserialize<ExpenseModel>(await DataStorageService.LocalGetAsync(StorageFileNames.ExpenseDataFileName));
			if (_expense is null)
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

		_expense = new()
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
		if (_expense.VehicleId > 0)
			_selectedVehicle = _vehicles.FirstOrDefault(s => s.Id == _expense.VehicleId) ?? _vehicles.FirstOrDefault();
		else
			_selectedVehicle = _vehicles.FirstOrDefault();

		if (_expense.CompanyId > 0)
			_selectedCompany = _companies.FirstOrDefault(s => s.Id == _expense.CompanyId) ?? _companies.FirstOrDefault();
		else
			_selectedCompany = _companies.FirstOrDefault(s => s.Id == _selectedVehicle.CompanyId) ?? _companies.FirstOrDefault();

		if (_expense.Id == 0)
		{
			var lastTransaction = await CommonData.LoadLastTableData<ExpenseModel>(FleetNames.Expense);
			if (lastTransaction is not null)
				_expense.TransactionDateTime = lastTransaction.TransactionDateTime;
		}
	}

	private async Task ResolveExpensesCart()
	{
		try
		{
			_expensesCart.Clear();

			if (await LoadExistingExpensesCart())
				return;

			if (await DataStorageService.LocalExists(StorageFileNames.ExpenseDetailsCartDataFileName))
				_expensesCart = JsonSerializer.Deserialize<List<ExpenseDetailsCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.ExpenseDetailsCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Expenses Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingExpensesCart()
	{
		if (_expense.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<ExpenseDetailsModel>(FleetNames.ExpenseDetails, _expense.Id);

		foreach (var item in existingCart)
		{
			if (_expenseTypes.FirstOrDefault(s => s.Id == item.ExpenseTypeId) is null)
			{
				var expenseType = await CommonData.LoadTableDataById<ExpenseTypeModel>(FleetNames.ExpenseType, item.ExpenseTypeId);
				await _toastNotification.ShowAsync("Expense Type Not Found", $"The Expense Type {expenseType?.Name} (ID: {item.ExpenseTypeId}) in the existing transaction cart was not found in the available expense types list. It may have been deleted or is inaccessible.", ToastType.Error);
				continue;
			}

			_expensesCart.Add(new()
			{
				ExpenseTypeId = item.ExpenseTypeId,
				ExpenseTypeName = _expenseTypes.First(s => s.Id == item.ExpenseTypeId).Name,
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
	private async Task OnVehicleChanged(VehicleModel value)
	{
		if (value is null || value.Id == 0)
			return;

		_selectedVehicle = value;
		_selectedCompany = _companies.FirstOrDefault(s => s.Id == _selectedVehicle.CompanyId);

		await SaveTransactionFile();
	}

	private async Task OnCompanyChanged(CompanyModel value)
	{
		if (value is null || value.Id == 0)
			return;

		_selectedCompany = value;
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
		_selectedExpensesCart.LedgerId = _selectedLedger?.Id;
		_selectedExpensesCart.LedgerName = _selectedLedger?.Name;
	}

	private async Task OnLedgerChanged(LedgerModel value)
	{
		if (value is null || value.Id == 0)
		{
			_selectedLedger = null;
			_selectedExpensesCart.LedgerId = null;
			_selectedExpensesCart.LedgerName = null;
		}

		else
		{
			_selectedLedger = value;
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

		var existingItem = _expensesCart.FirstOrDefault(s => s.ExpenseTypeId == _selectedExpenseType.Id);
		if (existingItem is not null)
			existingItem.Amount += _selectedExpensesCart.Amount;
		else
			_expensesCart.Add(new()
			{
				ExpenseTypeId = _selectedExpenseType.Id,
				ExpenseTypeName = _selectedExpenseType.Name,
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

		_selectedExpenseType = _expenseTypes.FirstOrDefault(s => s.Id == selectedCartItem.ExpenseTypeId);
		if (_selectedExpenseType is null)
			return;

		_selectedLedger = _ledgers.FirstOrDefault(s => s.Id == selectedCartItem.LedgerId);

		_selectedExpensesCart = new()
		{
			ExpenseTypeId = selectedCartItem.ExpenseTypeId,
			ExpenseTypeName = selectedCartItem.ExpenseTypeName,
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
			if (item.Amount <= 0)
				_expensesCart.Remove(item);

		_expense.CompanyId = _selectedCompany.Id;
		_expense.VehicleId = _selectedVehicle.Id;
		_expense.TotalExpense = _expensesCart.Sum(s => s.Amount);

		#region Financial Year
		_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_expense.TransactionDateTime);
		if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
			_expense.FinancialYearId = _selectedFinancialYear.Id;
		else
			await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
		#endregion

		if (Id is null)
			_expense.TransactionNo = await GenerateCodes.GenerateExpenseTransactionNo(_expense);

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		_expense.Status = true;
		_expense.TransactionDateTime = DateOnly.FromDateTime(_expense.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
		_expense.CreatedAt = currentDateTime;
		_expense.LastModifiedAt = currentDateTime;
		_expense.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_expense.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_expense.CreatedBy = _user.Id;
		_expense.LastModifiedBy = _user.Id;
	}

	private async Task SaveTransactionFile()
	{
		if (_isProcessing || _isLoading)
			return;

		try
		{
			_isProcessing = true;

			await UpdateFinancialDetails();

			if (_expensesCart.Count == 0 || _expense.Id > 0)
			{
				await DeleteLocalFiles();
				return;
			}

			await DataStorageService.LocalSaveAsync(StorageFileNames.ExpenseDataFileName, JsonSerializer.Serialize(_expense));
			await DataStorageService.LocalSaveAsync(StorageFileNames.ExpenseDetailsCartDataFileName, JsonSerializer.Serialize(_expensesCart));
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

			var expenses = ExpenseData.ConvertExpensesCartToDetails(_expensesCart, _expense.Id);
			_expense.Id = await ExpenseData.SaveTransaction(_expense, expenses);

			if (savePDF)
			{
				var (pdfStream, pdfFileName) = await ExpenseInvoiceExport.ExportInvoice(_expense.Id, InvoiceExportType.PDF);
				await SaveAndViewService.SaveAndView(pdfFileName, pdfStream);
			}

			if (saveExcel)
			{
				var (excelStream, excelFileName) = await ExpenseInvoiceExport.ExportInvoice(_expense.Id, InvoiceExportType.Excel);
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

			var decodeTransactionNo = await DecodeCode.DecodeTransactionNo(_expense.TransactionNo, true, false, CodeType.Expense);
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

			var decodeTransactionNo = await DecodeCode.DecodeTransactionNo(_expense.TransactionNo, false, true, CodeType.Expense);
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
			case "ExpenseReport":
				await AuthenticationService.NavigateToRoute(PageRouteNames.ExpenseReport, FormFactor, JSRuntime, NavigationManager);
				break;
			case "ExpenseDetailsReport":
				await AuthenticationService.NavigateToRoute(PageRouteNames.ExpenseDetailsReport, FormFactor, JSRuntime, NavigationManager);
				break;
		}
	}

	private async Task OnExpensesCartGridContextMenuItemClicked(ContextMenuClickEventArgs<ExpenseDetailsCartModel> args)
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
		await DataStorageService.LocalRemove(StorageFileNames.ExpenseDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.ExpenseDetailsCartDataFileName);
	}

	private async Task ResetPage()
	{
		await DeleteLocalFiles();
		NavigationManager.NavigateTo(PageRouteNames.Expense, true);
	}

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetTransactionsDashboard, true);
	#endregion
}
