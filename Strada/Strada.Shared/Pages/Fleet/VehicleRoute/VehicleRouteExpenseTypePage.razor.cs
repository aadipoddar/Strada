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
	private readonly List<ContextMenuItemModel> _gridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditSelectedItem", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverSelectedItem", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<VehicleRouteExpenseTypeModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteTransactionId = 0;
	private string _deleteTransactionName = string.Empty;

	private int _recoverTransactionId = 0;
	private string _recoverTransactionName = string.Empty;

	private ToastNotification _toastNotification;

	#region Load Data
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Fleet]);
		await InitializePage();
	}

	private async Task InitializePage()
	{
		LoadHotKeys();
		await LoadData();

		_isLoading = false;
		StateHasChanged();
	}

	private async Task LoadData()
	{
		_vehicleRouteExpenseTypes = await CommonData.LoadTableData<VehicleRouteExpenseTypeModel>(FleetNames.VehicleRouteExpenseType);

		if (!_showDeleted)
			_vehicleRouteExpenseTypes = [.. _vehicleRouteExpenseTypes.Where(v => v.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
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

			await VehicleRouteExpenseTypeData.SaveTransaction(_vehicleRouteExpenseType);

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
	private async Task ConfirmDelete()
	{
		try
		{
			_isProcessing = true;
			await _deleteConfirmationDialog.HideAsync();

			if (!_user.Admin)
				throw new Exception("You do not have permission to perform this action.");

			var vehicleRouteExpenseType = await CommonData.LoadTableDataById<VehicleRouteExpenseTypeModel>(FleetNames.VehicleRouteExpenseType, _deleteTransactionId)
				?? throw new Exception("Transaction not found.");

			vehicleRouteExpenseType.Status = false;
			await VehicleRouteExpenseTypeData.InsertVehicleRouteExpenseType(vehicleRouteExpenseType);

			await _toastNotification.ShowAsync("Deleted", "Transaction has been deleted successfully.", ToastType.Success);
			ResetPage();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Deleting", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteTransactionId = 0;
			_deleteTransactionName = string.Empty;
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

			var vehicleRouteExpenseType = await CommonData.LoadTableDataById<VehicleRouteExpenseTypeModel>(FleetNames.VehicleRouteExpenseType, _recoverTransactionId)
				?? throw new Exception("Transaction not found.");

			vehicleRouteExpenseType.Status = true;
			await VehicleRouteExpenseTypeData.InsertVehicleRouteExpenseType(vehicleRouteExpenseType);

			await _toastNotification.ShowAsync("Recovered", "Transaction has been recovered successfully.", ToastType.Success);
			ResetPage();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Recovering", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverTransactionId = 0;
			_recoverTransactionName = string.Empty;
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
	private void LoadHotKeys() =>
		_hotKeysContext = HotKeys.CreateContext()
			.Add(ModCode.Ctrl, Code.S, SaveTransaction, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteRecoverSelectedItem, "Delete / Recover selected", Exclude.None);

	private async Task OnMenuSelected(Syncfusion.Blazor.Navigations.MenuEventArgs<Syncfusion.Blazor.Navigations.MenuItem> args)
	{
		switch (args.Item.Id)
		{
			case "NewTransaction":
				ResetPage();
				break;
			case "SaveTransaction":
				await SaveTransaction();
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
			case "EditSelectedItem":
				await EditSelectedItem();
				break;
			case "DeleteRecoverSelectedItem":
				await DeleteRecoverSelectedItem();
				break;
		}
	}

	private async Task OnGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleRouteExpenseTypeModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditSelectedItem":
				await EditSelectedItem();
				break;
			case "DeleteRecoverSelectedItem":
				await DeleteRecoverSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count == 0)
			return;

		_vehicleRouteExpenseType = await CommonData.LoadTableDataById<VehicleRouteExpenseTypeModel>(FleetNames.VehicleRouteExpenseType, selectedRecords[0].Id);
		if (_vehicleRouteExpenseType is null)
			await _toastNotification.ShowAsync("Error while Editing", "Transaction Not Found.", ToastType.Error);

		StateHasChanged();
	}

	private async Task DeleteRecoverSelectedItem()
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
		_deleteTransactionId = id;
		_deleteTransactionName = name;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteTransactionId = 0;
		_deleteTransactionName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation(int id, string name)
	{
		_recoverTransactionId = id;
		_recoverTransactionName = name;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverTransactionId = 0;
		_recoverTransactionName = string.Empty;
		await _recoverConfirmationDialog.HideAsync();
	}

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadData();
		StateHasChanged();
	}

	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.VehicleRouteExpenseTypeMaster, true);

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
	#endregion
}
