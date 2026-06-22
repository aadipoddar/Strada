using Strada.Models.Accounts.Masters;
using Strada.Models.Common;

namespace Strada.Data.Accounts.Masters.Data;

public static class GroupData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(GroupData));

	public static async Task DeleteTransaction(GroupModel group, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), group, new { userId, platform });

	public static async Task RecoverTransaction(GroupModel group, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), group, new { userId, platform });

	public static async Task<int> SaveTransaction(GroupModel group, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), group, new { userId, platform });
}
