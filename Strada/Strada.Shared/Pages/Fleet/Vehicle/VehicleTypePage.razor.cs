using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Fleet.Vehicle;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.Vehicle;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.Vehicle;

public partial class VehicleTypePage : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private VehicleTypeModel _vehicleType = new();

	private List<VehicleTypeModel> _vehicleTypes = [];
	private List<VehicleTypeModel> _vehicleTypesAll = [];
	private readonly List<ContextMenuItemModel> _vehicleTypeGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditVehicleType", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverVehicleType", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<VehicleTypeModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteVehicleTypeId = 0;
	private string _deleteVehicleTypeName = string.Empty;

	private int _recoverVehicleTypeId = 0;
	private string _recoverVehicleTypeName = string.Empty;

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
			.Add(ModCode.Ctrl, Code.S, SaveVehicleType, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

		_vehicleTypesAll = await CommonData.LoadTableData<VehicleTypeModel>(FleetNames.VehicleType);
		_vehicleTypes = [.. _vehicleTypesAll];

		if (!_showDeleted)
			_vehicleTypes = [.. _vehicleTypes.Where(vt => vt.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion

	#region Saving
	private async Task ValidateForm()
	{
		if (!_user.Admin)
			throw new Exception("You do not have permission to perform this action.");

		_vehicleType.Name = _vehicleType.Name?.Trim() ?? "";
		_vehicleType.Name = _vehicleType.Name?.ToUpper() ?? "";

		_vehicleType.Code = _vehicleType.Code?.Trim() ?? "";
		_vehicleType.Code = _vehicleType.Code?.ToUpper() ?? "";

		_vehicleType.Remarks = _vehicleType.Remarks?.Trim() ?? "";
		_vehicleType.Status = true;

		if (string.IsNullOrWhiteSpace(_vehicleType.Name))
			throw new Exception("Vehicle Type name is required. Please enter a valid vehicle type name.");

		if (string.IsNullOrWhiteSpace(_vehicleType.Code))
			throw new Exception("Vehicle Type code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(_vehicleType.Remarks))
			_vehicleType.Remarks = null;

		if (_vehicleType.Id > 0)
		{
			var existingVehicleTypeByName = _vehicleTypesAll.FirstOrDefault(_ => _.Id != _vehicleType.Id && _.Name.Equals(_vehicleType.Name, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleTypeByName is not null)
				throw new Exception($"Vehicle Type name '{_vehicleType.Name}' already exists. Please choose a different name.");

			var existingVehicleTypeByCode = _vehicleTypesAll.FirstOrDefault(_ => _.Id != _vehicleType.Id && _.Code.Equals(_vehicleType.Code, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleTypeByCode is not null)
				throw new Exception($"Vehicle Type code '{_vehicleType.Code}' already exists. Please choose a different code.");
		}
		else
		{
			var existingVehicleTypeByName = _vehicleTypesAll.FirstOrDefault(_ => _.Name.Equals(_vehicleType.Name, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleTypeByName is not null)
				throw new Exception($"Vehicle Type name '{_vehicleType.Name}' already exists. Please choose a different name.");

			var existingVehicleTypeByCode = _vehicleTypesAll.FirstOrDefault(_ => _.Code.Equals(_vehicleType.Code, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleTypeByCode is not null)
				throw new Exception($"Vehicle Type code '{_vehicleType.Code}' already exists. Please choose a different code.");
		}
	}

	private async Task SaveVehicleType()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			if (_vehicleType.Id == 0)
				_vehicleType.Code = await GenerateCodes.GenerateVehicleTypeCode();

			await ValidateForm();
			await VehicleTypeData.InsertVehicleType(_vehicleType);

			await _toastNotification.ShowAsync("Success", $"Vehicle Type '{_vehicleType.Name}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleTypeMaster, true);
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
	private void OnEditVehicleType(VehicleTypeModel vehicleType)
	{
		_vehicleType = new()
		{
			Id = vehicleType.Id,
			Name = vehicleType.Name,
			Code = vehicleType.Code,
			Remarks = vehicleType.Remarks,
			Status = vehicleType.Status
		};

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

			var vehicleType = _vehicleTypesAll.FirstOrDefault(vt => vt.Id == _deleteVehicleTypeId)
				?? throw new Exception("Vehicle Type not found.");

			vehicleType.Status = false;
			await VehicleTypeData.InsertVehicleType(vehicleType);

			await _toastNotification.ShowAsync("Success", $"Vehicle Type '{vehicleType.Name}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleTypeMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete Vehicle Type: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteVehicleTypeId = 0;
			_deleteVehicleTypeName = string.Empty;
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

			var vehicleType = _vehicleTypesAll.FirstOrDefault(vt => vt.Id == _recoverVehicleTypeId)
				?? throw new Exception("Vehicle Type not found.");

			vehicleType.Status = true;
			await VehicleTypeData.InsertVehicleType(vehicleType);

			await _toastNotification.ShowAsync("Success", $"Vehicle Type '{vehicleType.Name}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleTypeMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover Vehicle Type: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverVehicleTypeId = 0;
			_recoverVehicleTypeName = string.Empty;
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

			var (stream, fileName) = await VehicleTypeExport.ExportMaster(_vehicleTypes, ReportExportType.Excel);
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

			var (stream, fileName) = await VehicleTypeExport.ExportMaster(_vehicleTypes, ReportExportType.PDF);
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
			case "NewVehicleType":
				ResetPage();
				break;
			case "SaveVehicleType":
				await SaveVehicleType();
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

	private async Task OnVehicleTypeGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleTypeModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditVehicleType":
				await EditSelectedItem();
				break;
			case "DeleteRecoverVehicleType":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			OnEditVehicleType(selectedRecords[0]);
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
		_deleteVehicleTypeId = id;
		_deleteVehicleTypeName = name;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteVehicleTypeId = 0;
		_deleteVehicleTypeName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation(int id, string name)
	{
		_recoverVehicleTypeId = id;
		_recoverVehicleTypeName = name;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverVehicleTypeId = 0;
		_recoverVehicleTypeName = string.Empty;
		await _recoverConfirmationDialog.HideAsync();
	}

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadData();
		StateHasChanged();
	}

	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.VehicleTypeMaster, true);

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
	#endregion

}