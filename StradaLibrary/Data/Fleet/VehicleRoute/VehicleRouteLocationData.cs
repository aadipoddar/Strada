using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Data.Fleet.VehicleRoute;

public static class VehicleRouteLocationData
{
	public static async Task<int> InsertRouteLocation(VehicleRouteLocationModel routeLocation) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleRouteLocation, routeLocation)).FirstOrDefault();

	private static async Task ValidateTransaction(VehicleRouteLocationModel routeLocation)
	{
		routeLocation.Name = routeLocation.Name?.Trim().ToUpper() ?? string.Empty;
		routeLocation.Code = routeLocation.Code?.Trim().ToUpper() ?? string.Empty;
		routeLocation.Remarks = routeLocation.Remarks?.Trim() ?? string.Empty;
		routeLocation.Status = true;

		if (string.IsNullOrWhiteSpace(routeLocation.Name))
			throw new Exception("Route Location name is required. Please enter a valid route location name.");

		if (routeLocation.Id == 0)
			routeLocation.Code = await GenerateCodes.GenerateRouteLocationCode();

		if (string.IsNullOrWhiteSpace(routeLocation.Code))
			throw new Exception("Route Location code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(routeLocation.Remarks))
			routeLocation.Remarks = null;

		var allLocations = await CommonData.LoadTableData<VehicleRouteLocationModel>(FleetNames.VehicleRouteLocation);

		var existingByName = allLocations.FirstOrDefault(x => x.Id != routeLocation.Id && x.Name.Equals(routeLocation.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Route Location name '{routeLocation.Name}' already exists. Please choose a different name.");

		var existingByCode = allLocations.FirstOrDefault(x => x.Id != routeLocation.Id && x.Code.Equals(routeLocation.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Route Location code '{routeLocation.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(VehicleRouteLocationModel routeLocation)
	{
		await ValidateTransaction(routeLocation);
		return await InsertRouteLocation(routeLocation);
	}
}
