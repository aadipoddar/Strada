using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Fleet.VehicleRoute;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.VehicleRoute;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleRoute;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.VehicleRoute;

public partial class OMCCardPage : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private OMCCardModel _omcCard = new();
	private OMCModel _selectedOMC;

	private List<OMCCardModel> _omcCards = [];
	private List<OMCCardModel> _omcCardsAll = [];
	private List<OMCModel> _omcs = [];
	private readonly List<ContextMenuItemModel> _omcCardGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditOMCCard", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverOMCCard", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<OMCCardModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteOMCCardId = 0;
	private string _deleteOMCCardName = string.Empty;

	private int _recoverOMCCardId = 0;
	private string _recoverOMCCardName = string.Empty;

	private ToastNotification _toastNotification;

	#region Load Data
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Fleet]);
		await LoadData();
		_isLoading = false;
		StateHasChanged();
	}

	private async Task LoadData()
	{
		_hotKeysContext = HotKeys.CreateContext()
			.Add(ModCode.Ctrl, Code.S, SaveOMCCard, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

		_omcCardsAll = await CommonData.LoadTableData<OMCCardModel>(FleetNames.OMCCard);
		_omcCards = [.. _omcCardsAll];
		_omcs = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC);
		_selectedOMC = _omcs.FirstOrDefault(omc => omc.Id == _omcCard.OMCId);

		if (!_showDeleted)
			_omcCards = [.. _omcCards.Where(omc => omc.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion

	#region Saving
	private async Task ValidateForm()
	{
		if (!_user.Admin)
			throw new Exception("You do not have permission to perform this action.");

		_omcCard.CardNumber = _omcCard.CardNumber?.Trim() ?? "";
		_omcCard.CardNumber = _omcCard.CardNumber?.ToUpper() ?? "";
		_omcCard.OMCId = _selectedOMC?.Id ?? 0;

		_omcCard.Code = _omcCard.Code?.Trim() ?? "";
		_omcCard.Code = _omcCard.Code?.ToUpper() ?? "";

		_omcCard.Remarks = _omcCard.Remarks?.Trim() ?? "";
		_omcCard.Status = true;

		if (string.IsNullOrWhiteSpace(_omcCard.CardNumber))
			throw new Exception("OMC card number is required. Please enter a valid OMC card number.");

		if (string.IsNullOrWhiteSpace(_omcCard.Code))
			throw new Exception("OMC card code is required. Please try again.");

		if (_omcCard.OMCId <= 0)
			throw new Exception("Associated OMC is required. Please select a valid OMC.");

		if (_omcCard.OpeningBalance < 0)
			throw new Exception("Opening balance cannot be negative.");

		if (string.IsNullOrWhiteSpace(_omcCard.Remarks))
			_omcCard.Remarks = null;

		if (_omcCard.Id > 0)
		{
			var existingOMCByCardNumber = _omcCardsAll.FirstOrDefault(_ => _.Id != _omcCard.Id && _.CardNumber.Equals(_omcCard.CardNumber, StringComparison.OrdinalIgnoreCase));
			if (existingOMCByCardNumber is not null)
				throw new Exception($"OMC card number '{_omcCard.CardNumber}' already exists. Please choose a different card number.");

			var existingOMCByCode = _omcCardsAll.FirstOrDefault(_ => _.Id != _omcCard.Id && _.Code.Equals(_omcCard.Code, StringComparison.OrdinalIgnoreCase));
			if (existingOMCByCode is not null)
				throw new Exception($"OMC card code '{_omcCard.Code}' already exists. Please choose a different code.");
		}
		else
		{
			var existingOMCByCardNumber = _omcCardsAll.FirstOrDefault(_ => _.CardNumber.Equals(_omcCard.CardNumber, StringComparison.OrdinalIgnoreCase));
			if (existingOMCByCardNumber is not null)
				throw new Exception($"OMC card number '{_omcCard.CardNumber}' already exists. Please choose a different card number.");

			var existingOMCByCode = _omcCardsAll.FirstOrDefault(_ => _.Code.Equals(_omcCard.Code, StringComparison.OrdinalIgnoreCase));
			if (existingOMCByCode is not null)
				throw new Exception($"OMC card code '{_omcCard.Code}' already exists. Please choose a different code.");
		}
	}

	private async Task SaveOMCCard()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			if (_omcCard.Id == 0)
				_omcCard.Code = await GenerateCodes.GenerateOMCCardCode();

			await ValidateForm();
			await OMCCardData.InsertOMCCard(_omcCard);

			await _toastNotification.ShowAsync("Success", $"OMC Card '{_omcCard.CardNumber}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleOMCCardMaster, true);
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

	#region Actions
	private async Task OnEditOMCCard(OMCCardModel omcCard)
	{
		_omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, omcCard.Id)
			?? throw new Exception("OMC Card not found.");
		_selectedOMC = _omcs.FirstOrDefault(omc => omc.Id == _omcCard.OMCId);

		StateHasChanged();
	}

	private async Task ConfirmDelete()
	{
		try
		{
			_isProcessing = true;
			await _deleteConfirmationDialog.HideAsync();

			if (!_user.Admin)
				throw new Exception("You do not have permission to perform this action.");

			var omcCard = _omcCardsAll.FirstOrDefault(o => o.Id == _deleteOMCCardId)
				?? throw new Exception("OMC Card not found.");

			omcCard.Status = false;
			await OMCCardData.InsertOMCCard(omcCard);

			await _toastNotification.ShowAsync("Success", $"OMC Card '{omcCard.CardNumber}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleOMCCardMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete OMC Card: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteOMCCardId = 0;
			_deleteOMCCardName = string.Empty;
		}
	}

	private async Task ConfirmRecover()
	{
		try
		{
			_isProcessing = true;
			await _recoverConfirmationDialog.HideAsync();

			if (!_user.Admin)
				throw new Exception("You do not have permission to perform this action.");

			var omcCard = _omcCardsAll.FirstOrDefault(o => o.Id == _recoverOMCCardId)
				?? throw new Exception("OMC Card not found.");

			omcCard.Status = true;
			await OMCCardData.InsertOMCCard(omcCard);

			await _toastNotification.ShowAsync("Success", $"OMC Card '{omcCard.CardNumber}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleOMCCardMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover OMC Card: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverOMCCardId = 0;
			_recoverOMCCardName = string.Empty;
		}
	}
	#endregion

	#region Exporting
	private async Task ExportExcel()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var (stream, fileName) = await OMCCardExport.ExportMaster(_omcCards, ReportExportType.Excel);
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

	private async Task ExportPdf()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var (stream, fileName) = await OMCCardExport.ExportMaster(_omcCards, ReportExportType.PDF);
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
	private async Task OnMenuSelected(Syncfusion.Blazor.Navigations.MenuEventArgs<Syncfusion.Blazor.Navigations.MenuItem> args)
	{
		switch (args.Item.Id)
		{
			case "NewOMCCard":
				ResetPage();
				break;
			case "SaveOMCCard":
				await SaveOMCCard();
				break;
			case "ToggleDeleted":
				await ToggleDeleted();
				break;
			case "ExportExcel":
				await ExportExcel();
				break;
			case "ExportPdf":
				await ExportPdf();
				break;
			case "EditSelected":
				await EditSelectedItem();
				break;
			case "DeleteRecoverSelected":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task OnOMCCardGridContextMenuItemClicked(ContextMenuClickEventArgs<OMCCardModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditOMCCard":
				await EditSelectedItem();
				break;
			case "DeleteRecoverOMCCard":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			await OnEditOMCCard(selectedRecords[0]);
	}

	private async Task DeleteSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
		{
			if (selectedRecords[0].Status)
				await ShowDeleteConfirmation(selectedRecords[0].Id, selectedRecords[0].CardNumber);
			else
				await ShowRecoverConfirmation(selectedRecords[0].Id, selectedRecords[0].CardNumber);
		}
	}

	private async Task ShowDeleteConfirmation(int id, string name)
	{
		_deleteOMCCardId = id;
		_deleteOMCCardName = name;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteOMCCardId = 0;
		_deleteOMCCardName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation(int id, string name)
	{
		_recoverOMCCardId = id;
		_recoverOMCCardName = name;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverOMCCardId = 0;
		_recoverOMCCardName = string.Empty;
		await _recoverConfirmationDialog.HideAsync();
	}

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadData();
		StateHasChanged();
	}

	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.VehicleOMCCardMaster, true);

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
	#endregion
}