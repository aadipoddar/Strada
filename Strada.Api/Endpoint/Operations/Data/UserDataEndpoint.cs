using Strada.Data.Operations.Data;
using Strada.Models.Common;
using Strada.Models.Operations;

namespace Strada.Api.Endpoint.Operations.Data;

public class UserDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(UserDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		// TODO: temporary — InsertUser bypasses SaveTransaction's validation. Remove later.
		group.MapPost(nameof(UserData.InsertUser),
			(UserModel user) => UserData.InsertUser(user));

		// TODO: strip Password (and LastCode* auth fields) before returning (security)
		group.MapGet(nameof(UserData.LoadUserByPhoneEmail), UserData.LoadUserByPhoneEmail);

		group.MapPost(nameof(UserData.ResetInsertUser), UserData.ResetInsertUser);

		group.MapPost(nameof(UserData.DeleteTransaction), UserData.DeleteTransaction);
		group.MapPost(nameof(UserData.RecoverTransaction), UserData.RecoverTransaction);
		group.MapPost(nameof(UserData.SaveTransaction), UserData.SaveTransaction);
	}
}