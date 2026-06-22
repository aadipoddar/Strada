namespace Strada.Models.Operations;

public enum CodeType
{
	FinancialAccounting,
	Ledger,

	Trip,
	Bill,
	Expense,
	OMCCardMoneyTransfer,

	Location,
	Route,
	Driver,

	TyreCompany,

	OMC,
	OMCCard,

	VehicleType,
	VehicleDocumentType,
	ExpenseType,
}

public class DecodeTransactionNoModel
{
	public CodeType CodeType { get; set; }
	public string PageRouteName { get; set; }
	public (MemoryStream stream, string fileName) PDFStream { get; set; }
	public (MemoryStream stream, string fileName) ExcelStream { get; set; }
}

public class DecodeTransactionNoResult
{
	public CodeType CodeType { get; set; }
	public string PageRouteName { get; set; }
	public FileResult Pdf { get; set; }
	public FileResult Excel { get; set; }
}

public class FileResult
{
	public byte[] Bytes { get; set; }
	public string FileName { get; set; }
}