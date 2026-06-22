using Strada.Models.Common;
using Strada.Models.Fleet.Route;

namespace Strada.Data.Fleet.Route.Data;

public static class LocationData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(LocationData));

	public static async Task DeleteTransaction(LocationModel location, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), location, new { userId, platform });

	public static async Task RecoverTransaction(LocationModel location, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), location, new { userId, platform });

	public static async Task<int> SaveTransaction(LocationModel location, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), location, new { userId, platform });
}
