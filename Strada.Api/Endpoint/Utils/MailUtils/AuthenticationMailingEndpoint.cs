using Strada.Data.Utils.MailUtils;
using Strada.Models.Common;
using Strada.Models.Operations;

namespace Strada.Api.Endpoint.Utils.MailUtils;

public class AuthenticationMailingEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(AuthenticationMailingEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(AuthenticationMailing.SendLoginCodeEmail),
			(SendLoginCodeEmailRequest request) => AuthenticationMailing.SendLoginCodeEmail(request.User, request.Code, request.RedirectLink, request.CodeExpiryMinutes));
	}

	private sealed record SendLoginCodeEmailRequest(UserModel User, string Code, string RedirectLink, int CodeExpiryMinutes);
}
