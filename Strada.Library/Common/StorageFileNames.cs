namespace Strada.Library.Common;

public static class StorageFileNames
{
	public static string UserDataFileName => "user_data.json";
	public static string UserDeviceIdDataFileName => "user_device_id_data.json";

	public static string FinancialAccountingDataFileName => "financial_accounting_data.json";
	public static string FinancialAccountingCartDataFileName => "financial_accounting_cart_data.json";

	public static string ExpenseDataFileName => "expense_data.json";
	public static string ExpenseDetailsCartDataFileName => "expense_details_cart_data.json";

	public static string OMCCardMoneyTransferDataFileName => "omc_card_money_transfer_data.json";
	public static string OMCCardMoneyTransferDetailsCartDataFileName => "omc_card_money_transfer_details_cart_data.json";

	public static string TripDataFileName => "trip_data.json";
	public static string TripExpensesCartDataFileName => "trip_expenses_cart_data.json";
	public static string TripCardPaymentsCartDataFileName => "trip_card_payments_cart_data.json";
	public static string TripLedgerPaymentsCartDataFileName => "trip_ledger_payments_cart_data.json";

	public static string BillDataFileName => "bill_data.json";
	public static string BillPendingTripsCartDataFileName => "bill_pending_trips_cart_data.json";
	public static string BillLedgerPaymentsCartDataFileName => "bill_ledger_payments_cart_data.json";
}