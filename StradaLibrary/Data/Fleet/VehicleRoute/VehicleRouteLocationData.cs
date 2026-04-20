using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Data.Fleet.VehicleRoute;

public static class VehicleRouteLocationData
{
	public static async Task<int> InsertRouteLocation(VehicleRouteLocationModel routeLocation) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleRouteLocation, routeLocation)).FirstOrDefault();
}
