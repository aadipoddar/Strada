using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Accounts.Masters;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Accounts.Masters;

public partial class StateUTPage : IAsyncDisposable
{
    private UserModel _user;
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private StateUTModel _stateUT = new();

    private List<StateUTModel> _stateUTs = [];
    private readonly List<ContextMenuItemModel> _stateUTGridContextMenuItems =
    [
        new() { Text = "Edit (Insert)", Id = "EditStateUT", IconCss = "e-icons e-edit", Target = ".e-content" },
        new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverStateUT", IconCss = "e-icons e-trash", Target = ".e-content" }
    ];

    private SfGrid<StateUTModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteStateUTId = 0;
    private string _deleteStateUTName = string.Empty;

    private int _recoverStateUTId = 0;
    private string _recoverStateUTName = string.Empty;

    private ToastNotification _toastNotification;

    #region Load Data
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Accounts]);
        await LoadData();
        _isLoading = false;
        StateHasChanged();
    }

    private async Task LoadData()
    {
        _hotKeysContext = HotKeys.CreateContext()
            .Add(ModCode.Ctrl, Code.S, SaveStateUT, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

        _stateUTs = await CommonData.LoadTableData<StateUTModel>(AccountNames.StateUT);

        if (!_showDeleted)
            _stateUTs = [.. _stateUTs.Where(g => g.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Saving
    private async Task ValidateForm()
    {
        if (!_user.Admin)
            throw new Exception("You do not have permission to perform this action.");

        _stateUT.Name = _stateUT.Name?.Trim() ?? "";
        _stateUT.Name = _stateUT.Name?.ToUpper() ?? "";

        _stateUT.Remarks = _stateUT.Remarks?.Trim() ?? "";
        _stateUT.Status = true;

        if (string.IsNullOrWhiteSpace(_stateUT.Name))
            throw new Exception("State/UT name is required. Please enter a valid state/UT name.");

        if (string.IsNullOrWhiteSpace(_stateUT.Remarks))
            _stateUT.Remarks = null;

        if (_stateUT.Id > 0)
        {
            var existingStateUT = _stateUTs.FirstOrDefault(_ => _.Id != _stateUT.Id && _.Name.Equals(_stateUT.Name, StringComparison.OrdinalIgnoreCase));
            if (existingStateUT is not null)
                throw new Exception($"State/UT name '{_stateUT.Name}' already exists. Please choose a different name.");
        }
        else
        {
            var existingStateUT = _stateUTs.FirstOrDefault(_ => _.Name.Equals(_stateUT.Name, StringComparison.OrdinalIgnoreCase));
            if (existingStateUT is not null)
                throw new Exception($"State/UT name '{_stateUT.Name}' already exists. Please choose a different name.");
        }
    }

    private async Task SaveStateUT()
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

            await ValidateForm();
            await StateUTData.InsertStateUT(_stateUT);

            await _toastNotification.ShowAsync("Success", $"State/UT '{_stateUT.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.StateUTMaster, true);
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

            var (stream, fileName) = await StateUTExport.ExportMaster(_stateUTs, ReportExportType.Excel);
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

            var (stream, fileName) = await StateUTExport.ExportMaster(_stateUTs, ReportExportType.PDF);
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

    #region Actions
    private void OnEditStateUT(StateUTModel stateUT)
    {
        _stateUT = new()
        {
            Id = stateUT.Id,
            Name = stateUT.Name,
            Remarks = stateUT.Remarks,
            UnionTerritory = stateUT.UnionTerritory,
            Status = stateUT.Status
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

            var stateUT = _stateUTs.FirstOrDefault(g => g.Id == _deleteStateUTId)
                ?? throw new Exception("State/UT not found.");

            stateUT.Status = false;
            await StateUTData.InsertStateUT(stateUT);

            await _toastNotification.ShowAsync("Success", $"State/UT '{stateUT.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.StateUTMaster, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete State/UT: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteStateUTId = 0;
            _deleteStateUTName = string.Empty;
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

            var stateUT = _stateUTs.FirstOrDefault(g => g.Id == _recoverStateUTId)
                ?? throw new Exception("State/UT not found.");

            stateUT.Status = true;
            await StateUTData.InsertStateUT(stateUT);

            await _toastNotification.ShowAsync("Success", $"State/UT '{stateUT.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.StateUTMaster, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover State/UT: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverStateUTId = 0;
            _recoverStateUTName = string.Empty;
        }
    }
    #endregion

    #region Utilities
    private async Task OnMenuSelected(Syncfusion.Blazor.Navigations.MenuEventArgs<Syncfusion.Blazor.Navigations.MenuItem> args)
    {
        switch (args.Item.Id)
        {
            case "NewStateUT":
                ResetPage();
                break;
            case "SaveStateUT":
                await SaveStateUT();
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

    private async Task OnStateUTGridContextMenuItemClicked(ContextMenuClickEventArgs<StateUTModel> args)
    {
        switch (args.Item.Id)
        {
            case "EditStateUT":
                await EditSelectedItem();
                break;
            case "DeleteRecoverStateUT":
                await DeleteSelectedItem();
                break;
        }
    }

    private async Task EditSelectedItem()
    {
        var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
        if (selectedRecords.Count > 0)
            OnEditStateUT(selectedRecords[0]);
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
        _deleteStateUTId = id;
        _deleteStateUTName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteStateUTId = 0;
        _deleteStateUTName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverStateUTId = id;
        _recoverStateUTName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverStateUTId = 0;
        _recoverStateUTName = string.Empty;
        await _recoverConfirmationDialog.HideAsync();
    }

    private async Task ToggleDeleted()
    {
        _showDeleted = !_showDeleted;
        await LoadData();
        StateHasChanged();
    }

    private void ResetPage() =>
        NavigationManager.NavigateTo(PageRouteNames.StateUTMaster, true);

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.AccountsDashboard);

    public ValueTask DisposeAsync()
    {
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
    #endregion
}
