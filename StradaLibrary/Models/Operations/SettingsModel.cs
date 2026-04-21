namespace StradaLibrary.Models.Operations;

public class SettingsModel
{
	public string Key { get; set; }
	public string Value { get; set; }
	public string Description { get; set; }
}

public static class SettingsKeys
{
	public static string EnableLoginWithCode => "EnableLoginWithCode";
	public static string MaxLoginAttempts => "MaxLoginAttempts";
	public static string EnableUsersToResetPassword => "EnableUsersToResetPassword";
	public static string CodeResendLimit => "CodeResendLimit";
	public static string CodeExpiryMinutes => "CodeExpiryMinutes";

	public static string LedgerCodePrefix => "LedgerCodePrefix";

	public static string VehicleTypeCodePrefix => "VehicleTypeCodePrefix";
	public static string DocumentTypeCodePrefix => "DocumentTypeCodePrefix";
	public static string OMCCodePrefix => "OMCCodePrefix";
	public static string OMCCardCodePrefix => "OMCCardCodePrefix";
	public static string VehicleRouteLocationCodePrefix => "VehicleRouteLocationCodePrefix";
	public static string VehicleDriverCodePrefix => "VehicleDriverCodePrefix";
	public static string VehicleRouteExpenseTypeCodePrefix => "VehicleRouteExpenseTypeCodePrefix";

	public static string FinancialAccountingTransactionPrefix => "FinancialAccountingTransactionPrefix";

	public static string PrimaryCompanyLinkingId => "PrimaryCompanyLinkingId";

	public static string CashLedgerId => "CashLedgerId";
	public static string GSTLedgerId => "GSTLedgerId";

	public static string DefaultSelectedVoucherId => "DefaultSelectedVoucherId";

	public static string AutoRefreshReportTimer => "AutoRefreshReportTimer";
}