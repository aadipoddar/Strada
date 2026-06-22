using Strada.Models.Common;
using Strada.Models.Fleet.OMC;

namespace Strada.Data.Fleet.OMC.Data;

public static class OMCCardData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(OMCCardData));

	public static async Task DeleteTransaction(OMCCardModel omcCard, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), omcCard, new { userId, platform });

	public static async Task RecoverTransaction(OMCCardModel omcCard, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), omcCard, new { userId, platform });

	public static async Task<int> SaveTransaction(OMCCardModel omcCard, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), omcCard, new { userId, platform });
}
