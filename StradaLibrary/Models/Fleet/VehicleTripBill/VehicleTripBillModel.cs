namespace StradaLibrary.Models.Fleet.VehicleTripBill;

public class VehicleTripBillModel
{
	public int Id { get; set; }
	public string TransactionNo { get; set; }
	public int CompanyId { get; set; }
	public DateTime TransactionDateTime { get; set; }
	public int FinancialYearId { get; set; }
	public string BillNo { get; set; }
	public int OMCId { get; set; }
	
	public decimal TotalGrossAmount { get; set; }
	public decimal TotalTDSAmount { get; set; }
	public decimal TotalPenaltyAmount { get; set; }
	public decimal TotalNetAmount { get; set; }
	public decimal TotalCardPaymentAmount { get; set; }
	public decimal TotalLedgerPaymentAmount { get; set; }
	
	public string? Remarks { get; set; }
	public int CreatedBy { get; set; }
	public DateTime CreatedAt { get; set; }
	public string CreatedFromPlatform { get; set; }
	public bool Status { get; set; }
	public int? LastModifiedBy { get; set; }
	public DateTime? LastModifiedAt { get; set; }
	public string? LastModifiedFromPlatform { get; set; }
}

public class VehicleTripBillOverviewModel
{
	public int Id { get; set; }
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
	public bool Status { get; set; }
}