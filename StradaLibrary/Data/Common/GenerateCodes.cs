using StradaLibrary.Data.Operations;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Accounts.FinancialAccounting;
using StradaLibrary.Exports.Accounts.Masters;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.FinancialAccounting;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleDocument;
using StradaLibrary.Models.Fleet.VehicleRoute;
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
				case CodeType.VehicleType:
					var vehicleType = await CommonData.LoadTableDataByCode<VehicleTypeModel>(FleetNames.VehicleType, code, sqlDataAccessTransaction);
					isDuplicate = vehicleType is not null;
					break;
				case CodeType.VehicleDocumentType:
					var vehicleDocumentType = await CommonData.LoadTableDataByCode<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType, code, sqlDataAccessTransaction);
					isDuplicate = vehicleDocumentType is not null;
					break;
				case CodeType.RouteLocation:
					var routeLocation = await CommonData.LoadTableDataByCode<VehicleRouteLocationModel>(FleetNames.RouteLocation, code, sqlDataAccessTransaction);
					isDuplicate = routeLocation is not null;
					break;
				case CodeType.OMC:
					var omc = await CommonData.LoadTableDataByCode<OMCModel>(FleetNames.OMC, code, sqlDataAccessTransaction);
					isDuplicate = omc is not null;
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
		var locationPrefix = (await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, 1, sqlDataAccessTransaction)).Code;
		var accountingPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.FinancialAccountingTransactionPrefix, sqlDataAccessTransaction)).Value;

		var lastAccounting = await CommonData.LoadLastTableDataByFinancialYear<FinancialAccountingModel>(AccountNames.FinancialAccounting, accounting.FinancialYearId, sqlDataAccessTransaction);
		if (lastAccounting is not null)
		{
			var lastTransactionNo = lastAccounting.TransactionNo;
			if (lastTransactionNo.StartsWith($"{locationPrefix}{financialYear.YearNo}{accountingPrefix}"))
			{
				var lastNumberPart = lastTransactionNo[(locationPrefix.Length + financialYear.YearNo.ToString().Length + accountingPrefix.Length)..];
				if (int.TryParse(lastNumberPart, out int lastNumber))
				{
					int nextNumber = lastNumber + 1;
					return await CheckDuplicateCode($"{locationPrefix}{financialYear.YearNo}{accountingPrefix}{nextNumber:D6}", 6, CodeType.FinancialAccounting, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{locationPrefix}{financialYear.YearNo}{accountingPrefix}000001", 6, CodeType.FinancialAccounting, sqlDataAccessTransaction);
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

	#region Fleet
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

	public static async Task<string> GenerateRouteLocationCode(SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var routeLocations = await CommonData.LoadTableData<VehicleRouteLocationModel>(FleetNames.RouteLocation, sqlDataAccessTransaction);
		var routeLocationPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.RouteLocationCodePrefix, sqlDataAccessTransaction)).Value;

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
					return await CheckDuplicateCode($"{routeLocationPrefix}{nextNumber:D5}", 5, CodeType.RouteLocation, sqlDataAccessTransaction);
				}
			}
		}

		return await CheckDuplicateCode($"{routeLocationPrefix}00001", 5, CodeType.RouteLocation, sqlDataAccessTransaction);
	}

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
	#endregion
}
