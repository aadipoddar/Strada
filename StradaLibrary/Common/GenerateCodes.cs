using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Fleet.Bill;
using Strada.Models.Fleet.Expense;
using Strada.Models.Fleet.OMC;
using Strada.Models.Fleet.Route;
using Strada.Models.Fleet.Trip;
using Strada.Models.Fleet.Tyre;
using Strada.Models.Fleet.Vehicle;
using Strada.Models.Fleet.VehicleDocument;
using Strada.Models.Operations;

using StradaLibrary.DataAccess;
using StradaLibrary.Fleet.Trip;
using StradaLibrary.Operations.Data;

namespace StradaLibrary.Common;

public static class GenerateCodes
{
	private static async Task<string> CheckDuplicateCode(string code, int numberLength, CodeType type, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var isDuplicate = true;
		while (isDuplicate)
		{
			switch (type)
			{
				#region Accounts
				case CodeType.FinancialAccounting:
					var accounting = await CommonData.LoadTableDataByTransactionNo<FinancialAccountingModel>(AccountNames.FinancialAccounting, code, sqlDataAccessTransaction);
					isDuplicate = accounting is not null;
					break;
				case CodeType.Ledger:
					var ledger = await CommonData.LoadTableDataByCode<LedgerModel>(AccountNames.Ledger, code, sqlDataAccessTransaction);
					isDuplicate = ledger is not null;
					break;
				#endregion

				#region Fleet
				case CodeType.Trip:
					var trip = await CommonData.LoadTableDataByTransactionNo<TripModel>(FleetNames.Trip, code, sqlDataAccessTransaction);
					isDuplicate = trip is not null;
					break;
				case CodeType.Bill:
					var bill = await CommonData.LoadTableDataByTransactionNo<BillModel>(FleetNames.Bill, code, sqlDataAccessTransaction);
					isDuplicate = bill is not null;
					break;
				case CodeType.Expense:
					var expense = await CommonData.LoadTableDataByTransactionNo<ExpenseModel>(FleetNames.Expense, code, sqlDataAccessTransaction);
					isDuplicate = expense is not null;
					break;
				case CodeType.OMCCardMoneyTransfer:
					var omcCardMoneyTransfer = await CommonData.LoadTableDataByTransactionNo<OMCCardMoneyTransferModel>(FleetNames.OMCCardMoneyTransfer, code, sqlDataAccessTransaction);
					isDuplicate = omcCardMoneyTransfer is not null;
					break;

				case CodeType.Location:
					var location = await CommonData.LoadTableDataByCode<LocationModel>(FleetNames.Location, code, sqlDataAccessTransaction);
					isDuplicate = location is not null;
					break;
				case CodeType.Route:
					var route = await CommonData.LoadTableDataByCode<RouteModel>(FleetNames.Route, code, sqlDataAccessTransaction);
					isDuplicate = route is not null;
					break;
				case CodeType.Driver:
					var driver = await CommonData.LoadTableDataByCode<DriverModel>(FleetNames.Driver, code, sqlDataAccessTransaction);
					isDuplicate = driver is not null;
					break;

				case CodeType.TyreCompany:
					var tyreCompany = await CommonData.LoadTableDataByCode<TyreCompanyModel>(FleetNames.TyreCompany, code, sqlDataAccessTransaction);
					isDuplicate = tyreCompany is not null;
					break;

				case CodeType.OMC:
					var omc = await CommonData.LoadTableDataByCode<OMCModel>(FleetNames.OMC, code, sqlDataAccessTransaction);
					isDuplicate = omc is not null;
					break;
				case CodeType.OMCCard:
					var omcCard = await CommonData.LoadTableDataByCode<OMCCardModel>(FleetNames.OMCCard, code, sqlDataAccessTransaction);
					isDuplicate = omcCard is not null;
					break;

				case CodeType.VehicleType:
					var vehicleType = await CommonData.LoadTableDataByCode<VehicleTypeModel>(FleetNames.VehicleType, code, sqlDataAccessTransaction);
					isDuplicate = vehicleType is not null;
					break;
				case CodeType.VehicleDocumentType:
					var vehicleDocumentType = await CommonData.LoadTableDataByCode<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType, code, sqlDataAccessTransaction);
					isDuplicate = vehicleDocumentType is not null;
					break;
				case CodeType.ExpenseType:
					var expenseType = await CommonData.LoadTableDataByCode<ExpenseTypeModel>(FleetNames.ExpenseType, code, sqlDataAccessTransaction);
					isDuplicate = expenseType is not null;
					break;
				#endregion
			}

			if (!isDuplicate)
				return code;

			var prefix = code[..(code.Length - numberLength)];
			var lastNumberPart = code[(code.Length - numberLength)..];
			if (int.TryParse(lastNumberPart, out int lastNumber))
			{
				int nextNumber = lastNumber + 1;
				code = $"{prefix}{nextNumber.ToString($"D{numberLength}")}";
			}
			else
				code = $"{prefix}{1.ToString($"D{numberLength}")}";
		}
		return code;
	}

	#region Accounts
	public static async Task<string> GenerateFinancialAccountingTransactionNo(FinancialAccountingModel accounting, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, accounting.FinancialYearId, sqlDataAccessTransaction);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, accounting.CompanyId, sqlDataAccessTransaction)).Code;
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.FinancialAccountingTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = await CommonData.LoadLastTableDataByCompanyFinancialYear<FinancialAccountingModel>(AccountNames.FinancialAccounting, accounting.CompanyId, accounting.FinancialYearId, sqlDataAccessTransaction);
		if (lastTransaction is not null)
		{
			var lastTransactionNo = lastTransaction.TransactionNo;
			if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + transactionPrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}{nextNumber:D5}", 5, CodeType.FinancialAccounting, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}00001", 5, CodeType.FinancialAccounting, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateLedgerCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var items = await CommonData.LoadTableData<LedgerModel>(AccountNames.Ledger, sqlDataAccessTransaction);
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.LedgerCodePrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = items.OrderByDescending(l => l.Id).FirstOrDefault();
		if (lastTransaction is not null)
		{
			var lastLedgerCode = lastTransaction.Code;
			if (lastLedgerCode.StartsWith(transactionPrefix))
			{
				var lastNumberPart = lastLedgerCode[transactionPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{transactionPrefix}{nextNumber:D5}", 5, CodeType.Ledger, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{transactionPrefix}00001", 5, CodeType.Ledger, sqlDataAccessTransaction);
	}
	#endregion

	#region Vehicle Transactions
	public static async Task<string> GenerateTripTransactionNo(TripModel trip, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, trip.FinancialYearId, sqlDataAccessTransaction);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, trip.CompanyId, sqlDataAccessTransaction)).Code;
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.TripTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = await CommonData.LoadLastTableDataByCompanyFinancialYear<TripModel>(FleetNames.Trip, trip.CompanyId, trip.FinancialYearId, sqlDataAccessTransaction);
		if (lastTransaction is not null)
		{
			var lastTransactionNo = lastTransaction.TransactionNo;
			if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + transactionPrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}{nextNumber:D5}", 5, CodeType.Trip, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}00001", 5, CodeType.Trip, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateTripSlNo(TripModel trip, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		// Sl No format is {CompanyCode}-{Number}, sequential per company per financial year (e.g. ARL-1, ARL-2, ...).
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, trip.CompanyId, sqlDataAccessTransaction)).Code;
		var nextNumber = 1;

		var lastTransaction = await CommonData.LoadLastTableDataByCompanyFinancialYear<TripModel>(FleetNames.Trip, trip.CompanyId, trip.FinancialYearId, sqlDataAccessTransaction);
		if (lastTransaction is not null && lastTransaction.SlNo.StartsWith($"{companyPrefix}-"))
		{
			var lastNumberPart = lastTransaction.SlNo[(companyPrefix.Length + 1)..]; // skip the company prefix and the '-' separator
			if (int.TryParse(lastNumberPart, out int lastNumber))
				nextNumber = lastNumber + 1;
		}

		while (await TripData.LoadTripBySlNoFinancialYear($"{companyPrefix}-{nextNumber}", trip.FinancialYearId, sqlDataAccessTransaction) is not null)
			nextNumber++;

		return $"{companyPrefix}-{nextNumber}";
	}

	public static async Task<string> GenerateBillTransactionNo(BillModel bill, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, bill.FinancialYearId, sqlDataAccessTransaction);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, bill.CompanyId, sqlDataAccessTransaction)).Code;
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.BillTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = await CommonData.LoadLastTableDataByCompanyFinancialYear<BillModel>(FleetNames.Bill, bill.CompanyId, bill.FinancialYearId, sqlDataAccessTransaction);
		if (lastTransaction is not null)
		{
			var lastTransactionNo = lastTransaction.TransactionNo;
			if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + transactionPrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}{nextNumber:D5}", 5, CodeType.Bill, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}00001", 5, CodeType.Bill, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateExpenseTransactionNo(ExpenseModel expense, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, expense.FinancialYearId, sqlDataAccessTransaction);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, expense.CompanyId, sqlDataAccessTransaction)).Code;
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ExpenseTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = await CommonData.LoadLastTableDataByCompanyFinancialYear<ExpenseModel>(FleetNames.Expense, expense.CompanyId, expense.FinancialYearId, sqlDataAccessTransaction);
		if (lastTransaction is not null)
		{
			var lastTransactionNo = lastTransaction.TransactionNo;
			if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + transactionPrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}{nextNumber:D5}", 5, CodeType.Expense, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}00001", 5, CodeType.Expense, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateOMCCardMoneyTransferTransactionNo(OMCCardMoneyTransferModel oMCCardMoneyTransfer, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, oMCCardMoneyTransfer.FinancialYearId, sqlDataAccessTransaction);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, oMCCardMoneyTransfer.CompanyId, sqlDataAccessTransaction)).Code;
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.OMCCardMoneyTransferTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = await CommonData.LoadLastTableDataByCompanyFinancialYear<OMCCardMoneyTransferModel>(FleetNames.OMCCardMoneyTransfer, oMCCardMoneyTransfer.CompanyId, oMCCardMoneyTransfer.FinancialYearId, sqlDataAccessTransaction);
		if (lastTransaction is not null)
		{
			var lastTransactionNo = lastTransaction.TransactionNo;
			if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + transactionPrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}{nextNumber:D5}", 5, CodeType.OMCCardMoneyTransfer, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{transactionPrefix}00001", 5, CodeType.OMCCardMoneyTransfer, sqlDataAccessTransaction);
	}
	#endregion

	#region Route
	public static async Task<string> GenerateLocationCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var items = await CommonData.LoadTableData<LocationModel>(FleetNames.Location, sqlDataAccessTransaction);
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.LocationCodePrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = items.OrderByDescending(rl => rl.Id).FirstOrDefault();
		if (lastTransaction is not null)
		{
			var lastLocationCode = lastTransaction.Code;
			if (lastLocationCode.StartsWith(transactionPrefix))
			{
				var lastNumberPart = lastLocationCode[transactionPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{transactionPrefix}{nextNumber:D5}", 5, CodeType.Location, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{transactionPrefix}00001", 5, CodeType.Location, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateRouteCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var items = await CommonData.LoadTableData<RouteModel>(FleetNames.Route, sqlDataAccessTransaction);
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.RouteCodePrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = items.OrderByDescending(vr => vr.Id).FirstOrDefault();
		if (lastTransaction is not null)
		{
			var lastRouteCode = lastTransaction.Code;
			if (lastRouteCode.StartsWith(transactionPrefix))
			{
				var lastNumberPart = lastRouteCode[transactionPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{transactionPrefix}{nextNumber:D5}", 5, CodeType.Route, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{transactionPrefix}00001", 5, CodeType.Route, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateDriverCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var items = await CommonData.LoadTableData<DriverModel>(FleetNames.Driver, sqlDataAccessTransaction);
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.DriverCodePrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = items.OrderByDescending(vd => vd.Id).FirstOrDefault();
		if (lastTransaction is not null)
		{
			var lastDriverCode = lastTransaction.Code;
			if (lastDriverCode.StartsWith(transactionPrefix))
			{
				var lastNumberPart = lastDriverCode[transactionPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{transactionPrefix}{nextNumber:D5}", 5, CodeType.Driver, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{transactionPrefix}00001", 5, CodeType.Driver, sqlDataAccessTransaction);
	}
	#endregion

	#region Tyre
	public static async Task<string> GenerateTyreCompanyCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var items = await CommonData.LoadTableData<TyreCompanyModel>(FleetNames.TyreCompany, sqlDataAccessTransaction);
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.TyreCompanyCodePrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = items.OrderByDescending(tc => tc.Id).FirstOrDefault();
		if (lastTransaction is not null)
		{
			var lastTransactionCode = lastTransaction.Code;
			if (lastTransactionCode.StartsWith(transactionPrefix))
			{
				var lastNumberPart = lastTransactionCode[transactionPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{transactionPrefix}{nextNumber:D5}", 5, CodeType.TyreCompany, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{transactionPrefix}00001", 5, CodeType.TyreCompany, sqlDataAccessTransaction);
	}
	#endregion

	#region OMC
	public static async Task<string> GenerateOMCCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var items = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC, sqlDataAccessTransaction);
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.OMCCodePrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = items.OrderByDescending(o => o.Id).FirstOrDefault();
		if (lastTransaction is not null)
		{
			var lastOmcCode = lastTransaction.Code;
			if (lastOmcCode.StartsWith(transactionPrefix))
			{
				var lastNumberPart = lastOmcCode[transactionPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{transactionPrefix}{nextNumber:D5}", 5, CodeType.OMC, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{transactionPrefix}00001", 5, CodeType.OMC, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateOMCCardCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var items = await CommonData.LoadTableData<OMCCardModel>(FleetNames.OMCCard, sqlDataAccessTransaction);
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.OMCCardCodePrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = items.OrderByDescending(oc => oc.Id).FirstOrDefault();
		if (lastTransaction is not null)
		{
			var lastOmcCardCode = lastTransaction.Code;
			if (lastOmcCardCode.StartsWith(transactionPrefix))
			{
				var lastNumberPart = lastOmcCardCode[transactionPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{transactionPrefix}{nextNumber:D5}", 5, CodeType.OMCCard, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{transactionPrefix}00001", 5, CodeType.OMCCard, sqlDataAccessTransaction);
	}
	#endregion

	#region Vehicle
	public static async Task<string> GenerateVehicleTypeCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var items = await CommonData.LoadTableData<VehicleTypeModel>(FleetNames.VehicleType, sqlDataAccessTransaction);
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleTypeCodePrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = items.OrderByDescending(vt => vt.Id).FirstOrDefault();
		if (lastTransaction is not null)
		{
			var lastVehicleTypeCode = lastTransaction.Code;
			if (lastVehicleTypeCode.StartsWith(transactionPrefix))
			{
				var lastNumberPart = lastVehicleTypeCode[transactionPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{transactionPrefix}{nextNumber:D5}", 5, CodeType.VehicleType, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{transactionPrefix}00001", 5, CodeType.VehicleType, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateVehicleDocumentTypeCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var items = await CommonData.LoadTableData<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType, sqlDataAccessTransaction);
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.DocumentTypeCodePrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = items.OrderByDescending(vdt => vdt.Id).FirstOrDefault();
		if (lastTransaction is not null)
		{
			var lastVehicleDocumentTypeCode = lastTransaction.Code;
			if (lastVehicleDocumentTypeCode.StartsWith(transactionPrefix))
			{
				var lastNumberPart = lastVehicleDocumentTypeCode[transactionPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{transactionPrefix}{nextNumber:D5}", 5, CodeType.VehicleDocumentType, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{transactionPrefix}00001", 5, CodeType.VehicleDocumentType, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateExpenseTypeCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var items = await CommonData.LoadTableData<ExpenseTypeModel>(FleetNames.ExpenseType, sqlDataAccessTransaction);
		var transactionPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ExpenseTypeCodePrefix, sqlDataAccessTransaction)).Value;

		var lastTransaction = items.OrderByDescending(v => v.Id).FirstOrDefault();
		if (lastTransaction is not null)
		{
			var lastExpenseTypeCode = lastTransaction.Code;
			if (lastExpenseTypeCode.StartsWith(transactionPrefix))
			{
				var lastNumberPart = lastExpenseTypeCode[transactionPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{transactionPrefix}{nextNumber:D5}", 5, CodeType.ExpenseType, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{transactionPrefix}00001", 5, CodeType.ExpenseType, sqlDataAccessTransaction);
	}
	#endregion
}
