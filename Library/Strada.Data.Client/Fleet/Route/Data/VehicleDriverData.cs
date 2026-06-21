using Strada.Models.Common;
using Strada.Models.Fleet.Route;

namespace Strada.Data.Fleet.Route.Data;

public static class VehicleDriverData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleDriverData));

	public static Task<List<VehicleDriverOverviewModel>> LoadVehicleDriverOverview() =>
		Api.Get<List<VehicleDriverOverviewModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadVehicleDriverOverview)));

	public static Task DeleteTransaction(VehicleDriverModel vehicleDriver, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), vehicleDriver, new { userId, platform });

	public static Task<int> SaveTransaction(VehicleDriverModel vehicleDriver, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), vehicleDriver, new { userId, platform });
}
