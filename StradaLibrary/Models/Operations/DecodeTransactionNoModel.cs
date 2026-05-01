namespace StradaLibrary.Models.Operations;

public enum CodeType
{
	FinancialAccounting,
	Ledger,

	Trip,
	Bill,
	Expense,

	Location,
	Route,
	Driver,

	OMC,
	OMCCard,

	VehicleType,
	VehicleDocumentType,
	ExpenseType,
}

public class DecodeTransactionNoModel
{
	public object TransactionModel { get; set; }
	public CodeType CodeType { get; set; }
	public string PageRouteName { get; set; }
	public (MemoryStream stream, string fileName) PDFStream { get; set; }
	public (MemoryStream stream, string fileName) ExcelStream { get; set; }
}