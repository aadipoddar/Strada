using Strada.Models.Accounts.Masters;
using Strada.Models.Common;

namespace Strada.Data.Accounts.Masters.Data;

public static class CompanyData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(CompanyData));

	public static async Task DeleteTransaction(CompanyModel company, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), company, new { userId, platform });

	public static async Task RecoverTransaction(CompanyModel company, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), company, new { userId, platform });

	public static async Task<int> SaveTransaction(CompanyModel company, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), company, new { userId, platform });
}
