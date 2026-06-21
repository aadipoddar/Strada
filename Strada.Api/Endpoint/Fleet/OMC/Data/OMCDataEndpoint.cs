using Strada.Data.Fleet.OMC.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.OMC.Data;

public class OMCDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(OMCDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(OMCData.DeleteTransaction), OMCData.DeleteTransaction);
		group.MapPost(nameof(OMCData.RecoverTransaction), OMCData.RecoverTransaction);
		group.MapPost(nameof(OMCData.SaveTransaction), OMCData.SaveTransaction);
	}
}
