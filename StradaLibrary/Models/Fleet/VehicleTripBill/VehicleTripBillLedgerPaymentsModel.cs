namespace StradaLibrary.Models.Fleet.VehicleTripBill;

public class VehicleTripBillLedgerPaymentsModel
{
	public int Id { get; set; }
	public int MasterId { get; set; }
	public int LedgerId { get; set; }
	public decimal Amount { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}

public class VehicleTripBillLedgerPaymentsCartModel
{
	public int LedgerId { get; set; }
	public string LedgerName { get; set; }
	public decimal Amount { get; set; }
	public string? Remarks { get; set; }
}

public class VehicleTripBillLedgerPaymentsOverviewModel
{
	public int Id { get; set; }
	public int LedgerId { get; set; }
	public string LedgerName { get; set; }
	public string LedgerCode { get; set; }
	public decimal PaymentAmount { get; set; }
	public string? PaymentRemarks { get; set; }

	public int MasterId { get; set; }
	public string TransactionNo { get; set; }
	public int CompanyId { get; set; }
	public string CompanyName { get; set; }
	public DateTime TransactionDateTime { get; set; }
	public int FinancialYearId { get; set; }
	public string FinancialYear { get; set; }

	public string BillNo { get; set; }
	public int OMCId { get; set; }
	public string OMCName { get; set; }

	public decimal TotalGrossAmount { get; set; }
	public decimal TotalTDSAmount { get; set; }
	public decimal TotalPenaltyAmount { get; set; }
	public decimal TotalNetAmount { get; set; }
	public decimal TotalCardPaymentAmount { get; set; }
	public decimal TotalLedgerPaymentAmount { get; set; }

	public string? Remarks { get; set; }
	public int CreatedBy { get; set; }
	public string CreatedByName { get; set; }
	public DateTime CreatedAt { get; set; }
	public string CreatedFromPlatform { get; set; }
	public int? LastModifiedBy { get; set; }
	public string? LastModifiedByUserName { get; set; }
	public DateTime? LastModifiedAt { get; set; }
	public string? LastModifiedFromPlatform { get; set; }
}