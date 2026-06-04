namespace StradaLibrary.Common;

public static class CommonNames
{
	#region Common
	public static string LoadTableData => "Load_TableData";
	public static string LoadTableDataById => "Load_TableData_By_Id";
	public static string LoadTableDataByStatus => "Load_TableData_By_Status";
	public static string LoadTableDataByMasterId => "Load_TableData_By_MasterId";
	public static string LoadTableDataByFinancialAccountingId => "Load_TableData_By_FinancialAccountingId";
	public static string LoadTableDataByCode => "Load_TableData_By_Code";
	public static string LoadTableDataByTransactionNo => "Load_TableData_By_TransactionNo";
	public static string LoadTableDataByDate => "Load_TableData_By_Date";
	public static string LoadLastTableData => "Load_LastTableData";
	public static string LoadLastTableDataByFinancialYear => "Load_LastTableData_By_FinancialYear";
	public static string LoadLastTableDataByCompanyFinancialYear => "Load_LastTableData_By_Company_FinancialYear";
	public static string LoadCurrentDateTime => "Load_CurrentDateTime";
	#endregion
}

public static class OperationNames
{
	#region Settings
	public static string Settings => "Settings";

	public static string UpdateSettings => "Update_Settings";
	public static string LoadSettingsByKey => "Load_Settings_By_Key";
	public static string ResetSettings => "Reset_Settings";
	#endregion

	#region User
	public static string User => "User";
	public static string InsertUser => "Insert_User";
	public static string LoadUserByPhoneEmail => "Load_User_By_Phone_Email";
	#endregion

	#region Audit Trail
	public static string AuditTrail => "AuditTrail";
	public static string InsertAuditTrail => "Insert_AuditTrail";
	#endregion
}

public static class AccountNames
{
	#region Financial Accounting
	public static string FinancialAccounting => "FinancialAccounting";
	public static string FinancialAccountingLedger => "FinancialAccountingLedger";

	public static string InsertFinancialAccounting => "Insert_FinancialAccounting";
	public static string InsertFinancialAccountingLedger => "Insert_FinancialAccountingLedger";

	public static string LoadFinancialAccountingByVoucherReference => "Load_FinancialAccounting_By_Voucher_Reference";
	public static string LoadTrialBalanceByCompanyDate => "Load_TrialBalance_By_Company_Date";

	public static string FinancialAccountingOverview => "FinancialAccounting_Overview";
	public static string FinancialAccountingLedgerOverview => "FinancialAccounting_Ledger_Overview";
	#endregion

	#region Masters
	public static string Company => "Company";
	public static string Group => "Group";
	public static string AccountType => "AccountType";
	public static string StateUT => "StateUT";
	public static string Ledger => "Ledger";
	public static string Voucher => "Voucher";
	public static string FinancialYear => "FinancialYear";

	public static string InsertCompany => "Insert_Company";
	public static string InsertGroup => "Insert_Group";
	public static string InsertAccountType => "Insert_AccountType";
	public static string InsertStateUT => "Insert_StateUT";
	public static string InsertLedger => "Insert_Ledger";
	public static string InsertVoucher => "Insert_Voucher";
	public static string InsertFinancialYear => "Insert_FinancialYear";

	public static string LoadFinancialYearByDateTime => "Load_FinancialYear_By_DateTime";
	#endregion
}

public static class FleetNames
{
	#region Bill
	public static string Bill => "Bill";
	public static string BillCardPayments => "BillCardPayments";
	public static string BillLedgerPayments => "BillLedgerPayments";

	public static string BillOverview => "Bill_Overview";
	public static string BillCardPaymentsOverview => "BillCardPayments_Overview";
	public static string BillLedgerPaymentsOverview => "BillLedgerPayments_Overview";

	public static string InsertBill => "Insert_Bill";
	public static string InsertBillCardPayments => "Insert_BillCardPayments";
	public static string InsertBillLedgerPayments => "Insert_BillLedgerPayments";
	#endregion

	#region Trip
	public static string Trip => "Trip";
	public static string TripExpenses => "TripExpenses";
	public static string TripCardPayments => "TripCardPayments";
	public static string TripLedgerPayments => "TripLedgerPayments";

	public static string TripOverview => "Trip_Overview";
	public static string TripExpensesOverview => "TripExpenses_Overview";
	public static string TripCardPaymentsOverview => "TripCardPayments_Overview";
	public static string TripLedgerPaymentsOverview => "TripLedgerPayments_Overview";
	public static string LoadTripOverviewByBillIdDate => "Load_Trip_Overview_By_BillId_Date";
	public static string LoadTripBySlNoFinancialYear => "Load_Trip_By_SlNo_FinancialYear";

	public static string InsertTrip => "Insert_Trip";
	public static string InsertTripExpenses => "Insert_TripExpenses";
	public static string InsertTripCardPayments => "Insert_TripCardPayments";
	public static string InsertTripLedgerPayments => "Insert_TripLedgerPayments";
	#endregion

	#region Expense
	public static string Expense => "Expense";
	public static string ExpenseDetails => "ExpenseDetails";

	public static string ExpenseOverview => "Expense_Overview";
	public static string ExpenseDetailsOverview => "ExpenseDetails_Overview";

	public static string InsertExpense => "Insert_Expense";
	public static string InsertExpenseDetails => "Insert_ExpenseDetails";
	#endregion

	#region Route
	public static string Location => "Location";
	public static string Route => "Route";
	public static string Driver => "Driver";
	public static string VehicleDriver => "VehicleDriver";

	public static string InsertLocation => "Insert_Location";
	public static string InsertRoute => "Insert_Route";
	public static string InsertDriver => "Insert_Driver";
	public static string InsertVehicleDriver => "Insert_VehicleDriver";

	public static string DeleteVehicleDriver => "Delete_VehicleDriver";
	#endregion

	#region OMC
	public static string OMC => "OMC";
	public static string OMCCard => "OMCCard";
	public static string OMCCardMoneyTransfer => "OMCCardMoneyTransfer";
	public static string OMCCardMoneyTransferDetails => "OMCCardMoneyTransferDetails";

	public static string OMCCardMoneyTransferOverview => "OMCCardMoneyTransfer_Overview";
	public static string OMCCardMoneyTransferDetailsOverview => "OMCCardMoneyTransferDetails_Overview";

	public static string InsertOMC => "Insert_OMC";
	public static string InsertOMCCard => "Insert_OMCCard";
	public static string InsertOMCCardMoneyTransfer => "Insert_OMCCardMoneyTransfer";
	public static string InsertOMCCardMoneyTransferDetails => "Insert_OMCCardMoneyTransferDetails";
	#endregion

	#region Vehicle Document
	public static string VehicleDocumentType => "VehicleDocumentType";
	public static string VehicleDocument => "VehicleDocument";

	public static string VehicleDocumentRenewalOverview => "VehicleDocument_Renewal_Overview";

	public static string InsertDocumentType => "Insert_VehicleDocumentType";
	public static string InsertDocument => "Insert_VehicleDocument";
	#endregion

	#region Vehicle
	public static string Vehicle => "Vehicle";
	public static string VehicleType => "VehicleType";
	public static string ExpenseType => "ExpenseType";

	public static string InsertVehicle => "Insert_Vehicle";
	public static string InsertVehicleType => "Insert_VehicleType";
	public static string InsertExpenseType => "Insert_ExpenseType";
	#endregion

	#region Tyre
	public static string TyreCompany => "TyreCompany";
	public static string TyreMounting => "TyreMounting";

	public static string InsertTyreCompany => "Insert_TyreCompany";
	public static string InsertTyreMounting => "Insert_TyreMounting";

	public static string DeleteTyreMounting => "Delete_TyreMounting";
	#endregion
}