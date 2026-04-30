namespace StradaLibrary.DataAccess;

public static class OperationNames
{
	#region Common
	public static string LoadTableData => "Load_TableData";
	public static string LoadTableDataById => "Load_TableData_By_Id";
	public static string LoadTableDataByStatus => "Load_TableData_By_Status";
	public static string LoadTableDataByMasterId => "Load_TableData_By_MasterId";
	public static string LoadTableDataByCode => "Load_TableData_By_Code";
	public static string LoadTableDataByTransactionNo => "Load_TableData_By_TransactionNo";
	public static string LoadTableDataByDate => "Load_TableData_By_Date";
	public static string LoadLastTableDataByCompanyFinancialYear => "Load_LastTableData_By_Company_FinancialYear";
	public static string LoadLastTableDataByFinancialYear => "Load_LastTableData_By_FinancialYear";
	public static string LoadCurrentDateTime => "Load_CurrentDateTime";
	#endregion

	#region Settings
	public static string Settings => "Settings";

	public static string UpdateSettings => "Update_Settings";
	public static string LoadSettingsByKey => "Load_Settings_By_Key";
	public static string ResetSettings => "Reset_Settings";
	#endregion

	#region User
	public static string User => "User";
	public static string InsertUser => "Insert_User";
	#endregion
}

public static class AccountNames
{
	#region Financial Accounting
	public static string FinancialAccounting => "FinancialAccounting";
	public static string FinancialAccountingDetail => "FinancialAccountingDetail";

	public static string InsertFinancialAccounting => "Insert_FinancialAccounting";
	public static string InsertFinancialAccountingDetail => "Insert_FinancialAccountingDetail";

	public static string LoadFinancialAccountingByVoucherReference => "Load_FinancialAccounting_By_Voucher_Reference";
	public static string LoadTrialBalanceByCompanyDate => "Load_TrialBalance_By_Company_Date";

	public static string FinancialAccountingOverview => "FinancialAccounting_Overview";
	public static string FinancialAccountingLedgerOverview => "FinancialAccounting_Ledger_Overview";
	#endregion

	#region Masters
	public static string Company => "Company";
	public static string Group => "Group";
	public static string Nature => "Nature";
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
	#region Vehicle Trip Bill
	public static string VehicleTripBill => "VehicleTripBill";
	public static string VehicleTripBillCardPayments => "VehicleTripBillCardPayments";
	public static string VehicleTripBillLedgerPayments => "VehicleTripBillLedgerPayments";

	public static string VehicleTripBillOverview => "VehicleTripBill_Overview";
	public static string VehicleTripBillCardPaymentsOverview => "VehicleTripBillCardPayments_Overview";
	public static string VehicleTripBillLedgerPaymentsOverview => "VehicleTripBillLedgerPayments_Overview";

	public static string InsertVehicleTripBill => "Insert_VehicleTripBill";
	public static string InsertVehicleTripBillCardPayments => "Insert_VehicleTripBillCardPayments";
	public static string InsertVehicleTripBillLedgerPayments => "Insert_VehicleTripBillLedgerPayments";
	#endregion

	#region Vehicle Trip
	public static string VehicleTrip => "VehicleTrip";
	public static string VehicleTripExpenses => "VehicleTripExpenses";
	public static string VehicleTripCardPayments => "VehicleTripCardPayments";

	public static string VehicleTripOverview => "VehicleTrip_Overview";
	public static string VehicleTripExpensesOverview => "VehicleTripExpenses_Overview";
	public static string VehicleTripCardPaymentsOverview => "VehicleTripCardPayments_Overview";
	public static string LoadVehicleTripOverviewByBillIdDate => "Load_VehicleTrip_Overview_By_BillId_Date";

	public static string InsertVehicleTrip => "Insert_VehicleTrip";
	public static string InsertVehicleTripExpenses => "Insert_VehicleTripExpenses";
	public static string InsertVehicleTripCardPayments => "Insert_VehicleTripCardPayments";
	#endregion
	
	#region Vehicle Expense
	public static string VehicleExpense => "VehicleExpense";
	public static string VehicleExpenseDetails => "VehicleExpenseDetails";

	public static string VehicleExpenseOverview => "VehicleExpense_Overview";
	public static string VehicleExpenseDetailsOverview => "VehicleExpenseDetails_Overview";

	public static string InsertVehicleExpense => "Insert_VehicleExpense";
	public static string InsertVehicleExpenseDetails => "Insert_VehicleExpenseDetails";
	#endregion

	#region Route
	public static string Location => "Location";
	public static string Route => "Route";
	public static string Driver => "Driver";
	
	public static string InsertLocation => "Insert_Location";
	public static string InsertRoute => "Insert_Route";
	public static string InsertDriver => "Insert_Driver";
	#endregion
	
	#region OMC
	public static string OMC => "OMC";
	public static string OMCCard => "OMCCard";

	public static string InsertOMC => "Insert_OMC";
	public static string InsertOMCCard => "Insert_OMCCard";
	#endregion

	#region Vehicle Document
	public static string VehicleDocumentType => "VehicleDocumentType";
	public static string VehicleDocument => "VehicleDocument";

	public static string InsertDocumentType => "Insert_VehicleDocumentType";
	public static string InsertDocument => "Insert_VehicleDocument";
	#endregion

	#region Vehicle
	public static string Vehicle => "Vehicle";
	public static string VehicleType => "VehicleType";
	public static string VehicleExpenseType => "VehicleExpenseType";

	public static string InsertVehicle => "Insert_Vehicle";
	public static string InsertVehicleType => "Insert_VehicleType";
	public static string InsertVehicleExpenseType => "Insert_VehicleExpenseType";
	#endregion
}