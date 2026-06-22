using Strada.Models.Accounts.Masters;
using Strada.Models.Common;

namespace Strada.Data.Accounts.Masters.Data;

public static class LedgerData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(LedgerData));

	public static async Task DeleteTransaction(LedgerModel ledger, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), ledger, new { userId, platform });

	public static async Task RecoverTransaction(LedgerModel ledger, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), ledger, new { userId, platform });

	public static async Task<int> SaveTransaction(LedgerModel ledger, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), ledger, new { userId, platform });
}
