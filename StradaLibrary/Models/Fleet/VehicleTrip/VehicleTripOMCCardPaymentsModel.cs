namespace StradaLibrary.Models.Fleet.VehicleTrip;

public class VehicleTripOMCCardPaymentsModel
{
	public int Id { get; set; }
	public int MasterId { get; set; }
	public int OMCCardId { get; set; }
	public decimal Amount { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}

public class VehicleTripOMCCardPaymentsCartModel
{
	public int OMCCardId { get; set; }
	public string OMCCardNumber { get; set; }
	public decimal Amount { get; set; }
	public string? Remarks { get; set; }
}