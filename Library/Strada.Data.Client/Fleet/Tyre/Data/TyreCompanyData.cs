using Strada.Models.Common;
using Strada.Models.Fleet.Tyre;

namespace Strada.Data.Fleet.Tyre.Data;

public static class TyreCompanyData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(TyreCompanyData));

	public static Task DeleteTransaction(TyreCompanyModel tyreCompany, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), tyreCompany, new { userId, platform });

	public static Task RecoverTransaction(TyreCompanyModel tyreCompany, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), tyreCompany, new { userId, platform });

	public static Task<int> SaveTransaction(TyreCompanyModel tyreCompany, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), tyreCompany, new { userId, platform });
}
