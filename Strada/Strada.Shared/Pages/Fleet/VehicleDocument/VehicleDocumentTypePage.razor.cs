using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Fleet.VehicleDocument;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.VehicleDocument;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleDocument;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Fleet.VehicleDocument;

public partial class VehicleDocumentTypePage : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private VehicleDocumentTypeModel _vehicleDocumentType = new();

	private List<VehicleDocumentTypeModel> _vehicleDocumentTypes = [];
	private List<VehicleDocumentTypeModel> _vehicleDocumentTypesAll = [];
	private readonly List<ContextMenuItemModel> _vehicleDocumentTypeGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditVehicleDocumentType", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverVehicleDocumentType", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<VehicleDocumentTypeModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteVehicleDocumentTypeId = 0;
	private string _deleteVehicleDocumentTypeName = string.Empty;

	private int _recoverVehicleDocumentTypeId = 0;
	private string _recoverVehicleDocumentTypeName = string.Empty;

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
			.Add(ModCode.Ctrl, Code.S, SaveVehicleDocumentType, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

		_vehicleDocumentTypesAll = await CommonData.LoadTableData<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType);
		_vehicleDocumentTypes = [.. _vehicleDocumentTypesAll];

		if (!_showDeleted)
			_vehicleDocumentTypes = [.. _vehicleDocumentTypes.Where(vdt => vdt.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion

	#region Saving
	private async Task ValidateForm()
	{
		if (!_user.Admin)
			throw new Exception("You do not have permission to perform this action.");

		_vehicleDocumentType.Name = _vehicleDocumentType.Name?.Trim() ?? "";
		_vehicleDocumentType.Name = _vehicleDocumentType.Name?.ToUpper() ?? "";

		_vehicleDocumentType.Code = _vehicleDocumentType.Code?.Trim() ?? "";
		_vehicleDocumentType.Code = _vehicleDocumentType.Code?.ToUpper() ?? "";

		_vehicleDocumentType.Remarks = _vehicleDocumentType.Remarks?.Trim() ?? "";
		_vehicleDocumentType.Status = true;

		if (string.IsNullOrWhiteSpace(_vehicleDocumentType.Name))
			throw new Exception("Vehicle Document Type name is required. Please enter a valid document type name.");

		if (string.IsNullOrWhiteSpace(_vehicleDocumentType.Code))
			throw new Exception("Vehicle Document Type code is required. Please try again.");

		if (_vehicleDocumentType.Rate < 0)
			throw new Exception("Rate cannot be negative.");

		if (string.IsNullOrWhiteSpace(_vehicleDocumentType.Remarks))
			_vehicleDocumentType.Remarks = null;

		if (_vehicleDocumentType.Id > 0)
		{
			var existingByName = _vehicleDocumentTypesAll.FirstOrDefault(_ => _.Id != _vehicleDocumentType.Id && _.Name.Equals(_vehicleDocumentType.Name, StringComparison.OrdinalIgnoreCase));
			if (existingByName is not null)
				throw new Exception($"Vehicle Document Type name '{_vehicleDocumentType.Name}' already exists. Please choose a different name.");

			var existingByCode = _vehicleDocumentTypesAll.FirstOrDefault(_ => _.Id != _vehicleDocumentType.Id && _.Code.Equals(_vehicleDocumentType.Code, StringComparison.OrdinalIgnoreCase));
			if (existingByCode is not null)
				throw new Exception($"Vehicle Document Type code '{_vehicleDocumentType.Code}' already exists. Please choose a different code.");
		}
		else
		{
			var existingByName = _vehicleDocumentTypesAll.FirstOrDefault(_ => _.Name.Equals(_vehicleDocumentType.Name, StringComparison.OrdinalIgnoreCase));
			if (existingByName is not null)
				throw new Exception($"Vehicle Document Type name '{_vehicleDocumentType.Name}' already exists. Please choose a different name.");

			var existingByCode = _vehicleDocumentTypesAll.FirstOrDefault(_ => _.Code.Equals(_vehicleDocumentType.Code, StringComparison.OrdinalIgnoreCase));
			if (existingByCode is not null)
				throw new Exception($"Vehicle Document Type code '{_vehicleDocumentType.Code}' already exists. Please choose a different code.");
		}
	}

	private async Task SaveVehicleDocumentType()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			if (_vehicleDocumentType.Id == 0)
				_vehicleDocumentType.Code = await GenerateCodes.GenerateVehicleDocumentTypeCode();

			await ValidateForm();
			await VehicleDocumentTypeData.InsertVehicleDocumentType(_vehicleDocumentType);

			await _toastNotification.ShowAsync("Success", $"Vehicle Document Type '{_vehicleDocumentType.Name}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleDocumentTypeMaster, true);
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
	private void OnEditVehicleDocumentType(VehicleDocumentTypeModel vehicleDocumentType)
	{
		_vehicleDocumentType = new()
		{
			Id = vehicleDocumentType.Id,
			Name = vehicleDocumentType.Name,
			Code = vehicleDocumentType.Code,
			Rate = vehicleDocumentType.Rate,
			Remarks = vehicleDocumentType.Remarks,
			Status = vehicleDocumentType.Status
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

			var vehicleDocumentType = _vehicleDocumentTypesAll.FirstOrDefault(vdt => vdt.Id == _deleteVehicleDocumentTypeId)
				?? throw new Exception("Vehicle Document Type not found.");

			vehicleDocumentType.Status = false;
			await VehicleDocumentTypeData.InsertVehicleDocumentType(vehicleDocumentType);

			await _toastNotification.ShowAsync("Success", $"Vehicle Document Type '{vehicleDocumentType.Name}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleDocumentTypeMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete Vehicle Document Type: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteVehicleDocumentTypeId = 0;
			_deleteVehicleDocumentTypeName = string.Empty;
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

			var vehicleDocumentType = _vehicleDocumentTypesAll.FirstOrDefault(vdt => vdt.Id == _recoverVehicleDocumentTypeId)
				?? throw new Exception("Vehicle Document Type not found.");

			vehicleDocumentType.Status = true;
			await VehicleDocumentTypeData.InsertVehicleDocumentType(vehicleDocumentType);

			await _toastNotification.ShowAsync("Success", $"Vehicle Document Type '{vehicleDocumentType.Name}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleDocumentTypeMaster, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover Vehicle Document Type: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverVehicleDocumentTypeId = 0;
			_recoverVehicleDocumentTypeName = string.Empty;
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

			var (stream, fileName) = await VehicleDocumentTypeExport.ExportMaster(_vehicleDocumentTypes, ReportExportType.Excel);
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

			var (stream, fileName) = await VehicleDocumentTypeExport.ExportMaster(_vehicleDocumentTypes, ReportExportType.PDF);
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
			case "NewVehicleDocumentType":
				ResetPage();
				break;
			case "SaveVehicleDocumentType":
				await SaveVehicleDocumentType();
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

	private async Task OnVehicleDocumentTypeGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleDocumentTypeModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditVehicleDocumentType":
				await EditSelectedItem();
				break;
			case "DeleteRecoverVehicleDocumentType":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			OnEditVehicleDocumentType(selectedRecords[0]);
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
		_deleteVehicleDocumentTypeId = id;
		_deleteVehicleDocumentTypeName = name;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteVehicleDocumentTypeId = 0;
		_deleteVehicleDocumentTypeName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation(int id, string name)
	{
		_recoverVehicleDocumentTypeId = id;
		_recoverVehicleDocumentTypeName = name;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverVehicleDocumentTypeId = 0;
		_recoverVehicleDocumentTypeName = string.Empty;
		await _recoverConfirmationDialog.HideAsync();
	}

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadData();
		StateHasChanged();
	}

	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.VehicleDocumentTypeMaster, true);

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
	#endregion
}
