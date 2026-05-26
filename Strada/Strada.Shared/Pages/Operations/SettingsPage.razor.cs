using Strada.Shared.Components.Dialog;

using StradaLibrary.Accounts.Masters.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Operations.Models;

namespace Strada.Shared.Pages.Operations;

public partial class SettingsPage
{
	#region Fields

	private bool _isLoading = true;
	private bool _isProcessing = false;

	private ToastNotification _toastNotification;
	private ConfirmationDialog _confirmationDialog;

	private string _confirmTitle = string.Empty;
	private string _confirmMessage = string.Empty;
	private Func<Task> _confirmAction;

	// Primary Configuration
	private string _primaryCompanyLinkingId = string.Empty;
	private CompanyModel _selectedCompany;
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
	private string _locationCodePrefix = string.Empty;
	private string _routeCodePrefix = string.Empty;
	private string _driverCodePrefix = string.Empty;
	private string _expenseTypeCodePrefix = string.Empty;

	// Transaction Prefixes
	private string _financialAccountingTransactionPrefix = string.Empty;
	private string _tripTransactionPrefix = string.Empty;
	private string _billTransactionPrefix = string.Empty;
	private string _expenseTransactionPrefix = string.Empty;

	// Ledger Linking
	private string _cashLedgerId = string.Empty;
	private LedgerModel _selectedCashLedger;
	private string _gstLedgerId = string.Empty;
	private LedgerModel _selectedGSTLedger;
	private string _billLedgerId = string.Empty;
	private LedgerModel _selectedBillLedger;
	private List<LedgerModel> _ledgers = [];

	// Default Values
	private string _defaultSelectedVoucherId = string.Empty;
	private VoucherModel _selectedDefaultVoucher;
	private string _billVoucherId = string.Empty;
	private VoucherModel _selectedBillVoucher;
	private List<VoucherModel> _vouchers = [];

	// Report Settings
	private int _autoRefreshReportTimer = 5;

	#endregion

	#region Load Data

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		try
		{
			await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Admin]);
			await LoadData();
			_isLoading = false;
			StateHasChanged();
		}
		catch { NavigationManager.NavigateTo(PageRouteNames.Dashboard); }
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

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.LocationCodePrefix);
		_locationCodePrefix = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.RouteCodePrefix);
		_routeCodePrefix = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.DriverCodePrefix);
		_driverCodePrefix = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.ExpenseTypeCodePrefix);
		_expenseTypeCodePrefix = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.FinancialAccountingTransactionPrefix);
		_financialAccountingTransactionPrefix = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.TripTransactionPrefix);
		_tripTransactionPrefix = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.BillTransactionPrefix);
		_billTransactionPrefix = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.ExpenseTransactionPrefix);
		_expenseTransactionPrefix = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
		_primaryCompanyLinkingId = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.CashLedgerId);
		_cashLedgerId = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.GSTLedgerId);
		_gstLedgerId = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.BillLedgerId);
		_billLedgerId = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.DefaultSelectedVoucherId);
		_defaultSelectedVoucherId = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.BillVoucherId);
		_billVoucherId = s?.Value ?? string.Empty;

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
		if (!string.IsNullOrWhiteSpace(_primaryCompanyLinkingId) && int.TryParse(_primaryCompanyLinkingId, out var companyId))
			_selectedCompany = _companies.FirstOrDefault(c => c.Id == companyId);

		if (!string.IsNullOrWhiteSpace(_cashLedgerId) && int.TryParse(_cashLedgerId, out var cashId))
			_selectedCashLedger = _ledgers.FirstOrDefault(l => l.Id == cashId);

		if (!string.IsNullOrWhiteSpace(_gstLedgerId) && int.TryParse(_gstLedgerId, out var gstId))
			_selectedGSTLedger = _ledgers.FirstOrDefault(l => l.Id == gstId);

		if (!string.IsNullOrWhiteSpace(_billLedgerId) && int.TryParse(_billLedgerId, out var billLedgerId))
			_selectedBillLedger = _ledgers.FirstOrDefault(l => l.Id == billLedgerId);

		if (!string.IsNullOrWhiteSpace(_defaultSelectedVoucherId) && int.TryParse(_defaultSelectedVoucherId, out var voucherId))
			_selectedDefaultVoucher = _vouchers.FirstOrDefault(v => v.Id == voucherId);

		if (!string.IsNullOrWhiteSpace(_billVoucherId) && int.TryParse(_billVoucherId, out var billVoucherId))
			_selectedBillVoucher = _vouchers.FirstOrDefault(v => v.Id == billVoucherId);
	}

	#endregion

	#region Change Handlers

	private void OnCompanyChange(CompanyModel value)
	{
		_selectedCompany = value;
		_primaryCompanyLinkingId = value?.Id.ToString() ?? string.Empty;
	}

	private void OnCashLedgerChange(LedgerModel value)
	{
		_selectedCashLedger = value;
		_cashLedgerId = value?.Id.ToString() ?? string.Empty;
	}

	private void OnGSTLedgerChange(LedgerModel value)
	{
		_selectedGSTLedger = value;
		_gstLedgerId = value?.Id.ToString() ?? string.Empty;
	}

	private void OnBillLedgerChange(LedgerModel value)
	{
		_selectedBillLedger = value;
		_billLedgerId = value?.Id.ToString() ?? string.Empty;
	}

	private void OnDefaultVoucherChange(VoucherModel value)
	{
		_selectedDefaultVoucher = value;
		_defaultSelectedVoucherId = value?.Id.ToString() ?? string.Empty;
	}

	private void OnBillVoucherChange(VoucherModel value)
	{
		_selectedBillVoucher = value;
		_billVoucherId = value?.Id.ToString() ?? string.Empty;
	}

	#endregion

	#region Save Settings

	private async Task SaveSettings()
	{
		if (_isProcessing) return;

		try
		{
			_isProcessing = true;
			StateHasChanged();

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
			await UpdateSetting(SettingsKeys.LocationCodePrefix, _locationCodePrefix, Desc(SettingsKeys.LocationCodePrefix));
			await UpdateSetting(SettingsKeys.RouteCodePrefix, _routeCodePrefix, Desc(SettingsKeys.RouteCodePrefix));
			await UpdateSetting(SettingsKeys.DriverCodePrefix, _driverCodePrefix, Desc(SettingsKeys.DriverCodePrefix));
			await UpdateSetting(SettingsKeys.ExpenseTypeCodePrefix, _expenseTypeCodePrefix, Desc(SettingsKeys.ExpenseTypeCodePrefix));

			await UpdateSetting(SettingsKeys.FinancialAccountingTransactionPrefix, _financialAccountingTransactionPrefix, Desc(SettingsKeys.FinancialAccountingTransactionPrefix));
			await UpdateSetting(SettingsKeys.TripTransactionPrefix, _tripTransactionPrefix, Desc(SettingsKeys.TripTransactionPrefix));
			await UpdateSetting(SettingsKeys.BillTransactionPrefix, _billTransactionPrefix, Desc(SettingsKeys.BillTransactionPrefix));
			await UpdateSetting(SettingsKeys.ExpenseTransactionPrefix, _expenseTransactionPrefix, Desc(SettingsKeys.ExpenseTransactionPrefix));

			await UpdateSetting(SettingsKeys.PrimaryCompanyLinkingId, _primaryCompanyLinkingId, Desc(SettingsKeys.PrimaryCompanyLinkingId));
			await UpdateSetting(SettingsKeys.CashLedgerId, _cashLedgerId, Desc(SettingsKeys.CashLedgerId));
			await UpdateSetting(SettingsKeys.GSTLedgerId, _gstLedgerId, Desc(SettingsKeys.GSTLedgerId));
			await UpdateSetting(SettingsKeys.BillLedgerId, _billLedgerId, Desc(SettingsKeys.BillLedgerId));
			await UpdateSetting(SettingsKeys.DefaultSelectedVoucherId, _defaultSelectedVoucherId, Desc(SettingsKeys.DefaultSelectedVoucherId));
			await UpdateSetting(SettingsKeys.BillVoucherId, _billVoucherId, Desc(SettingsKeys.BillVoucherId));
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

	private async Task ShowResetConfirmation() =>
		await ShowConfirmation("Reset", "Are you sure you want to restore all settings to their defaults?", ResetSettings);

	private async Task ResetSettings()
	{
		try
		{
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

	private async Task ShowConfirmation(string title, string message, Func<Task> action)
	{
		_confirmTitle = title;
		_confirmMessage = message;
		_confirmAction = action;
		StateHasChanged();
		await _confirmationDialog.ShowAsync();
	}

	private async Task OnConfirmed()
	{
		await _confirmationDialog.HideAsync();
		if (_confirmAction is not null)
			await _confirmAction();
		_confirmAction = null;
	}

	private async Task OnCancelled()
	{
		_confirmAction = null;
		await _confirmationDialog.HideAsync();
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

	#endregion
}
