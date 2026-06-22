using Strada.Models.Common;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Data.Fleet.Vehicle.Data;

public static class VehicleTypeData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleTypeData));

	public static async Task DeleteTransaction(VehicleTypeModel vehicleType, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), vehicleType, new { userId, platform });

	public static async Task RecoverTransaction(VehicleTypeModel vehicleType, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), vehicleType, new { userId, platform });

	public static async Task<int> SaveTransaction(VehicleTypeModel vehicleType, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), vehicleType, new { userId, platform });
}
