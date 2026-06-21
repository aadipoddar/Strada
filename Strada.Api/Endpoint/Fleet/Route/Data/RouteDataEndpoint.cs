using Strada.Models.Common;

// Alias to disambiguate from Microsoft.AspNetCore.Routing.RouteData (in scope via implicit usings).
using RouteData = Strada.Data.Fleet.Route.Data.RouteData;

namespace Strada.Api.Endpoint.Fleet.Route.Data;

public class RouteDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(RouteDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapGet(nameof(RouteData.LoadRouteOverview), RouteData.LoadRouteOverview);
		group.MapPost(nameof(RouteData.DeleteTransaction), RouteData.DeleteTransaction);
		group.MapPost(nameof(RouteData.RecoverTransaction), RouteData.RecoverTransaction);
		group.MapPost(nameof(RouteData.SaveTransaction), RouteData.SaveTransaction);
	}
}
