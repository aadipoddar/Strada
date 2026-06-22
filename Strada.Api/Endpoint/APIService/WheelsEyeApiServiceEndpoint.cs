using Strada.Data.APIService;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.APIService;

public class WheelsEyeApiServiceEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(WheelsEyeApiServiceEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapGet(nameof(WheelsEyeApiService.GetLiveVehicles), WheelsEyeApiService.GetLiveVehicles);
	}
}
