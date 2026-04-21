using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Fleet.VehicleRoute;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.VehicleRoute;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleRoute;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.VehicleRoute;

public partial class VehicleRouteExpenseTypePage : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private VehicleRouteExpenseTypeModel _vehicleRouteExpenseType = new();

	private List<VehicleRouteExpenseTypeModel> _vehicleRouteExpenseTypes = [];
	private List<VehicleRouteExpenseTypeModel> _vehicleRouteExpenseTypesAll = [];
	private readonly List<ContextMenuItemModel> _vehicleRouteExpenseTypeGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditVehicleRouteExpenseType", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverVehicleRouteExpenseType", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<VehicleRouteExpenseTypeModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteVehicleRouteExpenseTypeId = 0;
	private string _deleteVehicleRouteExpenseTypeName = string.Empty;

	private int _recoverVehicleRouteExpenseTypeId = 0;
	private string _recoverVehicleRouteExpenseTypeName = string.Empty;

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
			.Add(ModCode.Ctrl, Code.S, SaveVehicleRouteExpenseType, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

		_vehicleRouteExpenseTypesAll = await CommonData.LoadTableData<VehicleRouteExpenseTypeModel>(FleetNames.VehicleRouteExpenseType);
		_vehicleRouteExpenseTypes = [.. _vehicleRouteExpenseTypesAll];

		if (!_showDeleted)
			_vehicleRouteExpenseTypes = [.. _vehicleRouteExpenseTypes.Where(v => v.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion

	#region Saving
	private async Task ValidateForm()
	{
		if (!_user.Admin)
			throw new Exception("You do not have permission to perform this action.");

		_vehicleRouteExpenseType.Name = _vehicleRouteExpenseType.Name?.Trim() ?? "";
		_vehicleRouteExpenseType.Name = _vehicleRouteExpenseType.Name?.ToUpper() ?? "";

		_vehicleRouteExpenseType.Code = _vehicleRouteExpenseType.Code?.Trim() ?? "";
		_vehicleRouteExpenseType.Code = _vehicleRouteExpenseType.Code?.ToUpper() ?? "";

		_vehicleRouteExpenseType.Remarks = _vehicleRouteExpenseType.Remarks?.Trim() ?? "";
		_vehicleRouteExpenseType.Status = true;

		if (string.IsNullOrWhiteSpace(_vehicleRouteExpenseType.Name))
			throw new Exception("Vehicle Route Expense Type name is required. Please enter a valid name.");

		if (string.IsNullOrWhiteSpace(_vehicleRouteExpenseType.Code))
			throw new Exception("Vehicle Route Expense Type code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(_vehicleRouteExpenseType.Remarks))
			_vehicleRouteExpenseType.Remarks = null;

		if (_vehicleRouteExpenseType.Id > 0)
		{
			var existingVehicleRouteExpenseTypeByName = _vehicleRouteExpenseTypesAll.FirstOrDefault(_ => _.Id != _vehicleRouteExpenseType.Id && _.Name.Equals(_vehicleRouteExpenseType.Name, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleRouteExpenseTypeByName is not null)
				throw new Exception($"Vehicle Route Expense Type name '{_vehicleRouteExpenseType.Name}' already exists. Please choose a different name.");

			var existingVehicleRouteExpenseTypeByCode = _vehicleRouteExpenseTypesAll.FirstOrDefault(_ => _.Id != _vehicleRouteExpenseType.Id && _.Code.Equals(_vehicleRouteExpenseType.Code, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleRouteExpenseTypeByCode is not null)
				throw new Exception($"Vehicle Route Expense Type code '{_vehicleRouteExpenseType.Code}' already exists. Please choose a different code.");
		}
		else
		{
			var existingVehicleRouteExpenseTypeByName = _vehicleRouteExpenseTypesAll.FirstOrDefault(_ => _.Name.Equals(_vehicleRouteExpenseType.Name, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleRouteExpenseTypeByName is not null)
				throw new Exception($"Vehicle Route Expense Type name '{_vehicleRouteExpenseType.Name}' already exists. Please choose a different name.");

			var existingVehicleRouteExpenseTypeByCode = _vehicleRouteExpenseTypesAll.FirstOrDefault(_ => _.Code.Equals(_vehicleRouteExpenseType.Code, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleRouteExpenseTypeByCode is not null)
				throw new Exception($"Vehicle Route Expense Type code '{_vehicleRouteExpenseType.Code}' already exists. Please choose a different code.");
		}
	}

	private async Task SaveVehicleRouteExpenseType()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			if (_vehicleRouteExpenseType.Id == 0)
				_vehicleRouteExpenseType.Code = await GenerateCodes.GenerateVehicleRouteExpenseTypeCode();

			await ValidateForm();
			await VehicleRouteExpenseTypeData.InsertVehicleRouteExpenseType(_vehicleRouteExpenseType);

			await _toastNotification.ShowAsync("Success", $"Vehicle Route Expense Type '{_vehicleRouteExpenseType.Name}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleRouteExpenseTypeMaster, true);
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
	private async Task OnEditVehicleRouteExpenseType(VehicleRouteExpenseTypeModel vehicleRouteExpenseType)
	{
		_vehicleRouteExpenseType = await CommonData.LoadTableDataById<VehicleRouteExpenseTypeModel>(FleetNames.VehicleRouteExpenseType, vehicleRouteExpenseType.Id)
			?? throw new Exception("Vehicle Route Expense Type not found.");

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

			var vehicleRouteExpenseType = _vehicleRouteExpenseTypesAll.FirstOrDefault(o => o.Id == _deleteVehicleRouteExpenseTypeId)
				?? throw new Exception("Vehicle Route Expense Type not found.");

			vehicleRouteExpenseType.Status = false;
			await VehicleRouteExpenseTypeData.InsertVehicleRouteExpenseType(vehicleRouteExpenseType);

			await _toastNotification.ShowAsync("Success", $"Vehicle Route Expense Type '{vehicleRouteExpenseType.Name}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleRouteExpenseTypeMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete Vehicle Route Expense Type: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteVehicleRouteExpenseTypeId = 0;
			_deleteVehicleRouteExpenseTypeName = string.Empty;
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

			var vehicleRouteExpenseType = _vehicleRouteExpenseTypesAll.FirstOrDefault(o => o.Id == _recoverVehicleRouteExpenseTypeId)
				?? throw new Exception("Vehicle Route Expense Type not found.");

			vehicleRouteExpenseType.Status = true;
			await VehicleRouteExpenseTypeData.InsertVehicleRouteExpenseType(vehicleRouteExpenseType);

			await _toastNotification.ShowAsync("Success", $"Vehicle Route Expense Type '{vehicleRouteExpenseType.Name}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleRouteExpenseTypeMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover Vehicle Route Expense Type: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverVehicleRouteExpenseTypeId = 0;
			_recoverVehicleRouteExpenseTypeName = string.Empty;
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

			var (stream, fileName) = await VehicleRouteExpenseTypeExport.ExportMaster(_vehicleRouteExpenseTypes, ReportExportType.Excel);
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

			var (stream, fileName) = await VehicleRouteExpenseTypeExport.ExportMaster(_vehicleRouteExpenseTypes, ReportExportType.PDF);
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
			case "SaveVehicleRouteExpenseType":
				await SaveVehicleRouteExpenseType();
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

	private async Task OnVehicleRouteExpenseTypeGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleRouteExpenseTypeModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditVehicleRouteExpenseType":
				await EditSelectedItem();
				break;
			case "DeleteRecoverVehicleRouteExpenseType":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			await OnEditVehicleRouteExpenseType(selectedRecords[0]);
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
		_deleteVehicleRouteExpenseTypeId = id;
		_deleteVehicleRouteExpenseTypeName = name;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteVehicleRouteExpenseTypeId = 0;
		_deleteVehicleRouteExpenseTypeName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation(int id, string name)
	{
		_recoverVehicleRouteExpenseTypeId = id;
		_recoverVehicleRouteExpenseTypeName = name;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverVehicleRouteExpenseTypeId = 0;
		_recoverVehicleRouteExpenseTypeName = string.Empty;
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