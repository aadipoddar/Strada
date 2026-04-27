namespace StradaLibrary.Data.Operations;

public static class StorageFileNames
{
	public static string UserDataFileName => "user_data.json";
	public static string UserDeviceIdDataFileName => "user_device_id_data.json";

	public static string FinancialAccountingDataFileName => "financial_accounting_data.json";
	public static string FinancialAccountingCartDataFileName => "financial_accounting_cart_data.json";

	public static string VehicleExpenseDataFileName => "vehicle_expense_data.json";
	public static string VehicleExpenseDetailsCartDataFileName => "vehicle_expense_details_cart_data.json";

	public static string VehicleTripDataFileName => "vehicle_trip_data.json";
	public static string VehicleTripExpensesCartDataFileName => "vehicle_trip_expenses_cart_data.json";
	public static string VehicleTripPaymentsCartDataFileName => "vehicle_trip_payments_cart_data.json";
	
	public static string VehicleTripBillDataFileName => "vehicle_trip_bill_data.json";
	public static string VehicleTripBillPendingTripsCartDataFileName => "vehicle_trip_bill_pending_trips_cart_data.json";
	public static string VehicleTripBillCardPaymentsCartDataFileName => "vehicle_trip_bill_card_payments_cart_data.json";
	public static string VehicleTripBillLedgerPaymentsCartDataFileName => "vehicle_trip_bill_ledger_payments_cart_data.json";
}