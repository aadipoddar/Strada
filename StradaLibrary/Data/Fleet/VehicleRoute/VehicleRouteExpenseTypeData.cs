using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Data.Fleet.VehicleRoute;

public static class VehicleRouteExpenseTypeData
{
	public static async Task<int> InsertVehicleRouteExpenseType(VehicleRouteExpenseTypeModel vehicleRouteExpenseType) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleRouteExpenseType, vehicleRouteExpenseType)).FirstOrDefault();
}