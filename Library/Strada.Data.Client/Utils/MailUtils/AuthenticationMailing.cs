using Strada.Models.Common;
using Strada.Models.Operations;

namespace Strada.Data.Utils.MailUtils;

public static class AuthenticationMailing
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(AuthenticationMailing));

	public static Task SendLoginCodeEmail(UserModel user, string code, string redirectLink, int codeExpiryMinutes) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SendLoginCodeEmail)), new { user, code, redirectLink, codeExpiryMinutes });
}
