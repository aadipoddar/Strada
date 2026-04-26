namespace StradaLibrary.Data.Operations;

public static class PageRouteNames
{
	#region Operations
	public const string Login = "/login";
	public const string LoginWithCode = "/login-with-code";
	public const string LoginWithCodeRedirect = "login-with-code-redirect"; // Do not put leading slash

	public const string Dashboard = "/";
	public const string OperationsDashboard = "/operations";
	public const string ReportsDashboard = "/reports";

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

	public const string VehicleDocumentTypeMaster = "/fleet/vehicle-document-type";
	public const string VehicleDocument = "/fleet/vehicle-document";

	public const string VehicleMaster = "/fleet/vehicle";
	public const string VehicleTypeMaster = "/fleet/vehicle-type";
	#endregion

	#region Vehicle Trip
	public const string VehicleTripDashboard = "/vehicle-trip";

	public const string TripAdvance = "/vehicle-trip/trip-advance";
	public const string TripAdvanceReport = "/vehicle-trip/trip-advance-report";
	public const string TripAdvanceExpensesReport = "/vehicle-trip/trip-advance-expenses-report";
	public const string TripAdvancePaymentsReport = "/vehicle-trip/trip-advance-payments-report";

	public const string VehicleRouteLocationMaster = "/vehicle-trip/vehicle-route-location";
	public const string VehicleRouteMaster = "/vehicle-trip/vehicle-route";
	public const string VehicleDriverMaster = "/vehicle-trip/vehicle-driver";

	public const string OMCMaster = "/vehicle-trip/omc";
	public const string OMCCardMaster = "/vehicle-trip/omc-card";
	#endregion

	#region Vehicle Expense
	public const string VehicleExpenseDashboard = "/vehicle-expense";

	public const string VehicleExpense = "/vehicle-expense/vehicle-expense";
	public const string VehicleExpenseReport = "/vehicle-expense/vehicle-expense-report";

	public const string VehicleExpenseDetailsReport = "/vehicle-expense/vehicle-expense-details-report";
	public const string VehicleExpenseTypeMaster = "/vehicle-expense/vehicle-expense-type";
	#endregion
}
