namespace StradaLibrary.Models.Fleet.VehicleRoute;

public class VehicleRouteModel
{
	public int Id { get; set; }
	public int FromLocationId { get; set; }
	public int ToLocationId { get; set; }
	public string Code { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}