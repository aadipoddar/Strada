using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Fleet.VehicleDocument;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Fleet.VehicleDocument;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleDocument;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Strada.Shared.Pages.Fleet.VehicleDocument;

public partial class VehicleDocumentPage : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;

	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;
	private bool _isUploadDialogVisible = false;

	private VehicleDocumentModel _vehicleDocument = new() { TransactionDateTime = DateTime.Now, RenewalDate = DateTime.Now.AddYears(1) };
	private IBrowserFile _pendingDocumentFile;
	private string _documentUrlToDelete = string.Empty;

	private List<VehicleDocumentModel> _vehicleDocuments = [];
	private List<VehicleDocumentTypeModel> _vehicleDocumentTypes = [];
	private List<VehicleModel> _vehicles = [];
	private readonly List<ContextMenuItemModel> _vehicleDocumentGridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditVehicleDocument", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverVehicleDocument", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<VehicleDocumentModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteVehicleDocumentId = 0;
	private string _deleteVehicleDocumentTransactionNo = string.Empty;

	private int _recoverVehicleDocumentId = 0;
	private string _recoverVehicleDocumentTransactionNo = string.Empty;

	private ToastNotification _toastNotification;

	#region Load Data
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Fleet]);
		await LoadData();
	}

	private async Task LoadData()
	{
		_hotKeysContext = HotKeys.CreateContext()
			.Add(ModCode.Ctrl, Code.S, SaveTransaction, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.U, UploadDocument, "Upload document", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

		_vehicleDocumentTypes = await CommonData.LoadTableDataByStatus<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType);
		_vehicleDocumentTypes = [.. _vehicleDocumentTypes.OrderBy(vdt => vdt.Name)];

		_vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(FleetNames.Vehicle);
		_vehicles = [.. _vehicles.OrderBy(v => v.Code)];

		_vehicleDocuments = await CommonData.LoadTableData<VehicleDocumentModel>(FleetNames.VehicleDocument);
		if (!_showDeleted)
			_vehicleDocuments = [.. _vehicleDocuments.Where(vd => vd.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();

		_isLoading = false;
		StateHasChanged();
	}
	#endregion

	#region Change Events
	private void OnVehicleDocumentTypeChanged(ChangeEventArgs<VehicleDocumentTypeModel, VehicleDocumentTypeModel> args)
	{
		if (args.Value is null || args.Value.Id <= 0)
			return;

		_vehicleDocument.VehicleDocumentTypeId = args.Value.Id;
		_vehicleDocument.Rate = args.Value.Rate;
	}

	private void OnVehicleChanged(ChangeEventArgs<VehicleModel, VehicleModel> args)
	{
		if (args.Value is null || args.Value.Id <= 0)
			return;

		_vehicleDocument.VehicleId = args.Value.Id;
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
			await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

			var currentDateTime = await CommonData.LoadCurrentDateTime();
			_vehicleDocument.Status = true;
			_vehicleDocument.TransactionDateTime = DateOnly.FromDateTime(_vehicleDocument.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
			_vehicleDocument.CreatedBy = _user.Id;
			_vehicleDocument.CreatedAt = currentDateTime;
			_vehicleDocument.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
			_vehicleDocument.LastModifiedBy = null;
			_vehicleDocument.LastModifiedAt = null;
			_vehicleDocument.LastModifiedFromPlatform = null;

			await VehicleDocumentData.SaveTransaction(
				_vehicleDocument,
				_pendingDocumentFile?.OpenReadStream(maxAllowedSize: 52428800),
				_pendingDocumentFile?.Name,
				_documentUrlToDelete);

			await _toastNotification.ShowAsync("Success", $"Vehicle Document transaction '{_vehicleDocument.TransactionNo}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleDocument, true);
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
	private async Task OnEditVehicleDocument(VehicleDocumentModel vehicleDocument)
	{
		_vehicleDocument = await CommonData.LoadTableDataById<VehicleDocumentModel>(FleetNames.VehicleDocument, vehicleDocument.Id)
			?? throw new Exception("Vehicle Document transaction not found for editing.");
		_pendingDocumentFile = null;
		_documentUrlToDelete = string.Empty;
		_isUploadDialogVisible = false;
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

			var vehicleDocument = await CommonData.LoadTableDataById<VehicleDocumentModel>(FleetNames.VehicleDocument, _deleteVehicleDocumentId)
				?? throw new Exception("Vehicle Document transaction not found.");

			var currentDateTime = await CommonData.LoadCurrentDateTime();

			vehicleDocument.Status = false;
			vehicleDocument.LastModifiedBy = _user.Id;
			vehicleDocument.LastModifiedAt = currentDateTime;
			vehicleDocument.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();

			await VehicleDocumentData.InsertVehicleDocument(vehicleDocument);

			await _toastNotification.ShowAsync("Success", $"Vehicle Document transaction '{vehicleDocument.TransactionNo}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleDocument, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete Vehicle Document transaction: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteVehicleDocumentId = 0;
			_deleteVehicleDocumentTransactionNo = string.Empty;
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

			var vehicleDocument = await CommonData.LoadTableDataById<VehicleDocumentModel>(FleetNames.VehicleDocument, _recoverVehicleDocumentId)
				?? throw new Exception("Vehicle Document transaction not found.");

			var currentDateTime = await CommonData.LoadCurrentDateTime();

			vehicleDocument.Status = true;
			vehicleDocument.LastModifiedBy = _user.Id;
			vehicleDocument.LastModifiedAt = currentDateTime;
			vehicleDocument.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();

			await VehicleDocumentData.InsertVehicleDocument(vehicleDocument);

			await _toastNotification.ShowAsync("Success", $"Vehicle Document transaction '{vehicleDocument.TransactionNo}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.VehicleDocument, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover Vehicle Document transaction: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverVehicleDocumentId = 0;
			_recoverVehicleDocumentTransactionNo = string.Empty;
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

			var (stream, fileName) = await VehicleDocumentExport.ExportTransaction(_vehicleDocuments, ReportExportType.Excel);
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

			var (stream, fileName) = await VehicleDocumentExport.ExportTransaction(_vehicleDocuments, ReportExportType.PDF);
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

	#region Uploading Document
	private void UploadDocument()
	{
		if (_isProcessing)
			return;

		_isUploadDialogVisible = true;
		StateHasChanged();
	}

	private void CloseUploadDialog()
	{
		_isUploadDialogVisible = false;
		StateHasChanged();
	}

	private async Task OnUploaderFileChange(UploadChangeEventArgs args)
	{
		if (args.Files is null || args.Files.Count == 0 || args.Files[0].File is null)
			return;

		_pendingDocumentFile = args.Files[0].File;
		await _toastNotification.ShowAsync("Document Selected", $"'{_pendingDocumentFile.Name}' will be uploaded when you save the transaction.", ToastType.Info);
	}

	private Task OnRemoveFile(RemovingEventArgs args)
	{
		_pendingDocumentFile = null;
		return Task.CompletedTask;
	}

	private async Task DownloadExistingDocument()
	{
		try
		{
			if (string.IsNullOrWhiteSpace(_vehicleDocument.DocumentUrl))
				return;

			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var (stream, contentType) = await BlobStorageAccess.DownloadFileFromBlobStorage(_vehicleDocument.DocumentUrl);
			var fileName = _vehicleDocument.DocumentUrl.Split('/').Last();
			await SaveAndViewService.SaveAndView(fileName, stream);

			await _toastNotification.ShowAsync("Exported", "The export has been downloaded successfully.", ToastType.Success);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Exporting", ex.Message, ToastType.Error);
		}
	}

	private async Task MarkDocumentForRemoval()
	{
		if (string.IsNullOrWhiteSpace(_vehicleDocument.DocumentUrl))
		{
			_pendingDocumentFile = null;
			return;
		}

		_documentUrlToDelete = _vehicleDocument.DocumentUrl;
		_vehicleDocument.DocumentUrl = null;
		_pendingDocumentFile = null;

		await _toastNotification.ShowAsync("Document Removed", "Document will be removed when you save the transaction.", ToastType.Info);
	}
	#endregion

	#region Utilities
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
			case "UploadDocument":
				UploadDocument();
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

	private async Task OnVehicleDocumentGridContextMenuItemClicked(ContextMenuClickEventArgs<VehicleDocumentModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditVehicleDocument":
				await EditSelectedItem();
				break;
			case "DeleteRecoverVehicleDocument":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			await OnEditVehicleDocument(selectedRecords[0]);
	}

	private async Task DeleteSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
		{
			if (selectedRecords[0].Status)
				await ShowDeleteConfirmation(selectedRecords[0].Id, selectedRecords[0].TransactionNo);
			else
				await ShowRecoverConfirmation(selectedRecords[0].Id, selectedRecords[0].TransactionNo);
		}
	}

	private async Task ShowDeleteConfirmation(int id, string transactionNo)
	{
		_deleteVehicleDocumentId = id;
		_deleteVehicleDocumentTransactionNo = transactionNo;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteVehicleDocumentId = 0;
		_deleteVehicleDocumentTransactionNo = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ShowRecoverConfirmation(int id, string transactionNo)
	{
		_recoverVehicleDocumentId = id;
		_recoverVehicleDocumentTransactionNo = transactionNo;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverVehicleDocumentId = 0;
		_recoverVehicleDocumentTransactionNo = string.Empty;
		await _recoverConfirmationDialog.HideAsync();
	}

	private async Task ToggleDeleted()
	{
		_showDeleted = !_showDeleted;
		await LoadData();
		StateHasChanged();
	}

	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.VehicleDocument, true);

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
	#endregion
}