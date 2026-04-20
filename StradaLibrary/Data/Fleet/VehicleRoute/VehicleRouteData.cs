using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Data.Fleet.VehicleRoute;

public static class VehicleRouteData
{
	public static async Task<int> InsertVehicleRoute(VehicleRouteModel vehicleRoute) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleRoute, vehicleRoute)).FirstOrDefault();
}