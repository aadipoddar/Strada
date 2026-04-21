using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Fleet.Vehicle;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.Vehicle;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.Vehicle;

public partial class VehiclePage : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private VehicleModel _vehicle = new() { PurchaseDate = DateTime.Today };
	private VehicleTypeModel _selectedVehicleType;
	private CompanyModel _selectedCompany;

	private List<VehicleModel> _vehicles = [];
	private List<VehicleTypeModel> _vehicleTypes = [];
	private List<CompanyModel> _companies = [];
	private readonly List<ContextMenuItemModel> _vehicleGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditVehicle", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverVehicle", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<VehicleModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteVehicleId = 0;
	private string _deleteVehicleCode = string.Empty;

	private int _recoverVehicleId = 0;
	private string _recoverVehicleCode = string.Empty;

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
			.Add(ModCode.Ctrl, Code.S, SaveVehicle, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

		_vehicles = await CommonData.LoadTableData<VehicleModel>(FleetNames.Vehicle);
		_vehicleTypes = await CommonData.LoadTableData<VehicleTypeModel>(FleetNames.VehicleType);
		_companies = await CommonData.LoadTableData<CompanyModel>(AccountNames.Company);
		_selectedVehicleType = _vehicleTypes.FirstOrDefault(vt => vt.Id == _vehicle.VehicleTypeId);
		_selectedCompany = _companies.FirstOrDefault(c => c.Id == _vehicle.CompanyId);

		if (!_showDeleted)
			_vehicles = [.. _vehicles.Where(v => v.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion

	#region Saving
	private async Task ValidateForm()
	{
		if (!_user.Admin)
			throw new Exception("You do not have permission to perform this action.");

		_vehicle.VehicleTypeId = _selectedVehicleType?.Id ?? 0;
		_vehicle.CompanyId = _selectedCompany?.Id ?? 0;

		_vehicle.Code = _vehicle.Code?.Trim() ?? "";
		_vehicle.Code = _vehicle.Code?.ToUpper() ?? "";

		_vehicle.ShortCode = _vehicle.ShortCode?.Trim() ?? "";
		_vehicle.ShortCode = _vehicle.ShortCode?.ToUpper() ?? "";

		_vehicle.ChasisCode = _vehicle.ChasisCode?.Trim() ?? "";
		_vehicle.ChasisCode = _vehicle.ChasisCode?.ToUpper() ?? "";

		_vehicle.EngineCode = _vehicle.EngineCode?.Trim() ?? "";
		_vehicle.EngineCode = _vehicle.EngineCode?.ToUpper() ?? "";

		_vehicle.Remarks = _vehicle.Remarks?.Trim() ?? "";
		_vehicle.Status = true;

		if (string.IsNullOrWhiteSpace(_vehicle.Code))
			throw new Exception("Vehicle code is required. Please enter a valid vehicle code.");

		if (string.IsNullOrWhiteSpace(_vehicle.ShortCode))
			throw new Exception("Vehicle short code is required. Please enter a valid short code.");

		if (_vehicle.VehicleTypeId <= 0)
			throw new Exception("Vehicle Type is required. Please select a valid vehicle type.");

		if (_vehicle.CompanyId <= 0)
			throw new Exception("Company is required. Please select a valid company.");

		if (_vehicle.PurchaseDate == default)
			throw new Exception("Purchase date is required. Please select a valid purchase date.");

		if (_vehicle.OpeningKM < 0)
			throw new Exception("Opening KM cannot be negative.");

		if (string.IsNullOrWhiteSpace(_vehicle.ChasisCode))
			_vehicle.ChasisCode = null;

		if (string.IsNullOrWhiteSpace(_vehicle.EngineCode))
			_vehicle.EngineCode = null;

		if (string.IsNullOrWhiteSpace(_vehicle.Remarks))
			_vehicle.Remarks = null;

		if (_vehicle.Id > 0)
		{
			var existingVehicleCode = _vehicles.FirstOrDefault(_ => _.Id != _vehicle.Id && _.Code.Equals(_vehicle.Code, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleCode is not null)
				throw new Exception($"Vehicle code '{_vehicle.Code}' already exists. Please choose a different code.");

			if (!string.IsNullOrWhiteSpace(_vehicle.ChasisCode))
			{
				var existingChasisCode = _vehicles.FirstOrDefault(_ => _.Id != _vehicle.Id && !string.IsNullOrWhiteSpace(_.ChasisCode) && _.ChasisCode.Equals(_vehicle.ChasisCode, StringComparison.OrdinalIgnoreCase));
				if (existingChasisCode is not null)
					throw new Exception($"Chasis code '{_vehicle.ChasisCode}' already exists. Please choose a different chasis code.");
			}

			if (!string.IsNullOrWhiteSpace(_vehicle.EngineCode))
			{
				var existingEngineCode = _vehicles.FirstOrDefault(_ => _.Id != _vehicle.Id && !string.IsNullOrWhiteSpace(_.EngineCode) && _.EngineCode.Equals(_vehicle.EngineCode, StringComparison.OrdinalIgnoreCase));
				if (existingEngineCode is not null)
					throw new Exception($"Engine code '{_vehicle.EngineCode}' already exists. Please choose a different engine code.");
			}
		}
		else
		{
			var existingVehicleCode = _vehicles.FirstOrDefault(_ => _.Code.Equals(_vehicle.Code, StringComparison.OrdinalIgnoreCase));
			if (existingVehicleCode is not null)
				throw new Exception($"Vehicle code '{_vehicle.Code}' already exists. Please choose a different code.");

			if (!string.IsNullOrWhiteSpace(_vehicle.ChasisCode))
			{
				var existingChasisCode = _vehicles.FirstOrDefault(_ => !string.IsNullOrWhiteSpace(_.ChasisCode) && _.ChasisCode.Equals(_vehicle.ChasisCode, StringComparison.OrdinalIgnoreCase));
				if (existingChasisCode is not null)
					throw new Exception($"Chasis code '{_vehicle.ChasisCode}' already exists. Please choose a different chasis code.");
			}

			if (!string.IsNullOrWhiteSpace(_vehicle.EngineCode))
			{
				var existingEngineCode = _vehicles.FirstOrDefault(_ => !string.IsNullOrWhiteSpace(_.EngineCode) && _.EngineCode.Equals(_vehicle.EngineCode, StringComparison.OrdinalIgnoreCase));
				if (existingEngineCode is not null)
					throw new Exception($"Engine code '{_vehicle.EngineCode}' already exists. Please choose a different engine code.");
			}
		}
	}

	private async Task SaveVehicle()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			await ValidateForm();
			await VehicleData.InsertVehicle(_vehicle);

			await _toastNotification.ShowAsync("Success", $"Vehicle '{_vehicle.Code}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleMaster, true);
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
	private async Task OnEditVehicle(VehicleModel vehicle)
	{
		_vehicle = await CommonData.LoadTableDataById<VehicleModel>(FleetNames.Vehicle, vehicle.Id)
			?? throw new Exception("Vehicle not found.");
		_selectedVehicleType = _vehicleTypes.FirstOrDefault(vt => vt.Id == _vehicle.VehicleTypeId);
		_selectedCompany = _companies.FirstOrDefault(c => c.Id == _vehicle.CompanyId);

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

			var vehicle = _vehicles.FirstOrDefault(v => v.Id == _deleteVehicleId)
				?? throw new Exception("Vehicle not found.");

			vehicle.Status = false;
			await VehicleData.InsertVehicle(vehicle);

			await _toastNotification.ShowAsync("Success", $"Vehicle '{vehicle.Code}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete Vehicle: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteVehicleId = 0;
			_deleteVehicleCode = string.Empty;
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

			var vehicle = _vehicles.FirstOrDefault(v => v.Id == _recoverVehicleId)
				?? throw new Exception("Vehicle not found.");

			vehicle.Status = true;
			await VehicleData.InsertVehicle(vehicle);

			await _toastNotification.ShowAsync("Success", $"Vehicle '{vehicle.Code}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover Vehicle: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverVehicleId = 0;
			_recoverVehicleCode = string.Empty;
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

			var (stream, fileName) = await VehicleExport.ExportMaster(_vehicles, ReportExportType.Excel);
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

			var (stream, fileName) = await VehicleExport.ExportMaster(_vehicles, ReportExportType.PDF);
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
			case "NewVehicle":
				ResetPage();
				break;
			case "SaveVehicle":
				await SaveVehicle();
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

	private async Task OnVehicleGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditVehicle":
				await EditSelectedItem();
				break;
			case "DeleteRecoverVehicle":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			await OnEditVehicle(selectedRecords[0]);
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
		_deleteVehicleId = id;
		_deleteVehicleCode = code;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteVehicleId = 0;
		_deleteVehicleCode = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation(int id, string code)
	{
		_recoverVehicleId = id;
		_recoverVehicleCode = code;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverVehicleId = 0;
		_recoverVehicleCode = string.Empty;
		await _recoverConfirmationDialog.HideAsync();
	}

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadData();
		StateHasChanged();
	}

	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.VehicleMaster, true);

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
	#endregion

}