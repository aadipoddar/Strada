using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleTrip;

namespace StradaLibrary.Data.Fleet.VehicleTrip;

public static class VehicleTripData
{
	private static async Task<int> InsertVehicleTrip(VehicleTripModel vehicleTrip) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleTrip, vehicleTrip)).FirstOrDefault();

	private static async Task<int> InsertVehicleTripExpenses(VehicleTripExpensesModel vehicleTripExpenses) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleTripExpenses, vehicleTripExpenses)).FirstOrDefault();

	private static async Task<int> InsertVehicleTripOMCCardPayments(VehicleTripOMCCardPaymentsModel vehicleTripOMCCardPayments) =>
			(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleTripOMCCardPayments, vehicleTripOMCCardPayments)).FirstOrDefault();
}