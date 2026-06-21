using Strada.Models.Common;
using Strada.Models.Fleet.OMC;

namespace Strada.Data.Fleet.OMC.Data;

public static class OMCData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(OMCData));

	public static Task DeleteTransaction(OMCModel omc, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), omc, new { userId, platform });

	public static Task RecoverTransaction(OMCModel omc, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), omc, new { userId, platform });

	public static Task<int> SaveTransaction(OMCModel omc, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), omc, new { userId, platform });
}
