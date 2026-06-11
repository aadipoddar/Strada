using Strada.Shared.Components.Dialog;
using Strada.Shared.Components.Input;

using StradaLibrary.Accounts.Masters.Models;
using StradaLibrary.Fleet.OMC.Data;
using StradaLibrary.Fleet.OMC.Exports;
using StradaLibrary.Fleet.OMC.Models;
using StradaLibrary.Operations.Models;
using StradaLibrary.Utils.ExportUtils;

using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.OMC;

public partial class OMCCardPage
{
	private UserModel _user;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private OMCCardModel _omcCard = new();
	private OMCModel _selectedOMC;
	private LedgerModel _selectedLedger;

	private List<OMCCardModel> _omcCards = [];
	private List<OMCModel> _omcs = [];
	private List<LedgerModel> _ledgers = [];
	private readonly List<ContextMenuItemModel> _gridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditSelectedItem", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverSelectedItem", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<OMCCardModel> _sfGrid;
	private CustomTextField _firstFocus;
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
			_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Fleet]);
			await LoadData();
		}
		catch { NavigationManager.NavigateTo(PageRouteNames.Dashboard); }
	}

	private async Task LoadData()
	{
		_omcCards = await CommonData.LoadTableData<OMCCardModel>(FleetNames.OMCCard);
		_omcs = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC);
		_omcs = [.. _omcs.OrderBy(omc => omc.Name)];
		_selectedOMC = _omcs.FirstOrDefault(omc => omc.Id == _omcCard.OMCId);

		_ledgers = await CommonData.LoadTableData<LedgerModel>(AccountNames.Ledger);
		_ledgers = [.. _ledgers.OrderBy(ledger => ledger.Name)];
		_selectedLedger = _ledgers.FirstOrDefault(ledger => ledger.Id == _omcCard.LedgerId);

		if (!_showDeleted)
			_omcCards = [.. _omcCards.Where(omc => omc.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();

		_isLoading = false;
		StateHasChanged();

		if (_firstFocus is not null)
			await _firstFocus.FocusAsync();
	}
	#endregion

	#region Saving
	private async Task SaveTransaction()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();

			if (!_user.Admin)
				throw new Exception("You do not have permission to perform this action.");

			await _toastNotification.ShowAsync("Processing", "Please wait while the transaction is being saved...", ToastType.Info);

			_omcCard.OMCId = _selectedOMC?.Id ?? 0;
			_omcCard.LedgerId = _selectedLedger?.Id ?? 0;

			await OMCCardData.SaveTransaction(_omcCard, _user.Id, FormFactor.GetFormFactor() + FormFactor.GetPlatform());

			await _toastNotification.ShowAsync("Saved", "Transaction has been saved successfully.", ToastType.Success);
			ResetPage();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Saving", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
		}
	}
	#endregion

	#region Actions
	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count == 0)
			return;

		_omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, selectedRecords[0].Id);
		if (_omcCard is null)
		{
			await _toastNotification.ShowAsync("Error while Editing", "Transaction Not Found.", ToastType.Error);
			return;
		}

		_selectedOMC = _omcs.FirstOrDefault(omc => omc.Id == _omcCard.OMCId);
		_selectedLedger = _ledgers.FirstOrDefault(ledger => ledger.Id == _omcCard.LedgerId);
		StateHasChanged();
		await _firstFocus.FocusAsync();
	}

	private async Task DeleteRecoverTransaction(int id, bool isRecover)
	{
		try
		{
			if (!_user.Admin)
				throw new Exception("You do not have permission to perform this action.");

			_isProcessing = true;
			StateHasChanged();

			await _toastNotification.ShowAsync("Processing", $"{(isRecover ? "Recovering" : "Deleting")} transaction...", ToastType.Info);

			var omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, id)
				?? throw new Exception("Transaction not found.");

			if (isRecover) await OMCCardData.RecoverTransaction(omcCard, _user.Id, FormFactor.GetFormFactor() + FormFactor.GetPlatform());
			else await OMCCardData.DeleteTransaction(omcCard, _user.Id, FormFactor.GetFormFactor() + FormFactor.GetPlatform());

			await _toastNotification.ShowAsync("Success", $"Transaction {omcCard.CardNumber} has been {(isRecover ? "recovered" : "deleted")} successfully.", ToastType.Success);
			ResetPage();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"An error occurred while {(isRecover ? "recovering" : "deleting")} transaction: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task DeleteRecoverSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count == 0)
			return;

		var record = selectedRecords[0];

		await ShowConfirmation(record.Status ? "Delete" : "Recover",
			$"Are you sure you want to {(record.Status ? "delete" : "recover")} transaction {record.CardNumber}",
			() => DeleteRecoverTransaction(record.Id, !record.Status));
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

	#region Exporting
	private async Task ExportMaster(bool isExcel = false)
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var (stream, fileName) = await OMCCardExport.ExportMaster(_omcCards, isExcel ? ReportExportType.Excel : ReportExportType.PDF);
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
	#endregion

	#region Utilities
	private async Task OnGridContextMenuItemClicked(ContextMenuClickEventArgs<OMCCardModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditSelectedItem": await EditSelectedItem(); break;
			case "DeleteRecoverSelectedItem": await DeleteRecoverSelectedItem(); break;
		}
	}

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadData();
	}

	private void ResetPage() => PageRefresh.Request();
	#endregion
}
