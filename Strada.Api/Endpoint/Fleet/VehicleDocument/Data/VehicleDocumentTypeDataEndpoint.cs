using Strada.Data.Fleet.VehicleDocument.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.VehicleDocument.Data;

public class VehicleDocumentTypeDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VehicleDocumentTypeDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(VehicleDocumentTypeData.DeleteTransaction), VehicleDocumentTypeData.DeleteTransaction);
		group.MapPost(nameof(VehicleDocumentTypeData.RecoverTransaction), VehicleDocumentTypeData.RecoverTransaction);
		group.MapPost(nameof(VehicleDocumentTypeData.SaveTransaction), VehicleDocumentTypeData.SaveTransaction);
	}
}
