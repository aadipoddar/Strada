using Strada.Models.Accounts.Masters;
using Strada.Models.Common;

namespace Strada.Data.Accounts.Masters.Data;

public static class AccountTypeData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(AccountTypeData));

	public static async Task DeleteTransaction(AccountTypeModel accountType, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), accountType, new { userId, platform });

	public static async Task RecoverTransaction(AccountTypeModel accountType, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), accountType, new { userId, platform });

	public static async Task<int> SaveTransaction(AccountTypeModel accountType, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), accountType, new { userId, platform });
}
