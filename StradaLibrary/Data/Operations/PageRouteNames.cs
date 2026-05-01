namespace StradaLibrary.Data.Operations;

public static class PageRouteNames
{
	#region Operations
	public const string Login = "/login";
	public const string LoginWithCode = "/login-with-code";
	public const string LoginWithCodeRedirect = "login-with-code-redirect"; // Do not put leading slash

	public const string Dashboard = "/";
	public const string OperationsDashboard = "/operations";

	public const string User = "/operations/user";
	public const string Settings = "/operations/settings";
	#endregion

	#region Accounts
	public const string AccountsDashboard = "/accounts";
	public const string FinancialAccounting = "/accounts/financial-accounting";

	public const string FinancialAccountingReport = "/accounts/reports/financial-accounting";
	public const string AccountingLedgerReport = "/accounts/reports/accounting-ledger";
	public const string TrialBalanceReport = "/accounts/reports/trial-balance";
	public const string ProfitAndLossReport = "/accounts/reports/profit-and-loss";
	public const string BalanceSheetReport = "/accounts/reports/balance-sheet";

	public const string CompanyMaster = "/accounts/masters/company";
	public const string LedgerMaster = "/accounts/masters/ledger";
	public const string VoucherMaster = "/accounts/masters/voucher";
	public const string GroupMaster = "/accounts/masters/group";
	public const string AccountTypeMaster = "/accounts/masters/account-type";
	public const string FinancialYearMaster = "/accounts/masters/financial-year";
	public const string StateUTMaster = "/accounts/masters/state-ut";
	#endregion

	#region Fleet
	public const string FleetDashboard = "/fleet";
	public const string FleetReportsDashboard = "/fleet-reports";
	public const string FleetTransactionsDashboard = "/fleet-transactions";
	public const string FleetMastersDashboard = "/fleet-masters";

	public const string VehicleRegisterReport = "/fleet/vehicle-register-report";

	public const string Expense = "/fleet/expense";
	public const string ExpenseReport = "/fleet/expense-report";
	public const string ExpenseDetailsReport = "/fleet/expense-details-report";

	public const string Trip = "/fleet/trip";
	public const string TripReport = "/fleet/trip-report";
	public const string TripExpensesReport = "/fleet/trip-expenses-report";
	public const string TripPaymentsReport = "/fleet/trip-payments-report";

	public const string Bill = "/fleet/bill";
	public const string BillReport = "/fleet/bill-report";
	public const string BillLedgerPaymentsReport = "/fleet/bill-ledger-payments-report";

	public const string LocationMaster = "/fleet/location";
	public const string RouteMaster = "/fleet/route";
	public const string DriverMaster = "/fleet/driver";

	public const string OMCMaster = "/fleet/omc";
	public const string OMCCardMaster = "/fleet/omc-card";

	public const string VehicleDocumentTypeMaster = "/fleet/vehicle-document-type";
	public const string VehicleDocument = "/fleet/vehicle-document";

	public const string VehicleMaster = "/fleet/vehicle";
	public const string VehicleTypeMaster = "/fleet/vehicle-type";
	public const string ExpenseTypeMaster = "/fleet/expense-type";
	#endregion
}
