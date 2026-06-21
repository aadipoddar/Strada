using Strada.Data.Fleet.Vehicle.Data;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.Vehicle.Data;

public class ExpenseTypeDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(ExpenseTypeDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(ExpenseTypeData.DeleteTransaction), ExpenseTypeData.DeleteTransaction);
		group.MapPost(nameof(ExpenseTypeData.RecoverTransaction), ExpenseTypeData.RecoverTransaction);
		group.MapPost(nameof(ExpenseTypeData.SaveTransaction), ExpenseTypeData.SaveTransaction);
	}
}
