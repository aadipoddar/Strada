using Strada.Models.Common;
using Strada.Models.Fleet.Route;

namespace Strada.Data.Fleet.Route.Data;

public static class DriverData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(DriverData));

	public static Task<List<DriverOverviewModel>> LoadDriverOverview() =>
		Api.Get<List<DriverOverviewModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadDriverOverview)));

	public static Task DeleteTransaction(DriverModel driver, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), driver, new { userId, platform });

	public static Task RecoverTransaction(DriverModel driver, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), driver, new { userId, platform });

	public static Task<int> SaveTransaction(DriverModel driver, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), driver, new { userId, platform });
}
