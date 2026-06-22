namespace Strada.Models.APIService;

public class WheelsEyeVehicleModel
{
	public string VehicleNumber { get; set; }
	public string VehicleType { get; set; }

	public decimal Latitude { get; set; }
	public decimal Longitude { get; set; }
	public string Address { get; set; }
	public bool HasValidPosition => Latitude != 0 && Longitude != 0;

	public int Speed { get; set; }
	public bool IgnitionOn { get; set; }
	public decimal Angle { get; set; }

	public DateTime LastUpdate { get; set; }
	public bool IsStale { get; set; }

	public string Status =>
		IsStale ? "Offline" : Speed > 0 ? "Moving" : IgnitionOn ? "Idle" : "Stopped";
}
