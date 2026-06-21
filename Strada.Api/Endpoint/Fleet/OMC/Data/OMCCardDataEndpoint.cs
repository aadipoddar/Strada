using Strada.Data.Fleet.OMC.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.OMC.Data;

public class OMCCardDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(OMCCardDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(OMCCardData.DeleteTransaction), OMCCardData.DeleteTransaction);
		group.MapPost(nameof(OMCCardData.RecoverTransaction), OMCCardData.RecoverTransaction);
		group.MapPost(nameof(OMCCardData.SaveTransaction), OMCCardData.SaveTransaction);
	}
}
