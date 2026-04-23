namespace StradaLibrary.Models.Fleet.VehicleRoute;

public class VehicleDriverModel
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Mobile { get; set; }
	public string Code { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}

public class VehicleDriverOverviewModel
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Mobile { get; set; }
	public string DisplayName => $"{Name} ({Mobile})";
	public string Code { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}