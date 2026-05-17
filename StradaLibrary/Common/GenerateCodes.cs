using StradaLibrary.Accounts.FinancialAccounting.Models;
using StradaLibrary.Accounts.Masters.Models;
using StradaLibrary.DataAccess;
using StradaLibrary.Operations.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Fleet.Bill.Models;
using StradaLibrary.Fleet.Expense.Models;
using StradaLibrary.Fleet.OMC.Models;
using StradaLibrary.Fleet.Trip.Models;
using StradaLibrary.Fleet.Vehicle.Models;
using StradaLibrary.Fleet.Route.Models;
using StradaLibrary.Fleet.VehicleDocument.Models;

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
				case CodeType.FinancialAccounting:
					var accounting = await CommonData.LoadTableDataByTransactionNo<FinancialAccountingModel>(AccountNames.FinancialAccounting, code, sqlDataAccessTransaction);
					isDuplicate = accounting is not null;
					break;
				case CodeType.Ledger:
					var ledger = await CommonData.LoadTableDataByCode<LedgerModel>(AccountNames.Ledger, code, sqlDataAccessTransaction);
					isDuplicate = ledger is not null;
					break;

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
		var accountingPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.FinancialAccountingTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastAccounting = await CommonData.LoadLastTableDataByFinancialYear<FinancialAccountingModel>(AccountNames.FinancialAccounting, accounting.FinancialYearId, sqlDataAccessTransaction);
		if (lastAccounting is not null)
		{
			var lastTransactionNo = lastAccounting.TransactionNo;
			if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{accountingPrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + accountingPrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{accountingPrefix}{nextNumber:D5}", 5, CodeType.FinancialAccounting, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{accountingPrefix}00001", 5, CodeType.FinancialAccounting, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateLedgerCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var ledgers = await CommonData.LoadTableData<LedgerModel>(AccountNames.Ledger, sqlDataAccessTransaction);
		var ledgerPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.LedgerCodePrefix, sqlDataAccessTransaction)).Value;

		var lastLedger = ledgers.OrderByDescending(l => l.Id).FirstOrDefault();
		if (lastLedger is not null)
		{
			var lastLedgerCode = lastLedger.Code;
			if (lastLedgerCode.StartsWith(ledgerPrefix))
			{
				var lastNumberPart = lastLedgerCode[ledgerPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{ledgerPrefix}{nextNumber:D5}", 5, CodeType.Ledger, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{ledgerPrefix}00001", 5, CodeType.Ledger, sqlDataAccessTransaction);
	}
	#endregion

	#region Vehicle Transactions
	public static async Task<string> GenerateTripTransactionNo(TripModel trip, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, trip.FinancialYearId, sqlDataAccessTransaction);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, trip.CompanyId, sqlDataAccessTransaction)).Code;
		var tripPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.TripTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastTrip = await CommonData.LoadLastTableDataByFinancialYear<TripModel>(FleetNames.Trip, trip.FinancialYearId, sqlDataAccessTransaction);
		if (lastTrip is not null)
		{
			var lastTransactionNo = lastTrip.TransactionNo;
			if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{tripPrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + tripPrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{tripPrefix}{nextNumber:D5}", 5, CodeType.Trip, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{tripPrefix}00001", 5, CodeType.Trip, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateBillTransactionNo(BillModel bill, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, bill.FinancialYearId, sqlDataAccessTransaction);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, bill.CompanyId, sqlDataAccessTransaction)).Code;
		var billPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.BillTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastBill = await CommonData.LoadLastTableDataByFinancialYear<BillModel>(FleetNames.Bill, bill.FinancialYearId, sqlDataAccessTransaction);
		if (lastBill is not null)
		{
			var lastTransactionNo = lastBill.TransactionNo;
			if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{billPrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + billPrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{billPrefix}{nextNumber:D5}", 5, CodeType.Bill, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{billPrefix}00001", 5, CodeType.Bill, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateExpenseTransactionNo(ExpenseModel expense, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, expense.FinancialYearId, sqlDataAccessTransaction);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, expense.CompanyId, sqlDataAccessTransaction)).Code;
		var expensePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ExpenseTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastExpense = await CommonData.LoadLastTableDataByFinancialYear<ExpenseModel>(FleetNames.Expense, expense.FinancialYearId, sqlDataAccessTransaction);
		if (lastExpense is not null)
		{
			var lastTransactionNo = lastExpense.TransactionNo;
			if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{expensePrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + expensePrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{expensePrefix}{nextNumber:D5}", 5, CodeType.Expense, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{expensePrefix}00001", 5, CodeType.Expense, sqlDataAccessTransaction);
	}
	#endregion

	#region Route
	public static async Task<string> GenerateLocationCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var locations = await CommonData.LoadTableData<LocationModel>(FleetNames.Location, sqlDataAccessTransaction);
		var locationPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.LocationCodePrefix, sqlDataAccessTransaction)).Value;

		var lastLocation = locations.OrderByDescending(rl => rl.Id).FirstOrDefault();
		if (lastLocation is not null)
		{
			var lastLocationCode = lastLocation.Code;
			if (lastLocationCode.StartsWith(locationPrefix))
			{
				var lastNumberPart = lastLocationCode[locationPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{locationPrefix}{nextNumber:D5}", 5, CodeType.Location, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{locationPrefix}00001", 5, CodeType.Location, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateRouteCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var routes = await CommonData.LoadTableData<RouteModel>(FleetNames.Route, sqlDataAccessTransaction);
		var routePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.RouteCodePrefix, sqlDataAccessTransaction)).Value;

		var lastRoute = routes.OrderByDescending(vr => vr.Id).FirstOrDefault();
		if (lastRoute is not null)
		{
			var lastRouteCode = lastRoute.Code;
			if (lastRouteCode.StartsWith(routePrefix))
			{
				var lastNumberPart = lastRouteCode[routePrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{routePrefix}{nextNumber:D5}", 5, CodeType.Route, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{routePrefix}00001", 5, CodeType.Route, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateDriverCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var drivers = await CommonData.LoadTableData<DriverModel>(FleetNames.Driver, sqlDataAccessTransaction);
		var driverPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.DriverCodePrefix, sqlDataAccessTransaction)).Value;

		var lastDriver = drivers.OrderByDescending(vd => vd.Id).FirstOrDefault();
		if (lastDriver is not null)
		{
			var lastDriverCode = lastDriver.Code;
			if (lastDriverCode.StartsWith(driverPrefix))
			{
				var lastNumberPart = lastDriverCode[driverPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{driverPrefix}{nextNumber:D5}", 5, CodeType.Driver, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{driverPrefix}00001", 5, CodeType.Driver, sqlDataAccessTransaction);
	}
	#endregion

	#region OMC
	public static async Task<string> GenerateOMCCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var omcs = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC, sqlDataAccessTransaction);
		var omcPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.OMCCodePrefix, sqlDataAccessTransaction)).Value;

		var lastOmc = omcs.OrderByDescending(o => o.Id).FirstOrDefault();
		if (lastOmc is not null)
		{
			var lastOmcCode = lastOmc.Code;
			if (lastOmcCode.StartsWith(omcPrefix))
			{
				var lastNumberPart = lastOmcCode[omcPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{omcPrefix}{nextNumber:D5}", 5, CodeType.OMC, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{omcPrefix}00001", 5, CodeType.OMC, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateOMCCardCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var omcCards = await CommonData.LoadTableData<OMCCardModel>(FleetNames.OMCCard, sqlDataAccessTransaction);
		var omcCardPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.OMCCardCodePrefix, sqlDataAccessTransaction)).Value;

		var lastOmcCard = omcCards.OrderByDescending(oc => oc.Id).FirstOrDefault();
		if (lastOmcCard is not null)
		{
			var lastOmcCardCode = lastOmcCard.Code;
			if (lastOmcCardCode.StartsWith(omcCardPrefix))
			{
				var lastNumberPart = lastOmcCardCode[omcCardPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{omcCardPrefix}{nextNumber:D5}", 5, CodeType.OMCCard, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{omcCardPrefix}00001", 5, CodeType.OMCCard, sqlDataAccessTransaction);
	}
	#endregion

	#region Vehicle
	public static async Task<string> GenerateVehicleTypeCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var vehicleTypes = await CommonData.LoadTableData<VehicleTypeModel>(FleetNames.VehicleType, sqlDataAccessTransaction);
		var vehicleTypePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleTypeCodePrefix, sqlDataAccessTransaction)).Value;

		var lastVehicleType = vehicleTypes.OrderByDescending(vt => vt.Id).FirstOrDefault();
		if (lastVehicleType is not null)
		{
			var lastVehicleTypeCode = lastVehicleType.Code;
			if (lastVehicleTypeCode.StartsWith(vehicleTypePrefix))
			{
				var lastNumberPart = lastVehicleTypeCode[vehicleTypePrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{vehicleTypePrefix}{nextNumber:D5}", 5, CodeType.VehicleType, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{vehicleTypePrefix}00001", 5, CodeType.VehicleType, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateVehicleDocumentTypeCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var vehicleDocumentTypes = await CommonData.LoadTableData<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType, sqlDataAccessTransaction);
		var vehicleDocumentTypePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.DocumentTypeCodePrefix, sqlDataAccessTransaction)).Value;

		var lastVehicleDocumentType = vehicleDocumentTypes.OrderByDescending(vdt => vdt.Id).FirstOrDefault();
		if (lastVehicleDocumentType is not null)
		{
			var lastVehicleDocumentTypeCode = lastVehicleDocumentType.Code;
			if (lastVehicleDocumentTypeCode.StartsWith(vehicleDocumentTypePrefix))
			{
				var lastNumberPart = lastVehicleDocumentTypeCode[vehicleDocumentTypePrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{vehicleDocumentTypePrefix}{nextNumber:D5}", 5, CodeType.VehicleDocumentType, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{vehicleDocumentTypePrefix}00001", 5, CodeType.VehicleDocumentType, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateExpenseTypeCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var expenseTypes = await CommonData.LoadTableData<ExpenseTypeModel>(FleetNames.ExpenseType, sqlDataAccessTransaction);
		var expenseTypePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ExpenseTypeCodePrefix, sqlDataAccessTransaction)).Value;

		var lastExpenseType = expenseTypes.OrderByDescending(v => v.Id).FirstOrDefault();
		if (lastExpenseType is not null)
		{
			var lastExpenseTypeCode = lastExpenseType.Code;
			if (lastExpenseTypeCode.StartsWith(expenseTypePrefix))
			{
				var lastNumberPart = lastExpenseTypeCode[expenseTypePrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{expenseTypePrefix}{nextNumber:D5}", 5, CodeType.ExpenseType, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{expenseTypePrefix}00001", 5, CodeType.ExpenseType, sqlDataAccessTransaction);
	}
	#endregion
}
