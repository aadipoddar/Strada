using Strada.Models.Common;
using Strada.Models.Fleet.Trip;

namespace Strada.Data.Fleet.Trip.Data;

public static class TripData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(TripData));

	public static async Task<List<TripOverviewModel>> LoadTripOverviewByBillIdDate(int? BillId = null, DateTime? StartDate = null, DateTime? EndDate = null) =>
		await Api.Get<List<TripOverviewModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadTripOverviewByBillIdDate)), new { BillId, StartDate, EndDate });

	public static async Task<TripOverviewModel> LoadTripBySlNoFinancialYear(string SlNo, int FinancialYearId) =>
		await Api.Get<TripOverviewModel>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadTripBySlNoFinancialYear)), new { SlNo, FinancialYearId });

	public static async Task DeleteTransaction(TripModel trip) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), trip);

	public static async Task RecoverTransaction(TripModel trip) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), trip);

	public static async Task<int> SaveTransaction(TripModel trip, List<TripExpensesModel> expensesDetails, List<TripCardPaymentsModel> cardPaymentDetails, List<TripLedgerPaymentsModel> ledgerPaymentDetails, bool recover = false) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), new { trip, expensesDetails, cardPaymentDetails, ledgerPaymentDetails, recover });
}
