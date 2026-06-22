using Strada.Models.Accounts.Masters;
using Strada.Models.Common;

namespace Strada.Data.Accounts.Masters.Data;

public static class VoucherData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VoucherData));

	public static async Task DeleteTransaction(VoucherModel voucher, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), voucher, new { userId, platform });

	public static async Task RecoverTransaction(VoucherModel voucher, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), voucher, new { userId, platform });

	public static async Task<int> SaveTransaction(VoucherModel voucher, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), voucher, new { userId, platform });
}
