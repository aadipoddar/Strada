namespace StradaLibrary.Models.Fleet.VehicleTrip;

public class VehicleTripExpensesModel
{
	public int Id { get; set; }
	public int MasterId { get; set; }
	public int VehicleRouteExpenseTypeId { get; set; }
	public decimal Amount { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}

public class VehicleTripExpensesCartModel
{
	public int VehicleRouteExpenseTypeId { get; set; }
	public string VehicleRouteExpenseTypeName { get; set; }
	public decimal Amount { get; set; }
	public string? Remarks { get; set; }
}