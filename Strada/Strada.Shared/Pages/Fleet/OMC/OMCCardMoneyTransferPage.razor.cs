using Microsoft.AspNetCore.Components;

using Strada.Models.Accounts.Masters;
using Strada.Models.Fleet.OMC;
using Strada.Models.Operations;
using Strada.Shared.Components.Dialog;
using Strada.Shared.Components.Input;

using StradaLibrary.Accounts.Masters.Data;
using StradaLibrary.Common;
using StradaLibrary.Fleet.OMC.Data;

using Syncfusion.Blazor.Grids;

using System.Text.Json;

namespace Strada.Shared.Pages.Fleet.OMC;

public partial class OMCCardMoneyTransferPage
{
	[Parameter] public int? Id { get; set; }

	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;

	private CompanyModel _selectedCompany = new();
	private FinancialYearModel _selectedFinancialYear = new();
	private LedgerModel _selectedLedger = null;
	private OMCCardModel _selectedOMCCard = null;
	private OMCCardMoneyTransferDetailsCartModel _selectedTransferCart = new();
	private OMCCardMoneyTransferModel _transfer = new();

	private List<CompanyModel> _companies = [];
	private List<LedgerModel> _ledgers = [];
	private List<OMCCardModel> _omcCards = [];
	private List<OMCCardMoneyTransferDetailsCartModel> _transfersCart = [];

	private CustomAutoComplete<OMCCardModel> _sfOMCCardAutoComplete;
	private SfGrid<OMCCardMoneyTransferDetailsCartModel> _sfTransfersCartGrid;
	private CustomAutoComplete<LedgerModel> _firstFocus;
	private ToastNotification _toastNotification;

	private readonly List<ContextMenuItemModel> _transfersCartGridContextMenuItems =
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
		catch { await ResetPage(); }
	}

	private async Task InitializePage()
	{
		await LoadData();
		await ResolveTransaction();
		await LoadSelections();
		await ResolveTransfersCart();

		_isLoading = false;
		StateHasChanged();

		await SaveTransactionFile();

		if (_firstFocus is not null)
			await _firstFocus.FocusAsync();
	}

	private async Task LoadData()
	{
		_ledgers = await CommonData.LoadTableDataByStatus<LedgerModel>(AccountNames.Ledger);
		_companies = await CommonData.LoadTableDataByStatus<CompanyModel>(AccountNames.Company);
		_omcCards = await CommonData.LoadTableDataByStatus<OMCCardModel>(FleetNames.OMCCard);

		_ledgers = [.. _ledgers.OrderBy(s => s.Name)];
		_companies = [.. _companies.OrderBy(s => s.Name)];
		_omcCards = [.. _omcCards.OrderBy(s => s.CardNumber)];

		_selectedLedger = _ledgers.FirstOrDefault();
		_selectedCompany = _companies.FirstOrDefault();
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

		_transfer = await CommonData.LoadTableDataById<OMCCardMoneyTransferModel>(FleetNames.OMCCardMoneyTransfer, Id.Value);
		if (_transfer is null || _transfer.Id == 0)
		{
			await _toastNotification.ShowAsync("Transaction Not Found", "The requested transaction could not be found.", ToastType.Error);
			NavigationManager.NavigateTo(PageRouteNames.OMCCardMoneyTransfer, true);
		}

		return true;
	}

	private async Task<bool> TryRestoreFromLocalStorage()
	{
		if (!await DataStorageService.LocalExists(StorageFileNames.OMCCardMoneyTransferDataFileName))
			return false;

		try
		{
			_transfer = JsonSerializer.Deserialize<OMCCardMoneyTransferModel>(await DataStorageService.LocalGetAsync(StorageFileNames.OMCCardMoneyTransferDataFileName));
			if (_transfer is null)
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

		_transfer = new()
		{
			Id = 0,
			TransactionNo = string.Empty,
			CompanyId = _selectedCompany.Id,
			TransactionDateTime = currentDateTime,
			FinancialYearId = financialYear is null ? 0 : financialYear.Id,
			LedgerId = _selectedLedger.Id,
			TotalItems = 0,
			TotalAmount = 0,
			Remarks = string.Empty,
			CreatedBy = _user.Id,
			CreatedAt = DateTime.Now,
			CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform(),
			Status = true,
			LastModifiedAt = null,
			LastModifiedBy = null,
			LastModifiedFromPlatform = null
		};

		var lastTransaction = await CommonData.LoadLastTableData<OMCCardMoneyTransferModel>(FleetNames.OMCCardMoneyTransfer);
		if (lastTransaction is not null)
			_transfer.TransactionDateTime = lastTransaction.TransactionDateTime;

		await DeleteLocalFiles();
	}

	private async Task LoadSelections()
	{
		if (_transfer.LedgerId > 0)
			_selectedLedger = _ledgers.FirstOrDefault(s => s.Id == _transfer.LedgerId) ?? _ledgers.FirstOrDefault();
		else
			_selectedLedger = _ledgers.FirstOrDefault();

		if (_transfer.CompanyId > 0)
			_selectedCompany = _companies.FirstOrDefault(s => s.Id == _transfer.CompanyId) ?? _companies.FirstOrDefault();
		else
			_selectedCompany = _companies.FirstOrDefault();
	}

	private async Task ResolveTransfersCart()
	{
		try
		{
			_transfersCart.Clear();

			if (await LoadExistingTransfersCart())
				return;

			if (await DataStorageService.LocalExists(StorageFileNames.OMCCardMoneyTransferDetailsCartDataFileName))
				_transfersCart = JsonSerializer.Deserialize<List<OMCCardMoneyTransferDetailsCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.OMCCardMoneyTransferDetailsCartDataFileName));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Transfers Cart Data", ex.Message, ToastType.Error);
			await ResetPage();
		}
	}

	private async Task<bool> LoadExistingTransfersCart()
	{
		if (_transfer.Id <= 0)
			return false;

		var existingCart = await CommonData.LoadTableDataByMasterId<OMCCardMoneyTransferDetailsModel>(FleetNames.OMCCardMoneyTransferDetails, _transfer.Id);

		foreach (var item in existingCart)
		{
			if (_omcCards.FirstOrDefault(s => s.Id == item.OMCCardId) is null)
			{
				var omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, item.OMCCardId);
				await _toastNotification.ShowAsync("OMC Card Not Found", $"The OMC Card {omcCard?.CardNumber} (ID: {item.OMCCardId}) in the existing transaction cart was not found in the available OMC cards list. It may have been deleted or is inaccessible.", ToastType.Error);
				continue;
			}

			_transfersCart.Add(new()
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

	#region Changed Events
	private async Task OnLedgerChanged(LedgerModel value)
	{
		if (value is null || value.Id == 0)
			return;

		_selectedLedger = value;
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

	#region Transfers Cart
	private void OnOMCCardChanged(OMCCardModel value)
	{
		if (value is null || value.Id == 0)
		{
			_selectedOMCCard = null;
			_selectedTransferCart = new();
			return;
		}

		_selectedOMCCard = value;

		_selectedTransferCart.OMCCardId = _selectedOMCCard.Id;
		_selectedTransferCart.OMCCardNumber = _selectedOMCCard.CardNumber;
	}

	private async Task AddTransfersToCart()
	{
		if (_selectedOMCCard is null || _selectedOMCCard.Id <= 0 || _selectedTransferCart.Amount <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		var existingItem = _transfersCart.FirstOrDefault(s => s.OMCCardId == _selectedOMCCard.Id);
		if (existingItem is not null)
			existingItem.Amount += _selectedTransferCart.Amount;
		else
			_transfersCart.Add(new()
			{
				OMCCardId = _selectedOMCCard.Id,
				OMCCardNumber = _selectedOMCCard.CardNumber,
				Amount = _selectedTransferCart.Amount,
				Remarks = _selectedTransferCart.Remarks
			});

		_selectedOMCCard = null;
		_selectedTransferCart = new();

		await _sfOMCCardAutoComplete.FocusAsync();
		await SaveTransactionFile();
	}

	private async Task EditSelectedTransfersCartItem()
	{
		if (_sfTransfersCartGrid is null || _sfTransfersCartGrid.SelectedRecords is null || _sfTransfersCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartItem = _sfTransfersCartGrid.SelectedRecords.First();

		_selectedOMCCard = _omcCards.FirstOrDefault(s => s.Id == selectedCartItem.OMCCardId);
		if (_selectedOMCCard is null)
			return;

		_selectedTransferCart = new()
		{
			OMCCardId = selectedCartItem.OMCCardId,
			OMCCardNumber = selectedCartItem.OMCCardNumber,
			Amount = selectedCartItem.Amount,
			Remarks = selectedCartItem.Remarks
		};

		await _sfOMCCardAutoComplete.FocusAsync();
		await RemoveSelectedTransfersCartItem();
	}

	private async Task RemoveSelectedTransfersCartItem()
	{
		if (_sfTransfersCartGrid is null || _sfTransfersCartGrid.SelectedRecords is null || _sfTransfersCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartItem = _sfTransfersCartGrid.SelectedRecords.First();
		_transfersCart.Remove(selectedCartItem);
		await SaveTransactionFile();
	}
	#endregion

	#region Saving
	private async Task UpdateFinancialDetails()
	{
		foreach (var item in _transfersCart.ToList())
			if (item.Amount <= 0)
				_transfersCart.Remove(item);

		_transfer.CompanyId = _selectedCompany.Id;
		_transfer.LedgerId = _selectedLedger.Id;
		_transfer.TotalItems = _transfersCart.Count;
		_transfer.TotalAmount = _transfersCart.Sum(s => s.Amount);

		#region Financial Year
		_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_transfer.TransactionDateTime);
		if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
			_transfer.FinancialYearId = _selectedFinancialYear.Id;
		else
			await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
		#endregion

		if (Id is null)
			_transfer.TransactionNo = await GenerateCodes.GenerateOMCCardMoneyTransferTransactionNo(_transfer);

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		_transfer.Status = true;
		_transfer.TransactionDateTime = DateOnly.FromDateTime(_transfer.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
		_transfer.CreatedAt = currentDateTime;
		_transfer.LastModifiedAt = currentDateTime;
		_transfer.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_transfer.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
		_transfer.CreatedBy = _user.Id;
		_transfer.LastModifiedBy = _user.Id;
	}

	private async Task SaveTransactionFile()
	{
		if (_isProcessing || _isLoading)
			return;

		try
		{
			_isProcessing = true;

			await UpdateFinancialDetails();

			if (_transfersCart.Count == 0 || _transfer.Id > 0)
			{
				await DeleteLocalFiles();
				return;
			}

			await DataStorageService.LocalSaveAsync(StorageFileNames.OMCCardMoneyTransferDataFileName, JsonSerializer.Serialize(_transfer));
			await DataStorageService.LocalSaveAsync(StorageFileNames.OMCCardMoneyTransferDetailsCartDataFileName, JsonSerializer.Serialize(_transfersCart));
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Saving Transaction Data", ex.Message, ToastType.Error);
		}
		finally
		{
			if (_sfTransfersCartGrid is not null)
				await _sfTransfersCartGrid.Refresh();

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

			var transfers = OMCCardMoneyTransferData.ConvertTransfersCartToDetails(_transfersCart, _transfer.Id);
			_transfer.Id = await OMCCardMoneyTransferData.SaveTransaction(_transfer, transfers);
			_transfer = await CommonData.LoadTableDataById<OMCCardMoneyTransferModel>(FleetNames.OMCCardMoneyTransfer, _transfer.Id);

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
		if (_transfer.Id <= 0 || (_isProcessing && !force))
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var decodeTransactionNo = await DecodeCode.DecodeTransactionNo(_transfer.TransactionNo, !isExcel, isExcel, CodeType.OMCCardMoneyTransfer);
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

	#region Utilities
	private async Task OnTransfersCartGridContextMenuItemClicked(ContextMenuClickEventArgs<OMCCardMoneyTransferDetailsCartModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditCart": await EditSelectedTransfersCartItem(); break;
			case "DeleteCart": await RemoveSelectedTransfersCartItem(); break;
		}
	}

	private async Task DeleteLocalFiles()
	{
		await DataStorageService.LocalRemove(StorageFileNames.OMCCardMoneyTransferDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.OMCCardMoneyTransferDetailsCartDataFileName);
	}

	private async Task ResetPage()
	{
		await DeleteLocalFiles();
		PageRefresh.Request();
	}
	#endregion
}
