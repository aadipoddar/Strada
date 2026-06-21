using Strada.Models.Common;
using Strada.Models.Fleet.Route;

namespace Strada.Data.Fleet.Route.Data;

public static class RouteData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(RouteData));

	public static Task<List<RouteOverviewModel>> LoadRouteOverview() =>
		Api.Get<List<RouteOverviewModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadRouteOverview)));

	public static Task DeleteTransaction(RouteModel route, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), route, new { userId, platform });

	public static Task RecoverTransaction(RouteModel route, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), route, new { userId, platform });

	public static Task<int> SaveTransaction(RouteModel route, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), route, new { userId, platform });
}
