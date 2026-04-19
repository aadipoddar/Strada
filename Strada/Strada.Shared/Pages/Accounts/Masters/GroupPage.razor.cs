using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Accounts.Masters;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Accounts.Masters;

public partial class GroupPage : IAsyncDisposable
{
    private UserModel _user;
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private GroupModel _group = new();

    private List<GroupModel> _groups = [];
    private List<NatureModel> _natures = [];
    private readonly List<ContextMenuItemModel> _groupGridContextMenuItems =
    [
        new() { Text = "Edit (Insert)", Id = "EditGroup", IconCss = "e-icons e-edit", Target = ".e-content" },
        new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverGroup", IconCss = "e-icons e-trash", Target = ".e-content" }
    ];

    private SfGrid<GroupModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteGroupId = 0;
    private string _deleteGroupName = string.Empty;

    private int _recoverGroupId = 0;
    private string _recoverGroupName = string.Empty;

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
            .Add(ModCode.Ctrl, Code.S, SaveGroup, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

        _natures = await CommonData.LoadTableDataByStatus<NatureModel>(AccountNames.Nature);
        _groups = await CommonData.LoadTableData<GroupModel>(AccountNames.Group);

        if (!_showDeleted)
            _groups = [.. _groups.Where(g => g.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Saving
    private async Task ValidateForm()
    {
        if (!_user.Admin)
            throw new Exception("You do not have permission to perform this action.");

        _group.Name = _group.Name?.Trim() ?? "";
        _group.Name = _group.Name?.ToUpper() ?? "";

        _group.Remarks = _group.Remarks?.Trim() ?? "";
        _group.Status = true;

        if (string.IsNullOrWhiteSpace(_group.Name))
            throw new Exception("Group name is required. Please enter a valid group name.");

        if (_group.NatureId <= 0)
            throw new Exception("Nature is required. Please select a nature.");

        if (string.IsNullOrWhiteSpace(_group.Remarks))
            _group.Remarks = null;

        if (_group.Id > 0)
        {
            var existingGroup = _groups.FirstOrDefault(_ => _.Id != _group.Id && _.Name.Equals(_group.Name, StringComparison.OrdinalIgnoreCase));
            if (existingGroup is not null)
                throw new Exception($"Group name '{_group.Name}' already exists. Please choose a different name.");
        }
        else
        {
            var existingGroup = _groups.FirstOrDefault(_ => _.Name.Equals(_group.Name, StringComparison.OrdinalIgnoreCase));
            if (existingGroup is not null)
                throw new Exception($"Group name '{_group.Name}' already exists. Please choose a different name.");
        }
    }

    private async Task SaveGroup()
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

            await ValidateForm();
            await GroupData.InsertGroup(_group);

            await _toastNotification.ShowAsync("Success", $"Group '{_group.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.GroupMaster, true);
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
    private void OnEditGroup(GroupModel group)
    {
        _group = new()
        {
            Id = group.Id,
            Name = group.Name,
            NatureId = group.NatureId,
            Remarks = group.Remarks,
            Status = group.Status
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

            var group = _groups.FirstOrDefault(g => g.Id == _deleteGroupId)
                ?? throw new Exception("Group not found.");

            group.Status = false;
            await GroupData.InsertGroup(group);

            await _toastNotification.ShowAsync("Success", $"Group '{group.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.GroupMaster, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Group: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteGroupId = 0;
            _deleteGroupName = string.Empty;
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

            var group = _groups.FirstOrDefault(g => g.Id == _recoverGroupId)
             ?? throw new Exception("Group not found.");

            group.Status = true;
            await GroupData.InsertGroup(group);

            await _toastNotification.ShowAsync("Success", $"Group '{group.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.GroupMaster, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Group: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverGroupId = 0;
            _recoverGroupName = string.Empty;
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

            var (stream, fileName) = await GroupExport.ExportMaster(_groups, ReportExportType.Excel);
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

            var (stream, fileName) = await GroupExport.ExportMaster(_groups, ReportExportType.PDF);
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
            case "NewGroup":
                ResetPage();
                break;
            case "SaveGroup":
                await SaveGroup();
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

    private async Task OnGroupGridContextMenuItemClicked(ContextMenuClickEventArgs<GroupModel> args)
    {
        switch (args.Item.Id)
        {
            case "EditGroup":
                await EditSelectedItem();
                break;
            case "DeleteRecoverGroup":
                await DeleteSelectedItem();
                break;
        }
    }

    private async Task EditSelectedItem()
    {
        var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
        if (selectedRecords.Count > 0)
            OnEditGroup(selectedRecords[0]);
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
        _deleteGroupId = id;
        _deleteGroupName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteGroupId = 0;
        _deleteGroupName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverGroupId = id;
        _recoverGroupName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverGroupId = 0;
        _recoverGroupName = string.Empty;
        await _recoverConfirmationDialog.HideAsync();
    }

    private async Task ToggleDeleted()
    {
        _showDeleted = !_showDeleted;
        await LoadData();
        StateHasChanged();
    }

    private void ResetPage() =>
        NavigationManager.NavigateTo(PageRouteNames.GroupMaster, true);

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.AccountsDashboard);

    public async ValueTask DisposeAsync()
    {
        if (_hotKeysContext is not null)
            await _hotKeysContext.DisposeAsync();

        GC.SuppressFinalize(this);
    }
    #endregion
}
