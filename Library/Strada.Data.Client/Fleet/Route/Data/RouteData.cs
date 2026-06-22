using Strada.Models.Common;
using Strada.Models.Fleet.Route;

namespace Strada.Data.Fleet.Route.Data;

public static class RouteData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(RouteData));

	public static async Task<List<RouteOverviewModel>> LoadRouteOverview() =>
		await Api.Get<List<RouteOverviewModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadRouteOverview)));

	public static async Task DeleteTransaction(RouteModel route, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), route, new { userId, platform });

	public static async Task RecoverTransaction(RouteModel route, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), route, new { userId, platform });

	public static async Task<int> SaveTransaction(RouteModel route, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), route, new { userId, platform });
}
