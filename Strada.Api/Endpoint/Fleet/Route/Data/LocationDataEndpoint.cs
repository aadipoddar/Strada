using Strada.Data.Fleet.Route.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.Route.Data;

public class LocationDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(LocationDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(LocationData.DeleteTransaction), LocationData.DeleteTransaction);
		group.MapPost(nameof(LocationData.RecoverTransaction), LocationData.RecoverTransaction);
		group.MapPost(nameof(LocationData.SaveTransaction), LocationData.SaveTransaction);
	}
}
