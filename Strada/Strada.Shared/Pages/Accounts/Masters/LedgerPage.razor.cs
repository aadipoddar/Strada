using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Operations;
using StradaLibrary.Exports.Accounts.Masters;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;

namespace Strada.Shared.Pages.Accounts.Masters;

public partial class LedgerPage : IAsyncDisposable
{
    private UserModel _user;
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private LedgerModel _ledger = new();

    private List<LedgerModel> _ledgers = [];
    private List<GroupModel> _groups = [];
    private List<AccountTypeModel> _accountTypes = [];
    private List<StateUTModel> _stateUTs = [];
    private readonly List<ContextMenuItemModel> _ledgerGridContextMenuItems =
    [
        new() { Text = "Edit (Insert)", Id = "EditLedger", IconCss = "e-icons e-edit", Target = ".e-content" },
        new() { Text = "Delete / Recover (Del)", Id = "DeleteRecoverLedger", IconCss = "e-icons e-trash", Target = ".e-content" }
    ];

    private SfGrid<LedgerModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteLedgerId = 0;
    private string _deleteLedgerName = string.Empty;

    private int _recoverLedgerId = 0;
    private string _recoverLedgerName = string.Empty;

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
            .Add(ModCode.Ctrl, Code.S, SaveLedger, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.Delete, ToggleDeleted, "Show/Hide Deleted", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete / Recover selected", Exclude.None);

        _ledgers = await CommonData.LoadTableData<LedgerModel>(AccountNames.Ledger);
        _groups = await CommonData.LoadTableData<GroupModel>(AccountNames.Group);
        _accountTypes = await CommonData.LoadTableData<AccountTypeModel>(AccountNames.AccountType);
        _stateUTs = await CommonData.LoadTableData<StateUTModel>(AccountNames.StateUT);

        if (!_showDeleted)
            _ledgers = [.. _ledgers.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Saving
    private async Task ValidateForm()
    {
        if (!_user.Admin)
            throw new Exception("You do not have permission to perform this action.");

        _ledger.Name = _ledger.Name?.Trim() ?? "";
        _ledger.Name = _ledger.Name?.ToUpper() ?? "";

        _ledger.GSTNo = _ledger.GSTNo?.Trim() ?? "";
        _ledger.GSTNo = _ledger.GSTNo?.ToUpper() ?? "";

        _ledger.PANNo = _ledger.PANNo?.Trim() ?? "";
        _ledger.PANNo = _ledger.PANNo?.ToUpper() ?? "";

        _ledger.CINNo = _ledger.CINNo?.Trim() ?? "";
        _ledger.CINNo = _ledger.CINNo?.ToUpper() ?? "";

        _ledger.Alias = _ledger.Alias?.Trim() ?? "";
        _ledger.Alias = _ledger.Alias?.ToUpper() ?? "";

        _ledger.Phone = _ledger.Phone?.Trim() ?? "";
        _ledger.Email = _ledger.Email?.Trim() ?? "";
        _ledger.Address = _ledger.Address?.Trim() ?? "";

        _ledger.Remarks = _ledger.Remarks?.Trim() ?? "";
        _ledger.Status = true;

        if (string.IsNullOrWhiteSpace(_ledger.Name))
            throw new Exception("Ledger name is required. Please enter a valid ledger name.");

        if (_ledger.GroupId <= 0)
            throw new Exception("Group is required. Please select a valid group.");

        if (_ledger.AccountTypeId <= 0)
            throw new Exception("Account Type is required. Please select a valid account type.");

        if (_ledger.StateUTId <= 0)
            throw new Exception("State/UT is required. Please select a valid State/UT.");

        if (string.IsNullOrWhiteSpace(_ledger.GSTNo)) _ledger.GSTNo = null;
        if (string.IsNullOrWhiteSpace(_ledger.PANNo)) _ledger.PANNo = null;
        if (string.IsNullOrWhiteSpace(_ledger.CINNo)) _ledger.CINNo = null;
        if (string.IsNullOrWhiteSpace(_ledger.Alias)) _ledger.Alias = null;
        if (string.IsNullOrWhiteSpace(_ledger.Phone)) _ledger.Phone = null;
        if (string.IsNullOrWhiteSpace(_ledger.Email)) _ledger.Email = null;
        if (string.IsNullOrWhiteSpace(_ledger.Address)) _ledger.Address = null;
        if (string.IsNullOrWhiteSpace(_ledger.Remarks)) _ledger.Remarks = null;

        if (!string.IsNullOrWhiteSpace(_ledger.Phone) && !Helper.ValidatePhoneNumber(_ledger.Phone))
            throw new Exception("Invalid phone number format. Please enter a valid phone number.");

        if (!string.IsNullOrWhiteSpace(_ledger.Email) && !Helper.ValidateEmail(_ledger.Email))
            throw new Exception("Invalid email format. Please enter a valid email address.");

        if (_ledger.Id > 0)
        {
            var existingLedger = _ledgers.FirstOrDefault(_ => _.Id != _ledger.Id && _.Name.Equals(_ledger.Name, StringComparison.OrdinalIgnoreCase));
            if (existingLedger is not null)
                throw new Exception($"Ledger name '{_ledger.Name}' already exists. Please choose a different name.");

            if (!string.IsNullOrWhiteSpace(_ledger.Phone))
            {
                var duplicatePhoneLedger = _ledgers.FirstOrDefault(_ => _.Id != _ledger.Id && _.Phone.Equals(_ledger.Phone, StringComparison.OrdinalIgnoreCase));
                if (duplicatePhoneLedger is not null)
                    throw new Exception($"Phone number '{_ledger.Phone}' is already associated with another ledger. Please use a different phone number.");
            }

            if (!string.IsNullOrWhiteSpace(_ledger.Email))
            {
                var duplicateEmailLedger = _ledgers.FirstOrDefault(_ => _.Id != _ledger.Id && _.Email.Equals(_ledger.Email, StringComparison.OrdinalIgnoreCase));
                if (duplicateEmailLedger is not null)
                    throw new Exception($"Email '{_ledger.Email}' is already associated with another ledger. Please use a different email address.");
            }
        }
        else
        {
            var existingLedger = _ledgers.FirstOrDefault(_ => _.Name.Equals(_ledger.Name, StringComparison.OrdinalIgnoreCase));
            if (existingLedger is not null)
                throw new Exception($"Ledger name '{_ledger.Name}' already exists. Please choose a different name.");

            if (!string.IsNullOrWhiteSpace(_ledger.Phone))
            {
                var duplicatePhoneLedger = _ledgers.FirstOrDefault(_ => _.Phone.Equals(_ledger.Phone, StringComparison.OrdinalIgnoreCase));
                if (duplicatePhoneLedger is not null)
                    throw new Exception($"Phone number '{_ledger.Phone}' is already associated with another ledger. Please use a different phone number.");
            }

            if (!string.IsNullOrWhiteSpace(_ledger.Email))
            {
                var duplicateEmailLedger = _ledgers.FirstOrDefault(_ => _.Email.Equals(_ledger.Email, StringComparison.OrdinalIgnoreCase));
                if (duplicateEmailLedger is not null)
                    throw new Exception($"Email '{_ledger.Email}' is already associated with another ledger. Please use a different email address.");
            }
        }
    }

    private async Task SaveLedger()
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

            await ValidateForm();
            if (_ledger.Id == 0)
                _ledger.Code = await GenerateCodes.GenerateLedgerCode();

            await LedgerData.InsertLedger(_ledger);

            await _toastNotification.ShowAsync("Success", $"Ledger '{_ledger.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.LedgerMaster, true);
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
    private async Task OnEditLedger(LedgerModel ledger)
    {
        _ledger = await CommonData.LoadTableDataById<LedgerModel>(AccountNames.Ledger, ledger.Id)
            ?? throw new Exception("Ledger not found.");

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

            var ledger = _ledgers.FirstOrDefault(l => l.Id == _deleteLedgerId)
                ?? throw new Exception("Ledger not found.");

            ledger.Status = false;
            await LedgerData.InsertLedger(ledger);

            await _toastNotification.ShowAsync("Success", $"Ledger '{ledger.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.LedgerMaster, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Ledger: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteLedgerId = 0;
            _deleteLedgerName = string.Empty;
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

            var ledger = _ledgers.FirstOrDefault(l => l.Id == _recoverLedgerId)
             ?? throw new Exception("Ledger not found.");

            ledger.Status = true;
            await LedgerData.InsertLedger(ledger);

            await _toastNotification.ShowAsync("Success", $"Ledger '{ledger.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.LedgerMaster, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Ledger: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverLedgerId = 0;
            _recoverLedgerName = string.Empty;
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

            var (stream, fileName) = await LedgerExport.ExportMaster(_ledgers, ReportExportType.Excel);
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

            var (stream, fileName) = await LedgerExport.ExportMaster(_ledgers, ReportExportType.PDF);
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
            case "NewLedger":
                ResetPage();
                break;
            case "SaveLedger":
                await SaveLedger();
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

    private async Task OnLedgerGridContextMenuItemClicked(ContextMenuClickEventArgs<LedgerModel> args)
    {
        switch (args.Item.Id)
        {
            case "EditLedger":
                await EditSelectedItem();
                break;
            case "DeleteRecoverLedger":
                await DeleteSelectedItem();
                break;
        }
    }

    private async Task ShowDeleteConfirmation(int id, string name)
    {
        _deleteLedgerId = id;
        _deleteLedgerName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteLedgerId = 0;
        _deleteLedgerName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task EditSelectedItem()
    {
        var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
        if (selectedRecords.Count > 0)
            await OnEditLedger(selectedRecords[0]);
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

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverLedgerId = id;
        _recoverLedgerName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverLedgerId = 0;
        _recoverLedgerName = string.Empty;
        await _recoverConfirmationDialog.HideAsync();
    }

    private async Task ToggleDeleted()
    {
        _showDeleted = !_showDeleted;
        await LoadData();
        StateHasChanged();
    }

    private void ResetPage() =>
        NavigationManager.NavigateTo(PageRouteNames.LedgerMaster, true);

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.AccountsDashboard);

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ((IAsyncDisposable)HotKeys).DisposeAsync();
    }
    #endregion
}
