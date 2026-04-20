using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Data.Fleet.VehicleRoute;

public static class OMCData
{
	public static async Task<int> InsertOMC(OMCModel omc) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertOMC, omc)).FirstOrDefault();
}