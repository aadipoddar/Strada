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
	private bool _enableUsersToResetPassword = true;
	private int _maxLoginAttempts = 5;
	private int _codeResendLimit = 3;
	private int _codeExpiryMinutes = 10;

	// Master Code Prefixes
	private string _ledgerCodePrefix = string.Empty;
	private string _vehicleTypeCodePrefix = string.Empty;
	private string _documentTypeCodePrefix = string.Empty;
	private string _omcCodePrefix = string.Empty;
	private string _omcCardCodePrefix = string.Empty;
	private string _locationCodePrefix = string.Empty;
	private string _routeCodePrefix = string.Empty;
	private string _driverCodePrefix = string.Empty;
	private string _expenseTypeCodePrefix = string.Empty;
	private string _tyreCompanyCodePrefix = string.Empty;

	// Transaction Prefixes
	private string _financialAccountingTransactionPrefix = string.Empty;
	private string _tripTransactionPrefix = string.Empty;
	private string _billTransactionPrefix = string.Empty;
	private string _expenseTransactionPrefix = string.Empty;
	private string _omcCardMoneyTransferTransactionPrefix = string.Empty;

	// Ledger Linking
	private string _cashLedgerId = string.Empty;
	private LedgerModel _selectedCashLedger;
	private string _gstLedgerId = string.Empty;
	private LedgerModel _selectedGSTLedger;
	private string _billLedgerId = string.Empty;
	private LedgerModel _selectedBillLedger;
	private string _omcCardMoneyTransferLedgerId = string.Empty;
	private LedgerModel _selectedOMCCardMoneyTransferLedger;
	private List<LedgerModel> _ledgers = [];

	// Bank Reconciliation
	private string _bankAccountTypeId = string.Empty;
	private AccountTypeModel _selectedBankAccountType;
	private List<AccountTypeModel> _accountTypes = [];

	// Default Values
	private string _defaultSelectedVoucherId = string.Empty;
	private VoucherModel _selectedDefaultVoucher;
	private string _billVoucherId = string.Empty;
	private VoucherModel _selectedBillVoucher;
	private string _omcCardMoneyTransferVoucherId = string.Empty;
	private VoucherModel _selectedOMCCardMoneyTransferVoucher;
	private List<VoucherModel> _vouchers = [];

	// Report Settings
	private int _autoRefreshReportTimer = 5;
	private int _reportWarningDays = 30;

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
			await LoadAccountTypes();
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
		var map = (await CommonData.LoadTableData<SettingsModel>(OperationNames.Settings) ?? [])
			.ToDictionary(s => s.Key, s => s.Value);

		string Str(string key) => map.TryGetValue(key, out var v) ? v : null;
		int Int(string key, int fallback) => int.TryParse(Str(key), out var v) ? v : fallback;
		bool Bool(string key, bool fallback) => bool.TryParse(Str(key), out var v) ? v : fallback;

		// Primary Configuration
		_primaryCompanyLinkingId = Str(SettingsKeys.PrimaryCompanyLinkingId) ?? string.Empty;

		// Login Settings
		_enableLoginWithCode = Bool(SettingsKeys.EnableLoginWithCode, true);
		_enableUsersToResetPassword = Bool(SettingsKeys.EnableUsersToResetPassword, true);
		_maxLoginAttempts = Int(SettingsKeys.MaxLoginAttempts, 5);
		_codeResendLimit = Int(SettingsKeys.CodeResendLimit, 3);
		_codeExpiryMinutes = Int(SettingsKeys.CodeExpiryMinutes, 10);

		// Master Code Prefixes
		_ledgerCodePrefix = Str(SettingsKeys.LedgerCodePrefix) ?? string.Empty;
		_vehicleTypeCodePrefix = Str(SettingsKeys.VehicleTypeCodePrefix) ?? string.Empty;
		_documentTypeCodePrefix = Str(SettingsKeys.DocumentTypeCodePrefix) ?? string.Empty;
		_omcCodePrefix = Str(SettingsKeys.OMCCodePrefix) ?? string.Empty;
		_omcCardCodePrefix = Str(SettingsKeys.OMCCardCodePrefix) ?? string.Empty;
		_locationCodePrefix = Str(SettingsKeys.LocationCodePrefix) ?? string.Empty;
		_routeCodePrefix = Str(SettingsKeys.RouteCodePrefix) ?? string.Empty;
		_driverCodePrefix = Str(SettingsKeys.DriverCodePrefix) ?? string.Empty;
		_expenseTypeCodePrefix = Str(SettingsKeys.ExpenseTypeCodePrefix) ?? string.Empty;
		_tyreCompanyCodePrefix = Str(SettingsKeys.TyreCompanyCodePrefix) ?? string.Empty;

		// Transaction Prefixes
		_financialAccountingTransactionPrefix = Str(SettingsKeys.FinancialAccountingTransactionPrefix) ?? string.Empty;
		_tripTransactionPrefix = Str(SettingsKeys.TripTransactionPrefix) ?? string.Empty;
		_billTransactionPrefix = Str(SettingsKeys.BillTransactionPrefix) ?? string.Empty;
		_expenseTransactionPrefix = Str(SettingsKeys.ExpenseTransactionPrefix) ?? string.Empty;
		_omcCardMoneyTransferTransactionPrefix = Str(SettingsKeys.OMCCardMoneyTransferTransactionPrefix) ?? string.Empty;

		// Ledger Linking
		_cashLedgerId = Str(SettingsKeys.CashLedgerId) ?? string.Empty;
		_gstLedgerId = Str(SettingsKeys.GSTLedgerId) ?? string.Empty;
		_billLedgerId = Str(SettingsKeys.BillLedgerId) ?? string.Empty;
		_omcCardMoneyTransferLedgerId = Str(SettingsKeys.OMCCardMoneyTransferLedgerId) ?? string.Empty;

		// Bank Reconciliation
		_bankAccountTypeId = Str(SettingsKeys.BankAccountTypeId) ?? string.Empty;

		// Default Values
		_defaultSelectedVoucherId = Str(SettingsKeys.DefaultSelectedVoucherId) ?? string.Empty;
		_billVoucherId = Str(SettingsKeys.BillVoucherId) ?? string.Empty;
		_omcCardMoneyTransferVoucherId = Str(SettingsKeys.OMCCardMoneyTransferVoucherId) ?? string.Empty;

		// Report Settings
		_autoRefreshReportTimer = Int(SettingsKeys.AutoRefreshReportTimer, 5);
		_reportWarningDays = Int(SettingsKeys.ReportWarningDays, 30);
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

	private async Task LoadAccountTypes()
	{
		var result = await CommonData.LoadTableData<AccountTypeModel>(AccountNames.AccountType);
		_accountTypes = result ?? [];
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

		if (!string.IsNullOrWhiteSpace(_omcCardMoneyTransferLedgerId) && int.TryParse(_omcCardMoneyTransferLedgerId, out var omcCardMoneyTransferLedgerId))
			_selectedOMCCardMoneyTransferLedger = _ledgers.FirstOrDefault(l => l.Id == omcCardMoneyTransferLedgerId);

		if (!string.IsNullOrWhiteSpace(_bankAccountTypeId) && int.TryParse(_bankAccountTypeId, out var bankAccountTypeId))
			_selectedBankAccountType = _accountTypes.FirstOrDefault(a => a.Id == bankAccountTypeId);

		if (!string.IsNullOrWhiteSpace(_defaultSelectedVoucherId) && int.TryParse(_defaultSelectedVoucherId, out var voucherId))
			_selectedDefaultVoucher = _vouchers.FirstOrDefault(v => v.Id == voucherId);

		if (!string.IsNullOrWhiteSpace(_billVoucherId) && int.TryParse(_billVoucherId, out var billVoucherId))
			_selectedBillVoucher = _vouchers.FirstOrDefault(v => v.Id == billVoucherId);

		if (!string.IsNullOrWhiteSpace(_omcCardMoneyTransferVoucherId) && int.TryParse(_omcCardMoneyTransferVoucherId, out var omcCardMoneyTransferVoucherId))
			_selectedOMCCardMoneyTransferVoucher = _vouchers.FirstOrDefault(v => v.Id == omcCardMoneyTransferVoucherId);
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

	private void OnOMCCardMoneyTransferLedgerChange(LedgerModel value)
	{
		_selectedOMCCardMoneyTransferLedger = value;
		_omcCardMoneyTransferLedgerId = value?.Id.ToString() ?? string.Empty;
	}

	private void OnBankAccountTypeChange(AccountTypeModel value)
	{
		_selectedBankAccountType = value;
		_bankAccountTypeId = value?.Id.ToString() ?? string.Empty;
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

	private void OnOMCCardMoneyTransferVoucherChange(VoucherModel value)
	{
		_selectedOMCCardMoneyTransferVoucher = value;
		_omcCardMoneyTransferVoucherId = value?.Id.ToString() ?? string.Empty;
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

			// Primary Configuration
			await UpdateSetting(SettingsKeys.PrimaryCompanyLinkingId, _primaryCompanyLinkingId, Desc(SettingsKeys.PrimaryCompanyLinkingId));

			// Login Settings
			await UpdateSetting(SettingsKeys.EnableLoginWithCode, _enableLoginWithCode.ToString().ToLower(), Desc(SettingsKeys.EnableLoginWithCode));
			await UpdateSetting(SettingsKeys.EnableUsersToResetPassword, _enableUsersToResetPassword.ToString().ToLower(), Desc(SettingsKeys.EnableUsersToResetPassword));
			await UpdateSetting(SettingsKeys.MaxLoginAttempts, _maxLoginAttempts.ToString(), Desc(SettingsKeys.MaxLoginAttempts));
			await UpdateSetting(SettingsKeys.CodeResendLimit, _codeResendLimit.ToString(), Desc(SettingsKeys.CodeResendLimit));
			await UpdateSetting(SettingsKeys.CodeExpiryMinutes, _codeExpiryMinutes.ToString(), Desc(SettingsKeys.CodeExpiryMinutes));

			// Master Code Prefixes
			await UpdateSetting(SettingsKeys.LedgerCodePrefix, _ledgerCodePrefix, Desc(SettingsKeys.LedgerCodePrefix));
			await UpdateSetting(SettingsKeys.VehicleTypeCodePrefix, _vehicleTypeCodePrefix, Desc(SettingsKeys.VehicleTypeCodePrefix));
			await UpdateSetting(SettingsKeys.DocumentTypeCodePrefix, _documentTypeCodePrefix, Desc(SettingsKeys.DocumentTypeCodePrefix));
			await UpdateSetting(SettingsKeys.OMCCodePrefix, _omcCodePrefix, Desc(SettingsKeys.OMCCodePrefix));
			await UpdateSetting(SettingsKeys.OMCCardCodePrefix, _omcCardCodePrefix, Desc(SettingsKeys.OMCCardCodePrefix));
			await UpdateSetting(SettingsKeys.LocationCodePrefix, _locationCodePrefix, Desc(SettingsKeys.LocationCodePrefix));
			await UpdateSetting(SettingsKeys.RouteCodePrefix, _routeCodePrefix, Desc(SettingsKeys.RouteCodePrefix));
			await UpdateSetting(SettingsKeys.DriverCodePrefix, _driverCodePrefix, Desc(SettingsKeys.DriverCodePrefix));
			await UpdateSetting(SettingsKeys.ExpenseTypeCodePrefix, _expenseTypeCodePrefix, Desc(SettingsKeys.ExpenseTypeCodePrefix));
			await UpdateSetting(SettingsKeys.TyreCompanyCodePrefix, _tyreCompanyCodePrefix, Desc(SettingsKeys.TyreCompanyCodePrefix));

			// Transaction Prefixes
			await UpdateSetting(SettingsKeys.FinancialAccountingTransactionPrefix, _financialAccountingTransactionPrefix, Desc(SettingsKeys.FinancialAccountingTransactionPrefix));
			await UpdateSetting(SettingsKeys.TripTransactionPrefix, _tripTransactionPrefix, Desc(SettingsKeys.TripTransactionPrefix));
			await UpdateSetting(SettingsKeys.BillTransactionPrefix, _billTransactionPrefix, Desc(SettingsKeys.BillTransactionPrefix));
			await UpdateSetting(SettingsKeys.ExpenseTransactionPrefix, _expenseTransactionPrefix, Desc(SettingsKeys.ExpenseTransactionPrefix));
			await UpdateSetting(SettingsKeys.OMCCardMoneyTransferTransactionPrefix, _omcCardMoneyTransferTransactionPrefix, Desc(SettingsKeys.OMCCardMoneyTransferTransactionPrefix));

			// Ledger Linking
			await UpdateSetting(SettingsKeys.CashLedgerId, _cashLedgerId, Desc(SettingsKeys.CashLedgerId));
			await UpdateSetting(SettingsKeys.GSTLedgerId, _gstLedgerId, Desc(SettingsKeys.GSTLedgerId));
			await UpdateSetting(SettingsKeys.BillLedgerId, _billLedgerId, Desc(SettingsKeys.BillLedgerId));
			await UpdateSetting(SettingsKeys.OMCCardMoneyTransferLedgerId, _omcCardMoneyTransferLedgerId, Desc(SettingsKeys.OMCCardMoneyTransferLedgerId));

			// Bank Reconciliation
			await UpdateSetting(SettingsKeys.BankAccountTypeId, _bankAccountTypeId, Desc(SettingsKeys.BankAccountTypeId));

			// Default Values
			await UpdateSetting(SettingsKeys.DefaultSelectedVoucherId, _defaultSelectedVoucherId, Desc(SettingsKeys.DefaultSelectedVoucherId));
			await UpdateSetting(SettingsKeys.BillVoucherId, _billVoucherId, Desc(SettingsKeys.BillVoucherId));
			await UpdateSetting(SettingsKeys.OMCCardMoneyTransferVoucherId, _omcCardMoneyTransferVoucherId, Desc(SettingsKeys.OMCCardMoneyTransferVoucherId));

			// Report Settings
			await UpdateSetting(SettingsKeys.AutoRefreshReportTimer, _autoRefreshReportTimer.ToString(), Desc(SettingsKeys.AutoRefreshReportTimer));
			await UpdateSetting(SettingsKeys.ReportWarningDays, _reportWarningDays.ToString(), Desc(SettingsKeys.ReportWarningDays));

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
}
