namespace StradaLibrary.Operations.Models;

public class SettingsModel
{
	public string Key { get; set; }
	public string Value { get; set; }
	public string Description { get; set; }
}

public static class SettingsKeys
{
	// Primary Configuration
	public static string PrimaryCompanyLinkingId => "PrimaryCompanyLinkingId";

	// Login Settings
	public static string EnableLoginWithCode => "EnableLoginWithCode";
	public static string EnableUsersToResetPassword => "EnableUsersToResetPassword";
	public static string MaxLoginAttempts => "MaxLoginAttempts";
	public static string CodeResendLimit => "CodeResendLimit";
	public static string CodeExpiryMinutes => "CodeExpiryMinutes";

	// Master Code Prefixes
	public static string LedgerCodePrefix => "LedgerCodePrefix";
	public static string VehicleTypeCodePrefix => "VehicleTypeCodePrefix";
	public static string DocumentTypeCodePrefix => "DocumentTypeCodePrefix";
	public static string OMCCodePrefix => "OMCCodePrefix";
	public static string OMCCardCodePrefix => "OMCCardCodePrefix";
	public static string LocationCodePrefix => "LocationCodePrefix";
	public static string RouteCodePrefix => "RouteCodePrefix";
	public static string DriverCodePrefix => "DriverCodePrefix";
	public static string ExpenseTypeCodePrefix => "ExpenseTypeCodePrefix";
	public static string TyreCompanyCodePrefix => "TyreCompanyCodePrefix";

	// Transaction Prefixes
	public static string FinancialAccountingTransactionPrefix => "FinancialAccountingTransactionPrefix";
	public static string TripTransactionPrefix => "TripTransactionPrefix";
	public static string BillTransactionPrefix => "BillTransactionPrefix";
	public static string ExpenseTransactionPrefix => "ExpenseTransactionPrefix";
	public static string OMCCardMoneyTransferTransactionPrefix => "OMCCardMoneyTransferTransactionPrefix";

	// Ledger Linking
	public static string CashLedgerId => "CashLedgerId";
	public static string GSTLedgerId => "GSTLedgerId";
	public static string BillLedgerId => "BillLedgerId";
	public static string OMCCardMoneyTransferLedgerId => "OMCCardMoneyTransferLedgerId";

	// Bank Reconciliation
	public static string BankAccountTypeId => "BankAccountTypeId";

	// Default Values
	public static string DefaultSelectedVoucherId => "DefaultSelectedVoucherId";
	public static string BillVoucherId => "BillVoucherId";
	public static string OMCCardMoneyTransferVoucherId => "OMCCardMoneyTransferVoucherId";

	// Report Settings
	public static string AutoRefreshReportTimer => "AutoRefreshReportTimer";
	public static string ReportWarningDays => "ReportWarningDays";

	// Notification Settings
	public static string NotificationEmail => "NotificationEmail";
}