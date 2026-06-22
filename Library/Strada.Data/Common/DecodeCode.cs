using Strada.Data.Accounts.FinancialAccounting.Exports;
using Strada.Data.Accounts.Masters.Exports;
using Strada.Data.Fleet.Bill.Exports;
using Strada.Data.Fleet.Expense.Exports;
using Strada.Data.Fleet.OMC.Exports;
using Strada.Data.Fleet.Route.Exports;
using Strada.Data.Fleet.Trip.Exports;
using Strada.Data.Fleet.Tyre.Exports;
using Strada.Data.Fleet.Vehicle.Exports;
using Strada.Data.Fleet.VehicleDocument.Exports;
using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Bill;
using Strada.Models.Fleet.Expense;
using Strada.Models.Fleet.OMC;
using Strada.Models.Fleet.Route;
using Strada.Models.Fleet.Trip;
using Strada.Models.Fleet.Tyre;
using Strada.Models.Fleet.Vehicle;
using Strada.Models.Fleet.VehicleDocument;
using Strada.Models.Operations;

namespace Strada.Data.Common;

public static class DecodeCode
{
	public static async Task<DecodeTransactionNoModel> DecodeTransactionNo(string transactionNo, bool pdf = true, bool excel = true, CodeType? codeType = null)
	{
		if (string.IsNullOrWhiteSpace(transactionNo))
			return null;

		DecodeTransactionNoModel decodeTransactionNoModel = new();

		if (codeType is null) decodeTransactionNoModel = await DecodeTransactionType(transactionNo);
		else decodeTransactionNoModel.CodeType = codeType.Value;

		object transactionModel;

		switch (decodeTransactionNoModel.CodeType)
		{
			#region Accounts
			case CodeType.FinancialAccounting:
				transactionModel = await CommonData.LoadTableDataByTransactionNo<FinancialAccountingModel>(AccountNames.FinancialAccounting, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.FinancialAccounting}/{(transactionModel as FinancialAccountingModel).Id}";
				if (pdf) decodeTransactionNoModel.PDFStream = await FinancialAccountingInvoiceExport.ExportInvoice((transactionModel as FinancialAccountingModel).Id, InvoiceExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await FinancialAccountingInvoiceExport.ExportInvoice((transactionModel as FinancialAccountingModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.Ledger:
				var ledgers = await CommonData.LoadTableData<LedgerModel>(AccountNames.Ledger);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.LedgerMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await LedgerExport.ExportMaster(ledgers, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await LedgerExport.ExportMaster(ledgers, ReportExportType.Excel);
				break;
			#endregion

			#region Fleet Transactions
			case CodeType.Trip:
				transactionModel = await CommonData.LoadTableDataByTransactionNo<TripModel>(FleetNames.Trip, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.Trip}/{(transactionModel as TripModel).Id}";
				if (pdf) decodeTransactionNoModel.PDFStream = await TripInvoiceExport.ExportInvoice((transactionModel as TripModel).Id, InvoiceExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await TripInvoiceExport.ExportInvoice((transactionModel as TripModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.Bill:
				transactionModel = await CommonData.LoadTableDataByTransactionNo<BillModel>(FleetNames.Bill, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.Bill}/{(transactionModel as BillModel).Id}";
				if (pdf) decodeTransactionNoModel.PDFStream = await BillInvoiceExport.ExportInvoice((transactionModel as BillModel).Id, InvoiceExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await BillInvoiceExport.ExportInvoice((transactionModel as BillModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.Expense:
				transactionModel = await CommonData.LoadTableDataByTransactionNo<ExpenseModel>(FleetNames.Expense, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.Expense}/{(transactionModel as ExpenseModel).Id}";
				if (pdf) decodeTransactionNoModel.PDFStream = await ExpenseInvoiceExport.ExportInvoice((transactionModel as ExpenseModel).Id, InvoiceExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await ExpenseInvoiceExport.ExportInvoice((transactionModel as ExpenseModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.OMCCardMoneyTransfer:
				transactionModel = await CommonData.LoadTableDataByTransactionNo<OMCCardMoneyTransferModel>(FleetNames.OMCCardMoneyTransfer, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.OMCCardMoneyTransfer}/{(transactionModel as OMCCardMoneyTransferModel).Id}";
				if (pdf) decodeTransactionNoModel.PDFStream = await OMCCardMoneyTransferInvoiceExport.ExportInvoice((transactionModel as OMCCardMoneyTransferModel).Id, InvoiceExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await OMCCardMoneyTransferInvoiceExport.ExportInvoice((transactionModel as OMCCardMoneyTransferModel).Id, InvoiceExportType.Excel);
				break;
			#endregion

			#region Fleet Masters
			case CodeType.Location:
				var locations = await CommonData.LoadTableData<LocationModel>(FleetNames.Location);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.LocationMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await LocationExport.ExportMaster(locations, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await LocationExport.ExportMaster(locations, ReportExportType.Excel);
				break;
			case CodeType.Route:
				var routes = await CommonData.LoadTableData<RouteModel>(FleetNames.Route);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.RouteMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await RouteExport.ExportMaster(routes, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await RouteExport.ExportMaster(routes, ReportExportType.Excel);
				break;
			case CodeType.Driver:
				var drivers = await CommonData.LoadTableData<DriverModel>(FleetNames.Driver);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.DriverMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await DriverExport.ExportMaster(drivers, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await DriverExport.ExportMaster(drivers, ReportExportType.Excel);
				break;

			case CodeType.TyreCompany:
				var tyreCompanies = await CommonData.LoadTableData<TyreCompanyModel>(FleetNames.TyreCompany);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.TyreCompanyMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await TyreCompanyExport.ExportMaster(tyreCompanies, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await TyreCompanyExport.ExportMaster(tyreCompanies, ReportExportType.Excel);
				break;

			case CodeType.OMC:
				var omcs = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.OMCMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await OMCExport.ExportMaster(omcs, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await OMCExport.ExportMaster(omcs, ReportExportType.Excel);
				break;
			case CodeType.OMCCard:
				var omcCards = await CommonData.LoadTableData<OMCCardModel>(FleetNames.OMCCard);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.OMCCardMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await OMCCardExport.ExportMaster(omcCards, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await OMCCardExport.ExportMaster(omcCards, ReportExportType.Excel);
				break;

			case CodeType.VehicleType:
				var vehicleTypes = await CommonData.LoadTableData<VehicleTypeModel>(FleetNames.VehicleType);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.VehicleTypeMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await VehicleTypeExport.ExportMaster(vehicleTypes, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await VehicleTypeExport.ExportMaster(vehicleTypes, ReportExportType.Excel);
				break;
			case CodeType.VehicleDocumentType:
				var vehicleDocumentTypes = await CommonData.LoadTableData<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.VehicleDocumentTypeMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await VehicleDocumentTypeExport.ExportMaster(vehicleDocumentTypes, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await VehicleDocumentTypeExport.ExportMaster(vehicleDocumentTypes, ReportExportType.Excel);
				break;
			case CodeType.ExpenseType:
				var expenseTypes = await CommonData.LoadTableData<ExpenseTypeModel>(FleetNames.ExpenseType);
				decodeTransactionNoModel.PageRouteName = $"{PageRouteNames.ExpenseTypeMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await ExpenseTypeExport.ExportMaster(expenseTypes, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await ExpenseTypeExport.ExportMaster(expenseTypes, ReportExportType.Excel);
				break;
			#endregion

			default:
				break;
		}

		return decodeTransactionNoModel;
	}

	private static async Task<DecodeTransactionNoModel> DecodeTransactionType(string transactionNo)
	{
		DecodeTransactionNoModel decodeTransactionNoModel = new();

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

		return decodeTransactionNoModel;
	}
}
