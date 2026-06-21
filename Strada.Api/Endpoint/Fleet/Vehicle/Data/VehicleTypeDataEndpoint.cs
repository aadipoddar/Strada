using Strada.Data.Fleet.Vehicle.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.Vehicle.Data;

public class VehicleTypeDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VehicleTypeDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(VehicleTypeData.DeleteTransaction), VehicleTypeData.DeleteTransaction);
		group.MapPost(nameof(VehicleTypeData.RecoverTransaction), VehicleTypeData.RecoverTransaction);
		group.MapPost(nameof(VehicleTypeData.SaveTransaction), VehicleTypeData.SaveTransaction);
	}
}
