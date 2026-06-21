using Strada.Data.Fleet.Tyre.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.Tyre.Data;

public class TyreMountingDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(TyreMountingDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(TyreMountingData.DeleteTransaction), TyreMountingData.DeleteTransaction);
		group.MapPost(nameof(TyreMountingData.SaveTransaction), TyreMountingData.SaveTransaction);
	}
}
