using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Fleet.VehicleRoute;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.VehicleRoute;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleRoute;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.VehicleRoute;

public partial class VehicleRoutePage : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private VehicleRouteModel _vehicleRoute = new();
	private VehicleRouteLocationModel _selectedFromLocation;
	private VehicleRouteLocationModel _selectedToLocation;

	private List<VehicleRouteModel> _vehicleRoutes = [];
	private List<VehicleRouteModel> _vehicleRoutesAll = [];
	private List<VehicleRouteLocationModel> _routeLocations = [];
	private readonly List<ContextMenuItemModel> _vehicleRouteGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditVehicleRoute", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverVehicleRoute", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<VehicleRouteModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteVehicleRouteId = 0;
	private string _deleteVehicleRouteCode = string.Empty;

	private int _recoverVehicleRouteId = 0;
	private string _recoverVehicleRouteCode = string.Empty;

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
			.Add(ModCode.Ctrl, Code.S, SaveVehicleRoute, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

		_vehicleRoutesAll = await CommonData.LoadTableData<VehicleRouteModel>(FleetNames.VehicleRoute);
		_vehicleRoutes = [.. _vehicleRoutesAll];
		_routeLocations = await CommonData.LoadTableData<VehicleRouteLocationModel>(FleetNames.VehicleRouteLocation);
		_selectedFromLocation = _routeLocations.FirstOrDefault(rl => rl.Id == _vehicleRoute.FromLocationId);
		_selectedToLocation = _routeLocations.FirstOrDefault(rl => rl.Id == _vehicleRoute.ToLocationId);

		if (!_showDeleted)
			_vehicleRoutes = [.. _vehicleRoutes.Where(vr => vr.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion

	#region Saving
	private async Task ValidateForm()
	{
		if (!_user.Admin)
			throw new Exception("You do not have permission to perform this action.");

		_vehicleRoute.FromLocationId = _selectedFromLocation?.Id ?? 0;
		_vehicleRoute.ToLocationId = _selectedToLocation?.Id ?? 0;

		_vehicleRoute.Code = _vehicleRoute.Code?.Trim() ?? "";
		_vehicleRoute.Code = _vehicleRoute.Code?.ToUpper() ?? "";

		_vehicleRoute.Remarks = _vehicleRoute.Remarks?.Trim() ?? "";
		_vehicleRoute.Status = true;

		if (_vehicleRoute.FromLocationId <= 0)
			throw new Exception("From location is required. Please select a valid from location.");

		if (_vehicleRoute.ToLocationId <= 0)
			throw new Exception("To location is required. Please select a valid to location.");

		if (_vehicleRoute.FromLocationId == _vehicleRoute.ToLocationId)
			throw new Exception("From location and to location cannot be the same.");

		VehicleRouteModel existingVehicleRouteByLocationPair;
		if (_vehicleRoute.Id > 0)
			existingVehicleRouteByLocationPair = _vehicleRoutesAll.FirstOrDefault(_ => _.Id != _vehicleRoute.Id && _.FromLocationId == _vehicleRoute.FromLocationId && _.ToLocationId == _vehicleRoute.ToLocationId);
		else
			existingVehicleRouteByLocationPair = _vehicleRoutesAll.FirstOrDefault(_ => _.FromLocationId == _vehicleRoute.FromLocationId && _.ToLocationId == _vehicleRoute.ToLocationId);

		if (existingVehicleRouteByLocationPair is not null)
		{
			var fromLocationName = _routeLocations.FirstOrDefault(rl => rl.Id == _vehicleRoute.FromLocationId)?.Name ?? _vehicleRoute.FromLocationId.ToString();
			var toLocationName = _routeLocations.FirstOrDefault(rl => rl.Id == _vehicleRoute.ToLocationId)?.Name ?? _vehicleRoute.ToLocationId.ToString();
			throw new Exception($"Vehicle route '{fromLocationName} -> {toLocationName}' already exists. Duplicate route entries are not allowed.");
		}

		if (string.IsNullOrWhiteSpace(_vehicleRoute.Code))
			throw new Exception("Route code is required. Please enter a valid route code.");

		if (string.IsNullOrWhiteSpace(_vehicleRoute.Remarks))
			_vehicleRoute.Remarks = null;

		if (_vehicleRoute.Id > 0)
		{
			var existingVehicleRouteByCode = _vehicleRoutesAll.FirstOrDefault(_ => _.Id != _vehicleRoute.Id && _.Code.Equals(_vehicleRoute.Code, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleRouteByCode is not null)
				throw new Exception($"Vehicle route code '{_vehicleRoute.Code}' already exists. Please choose a different code.");
		}
		else
		{
			var existingVehicleRouteByCode = _vehicleRoutesAll.FirstOrDefault(_ => _.Code.Equals(_vehicleRoute.Code, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleRouteByCode is not null)
				throw new Exception($"Vehicle route code '{_vehicleRoute.Code}' already exists. Please choose a different code.");
		}
	}

	private async Task SaveVehicleRoute()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			await ValidateForm();
			await VehicleRouteData.InsertVehicleRoute(_vehicleRoute);

			await _toastNotification.ShowAsync("Success", $"Vehicle Route '{_vehicleRoute.Code}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleRouteMaster, true);
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
	private void OnEditVehicleRoute(VehicleRouteModel vehicleRoute)
	{
		_vehicleRoute = new()
		{
			Id = vehicleRoute.Id,
			FromLocationId = vehicleRoute.FromLocationId,
			ToLocationId = vehicleRoute.ToLocationId,
			Code = vehicleRoute.Code,
			Remarks = vehicleRoute.Remarks,
			Status = vehicleRoute.Status
		};
		_selectedFromLocation = _routeLocations.FirstOrDefault(rl => rl.Id == _vehicleRoute.FromLocationId);
		_selectedToLocation = _routeLocations.FirstOrDefault(rl => rl.Id == _vehicleRoute.ToLocationId);

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

			var vehicleRoute = _vehicleRoutesAll.FirstOrDefault(vr => vr.Id == _deleteVehicleRouteId)
				?? throw new Exception("Vehicle Route not found.");

			vehicleRoute.Status = false;
			await VehicleRouteData.InsertVehicleRoute(vehicleRoute);

			await _toastNotification.ShowAsync("Success", $"Vehicle Route '{vehicleRoute.Code}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleRouteMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete Vehicle Route: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteVehicleRouteId = 0;
			_deleteVehicleRouteCode = string.Empty;
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

			var vehicleRoute = _vehicleRoutesAll.FirstOrDefault(vr => vr.Id == _recoverVehicleRouteId)
				?? throw new Exception("Vehicle Route not found.");

			vehicleRoute.Status = true;
			await VehicleRouteData.InsertVehicleRoute(vehicleRoute);

			await _toastNotification.ShowAsync("Success", $"Vehicle Route '{vehicleRoute.Code}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleRouteMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover Vehicle Route: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverVehicleRouteId = 0;
			_recoverVehicleRouteCode = string.Empty;
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

			var (stream, fileName) = await VehicleRouteExport.ExportMaster(_vehicleRoutes, ReportExportType.Excel);
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

			var (stream, fileName) = await VehicleRouteExport.ExportMaster(_vehicleRoutes, ReportExportType.PDF);
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
			case "NewVehicleRoute":
				ResetPage();
				break;
			case "SaveVehicleRoute":
				await SaveVehicleRoute();
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

	private async Task OnVehicleRouteGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleRouteModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditVehicleRoute":
				await EditSelectedItem();
				break;
			case "DeleteRecoverVehicleRoute":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			OnEditVehicleRoute(selectedRecords[0]);
	}

	private async Task DeleteSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
		{
			if (selectedRecords[0].Status)
				await ShowDeleteConfirmation(selectedRecords[0].Id, selectedRecords[0].Code);
			else
				await ShowRecoverConfirmation(selectedRecords[0].Id, selectedRecords[0].Code);
		}
	}

	private async Task ShowDeleteConfirmation(int id, string code)
	{
		_deleteVehicleRouteId = id;
		_deleteVehicleRouteCode = code;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteVehicleRouteId = 0;
		_deleteVehicleRouteCode = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation(int id, string code)
	{
		_recoverVehicleRouteId = id;
		_recoverVehicleRouteCode = code;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverVehicleRouteId = 0;
		_recoverVehicleRouteCode = string.Empty;
		await _recoverConfirmationDialog.HideAsync();
	}

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadData();
		StateHasChanged();
	}

	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.VehicleRouteMaster, true);

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
	#endregion
}