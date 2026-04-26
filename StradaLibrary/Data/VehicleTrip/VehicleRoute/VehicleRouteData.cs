using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.VehicleTrip.VehicleRoute;

namespace StradaLibrary.Data.VehicleTrip.VehicleRoute;

public static class VehicleRouteData
{
	public static async Task<int> InsertVehicleRoute(VehicleRouteModel vehicleRoute) =>
		(await SqlDataAccess.LoadData<int, dynamic>(VehicleTripNames.InsertVehicleRoute, vehicleRoute)).FirstOrDefault();

	public static async Task<List<VehicleRouteOverviewModel>> LoadVehicleRouteOverview()
	{
		var routes = await CommonData.LoadTableData<VehicleRouteModel>(VehicleTripNames.VehicleRoute);
		routes = [.. routes.Where(r => r.Status)];
		var locations = await CommonData.LoadTableData<VehicleRouteLocationModel>(VehicleTripNames.VehicleRouteLocation);
		List<VehicleRouteOverviewModel> routeLocations = [];

		foreach (var route in routes)
			routeLocations.Add(new ()
			{
				Id = route.Id,
				FromLocationId = route.FromLocationId,
				FromLocationName = locations.FirstOrDefault(l => l.Id == route.FromLocationId)?.Name ?? string.Empty,
				ToLocationId = route.ToLocationId,
				ToLocationName = locations.FirstOrDefault(l => l.Id == route.ToLocationId)?.Name ?? string.Empty,
				RouteDisplay = $"{locations.FirstOrDefault(l => l.Id == route.FromLocationId)?.Name ?? route.FromLocationId.ToString()} - {locations.FirstOrDefault(l => l.Id == route.ToLocationId)?.Name ?? route.ToLocationId.ToString()}",
				Code = route.Code,
				EstimatedHours = route.EstimatedHours,
				EstimatedDistance = route.EstimatedDistance,
				EstimatedFuelConsumption = route.EstimatedFuelConsumption,
				EstimatedCost = route.EstimatedCost,
				Remarks = route.Remarks,
				Status = route.Status
			});

		return routeLocations;
	}

	private static async Task ValidateTransaction(VehicleRouteModel vehicleRoute)
	{
		vehicleRoute.Remarks = vehicleRoute.Remarks?.Trim() ?? string.Empty;
		vehicleRoute.Status = true;

		if (vehicleRoute.FromLocationId <= 0)
			throw new Exception("From location is required. Please select a valid from location.");

		if (vehicleRoute.ToLocationId <= 0)
			throw new Exception("To location is required. Please select a valid to location.");

		if (vehicleRoute.FromLocationId == vehicleRoute.ToLocationId)
			throw new Exception("From location and to location cannot be the same.");

		if (vehicleRoute.EstimatedHours < 0)
			throw new Exception("Estimated hours must be greater than zero.");

		if (vehicleRoute.EstimatedDistance < 0)
			throw new Exception("Estimated distance must be greater than zero.");

		if (vehicleRoute.EstimatedFuelConsumption < 0)
			throw new Exception("Estimated fuel consumption must be greater than zero.");

		if (vehicleRoute.EstimatedCost < 0)
			throw new Exception("Estimated cost must be greater than zero.");

		if (vehicleRoute.Id == 0)
			vehicleRoute.Code = await GenerateCodes.GenerateVehicleRouteCode();

		if (string.IsNullOrWhiteSpace(vehicleRoute.Code))
			throw new Exception("Route code is required. Please enter a valid route code.");

		if (string.IsNullOrWhiteSpace(vehicleRoute.Remarks))
			vehicleRoute.Remarks = null;

		var allRoutes = await CommonData.LoadTableData<VehicleRouteModel>(VehicleTripNames.VehicleRoute);
		var routeLocations = await CommonData.LoadTableData<VehicleRouteLocationModel>(VehicleTripNames.VehicleRouteLocation);

		var existingByLocationPair = allRoutes.FirstOrDefault(r =>
			r.Id != vehicleRoute.Id &&
			r.FromLocationId == vehicleRoute.FromLocationId &&
			r.ToLocationId == vehicleRoute.ToLocationId);

		if (existingByLocationPair is not null)
		{
			var fromLocationName = routeLocations.FirstOrDefault(rl => rl.Id == vehicleRoute.FromLocationId)?.Name ?? vehicleRoute.FromLocationId.ToString();
			var toLocationName = routeLocations.FirstOrDefault(rl => rl.Id == vehicleRoute.ToLocationId)?.Name ?? vehicleRoute.ToLocationId.ToString();
			throw new Exception($"Vehicle route '{fromLocationName} -> {toLocationName}' already exists. Duplicate route entries are not allowed.");
		}

		var existingByCode = allRoutes.FirstOrDefault(r =>
			r.Id != vehicleRoute.Id &&
			r.Code.Equals(vehicleRoute.Code, StringComparison.OrdinalIgnoreCase));

		if (existingByCode is not null)
			throw new Exception($"Vehicle route code '{vehicleRoute.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(VehicleRouteModel vehicleRoute)
	{
		await ValidateTransaction(vehicleRoute);
		return await InsertVehicleRoute(vehicleRoute);
	}
}
