using Strada.Data.Accounts.Masters.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Accounts.Masters.Data;

public class StateUTDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(StateUTDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(StateUTData.DeleteTransaction), StateUTData.DeleteTransaction);
		group.MapPost(nameof(StateUTData.RecoverTransaction), StateUTData.RecoverTransaction);
		group.MapPost(nameof(StateUTData.SaveTransaction), StateUTData.SaveTransaction);
	}
}
