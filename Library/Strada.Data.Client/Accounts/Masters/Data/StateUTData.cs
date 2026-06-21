using Strada.Models.Accounts.Masters;
using Strada.Models.Common;

namespace Strada.Data.Accounts.Masters.Data;

public static class StateUTData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(StateUTData));

	public static Task DeleteTransaction(StateUTModel state, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), state, new { userId, platform });

	public static Task RecoverTransaction(StateUTModel state, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), state, new { userId, platform });

	public static Task<int> SaveTransaction(StateUTModel state, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), state, new { userId, platform });
}
