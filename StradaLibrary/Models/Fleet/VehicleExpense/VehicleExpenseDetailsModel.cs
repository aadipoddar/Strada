namespace StradaLibrary.Models.Fleet.VehicleExpense;

public class VehicleExpenseDetailsModel
{
	public int Id { get; set; }
	public int MasterId { get; set; }
	public int VehicleExpenseTypeId { get; set; }
	public int? LedgerId { get; set; }
	public decimal Amount { get; set; }
	public string? IdentificationNo { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}

public class VehicleExpenseDetailsCartModel
{
	public int VehicleExpenseTypeId { get; set; }
	public string VehicleExpenseTypeName { get; set; }
	public int? LedgerId { get; set; }
	public string? LedgerName { get; set; }
	public decimal Amount { get; set; }
	public string? IdentificationNo { get; set; }
	public string? Remarks { get; set; }
}

public class VehicleExpenseDetailsOverviewModel
{
	public int Id { get; set; }
	public int VehicleExpenseTypeId { get; set; }
	public string ExpenseTypeName { get; set; }
	public string ExpenseTypeCode { get; set; }
	public int? LedgerId { get; set; }
	public string? LedgerName { get; set; }
	public decimal ExpenseAmount { get; set; }
	public string? IdentificationNo { get; set; }
	public string? ExpenseRemarks { get; set; }

	public int MasterId { get; set; }
	public string TransactionNo { get; set; }
	public int CompanyId { get; set; }
	public string CompanyName { get; set; }
	public DateTime TransactionDateTime { get; set; }
	public int FinancialYearId { get; set; }
	public string FinancialYear { get; set; }

	public int VehicleId { get; set; }
	public string VehicleCode { get; set; }
	public decimal TotalExpense { get; set; }

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
