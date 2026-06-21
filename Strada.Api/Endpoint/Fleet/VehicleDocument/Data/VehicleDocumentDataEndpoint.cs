using Strada.Data.Fleet.VehicleDocument.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.VehicleDocument.Data;

public class VehicleDocumentDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(VehicleDocumentDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(VehicleDocumentData.DeleteTransaction), VehicleDocumentData.DeleteTransaction);
		group.MapPost(nameof(VehicleDocumentData.RecoverTransaction), VehicleDocumentData.RecoverTransaction);

		// SaveTransaction (file upload) pending the multipart pattern.
	}
}
