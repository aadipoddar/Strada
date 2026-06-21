using Strada.Data.Fleet.Tyre.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.Tyre.Data;

public class TyreCompanyDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(TyreCompanyDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(TyreCompanyData.DeleteTransaction), TyreCompanyData.DeleteTransaction);
		group.MapPost(nameof(TyreCompanyData.RecoverTransaction), TyreCompanyData.RecoverTransaction);
		group.MapPost(nameof(TyreCompanyData.SaveTransaction), TyreCompanyData.SaveTransaction);
	}
}
