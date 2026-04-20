using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Fleet.VehicleRoute;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.VehicleRoute;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleRoute;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.VehicleRoute;

public partial class OMCPage : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private OMCModel _omc = new();

	private List<OMCModel> _omcs = [];
	private List<OMCModel> _omcsAll = [];
	private readonly List<ContextMenuItemModel> _omcGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditOMC", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverOMC", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<OMCModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteOMCId = 0;
	private string _deleteOMCName = string.Empty;

	private int _recoverOMCId = 0;
	private string _recoverOMCName = string.Empty;

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
			.Add(ModCode.Ctrl, Code.S, SaveOMC, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

		_omcsAll = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC);
		_omcs = [.. _omcsAll];

		if (!_showDeleted)
			_omcs = [.. _omcs.Where(omc => omc.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion

	#region Saving
	private async Task ValidateForm()
	{
		if (!_user.Admin)
			throw new Exception("You do not have permission to perform this action.");

		_omc.Name = _omc.Name?.Trim() ?? "";
		_omc.Name = _omc.Name?.ToUpper() ?? "";

		_omc.Code = _omc.Code?.Trim() ?? "";
		_omc.Code = _omc.Code?.ToUpper() ?? "";

		_omc.Remarks = _omc.Remarks?.Trim() ?? "";
		_omc.Status = true;

		if (string.IsNullOrWhiteSpace(_omc.Name))
			throw new Exception("OMC name is required. Please enter a valid OMC name.");

		if (string.IsNullOrWhiteSpace(_omc.Code))
			throw new Exception("OMC code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(_omc.Remarks))
			_omc.Remarks = null;

		if (_omc.Id > 0)
		{
			var existingOMCByName = _omcsAll.FirstOrDefault(_ => _.Id != _omc.Id && _.Name.Equals(_omc.Name, StringComparison.OrdinalIgnoreCase));
			if (existingOMCByName is not null)
				throw new Exception($"OMC name '{_omc.Name}' already exists. Please choose a different name.");

			var existingOMCByCode = _omcsAll.FirstOrDefault(_ => _.Id != _omc.Id && _.Code.Equals(_omc.Code, StringComparison.OrdinalIgnoreCase));
			if (existingOMCByCode is not null)
				throw new Exception($"OMC code '{_omc.Code}' already exists. Please choose a different code.");
		}
		else
		{
			var existingOMCByName = _omcsAll.FirstOrDefault(_ => _.Name.Equals(_omc.Name, StringComparison.OrdinalIgnoreCase));
			if (existingOMCByName is not null)
				throw new Exception($"OMC name '{_omc.Name}' already exists. Please choose a different name.");

			var existingOMCByCode = _omcsAll.FirstOrDefault(_ => _.Code.Equals(_omc.Code, StringComparison.OrdinalIgnoreCase));
			if (existingOMCByCode is not null)
				throw new Exception($"OMC code '{_omc.Code}' already exists. Please choose a different code.");
		}
	}

	private async Task SaveOMC()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			if (_omc.Id == 0)
				_omc.Code = await GenerateCodes.GenerateOMCCode();

			await ValidateForm();
			await OMCData.InsertOMC(_omc);

			await _toastNotification.ShowAsync("Success", $"OMC '{_omc.Name}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleOMCMaster, true);
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
	private async Task OnEditOMC(OMCModel omc)
	{
		_omc = await CommonData.LoadTableDataById<OMCModel>(FleetNames.OMC, omc.Id)
			?? throw new Exception("OMC not found.");

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

			var omc = _omcsAll.FirstOrDefault(o => o.Id == _deleteOMCId)
				?? throw new Exception("OMC not found.");

			omc.Status = false;
			await OMCData.InsertOMC(omc);

			await _toastNotification.ShowAsync("Success", $"OMC '{omc.Name}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleOMCMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete OMC: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteOMCId = 0;
			_deleteOMCName = string.Empty;
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

			var omc = _omcsAll.FirstOrDefault(o => o.Id == _recoverOMCId)
				?? throw new Exception("OMC not found.");

			omc.Status = true;
			await OMCData.InsertOMC(omc);

			await _toastNotification.ShowAsync("Success", $"OMC '{omc.Name}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleOMCMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover OMC: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverOMCId = 0;
			_recoverOMCName = string.Empty;
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

			var (stream, fileName) = await OMCExport.ExportMaster(_omcs, ReportExportType.Excel);
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

			var (stream, fileName) = await OMCExport.ExportMaster(_omcs, ReportExportType.PDF);
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
			case "NewOMC":
				ResetPage();
				break;
			case "SaveOMC":
				await SaveOMC();
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

	private async Task OnOMCGridContextMenuItemClicked(ContextMenuClickEventArgs<OMCModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditOMC":
				await EditSelectedItem();
				break;
			case "DeleteRecoverOMC":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			await OnEditOMC(selectedRecords[0]);
	}

	private async Task DeleteSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
		{
			if (selectedRecords[0].Status)
				await ShowDeleteConfirmation(selectedRecords[0].Id, selectedRecords[0].Name);
			else
				await ShowRecoverConfirmation(selectedRecords[0].Id, selectedRecords[0].Name);
		}
	}

	private async Task ShowDeleteConfirmation(int id, string name)
	{
		_deleteOMCId = id;
		_deleteOMCName = name;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteOMCId = 0;
		_deleteOMCName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation(int id, string name)
	{
		_recoverOMCId = id;
		_recoverOMCName = name;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverOMCId = 0;
		_recoverOMCName = string.Empty;
		await _recoverConfirmationDialog.HideAsync();
	}

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadData();
		StateHasChanged();
	}

	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.VehicleOMCMaster, true);

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
	#endregion
}