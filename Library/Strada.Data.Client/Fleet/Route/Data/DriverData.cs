using Strada.Models.Common;
using Strada.Models.Fleet.Route;

namespace Strada.Data.Fleet.Route.Data;

public static class DriverData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(DriverData));

	public static async Task<List<DriverOverviewModel>> LoadDriverOverview() =>
		await Api.Get<List<DriverOverviewModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadDriverOverview)));

	public static async Task DeleteTransaction(DriverModel driver, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), driver, new { userId, platform });

	public static async Task RecoverTransaction(DriverModel driver, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), driver, new { userId, platform });

	public static async Task<int> SaveTransaction(DriverModel driver, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), driver, new { userId, platform });
}
