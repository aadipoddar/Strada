using StradaLibrary.Data.Operations;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Accounts.FinancialAccounting;
using StradaLibrary.Exports.Accounts.Masters;
using StradaLibrary.Exports.Fleet.OMC;
using StradaLibrary.Exports.Fleet.Vehicle;
using StradaLibrary.Exports.Fleet.VehicleDocument;
using StradaLibrary.Exports.Fleet.VehicleRepair;
using StradaLibrary.Exports.Fleet.VehicleRoute;
using StradaLibrary.Exports.Fleet.VehicleTrip;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.FinancialAccounting;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleDocument;
using StradaLibrary.Models.Fleet.VehicleRepair;
using StradaLibrary.Models.Fleet.VehicleRoute;
using StradaLibrary.Models.Fleet.VehicleTrip;
using StradaLibrary.Models.Operations;

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

			case CodeType.VehicleTrip:
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<VehicleTripModel>(FleetNames.VehicleTrip, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.VehicleTrip}/{(decodeTransactionNoModel.TransactionModel as VehicleTripModel).Id}";
				decodeTransactionNoModel.PDFStream = await VehicleTripInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as VehicleTripModel).Id, InvoiceExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await VehicleTripInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as VehicleTripModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.VehicleRepair:
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<VehicleRepairModel>(FleetNames.VehicleRepair, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.VehicleRepair}/{(decodeTransactionNoModel.TransactionModel as VehicleRepairModel).Id}";
				decodeTransactionNoModel.PDFStream = await VehicleRepairInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as VehicleRepairModel).Id, InvoiceExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await VehicleRepairInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as VehicleRepairModel).Id, InvoiceExportType.Excel);
				break;

			case CodeType.VehicleRouteLocation:
				var routeLocations = await CommonData.LoadTableData<VehicleRouteLocationModel>(FleetNames.VehicleRouteLocation);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<VehicleRouteLocationModel>(FleetNames.VehicleRouteLocation, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.VehicleRouteLocationMaster}/{(decodeTransactionNoModel.TransactionModel as VehicleRouteLocationModel).Id}";
				decodeTransactionNoModel.PDFStream = await VehicleRouteLocationExport.ExportMaster(routeLocations, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await VehicleRouteLocationExport.ExportMaster(routeLocations, ReportExportType.Excel);
				break;
			case CodeType.VehicleRoute:
				var vehicleRoutes = await CommonData.LoadTableData<VehicleRouteModel>(FleetNames.VehicleRoute);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<VehicleRouteModel>(FleetNames.VehicleRoute, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.VehicleRouteMaster}/{(decodeTransactionNoModel.TransactionModel as VehicleRouteModel).Id}";
				decodeTransactionNoModel.PDFStream = await VehicleRouteExport.ExportMaster(vehicleRoutes, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await VehicleRouteExport.ExportMaster(vehicleRoutes, ReportExportType.Excel);
				break;
			case CodeType.VehicleDriver:
				var vehicleDrivers = await CommonData.LoadTableData<VehicleDriverModel>(FleetNames.VehicleDriver);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<VehicleDriverModel>(FleetNames.VehicleDriver, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.VehicleDriverMaster}/{(decodeTransactionNoModel.TransactionModel as VehicleDriverModel).Id}";
				decodeTransactionNoModel.PDFStream = await VehicleDriverExport.ExportMaster(vehicleDrivers, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await VehicleDriverExport.ExportMaster(vehicleDrivers, ReportExportType.Excel);
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
			case CodeType.VehicleExpenseType:
				var vehicleExpenseTypes = await CommonData.LoadTableData<VehicleExpenseTypeModel>(FleetNames.VehicleExpenseType);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<VehicleExpenseTypeModel>(FleetNames.VehicleExpenseType, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.VehicleExpenseTypeMaster}/{(decodeTransactionNoModel.TransactionModel as VehicleExpenseTypeModel).Id}";
				decodeTransactionNoModel.PDFStream = await VehicleExpenseTypeExport.ExportMaster(vehicleExpenseTypes, ReportExportType.PDF);
				decodeTransactionNoModel.ExcelStream = await VehicleExpenseTypeExport.ExportMaster(vehicleExpenseTypes, ReportExportType.Excel);
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

				case CodeType.VehicleTrip:
					var vehicleTrip = await CommonData.LoadTableDataByTransactionNo<VehicleTripModel>(FleetNames.VehicleTrip, code, sqlDataAccessTransaction);
					isDuplicate = vehicleTrip is not null;
					break;
				case CodeType.VehicleRepair:
					var vehicleRepair = await CommonData.LoadTableDataByTransactionNo<VehicleRepairModel>(FleetNames.VehicleRepair, code, sqlDataAccessTransaction);
					isDuplicate = vehicleRepair is not null;
					break;
				
				case CodeType.VehicleRouteLocation:
					var routeLocation = await CommonData.LoadTableDataByCode<VehicleRouteLocationModel>(FleetNames.VehicleRouteLocation, code, sqlDataAccessTransaction);
					isDuplicate = routeLocation is not null;
					break;
				case CodeType.VehicleRoute:
					var vehicleRoute = await CommonData.LoadTableDataByCode<VehicleRouteModel>(FleetNames.VehicleRoute, code, sqlDataAccessTransaction);
					isDuplicate = vehicleRoute is not null;
					break;
				case CodeType.VehicleDriver:
					var vehicleDriver = await CommonData.LoadTableDataByCode<VehicleDriverModel>(FleetNames.VehicleDriver, code, sqlDataAccessTransaction);
					isDuplicate = vehicleDriver is not null;
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
				case CodeType.VehicleExpenseType:
					var vehicleExpenseType = await CommonData.LoadTableDataByCode<VehicleExpenseTypeModel>(FleetNames.VehicleExpenseType, code, sqlDataAccessTransaction);
					isDuplicate = vehicleExpenseType is not null;
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
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{accountingPrefix}{nextNumber:D6}", 6, CodeType.FinancialAccounting, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{accountingPrefix}000001", 6, CodeType.FinancialAccounting, sqlDataAccessTransaction);
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

	#region Trip Repair
	public static async Task<string> GenerateVehicleTripTransactionNo(VehicleTripModel vehicleTrip, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, vehicleTrip.FinancialYearId, sqlDataAccessTransaction);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, vehicleTrip.CompanyId, sqlDataAccessTransaction)).Code;
		var tripPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleTripTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastVehicleTrip = await CommonData.LoadLastTableDataByFinancialYear<VehicleTripModel>(FleetNames.VehicleTrip, vehicleTrip.FinancialYearId, sqlDataAccessTransaction);
		if (lastVehicleTrip is not null)
		{
			var lastTransactionNo = lastVehicleTrip.TransactionNo;
			if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{tripPrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + tripPrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{tripPrefix}{nextNumber:D6}", 6, CodeType.VehicleTrip, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{tripPrefix}000001", 6, CodeType.VehicleTrip, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateVehicleRepairTransactionNo(VehicleRepairModel vehicleRepair, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(AccountNames.FinancialYear, vehicleRepair.FinancialYearId, sqlDataAccessTransaction);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, vehicleRepair.CompanyId, sqlDataAccessTransaction)).Code;
		var repairPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleRepairTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastVehicleRepair = await CommonData.LoadLastTableDataByFinancialYear<VehicleRepairModel>(FleetNames.VehicleRepair, vehicleRepair.FinancialYearId, sqlDataAccessTransaction);
		if (lastVehicleRepair is not null)
		{
			var lastTransactionNo = lastVehicleRepair.TransactionNo;
			if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{repairPrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + repairPrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{repairPrefix}{nextNumber:D6}", 6, CodeType.VehicleRepair, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{repairPrefix}000001", 6, CodeType.VehicleRepair, sqlDataAccessTransaction);
	}
	#endregion

	#region Vehicle Route
	public static async Task<string> GenerateVehicleRouteLocationCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var routeLocations = await CommonData.LoadTableData<VehicleRouteLocationModel>(FleetNames.VehicleRouteLocation, sqlDataAccessTransaction);
		var routeLocationPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleRouteLocationCodePrefix, sqlDataAccessTransaction)).Value;

		var lastRouteLocation = routeLocations.OrderByDescending(rl => rl.Id).FirstOrDefault();
		if (lastRouteLocation is not null)
		{
			var lastRouteLocationCode = lastRouteLocation.Code;
			if (lastRouteLocationCode.StartsWith(routeLocationPrefix))
			{
				var lastNumberPart = lastRouteLocationCode[routeLocationPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{routeLocationPrefix}{nextNumber:D5}", 5, CodeType.VehicleRouteLocation, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{routeLocationPrefix}00001", 5, CodeType.VehicleRouteLocation, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateVehicleRouteCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var vehicleRoutes = await CommonData.LoadTableData<VehicleRouteModel>(FleetNames.VehicleRoute, sqlDataAccessTransaction);
		var vehicleRoutePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleRouteCodePrefix, sqlDataAccessTransaction)).Value;

		var lastVehicleRoute = vehicleRoutes.OrderByDescending(vr => vr.Id).FirstOrDefault();
		if (lastVehicleRoute is not null)
		{
			var lastVehicleRouteCode = lastVehicleRoute.Code;
			if (lastVehicleRouteCode.StartsWith(vehicleRoutePrefix))
			{
				var lastNumberPart = lastVehicleRouteCode[vehicleRoutePrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{vehicleRoutePrefix}{nextNumber:D5}", 5, CodeType.VehicleRoute, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{vehicleRoutePrefix}00001", 5, CodeType.VehicleRoute, sqlDataAccessTransaction);
	}

	public static async Task<string> GenerateVehicleDriverCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var vehicleDrivers = await CommonData.LoadTableData<VehicleDriverModel>(FleetNames.VehicleDriver, sqlDataAccessTransaction);
		var vehicleDriverPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleDriverCodePrefix, sqlDataAccessTransaction)).Value;

		var lastVehicleDriver = vehicleDrivers.OrderByDescending(vd => vd.Id).FirstOrDefault();
		if (lastVehicleDriver is not null)
		{
			var lastVehicleDriverCode = lastVehicleDriver.Code;
			if (lastVehicleDriverCode.StartsWith(vehicleDriverPrefix))
			{
				var lastNumberPart = lastVehicleDriverCode[vehicleDriverPrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{vehicleDriverPrefix}{nextNumber:D5}", 5, CodeType.VehicleDriver, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{vehicleDriverPrefix}00001", 5, CodeType.VehicleDriver, sqlDataAccessTransaction);
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

	public static async Task<string> GenerateVehicleExpenseTypeCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var vehicleExpenseTypes = await CommonData.LoadTableData<VehicleExpenseTypeModel>(FleetNames.VehicleExpenseType, sqlDataAccessTransaction);
		var vehicleExpenseTypePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleExpenseTypeCodePrefix, sqlDataAccessTransaction)).Value;

		var lastVehicleExpenseType = vehicleExpenseTypes.OrderByDescending(v => v.Id).FirstOrDefault();
		if (lastVehicleExpenseType is not null)
		{
			var lastVehicleExpenseTypeCode = lastVehicleExpenseType.Code;
			if (lastVehicleExpenseTypeCode.StartsWith(vehicleExpenseTypePrefix))
			{
				var lastNumberPart = lastVehicleExpenseTypeCode[vehicleExpenseTypePrefix.Length..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{vehicleExpenseTypePrefix}{nextNumber:D5}", 5, CodeType.VehicleExpenseType, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{vehicleExpenseTypePrefix}00001", 5, CodeType.VehicleExpenseType, sqlDataAccessTransaction);
	}
	#endregion
}
