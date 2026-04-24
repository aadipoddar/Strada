namespace StradaLibrary.Models.Operations;

public enum CodeType
{
	FinancialAccounting,
	Ledger,
	VehicleTrip,
	VehicleType,
	VehicleDocumentType,
	OMC,
	OMCCard,
	VehicleRouteLocation,
	VehicleRoute,
	VehicleDriver,
	VehicleExpenseType,
}

public class DecodeTransactionNoModel
{
	public object TransactionModel { get; set; }
	public CodeType CodeType { get; set; }
	public string PageRouteName { get; set; }
	public (MemoryStream stream, string fileName) PDFStream { get; set; }
	public (MemoryStream stream, string fileName) ExcelStream { get; set; }
}