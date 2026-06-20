using Strada.Data.Common;
using Strada.Data.DataAccess;
using Strada.Data.Operations.Data;
using Strada.Models.Common;
using Strada.Models.Fleet.Route;
using Strada.Models.Operations;

namespace Strada.Data.Fleet.Route.Data;

public static class RouteData
{
	public static async Task<int> InsertRoute(RouteModel route, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertRoute, route, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Route.");

	public static async Task DeleteTransaction(RouteModel route, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			route.Status = false;
			await InsertRoute(route, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = FleetNames.Route,
				RecordNo = route.Code,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task RecoverTransaction(RouteModel route, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			route.Status = true;
			await InsertRoute(route, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = FleetNames.Route,
				RecordNo = route.Code,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task<List<RouteOverviewModel>> LoadRouteOverview()
	{
		var routes = await CommonData.LoadTableData<RouteModel>(FleetNames.Route);
		routes = [.. routes.Where(r => r.Status)];
		var locations = await CommonData.LoadTableData<LocationModel>(FleetNames.Location);
		List<RouteOverviewModel> routeLocations = [];

		foreach (var route in routes)
			routeLocations.Add(new()
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

	private static async Task ValidateTransaction(RouteModel route)
	{
		route.Remarks = route.Remarks?.Trim() ?? string.Empty;
		route.Status = true;

		if (route.FromLocationId <= 0)
			throw new Exception("From location is required. Please select a valid from location.");

		if (route.ToLocationId <= 0)
			throw new Exception("To location is required. Please select a valid to location.");

		if (route.FromLocationId == route.ToLocationId)
			throw new Exception("From location and to location cannot be the same.");

		if (route.EstimatedHours < 0)
			throw new Exception("Estimated hours must be greater than zero.");

		if (route.EstimatedDistance < 0)
			throw new Exception("Estimated distance must be greater than zero.");

		if (route.EstimatedFuelConsumption < 0)
			throw new Exception("Estimated fuel consumption must be greater than zero.");

		if (route.EstimatedCost < 0)
			throw new Exception("Estimated cost must be greater than zero.");

		if (route.Id == 0)
			route.Code = await GenerateCodes.GenerateRouteCode();

		if (string.IsNullOrWhiteSpace(route.Code))
			throw new Exception("Route code is required. Please enter a valid route code.");

		if (string.IsNullOrWhiteSpace(route.Remarks))
			route.Remarks = null;

		var allRoutes = await CommonData.LoadTableData<RouteModel>(FleetNames.Route);
		var locations = await CommonData.LoadTableData<LocationModel>(FleetNames.Location);

		var existingByLocationPair = allRoutes.FirstOrDefault(r =>
			r.Id != route.Id &&
			r.FromLocationId == route.FromLocationId &&
			r.ToLocationId == route.ToLocationId);

		if (existingByLocationPair is not null)
		{
			var fromLocationName = locations.FirstOrDefault(rl => rl.Id == route.FromLocationId)?.Name ?? route.FromLocationId.ToString();
			var toLocationName = locations.FirstOrDefault(rl => rl.Id == route.ToLocationId)?.Name ?? route.ToLocationId.ToString();
			throw new Exception($"Route '{fromLocationName} -> {toLocationName}' already exists. Duplicate route entries are not allowed.");
		}

		var existingByCode = allRoutes.FirstOrDefault(r =>
			r.Id != route.Id &&
			r.Code.Equals(route.Code, StringComparison.OrdinalIgnoreCase));

		if (existingByCode is not null)
			throw new Exception($"Route code '{route.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(RouteModel route, int userId, string platform)
	{
		await ValidateTransaction(route);

		var isUpdate = route.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<RouteModel>(FleetNames.Route, route.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertRoute(route, transaction);
			var diff = AuditTrailData.GetDifference(previous, route);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = FleetNames.Route,
				RecordNo = route.Code,
				RecordValue = isUpdate ? diff : null,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
