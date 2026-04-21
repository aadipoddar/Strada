using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Data.Fleet.VehicleRoute;

public static class OMCCardData
{
	public static async Task<int> InsertOMCCard(OMCCardModel omcCard) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertOMCCard, omcCard)).FirstOrDefault();
}