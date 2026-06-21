using Strada.Data.Fleet.Trip;
using Strada.Models.Common;
using Strada.Models.Fleet.Trip;

namespace Strada.Api.Endpoint.Fleet.Trip.Data;

public class TripDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(TripDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapGet(nameof(TripData.LoadTripOverviewByBillIdDate),
			(int? BillId, DateTime? StartDate, DateTime? EndDate) => TripData.LoadTripOverviewByBillIdDate(BillId, StartDate, EndDate));

		group.MapGet(nameof(TripData.LoadTripBySlNoFinancialYear),
			(string SlNo, int FinancialYearId) => TripData.LoadTripBySlNoFinancialYear(SlNo, FinancialYearId));

		group.MapPost(nameof(TripData.DeleteTransaction),
			(TripModel trip) => TripData.DeleteTransaction(trip));

		group.MapPost(nameof(TripData.RecoverTransaction), TripData.RecoverTransaction);

		group.MapPost(nameof(TripData.SaveTransaction),
			(TripSaveRequest request) => TripData.SaveTransaction(request.Trip, request.ExpensesDetails, request.CardPaymentDetails, request.LedgerPaymentDetails, request.Recover));
	}

	private sealed record TripSaveRequest(
		TripModel Trip,
		List<TripExpensesModel> ExpensesDetails,
		List<TripCardPaymentsModel> CardPaymentDetails,
		List<TripLedgerPaymentsModel> LedgerPaymentDetails,
		bool Recover);
}
