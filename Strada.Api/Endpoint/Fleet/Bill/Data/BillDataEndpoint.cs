using Strada.Data.Fleet.Bill.Data;
using Strada.Models.Common;
using Strada.Models.Fleet.Bill;
using Strada.Models.Fleet.Trip;

namespace Strada.Api.Endpoint.Fleet.Bill.Data;

public class BillDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(BillDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(BillData.DeleteTransaction),
			(BillModel bill) => BillData.DeleteTransaction(bill));

		group.MapPost(nameof(BillData.SaveTransaction),
			(BillSaveRequest request) => BillData.SaveTransaction(request.Bill, request.LedgerPayments, request.Trips, request.ShowNotification));
	}

	private sealed record BillSaveRequest(
		BillModel Bill,
		List<BillLedgerPaymentsModel> LedgerPayments,
		List<TripOverviewModel> Trips,
		bool ShowNotification);
}
