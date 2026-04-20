using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Fleet.VehicleRoute;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.VehicleRoute;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleRoute;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.VehicleRoute;

public partial class VehicleRouteLocationPage : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private VehicleRouteLocationModel _routeLocation = new();

	private List<VehicleRouteLocationModel> _routeLocations = [];
	private List<VehicleRouteLocationModel> _routeLocationsAll = [];
	private readonly List<ContextMenuItemModel> _routeLocationGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditRouteLocation", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverRouteLocation", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<VehicleRouteLocationModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteRouteLocationId = 0;
	private string _deleteRouteLocationName = string.Empty;

	private int _recoverRouteLocationId = 0;
	private string _recoverRouteLocationName = string.Empty;

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
			.Add(ModCode.Ctrl, Code.S, SaveRouteLocation, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

		_routeLocationsAll = await CommonData.LoadTableData<VehicleRouteLocationModel>(FleetNames.VehicleRouteLocation);
		_routeLocations = [.. _routeLocationsAll];

		if (!_showDeleted)
			_routeLocations = [.. _routeLocations.Where(rl => rl.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion

	#region Saving
	private async Task ValidateForm()
	{
		if (!_user.Admin)
			throw new Exception("You do not have permission to perform this action.");

		_routeLocation.Name = _routeLocation.Name?.Trim() ?? "";
		_routeLocation.Name = _routeLocation.Name?.ToUpper() ?? "";

		_routeLocation.Code = _routeLocation.Code?.Trim() ?? "";
		_routeLocation.Code = _routeLocation.Code?.ToUpper() ?? "";

		_routeLocation.Remarks = _routeLocation.Remarks?.Trim() ?? "";
		_routeLocation.Status = true;

		if (string.IsNullOrWhiteSpace(_routeLocation.Name))
			throw new Exception("Route Location name is required. Please enter a valid route location name.");

		if (string.IsNullOrWhiteSpace(_routeLocation.Code))
			throw new Exception("Route Location code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(_routeLocation.Remarks))
			_routeLocation.Remarks = null;

		if (_routeLocation.Id > 0)
		{
			var existingRouteLocationByName = _routeLocationsAll.FirstOrDefault(_ => _.Id != _routeLocation.Id && _.Name.Equals(_routeLocation.Name, StringComparison.OrdinalIgnoreCase));
			if (existingRouteLocationByName is not null)
				throw new Exception($"Route Location name '{_routeLocation.Name}' already exists. Please choose a different name.");

			var existingRouteLocationByCode = _routeLocationsAll.FirstOrDefault(_ => _.Id != _routeLocation.Id && _.Code.Equals(_routeLocation.Code, StringComparison.OrdinalIgnoreCase));
			if (existingRouteLocationByCode is not null)
				throw new Exception($"Route Location code '{_routeLocation.Code}' already exists. Please choose a different code.");
		}
		else
		{
			var existingRouteLocationByName = _routeLocationsAll.FirstOrDefault(_ => _.Name.Equals(_routeLocation.Name, StringComparison.OrdinalIgnoreCase));
			if (existingRouteLocationByName is not null)
				throw new Exception($"Route Location name '{_routeLocation.Name}' already exists. Please choose a different name.");

			var existingRouteLocationByCode = _routeLocationsAll.FirstOrDefault(_ => _.Code.Equals(_routeLocation.Code, StringComparison.OrdinalIgnoreCase));
			if (existingRouteLocationByCode is not null)
				throw new Exception($"Route Location code '{_routeLocation.Code}' already exists. Please choose a different code.");
		}
	}

	private async Task SaveRouteLocation()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			if (_routeLocation.Id == 0)
				_routeLocation.Code = await GenerateCodes.GenerateRouteLocationCode();

			await ValidateForm();
			await VehicleRouteLocationData.InsertRouteLocation(_routeLocation);

			await _toastNotification.ShowAsync("Success", $"Route Location '{_routeLocation.Name}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleRouteLocationMaster, true);
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
	private async Task OnEditRouteLocation(VehicleRouteLocationModel routeLocation)
	{
		_routeLocation = await CommonData.LoadTableDataById<VehicleRouteLocationModel>(FleetNames.VehicleRouteLocation, routeLocation.Id)
			?? throw new Exception("Route Location not found.");

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

			var routeLocation = _routeLocationsAll.FirstOrDefault(rl => rl.Id == _deleteRouteLocationId)
				?? throw new Exception("Route Location not found.");

			routeLocation.Status = false;
			await VehicleRouteLocationData.InsertRouteLocation(routeLocation);

			await _toastNotification.ShowAsync("Success", $"Route Location '{routeLocation.Name}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleRouteLocationMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete Route Location: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteRouteLocationId = 0;
			_deleteRouteLocationName = string.Empty;
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

			var routeLocation = _routeLocationsAll.FirstOrDefault(rl => rl.Id == _recoverRouteLocationId)
				?? throw new Exception("Route Location not found.");

			routeLocation.Status = true;
			await VehicleRouteLocationData.InsertRouteLocation(routeLocation);

			await _toastNotification.ShowAsync("Success", $"Route Location '{routeLocation.Name}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleRouteLocationMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover Route Location: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverRouteLocationId = 0;
			_recoverRouteLocationName = string.Empty;
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

			var (stream, fileName) = await VehicleRouteLocationExport.ExportMaster(_routeLocations, ReportExportType.Excel);
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

			var (stream, fileName) = await VehicleRouteLocationExport.ExportMaster(_routeLocations, ReportExportType.PDF);
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
			case "NewRouteLocation":
				ResetPage();
				break;
			case "SaveRouteLocation":
				await SaveRouteLocation();
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

	private async Task OnRouteLocationGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleRouteLocationModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditRouteLocation":
				await EditSelectedItem();
				break;
			case "DeleteRecoverRouteLocation":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			await OnEditRouteLocation(selectedRecords[0]);
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
		_deleteRouteLocationId = id;
		_deleteRouteLocationName = name;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteRouteLocationId = 0;
		_deleteRouteLocationName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation(int id, string name)
	{
		_recoverRouteLocationId = id;
		_recoverRouteLocationName = name;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverRouteLocationId = 0;
		_recoverRouteLocationName = string.Empty;
		await _recoverConfirmationDialog.HideAsync();
	}

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadData();
		StateHasChanged();
	}

	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.VehicleRouteLocationMaster, true);

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
	#endregion
}
