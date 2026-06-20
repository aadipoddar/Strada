using Carter;

using Strada.Data.Operations.Data;
using Strada.Models.Operations;

namespace Strada.Api.Endpoints;

public class UserEndpoints : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/User").WithTags("User");

		// TODO: temporary — InsertUser bypasses SaveTransaction's validation. Remove later.
		group.MapPost($"/{nameof(UserData.InsertUser)}", (UserModel user) => UserData.InsertUser(user));

		// TODO: strip Password (and LastCode* auth fields) before returning (security)
		group.MapGet($"/{nameof(UserData.LoadUserByPhoneEmail)}", UserData.LoadUserByPhoneEmail);

		group.MapPost($"/{nameof(UserData.ResetInsertUser)}", UserData.ResetInsertUser);
		group.MapPost($"/{nameof(UserData.SaveTransaction)}", UserData.SaveTransaction);
		group.MapPost($"/{nameof(UserData.DeleteTransaction)}", UserData.DeleteTransaction);
		group.MapPost($"/{nameof(UserData.RecoverTransaction)}", UserData.RecoverTransaction);
	}
}