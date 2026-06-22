using Strada.Models.APIService;
using Strada.Models.Common;

namespace Strada.Data.APIService;

public static class WheelsEyeApiService
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(WheelsEyeApiService));

	public static async Task<List<WheelsEyeVehicleModel>> GetLiveVehicles() =>
		await Api.Get<List<WheelsEyeVehicleModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GetLiveVehicles)));
}
