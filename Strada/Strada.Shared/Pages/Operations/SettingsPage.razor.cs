using Strada.Shared.Components.Dialog;
using StradaLibrary.Data.Common;
using StradaLibrary.Data.Operations;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Operations;
using Syncfusion.Blazor.DropDowns;

namespace Strada.Shared.Pages.Operations;

public partial class SettingsPage
{
    #region Fields

    private bool _isLoading = true;
    private bool _isProcessing = false;

    private ToastNotification _toastNotification;
    private ResetConfirmationDialog _resetConfirmationDialog = default!;

    // Primary Configuration
    private string _primaryCompanyLinkingId = string.Empty;
    private string _selectedCompanyName = string.Empty;
    private List<CompanyModel> _companies = [];

    // Login Settings
    private bool _enableLoginWithCode = true;
    private int _maxLoginAttempts = 5;
    private bool _enableUsersToResetPassword = true;
    private int _codeResendLimit = 3;
    private int _codeExpiryMinutes = 10;

    // Code Prefixes
    private string _ledgerCodePrefix = string.Empty;
    private string _vehicleTypeCodePrefix = string.Empty;
    private string _documentTypeCodePrefix = string.Empty;
    private string _omcCodePrefix = string.Empty;
    private string _omcCardCodePrefix = string.Empty;
    private string _vehicleRouteLocationCodePrefix = string.Empty;
    private string _vehicleRouteCodePrefix = string.Empty;
    private string _vehicleDriverCodePrefix = string.Empty;
    private string _vehicleExpenseTypeCodePrefix = string.Empty;

    // Transaction Prefixes
    private string _financialAccountingTransactionPrefix = string.Empty;
    private string _vehicleTripTransactionPrefix = string.Empty;
    private string _vehicleTripBillTransactionPrefix = string.Empty;
    private string _vehicleExpenseTransactionPrefix = string.Empty;

    // Ledger Linking
    private string _cashLedgerId = string.Empty;
    private string _selectedCashLedgerName = string.Empty;
    private string _gstLedgerId = string.Empty;
    private string _selectedGSTLedgerName = string.Empty;
    private List<LedgerModel> _ledgers = [];

    // Default Values
    private string _defaultSelectedVoucherId = string.Empty;
    private string _selectedDefaultVoucherName = string.Empty;
    private List<VoucherModel> _vouchers = [];

    // Report Settings
    private int _autoRefreshReportTimer = 5;

    #endregion

    #region Load Data

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Admin]);
        await LoadData();
        _isLoading = false;
        StateHasChanged();
    }

    private async Task LoadData()
    {
        try
        {
            await LoadAllSettings();
            await LoadCompanies();
            await LoadLedgers();
            await LoadVouchers();
            MapSelections();
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to load settings: {ex.Message}", ToastType.Error);
        }
    }

    private async Task LoadAllSettings()
    {
        var s = await SettingsData.LoadSettingsByKey(SettingsKeys.EnableLoginWithCode);
        _enableLoginWithCode = !bool.TryParse(s?.Value, out var v1) || v1;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.MaxLoginAttempts);
        _maxLoginAttempts = int.TryParse(s?.Value, out var v2) ? v2 : 5;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.EnableUsersToResetPassword);
        _enableUsersToResetPassword = !bool.TryParse(s?.Value, out var v3) || v3;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.CodeResendLimit);
        _codeResendLimit = int.TryParse(s?.Value, out var v4) ? v4 : 3;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.CodeExpiryMinutes);
        _codeExpiryMinutes = int.TryParse(s?.Value, out var v5) ? v5 : 10;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.LedgerCodePrefix);
        _ledgerCodePrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleTypeCodePrefix);
        _vehicleTypeCodePrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.DocumentTypeCodePrefix);
        _documentTypeCodePrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.OMCCodePrefix);
        _omcCodePrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.OMCCardCodePrefix);
        _omcCardCodePrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleRouteLocationCodePrefix);
        _vehicleRouteLocationCodePrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleRouteCodePrefix);
        _vehicleRouteCodePrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleDriverCodePrefix);
        _vehicleDriverCodePrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleExpenseTypeCodePrefix);
        _vehicleExpenseTypeCodePrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.FinancialAccountingTransactionPrefix);
        _financialAccountingTransactionPrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleTripTransactionPrefix);
        _vehicleTripTransactionPrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleTripBillTransactionPrefix);
        _vehicleTripBillTransactionPrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleExpenseTransactionPrefix);
        _vehicleExpenseTransactionPrefix = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
        _primaryCompanyLinkingId = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.CashLedgerId);
        _cashLedgerId = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.GSTLedgerId);
        _gstLedgerId = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.DefaultSelectedVoucherId);
        _defaultSelectedVoucherId = s?.Value ?? string.Empty;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.AutoRefreshReportTimer);
        _autoRefreshReportTimer = int.TryParse(s?.Value, out var v6) ? v6 : 5;
    }

    private async Task LoadCompanies()
    {
        var result = await CommonData.LoadTableData<CompanyModel>(AccountNames.Company);
        _companies = result ?? [];
    }

    private async Task LoadLedgers()
    {
        var result = await CommonData.LoadTableData<LedgerModel>(AccountNames.Ledger);
        _ledgers = result ?? [];
    }

    private async Task LoadVouchers()
    {
        var result = await CommonData.LoadTableData<VoucherModel>(AccountNames.Voucher);
        _vouchers = result ?? [];
    }

    private void MapSelections()
    {
        if (!string.IsNullOrEmpty(_primaryCompanyLinkingId) && int.TryParse(_primaryCompanyLinkingId, out var companyId))
            _selectedCompanyName = _companies.FirstOrDefault(c => c.Id == companyId)?.Name ?? string.Empty;

        if (!string.IsNullOrEmpty(_cashLedgerId) && int.TryParse(_cashLedgerId, out var cashId))
            _selectedCashLedgerName = _ledgers.FirstOrDefault(l => l.Id == cashId)?.Name ?? string.Empty;

        if (!string.IsNullOrEmpty(_gstLedgerId) && int.TryParse(_gstLedgerId, out var gstId))
            _selectedGSTLedgerName = _ledgers.FirstOrDefault(l => l.Id == gstId)?.Name ?? string.Empty;

        if (!string.IsNullOrEmpty(_defaultSelectedVoucherId) && int.TryParse(_defaultSelectedVoucherId, out var voucherId))
            _selectedDefaultVoucherName = _vouchers.FirstOrDefault(v => v.Id == voucherId)?.Name ?? string.Empty;
    }

    #endregion

    #region Change Handlers

    private void OnCompanyChange(ChangeEventArgs<string, CompanyModel> args)
    {
        if (args.ItemData is not null)
            _primaryCompanyLinkingId = args.ItemData.Id.ToString();
    }

    private void OnCashLedgerChange(ChangeEventArgs<string, LedgerModel> args)
    {
        if (args.ItemData is not null)
            _cashLedgerId = args.ItemData.Id.ToString();
    }

    private void OnGSTLedgerChange(ChangeEventArgs<string, LedgerModel> args)
    {
        if (args.ItemData is not null)
            _gstLedgerId = args.ItemData.Id.ToString();
    }

    private void OnDefaultVoucherChange(ChangeEventArgs<string, VoucherModel> args)
    {
        if (args.ItemData is not null)
            _defaultSelectedVoucherId = args.ItemData.Id.ToString();
    }

    #endregion

    #region Save Settings

    private async Task SaveSettings()
    {
        if (_isProcessing) return;

        try
        {
            _isProcessing = true;

            if (string.IsNullOrWhiteSpace(_primaryCompanyLinkingId))
            {
                await _toastNotification.ShowAsync("Validation", "Primary Company is required.", ToastType.Warning);
                return;
            }

            await _toastNotification.ShowAsync("Saving", "Processing settings...", ToastType.Info);

            var settings = await CommonData.LoadTableData<SettingsModel>(OperationNames.Settings);
            string Desc(string key) => settings.FirstOrDefault(s => s.Key == key)?.Description ?? string.Empty;

            await UpdateSetting(SettingsKeys.EnableLoginWithCode, _enableLoginWithCode.ToString().ToLower(), Desc(SettingsKeys.EnableLoginWithCode));
            await UpdateSetting(SettingsKeys.MaxLoginAttempts, _maxLoginAttempts.ToString(), Desc(SettingsKeys.MaxLoginAttempts));
            await UpdateSetting(SettingsKeys.EnableUsersToResetPassword, _enableUsersToResetPassword.ToString().ToLower(), Desc(SettingsKeys.EnableUsersToResetPassword));
            await UpdateSetting(SettingsKeys.CodeResendLimit, _codeResendLimit.ToString(), Desc(SettingsKeys.CodeResendLimit));
            await UpdateSetting(SettingsKeys.CodeExpiryMinutes, _codeExpiryMinutes.ToString(), Desc(SettingsKeys.CodeExpiryMinutes));

            await UpdateSetting(SettingsKeys.LedgerCodePrefix, _ledgerCodePrefix, Desc(SettingsKeys.LedgerCodePrefix));
            await UpdateSetting(SettingsKeys.VehicleTypeCodePrefix, _vehicleTypeCodePrefix, Desc(SettingsKeys.VehicleTypeCodePrefix));
            await UpdateSetting(SettingsKeys.DocumentTypeCodePrefix, _documentTypeCodePrefix, Desc(SettingsKeys.DocumentTypeCodePrefix));
            await UpdateSetting(SettingsKeys.OMCCodePrefix, _omcCodePrefix, Desc(SettingsKeys.OMCCodePrefix));
            await UpdateSetting(SettingsKeys.OMCCardCodePrefix, _omcCardCodePrefix, Desc(SettingsKeys.OMCCardCodePrefix));
            await UpdateSetting(SettingsKeys.VehicleRouteLocationCodePrefix, _vehicleRouteLocationCodePrefix, Desc(SettingsKeys.VehicleRouteLocationCodePrefix));
            await UpdateSetting(SettingsKeys.VehicleRouteCodePrefix, _vehicleRouteCodePrefix, Desc(SettingsKeys.VehicleRouteCodePrefix));
            await UpdateSetting(SettingsKeys.VehicleDriverCodePrefix, _vehicleDriverCodePrefix, Desc(SettingsKeys.VehicleDriverCodePrefix));
            await UpdateSetting(SettingsKeys.VehicleExpenseTypeCodePrefix, _vehicleExpenseTypeCodePrefix, Desc(SettingsKeys.VehicleExpenseTypeCodePrefix));

            await UpdateSetting(SettingsKeys.FinancialAccountingTransactionPrefix, _financialAccountingTransactionPrefix, Desc(SettingsKeys.FinancialAccountingTransactionPrefix));
            await UpdateSetting(SettingsKeys.VehicleTripTransactionPrefix, _vehicleTripTransactionPrefix, Desc(SettingsKeys.VehicleTripTransactionPrefix));
            await UpdateSetting(SettingsKeys.VehicleTripBillTransactionPrefix, _vehicleTripBillTransactionPrefix, Desc(SettingsKeys.VehicleTripBillTransactionPrefix));
            await UpdateSetting(SettingsKeys.VehicleExpenseTransactionPrefix, _vehicleExpenseTransactionPrefix, Desc(SettingsKeys.VehicleExpenseTransactionPrefix));

            await UpdateSetting(SettingsKeys.PrimaryCompanyLinkingId, _primaryCompanyLinkingId, Desc(SettingsKeys.PrimaryCompanyLinkingId));
            await UpdateSetting(SettingsKeys.CashLedgerId, _cashLedgerId, Desc(SettingsKeys.CashLedgerId));
            await UpdateSetting(SettingsKeys.GSTLedgerId, _gstLedgerId, Desc(SettingsKeys.GSTLedgerId));
            await UpdateSetting(SettingsKeys.DefaultSelectedVoucherId, _defaultSelectedVoucherId, Desc(SettingsKeys.DefaultSelectedVoucherId));
            await UpdateSetting(SettingsKeys.AutoRefreshReportTimer, _autoRefreshReportTimer.ToString(), Desc(SettingsKeys.AutoRefreshReportTimer));

            await _toastNotification.ShowAsync("Saved", "Settings saved successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save settings: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private static async Task UpdateSetting(string key, string value, string description)
    {
        await SettingsData.UpdateSettings(new SettingsModel
        {
            Key = key,
            Value = value ?? string.Empty,
            Description = description
        });
    }

    #endregion

    #region Reset Settings

    private async Task ShowResetConfirmation() => await _resetConfirmationDialog.ShowAsync();

    private async Task CancelReset() => await _resetConfirmationDialog.HideAsync();

    private async Task ConfirmReset()
    {
        try
        {
            await _resetConfirmationDialog.HideAsync();
            _isProcessing = true;

            await _toastNotification.ShowAsync("Resetting", "Restoring default settings...", ToastType.Info);
            await SettingsData.ResetSettings();
            await LoadData();
            await _toastNotification.ShowAsync("Reset", "Settings restored to defaults.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to reset settings: {ex.Message}", ToastType.Error);
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
            case "SaveSettings":
                await SaveSettings();
                break;
            case "ResetSettings":
                await ShowResetConfirmation();
                break;
        }
    }

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.OperationsDashboard);

    #endregion
}
