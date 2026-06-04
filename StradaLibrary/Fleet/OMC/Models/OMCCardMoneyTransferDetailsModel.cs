namespace StradaLibrary.Fleet.OMC.Models;

public class OMCCardMoneyTransferDetailsModel
{
	public int Id { get; set; }
	public int MasterId { get; set; }
	public int OMCCardId { get; set; }
	public decimal Amount { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}

public class OMCCardMoneyTransferDetailsCartModel
{
	public int OMCCardId { get; set; }
	public string OMCCardNumber { get; set; }
	public decimal Amount { get; set; }
	public string? Remarks { get; set; }
}

public class OMCCardMoneyTransferDetailsOverviewModel
{
	public int Id { get; set; }
	public int OMCCardId { get; set; }
	public string OMCCardNumber { get; set; }
	public string OMCCardCode { get; set; }

	public int OMCId { get; set; }
	public string OMCName { get; set; }

	public decimal TransferAmount { get; set; }
	public string? TransferRemarks { get; set; }

	public int MasterId { get; set; }
	public string TransactionNo { get; set; }
	public int CompanyId { get; set; }
	public string CompanyName { get; set; }

	public DateTime TransactionDateTime { get; set; }
	public int FinancialYearId { get; set; }
	public string FinancialYear { get; set; }

	public int LedgerId { get; set; }
	public string LedgerName { get; set; }
	public decimal TotalItems { get; set; }
	public decimal TotalAmount { get; set; }

	public string? Remarks { get; set; }
	public int? FinancialAccountingId { get; set; }
	public string? FinancialAccountingTransactionNo { get; set; }
	public int CreatedBy { get; set; }
	public string CreatedByName { get; set; }
	public DateTime CreatedAt { get; set; }
	public string CreatedFromPlatform { get; set; }
	public int? LastModifiedBy { get; set; }
	public string? LastModifiedByUserName { get; set; }
	public DateTime? LastModifiedAt { get; set; }
	public string? LastModifiedFromPlatform { get; set; }
	public bool MasterStatus { get; set; }
}
