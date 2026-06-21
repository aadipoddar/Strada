using Strada.Models.Common;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Data.Fleet.Vehicle.Data;

public static class VehicleData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleData));

	public static Task DeleteTransaction(VehicleModel vehicle, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), vehicle, new { userId, platform });

	public static Task RecoverTransaction(VehicleModel vehicle, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), vehicle, new { userId, platform });

	public static Task<int> SaveTransaction(VehicleModel vehicle, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), vehicle, new { userId, platform });
}
