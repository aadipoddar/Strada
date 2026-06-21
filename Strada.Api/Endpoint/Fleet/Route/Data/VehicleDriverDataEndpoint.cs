using Strada.Data.Fleet.Route.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.Route.Data;

public class VehicleDriverDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VehicleDriverDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapGet(nameof(VehicleDriverData.LoadVehicleDriverOverview), VehicleDriverData.LoadVehicleDriverOverview);
		group.MapPost(nameof(VehicleDriverData.DeleteTransaction), VehicleDriverData.DeleteTransaction);
		group.MapPost(nameof(VehicleDriverData.SaveTransaction), VehicleDriverData.SaveTransaction);
	}
}
