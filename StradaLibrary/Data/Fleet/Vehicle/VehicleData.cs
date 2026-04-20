using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.Vehicle;

namespace StradaLibrary.Data.Fleet.Vehicle;

public static class VehicleData
{
	public static async Task<int> InsertVehicle(VehicleModel vehicle) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicle, vehicle)).FirstOrDefault();
}