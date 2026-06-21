using Strada.Models.Common;
using Strada.Models.Fleet.Trip;

namespace Strada.Data.Fleet.Trip;

public static class TripData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(TripData));

	public static Task<List<TripOverviewModel>> LoadTripOverviewByBillIdDate(int? BillId = null, DateTime? StartDate = null, DateTime? EndDate = null) =>
		Api.Get<List<TripOverviewModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadTripOverviewByBillIdDate)), new { BillId, StartDate, EndDate });

	public static Task<TripOverviewModel> LoadTripBySlNoFinancialYear(string SlNo, int FinancialYearId) =>
		Api.Get<TripOverviewModel>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadTripBySlNoFinancialYear)), new { SlNo, FinancialYearId });

	public static Task DeleteTransaction(TripModel trip) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), trip);

	public static Task RecoverTransaction(TripModel trip) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), trip);

	public static Task<int> SaveTransaction(TripModel trip, List<TripExpensesModel> expensesDetails, List<TripCardPaymentsModel> cardPaymentDetails, List<TripLedgerPaymentsModel> ledgerPaymentDetails, bool recover = false) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), new { trip, expensesDetails, cardPaymentDetails, ledgerPaymentDetails, recover });
}
