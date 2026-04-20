using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.Vehicle;

namespace StradaLibrary.Data.Fleet.Vehicle;

public static class VehicleTypeData
{
	public static async Task<int> InsertVehicleType(VehicleTypeModel vehicleType) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleType, vehicleType)).FirstOrDefault();
}