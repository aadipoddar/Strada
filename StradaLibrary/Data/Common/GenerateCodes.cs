using StradaLibrary.Data.Operations;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Accounts.FinancialAccounting;
using StradaLibrary.Exports.Accounts.Masters;
using StradaLibrary.Exports.Fleet.OMC;
using StradaLibrary.Exports.Fleet.Route;
using StradaLibrary.Exports.Fleet.Vehicle;
using StradaLibrary.Exports.Fleet.VehicleDocument;
using StradaLibrary.Exports.Fleet.Trip;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.FinancialAccounting;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.Route;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleDocument;
using StradaLibrary.Models.Fleet.Trip;
using StradaLibrary.Models.Operations;
using StradaLibrary.Models.Fleet.Expense;
using StradaLibrary.Exports.Fleet.Expense;
using StradaLibrary.Models.Fleet.Bill;
using StradaLibrary.Exports.Fleet.Bill;

namespace StradaLibrary.Data.Common;

public static class GenerateCodes
{
	public static async Task<DecodeTransactionNoModel> DecodeTransactionNo(string transactionNo)
	{
		DecodeTransactionNoModel decodeTransactionNoModel = new();

		if (string.IsNullOrWhiteSpace(transactionNo))
			return decodeTransactionNoModel;

		var beforecodeTypePart = "";
		var codeTypePart = "";

		foreach (var character in transactionNo)
		{
			if (char.IsLetter(character))
				beforecodeTypePart += character;

			if (char.IsDigit(character))
				break;
		}

		foreach (var character in transactionNo[(beforecodeTypePart.Length + 2)..])
		{
			if (char.IsLetter(character))
				codeTypePart += character;

			if (char.IsDigit(character))
				break;
		}

		var settings = await CommonData.LoadTableData<SettingsModel>(OperationNames.Settings);

		if (string.IsNullOrWhiteSpace(codeTypePart))
		{
			if (string.IsNullOrWhiteSpace(beforecodeTypePart))
				return decodeTransactionNoModel;

			codeTypePart = beforecodeTypePart;

			var settingsKey = settings.FirstOrDefault(s => s.Value == codeTypePart).Key;
			settingsKey = settingsKey.Replace("CodePrefix", "");
			decodeTransactionNoModel.CodeType = Enum.Parse<CodeType>(settingsKey);
		}

		else
		{
			var settingsKey = settings.FirstOrDefault(s => s.Value == codeTypePart).Key;
			settingsKey = settingsKey.Replace("TransactionPrefix", "");
			decodeTransactionNoModel.CodeType = Enum.Parse<CodeType>(settingsKey);
		}

		switch (decodeTransactionNoModel.CodeType)
		{
			case CodeType.FinancialAccounting:
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<FinancialAccountingModel>(AccountNames.FinancialAccounting, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.FinancialAccounting}/{(decodeTransactionNoModel.TransactionModel as FinancialAccountingModel).Id}";
				decodeTransactionNoModel.PDFStream = await FinancialAccountingInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as FinancialAccountingModel).Id, InvoiceExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await FinancialAccountingInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as FinancialAccountingModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.Ledger:
				var ledgers = await CommonData.LoadTableData<LedgerModel>(AccountNames.Ledger);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<LedgerModel>(AccountNames.Ledger, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.LedgerMaster}";
				decodeTransactionNoModel.PDFStream = await LedgerExport.ExportMaster(ledgers, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await LedgerExport.ExportMaster(ledgers, ReportExportType.Excel);
				break;

			case CodeType.Trip:
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<TripModel>(FleetNames.Trip, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.Trip}/{(decodeTransactionNoModel.TransactionModel as TripModel).Id}";
				decodeTransactionNoModel.PDFStream = await TripInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as TripModel).Id, InvoiceExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await TripInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as TripModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.Bill:
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<BillModel>(FleetNames.Bill, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.Bill}/{(decodeTransactionNoModel.TransactionModel as BillModel).Id}";
				decodeTransactionNoModel.PDFStream = await BillInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as BillModel).Id, InvoiceExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await BillInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as BillModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.Expense:
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<ExpenseModel>(FleetNames.Expense, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.Expense}/{(decodeTransactionNoModel.TransactionModel as ExpenseModel).Id}";
				decodeTransactionNoModel.PDFStream = await ExpenseInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as ExpenseModel).Id, InvoiceExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await ExpenseInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as ExpenseModel).Id, InvoiceExportType.Excel);
				break;

			case CodeType.Location:
				var locations = await CommonData.LoadTableData<LocationModel>(FleetNames.Location);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<LocationModel>(FleetNames.Location, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.LocationMaster}/{(decodeTransactionNoModel.TransactionModel as LocationModel).Id}";
				decodeTransactionNoModel.PDFStream = await LocationExport.ExportMaster(locations, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await LocationExport.ExportMaster(locations, ReportExportType.Excel);
				break;
			case CodeType.Route:
				var routes = await CommonData.LoadTableData<RouteModel>(FleetNames.Route);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<RouteModel>(FleetNames.Route, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.RouteMaster}/{(decodeTransactionNoModel.TransactionModel as RouteModel).Id}";
				decodeTransactionNoModel.PDFStream = await RouteExport.ExportMaster(routes, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await RouteExport.ExportMaster(routes, ReportExportType.Excel);
				break;
			case CodeType.Driver:
				var drivers = await CommonData.LoadTableData<DriverModel>(FleetNames.Driver);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<DriverModel>(FleetNames.Driver, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.DriverMaster}/{(decodeTransactionNoModel.TransactionModel as DriverModel).Id}";
				decodeTransactionNoModel.PDFStream = await DriverExport.ExportMaster(drivers, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await DriverExport.ExportMaster(drivers, ReportExportType.Excel);
				break;

			case CodeType.OMC:
				var omcs = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<OMCModel>(FleetNames.OMC, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.OMCMaster}/{(decodeTransactionNoModel.TransactionModel as OMCModel).Id}";
				decodeTransactionNoModel.PDFStream = await OMCExport.ExportMaster(omcs, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await OMCExport.ExportMaster(omcs, ReportExportType.Excel);
				break;
			case CodeType.OMCCard:
				var omcCards = await CommonData.LoadTableData<OMCCardModel>(FleetNames.OMCCard);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<OMCCardModel>(FleetNames.OMCCard, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.OMCCardMaster}/{(decodeTransactionNoModel.TransactionModel as OMCCardModel).Id}";
				decodeTransactionNoModel.PDFStream = await OMCCardExport.ExportMaster(omcCards, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await OMCCardExport.ExportMaster(omcCards, ReportExportType.Excel);
				break;

			case CodeType.VehicleType:
				var vehicleTypes = await CommonData.LoadTableData<VehicleTypeModel>(FleetNames.VehicleType);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<VehicleTypeModel>(FleetNames.VehicleType, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.VehicleTypeMaster}/{(decodeTransactionNoModel.TransactionModel as VehicleTypeModel).Id}";
				decodeTransactionNoModel.PDFStream = await VehicleTypeExport.ExportMaster(vehicleTypes, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await VehicleTypeExport.ExportMaster(vehicleTypes, ReportExportType.Excel);
				break;
			case CodeType.VehicleDocumentType:
				var vehicleDocumentTypes = await CommonData.LoadTableData<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.VehicleDocumentTypeMaster}/{(decodeTransactionNoModel.TransactionModel as VehicleDocumentTypeModel).Id}";
				decodeTransactionNoModel.PDFStream = await VehicleDocumentTypeExport.ExportMaster(vehicleDocumentTypes, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await VehicleDocumentTypeExport.ExportMaster(vehicleDocumentTypes, ReportExportType.Excel);
				break;
			case CodeType.ExpenseType:
				var expenseTypes = await CommonData.LoadTableData<ExpenseTypeModel>(FleetNames.ExpenseType);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<ExpenseTypeModel>(FleetNames.ExpenseType, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.ExpenseTypeMaster}/{(decodeTransactionNoModel.TransactionModel as ExpenseTypeModel).Id}";
				decodeTransactionNoModel.PDFStream = await ExpenseTypeExport.ExportMaster(expenseTypes, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await ExpenseTypeExport.ExportMaster(expenseTypes, ReportExportType.Excel);
				break;
			default:
				break;
		}

		return decodeTransactionNoModel;
	}

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
