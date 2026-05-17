namespace StradaLibrary.Fleet.Trip.Models;

public class TripExpensesModel
{
	public int Id { get; set; }
	public int MasterId { get; set; }
	public int ExpenseTypeId { get; set; }
	public decimal Amount { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}

public class TripExpensesCartModel
{
	public int ExpenseTypeId { get; set; }
	public string ExpenseTypeName { get; set; }
	public decimal Amount { get; set; }
	public string? Remarks { get; set; }
}

public class TripExpensesOverviewModel
{
	public int Id { get; set; }
	public int ExpenseTypeId { get; set; }
	public string ExpenseTypeName { get; set; }
	public string ExpenseTypeCode { get; set; }
	public decimal ExpenseAmount { get; set; }
	public string? ExpenseRemarks { get; set; }

	public int MasterId { get; set; }
	public string TransactionNo { get; set; }
	public int CompanyId { get; set; }
	public string CompanyName { get; set; }
	public DateTime TransactionDateTime { get; set; }
	public int FinancialYearId { get; set; }
	public string FinancialYear { get; set; }

	public string? SlNo { get; set; }
	public string? ChallanNo { get; set; }
	public int OMCId { get; set; }
	public string OMCName { get; set; }
	public int VehicleId { get; set; }
	public string VehicleCode { get; set; }
	public int RouteId { get; set; }
	public string FromLocation { get; set; }
	public string ToLocation { get; set; }
	public string RouteDisplay { get; set; }
	public int DriverId { get; set; }
	public string DriverName { get; set; }
	public string DriverMobile { get; set; }
	public string DriverDisplay { get; set; }

	public decimal Quantity { get; set; }
	public decimal EstimatedDistance { get; set; }
	public decimal EstimatedHours { get; set; }
	public decimal EstimatedFuelConsumption { get; set; }
	public decimal EstimatedCost { get; set; }
	public decimal TotalExpense { get; set; }

	public bool VehicleEmpty { get; set; }

	public int? BillId { get; set; }
	public string? BillNo { get; set; }
	public DateTime? BillDateTime { get; set; }
	public decimal? GrossAmount { get; set; }
	public decimal? PenaltyAmount { get; set; }
	public decimal? NetAmount { get; set; }

	public decimal? ProfitLoss { get; set; }
	public int? PendingDays { get; set; }

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