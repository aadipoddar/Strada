using Strada.Library.Accounts.FinancialAccounting.Exports;
using Strada.Library.Accounts.FinancialAccounting.Models;
using Strada.Library.Accounts.Masters.Exports;
using Strada.Library.Accounts.Masters.Models;
using Strada.Library.Fleet.Bill.Exports;
using Strada.Library.Fleet.Bill.Models;
using Strada.Library.Fleet.Expense.Exports;
using Strada.Library.Fleet.Expense.Models;
using Strada.Library.Fleet.OMC.Exports;
using Strada.Library.Fleet.OMC.Models;
using Strada.Library.Fleet.Route.Exports;
using Strada.Library.Fleet.Route.Models;
using Strada.Library.Fleet.Trip.Exports;
using Strada.Library.Fleet.Trip.Models;
using Strada.Library.Fleet.Tyre.Exports;
using Strada.Library.Fleet.Tyre.Models;
using Strada.Library.Fleet.Vehicle.Exports;
using Strada.Library.Fleet.Vehicle.Models;
using Strada.Library.Fleet.VehicleDocument.Exports;
using Strada.Library.Fleet.VehicleDocument.Models;
using Strada.Library.Operations.Models;
using Strada.Library.Utils.ExportUtils;

namespace Strada.Library.Common;

public static class DecodeCode
{
	public static async Task<DecodeTransactionNoModel> DecodeTransactionNo(string transactionNo, bool pdf = true, bool excel = true, CodeType? codeType = null)
	{
		if (string.IsNullOrWhiteSpace(transactionNo))
			return null;

		DecodeTransactionNoModel decodeTransactionNoModel = new();

		if (codeType is null)
			decodeTransactionNoModel = await DecodeTransactionType(transactionNo);
		else
			decodeTransactionNoModel.CodeType = codeType.Value;

		switch (decodeTransactionNoModel.CodeType)
		{
			#region Accounts
			case CodeType.FinancialAccounting:
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<FinancialAccountingModel>(AccountNames.FinancialAccounting, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{AccountRouteNames.FinancialAccounting}/{(decodeTransactionNoModel.TransactionModel as FinancialAccountingModel).Id}";
				if (pdf) decodeTransactionNoModel.PDFStream = await FinancialAccountingInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as FinancialAccountingModel).Id, InvoiceExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await FinancialAccountingInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as FinancialAccountingModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.Ledger:
				var ledgers = await CommonData.LoadTableData<LedgerModel>(AccountNames.Ledger);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<LedgerModel>(AccountNames.Ledger, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{AccountRouteNames.LedgerMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await LedgerExport.ExportMaster(ledgers, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await LedgerExport.ExportMaster(ledgers, ReportExportType.Excel);
				break;
			#endregion

			#region Fleet Transactions
			case CodeType.Trip:
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<TripModel>(FleetNames.Trip, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.Trip}/{(decodeTransactionNoModel.TransactionModel as TripModel).Id}";
				if (pdf) decodeTransactionNoModel.PDFStream = await TripInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as TripModel).Id, InvoiceExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await TripInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as TripModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.Bill:
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<BillModel>(FleetNames.Bill, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.Bill}/{(decodeTransactionNoModel.TransactionModel as BillModel).Id}";
				if (pdf) decodeTransactionNoModel.PDFStream = await BillInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as BillModel).Id, InvoiceExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await BillInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as BillModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.Expense:
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<ExpenseModel>(FleetNames.Expense, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.Expense}/{(decodeTransactionNoModel.TransactionModel as ExpenseModel).Id}";
				if (pdf) decodeTransactionNoModel.PDFStream = await ExpenseInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as ExpenseModel).Id, InvoiceExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await ExpenseInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as ExpenseModel).Id, InvoiceExportType.Excel);
				break;
			case CodeType.OMCCardMoneyTransfer:
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByTransactionNo<OMCCardMoneyTransferModel>(FleetNames.OMCCardMoneyTransfer, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.OMCCardMoneyTransfer}/{(decodeTransactionNoModel.TransactionModel as OMCCardMoneyTransferModel).Id}";
				if (pdf) decodeTransactionNoModel.PDFStream = await OMCCardMoneyTransferInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as OMCCardMoneyTransferModel).Id, InvoiceExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await OMCCardMoneyTransferInvoiceExport.ExportInvoice((decodeTransactionNoModel.TransactionModel as OMCCardMoneyTransferModel).Id, InvoiceExportType.Excel);
				break;
			#endregion

			#region Fleet Masters
			case CodeType.Location:
				var locations = await CommonData.LoadTableData<LocationModel>(FleetNames.Location);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<LocationModel>(FleetNames.Location, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.LocationMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await LocationExport.ExportMaster(locations, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await LocationExport.ExportMaster(locations, ReportExportType.Excel);
				break;
			case CodeType.Route:
				var routes = await CommonData.LoadTableData<RouteModel>(FleetNames.Route);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<RouteModel>(FleetNames.Route, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.RouteMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await RouteExport.ExportMaster(routes, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await RouteExport.ExportMaster(routes, ReportExportType.Excel);
				break;
			case CodeType.Driver:
				var drivers = await CommonData.LoadTableData<DriverModel>(FleetNames.Driver);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<DriverModel>(FleetNames.Driver, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.DriverMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await DriverExport.ExportMaster(drivers, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await DriverExport.ExportMaster(drivers, ReportExportType.Excel);
				break;

			case CodeType.TyreCompany:
				var tyreCompanies = await CommonData.LoadTableData<TyreCompanyModel>(FleetNames.TyreCompany);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<TyreCompanyModel>(FleetNames.TyreCompany, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.TyreCompanyMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await TyreCompanyExport.ExportMaster(tyreCompanies, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await TyreCompanyExport.ExportMaster(tyreCompanies, ReportExportType.Excel);
				break;

			case CodeType.OMC:
				var omcs = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<OMCModel>(FleetNames.OMC, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.OMCMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await OMCExport.ExportMaster(omcs, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await OMCExport.ExportMaster(omcs, ReportExportType.Excel);
				break;
			case CodeType.OMCCard:
				var omcCards = await CommonData.LoadTableData<OMCCardModel>(FleetNames.OMCCard);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<OMCCardModel>(FleetNames.OMCCard, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.OMCCardMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await OMCCardExport.ExportMaster(omcCards, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await OMCCardExport.ExportMaster(omcCards, ReportExportType.Excel);
				break;

			case CodeType.VehicleType:
				var vehicleTypes = await CommonData.LoadTableData<VehicleTypeModel>(FleetNames.VehicleType);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<VehicleTypeModel>(FleetNames.VehicleType, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.VehicleTypeMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await VehicleTypeExport.ExportMaster(vehicleTypes, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await VehicleTypeExport.ExportMaster(vehicleTypes, ReportExportType.Excel);
				break;
			case CodeType.VehicleDocumentType:
				var vehicleDocumentTypes = await CommonData.LoadTableData<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.VehicleDocumentTypeMaster}";
				if (pdf) decodeTransactionNoModel.PDFStream = await VehicleDocumentTypeExport.ExportMaster(vehicleDocumentTypes, ReportExportType.PDF);
				if (excel) decodeTransactionNoModel.ExcelStream = await VehicleDocumentTypeExport.ExportMaster(vehicleDocumentTypes, ReportExportType.Excel);
				break;
			case CodeType.ExpenseType:
				var expenseTypes = await CommonData.LoadTableData<ExpenseTypeModel>(FleetNames.ExpenseType);
				decodeTransactionNoModel.TransactionModel = await CommonData.LoadTableDataByCode<ExpenseTypeModel>(FleetNames.ExpenseType, transactionNo);
				decodeTransactionNoModel.PageRouteName = $"{FleetRouteNames.ExpenseTypeMaster}";
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
