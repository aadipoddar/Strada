namespace StradaLibrary.Models.Fleet.VehicleTrip;

public class VehicleTripModel
{
	public int Id { get; set; }
	public string TransactionNo { get; set; }
	public int CompanyId { get; set; }
	public DateTime TransactionDateTime { get; set; }
	public int FinancialYearId { get; set; }
	public string ChallanNo { get; set; }
	public DateTime ChallanDateTime { get; set; }
	public int OMCId { get; set; }
	public int VehicleId { get; set; }
	public int DriverId { get; set; }
	public int RouteId { get; set; }
	public decimal Quantity { get; set; }
	public decimal TotalExpense { get; set; }
	public string? Remarks { get; set; }
	public int CreatedBy { get; set; }
	public DateTime CreatedAt { get; set; }
	public string CreatedFromPlatform { get; set; }
	public bool Status { get; set; }
	public int? LastModifiedBy { get; set; }
	public DateTime? LastModifiedAt { get; set; }
	public string? LastModifiedFromPlatform { get; set; }
}
