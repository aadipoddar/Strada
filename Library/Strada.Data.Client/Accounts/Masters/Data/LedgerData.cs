using Strada.Models.Accounts.Masters;
using Strada.Models.Common;

namespace Strada.Data.Accounts.Masters.Data;

public static class LedgerData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(LedgerData));

	public static Task DeleteTransaction(LedgerModel ledger, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), ledger, new { userId, platform });

	public static Task RecoverTransaction(LedgerModel ledger, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), ledger, new { userId, platform });

	public static Task<int> SaveTransaction(LedgerModel ledger, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), ledger, new { userId, platform });
}
