using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Data.Fleet.VehicleRoute;

public static class VehicleDriverData
{
	public static async Task<int> InsertVehicleDriver(VehicleDriverModel vehicleDriver) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleDriver, vehicleDriver)).FirstOrDefault();
}