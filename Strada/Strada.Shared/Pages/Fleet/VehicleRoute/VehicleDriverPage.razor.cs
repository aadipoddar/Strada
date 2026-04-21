using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Fleet.VehicleRoute;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.VehicleRoute;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleRoute;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.VehicleRoute;

public partial class VehicleDriverPage : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private VehicleDriverModel _vehicleDriver = new();

	private List<VehicleDriverModel> _vehicleDrivers = [];
	private List<VehicleDriverModel> _vehicleDriversAll = [];
	private readonly List<ContextMenuItemModel> _vehicleDriverGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditVehicleDriver", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverVehicleDriver", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<VehicleDriverModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteVehicleDriverId = 0;
	private string _deleteVehicleDriverName = string.Empty;

	private int _recoverVehicleDriverId = 0;
	private string _recoverVehicleDriverName = string.Empty;

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
			.Add(ModCode.Ctrl, Code.S, SaveVehicleDriver, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

		_vehicleDriversAll = await CommonData.LoadTableData<VehicleDriverModel>(FleetNames.VehicleDriver);
		_vehicleDrivers = [.. _vehicleDriversAll];

		if (!_showDeleted)
			_vehicleDrivers = [.. _vehicleDrivers.Where(vehicleDriver => vehicleDriver.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion

	#region Saving
	private async Task ValidateForm()
	{
		if (!_user.Admin)
			throw new Exception("You do not have permission to perform this action.");

		_vehicleDriver.Name = _vehicleDriver.Name?.Trim() ?? "";
		_vehicleDriver.Name = _vehicleDriver.Name?.ToUpper() ?? "";

		_vehicleDriver.Mobile = _vehicleDriver.Mobile?.Trim() ?? "";

		_vehicleDriver.Code = _vehicleDriver.Code?.Trim() ?? "";
		_vehicleDriver.Code = _vehicleDriver.Code?.ToUpper() ?? "";

		_vehicleDriver.Remarks = _vehicleDriver.Remarks?.Trim() ?? "";
		_vehicleDriver.Status = true;

		if (string.IsNullOrWhiteSpace(_vehicleDriver.Name))
			throw new Exception("Vehicle Driver name is required. Please enter a valid vehicle driver name.");

		if (string.IsNullOrWhiteSpace(_vehicleDriver.Mobile))
			throw new Exception("Mobile is required. Please enter a valid mobile number.");

		if (!_vehicleDriver.Mobile.ValidatePhoneNumber())
			throw new Exception("Mobile must be exactly 10 numeric digits.");

		if (string.IsNullOrWhiteSpace(_vehicleDriver.Code))
			throw new Exception("Vehicle Driver code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(_vehicleDriver.Remarks))
			_vehicleDriver.Remarks = null;

		if (_vehicleDriver.Id > 0)
		{
			var existingVehicleDriverByName = _vehicleDriversAll.FirstOrDefault(_ => _.Id != _vehicleDriver.Id && _.Name.Equals(_vehicleDriver.Name, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleDriverByName is not null)
				throw new Exception($"Vehicle Driver name '{_vehicleDriver.Name}' already exists. Please choose a different name.");

			var existingVehicleDriverByMobile = _vehicleDriversAll.FirstOrDefault(_ => _.Id != _vehicleDriver.Id && _.Mobile.Equals(_vehicleDriver.Mobile, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleDriverByMobile is not null)
				throw new Exception($"Mobile '{_vehicleDriver.Mobile}' already exists. Please choose a different mobile number.");

			var existingVehicleDriverByCode = _vehicleDriversAll.FirstOrDefault(_ => _.Id != _vehicleDriver.Id && _.Code.Equals(_vehicleDriver.Code, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleDriverByCode is not null)
				throw new Exception($"Vehicle Driver code '{_vehicleDriver.Code}' already exists. Please choose a different code.");
		}
		else
		{
			var existingVehicleDriverByName = _vehicleDriversAll.FirstOrDefault(_ => _.Name.Equals(_vehicleDriver.Name, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleDriverByName is not null)
				throw new Exception($"Vehicle Driver name '{_vehicleDriver.Name}' already exists. Please choose a different name.");

			var existingVehicleDriverByMobile = _vehicleDriversAll.FirstOrDefault(_ => _.Mobile.Equals(_vehicleDriver.Mobile, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleDriverByMobile is not null)
				throw new Exception($"Mobile '{_vehicleDriver.Mobile}' already exists. Please choose a different mobile number.");

			var existingVehicleDriverByCode = _vehicleDriversAll.FirstOrDefault(_ => _.Code.Equals(_vehicleDriver.Code, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleDriverByCode is not null)
				throw new Exception($"Vehicle Driver code '{_vehicleDriver.Code}' already exists. Please choose a different code.");
		}
	}

	private async Task SaveVehicleDriver()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			if (_vehicleDriver.Id == 0)
				_vehicleDriver.Code = await GenerateCodes.GenerateVehicleDriverCode();

			await ValidateForm();
			await VehicleDriverData.InsertVehicleDriver(_vehicleDriver);

			await _toastNotification.ShowAsync("Success", $"Vehicle Driver '{_vehicleDriver.Name}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleDriverMaster, true);
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
	private async Task OnEditVehicleDriver(VehicleDriverModel vehicleDriver)
	{
		_vehicleDriver = await CommonData.LoadTableDataById<VehicleDriverModel>(FleetNames.VehicleDriver, vehicleDriver.Id)
			?? throw new Exception("Vehicle Driver not found.");
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

			var vehicleDriver = _vehicleDriversAll.FirstOrDefault(vd => vd.Id == _deleteVehicleDriverId)
				?? throw new Exception("Vehicle Driver not found.");

			vehicleDriver.Status = false;
			await VehicleDriverData.InsertVehicleDriver(vehicleDriver);

			await _toastNotification.ShowAsync("Success", $"Vehicle Driver '{vehicleDriver.Name}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleDriverMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete Vehicle Driver: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteVehicleDriverId = 0;
			_deleteVehicleDriverName = string.Empty;
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

			var vehicleDriver = _vehicleDriversAll.FirstOrDefault(vd => vd.Id == _recoverVehicleDriverId)
				?? throw new Exception("Vehicle Driver not found.");

			vehicleDriver.Status = true;
			await VehicleDriverData.InsertVehicleDriver(vehicleDriver);

			await _toastNotification.ShowAsync("Success", $"Vehicle Driver '{vehicleDriver.Name}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleDriverMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover Vehicle Driver: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverVehicleDriverId = 0;
			_recoverVehicleDriverName = string.Empty;
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

			var (stream, fileName) = await VehicleDriverExport.ExportMaster(_vehicleDrivers, ReportExportType.Excel);
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

			var (stream, fileName) = await VehicleDriverExport.ExportMaster(_vehicleDrivers, ReportExportType.PDF);
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
			case "NewVehicleDriver":
				ResetPage();
				break;
			case "SaveVehicleDriver":
				await SaveVehicleDriver();
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

	private async Task OnVehicleDriverGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleDriverModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditVehicleDriver":
				await EditSelectedItem();
				break;
			case "DeleteRecoverVehicleDriver":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			await OnEditVehicleDriver(selectedRecords[0]);
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
		_deleteVehicleDriverId = id;
		_deleteVehicleDriverName = name;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteVehicleDriverId = 0;
		_deleteVehicleDriverName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation(int id, string name)
	{
		_recoverVehicleDriverId = id;
		_recoverVehicleDriverName = name;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverVehicleDriverId = 0;
		_recoverVehicleDriverName = string.Empty;
		await _recoverConfirmationDialog.HideAsync();
	}

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadData();
		StateHasChanged();
	}

	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.VehicleDriverMaster, true);

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
	#endregion
}