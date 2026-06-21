using Strada.Models.Common;
using Strada.Models.Fleet.Tyre;

namespace Strada.Data.Fleet.Tyre.Data;

public static class TyreMountingData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(TyreMountingData));

	public static Task DeleteTransaction(TyreMountingModel tyreMounting, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), tyreMounting, new { userId, platform });

	public static Task<int> SaveTransaction(TyreMountingModel tyreMounting, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), tyreMounting, new { userId, platform });
}
