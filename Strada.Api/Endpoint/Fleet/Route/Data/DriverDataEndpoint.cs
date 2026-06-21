using Strada.Data.Fleet.Route.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.Route.Data;

public class DriverDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(DriverDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapGet(nameof(DriverData.LoadDriverOverview), DriverData.LoadDriverOverview);
		group.MapPost(nameof(DriverData.DeleteTransaction), DriverData.DeleteTransaction);
		group.MapPost(nameof(DriverData.RecoverTransaction), DriverData.RecoverTransaction);
		group.MapPost(nameof(DriverData.SaveTransaction), DriverData.SaveTransaction);
	}
}
