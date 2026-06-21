using Strada.Data.Fleet.Vehicle.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.Vehicle.Data;

public class VehicleDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VehicleDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(VehicleData.DeleteTransaction), VehicleData.DeleteTransaction);
		group.MapPost(nameof(VehicleData.RecoverTransaction), VehicleData.RecoverTransaction);
		group.MapPost(nameof(VehicleData.SaveTransaction), VehicleData.SaveTransaction);
	}
}
