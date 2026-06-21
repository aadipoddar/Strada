using Strada.Models.Common;
using Strada.Models.Fleet.Bill;
using Strada.Models.Fleet.Trip;

namespace Strada.Data.Fleet.Bill.Data;

public static class BillData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(BillData));

	public static Task DeleteTransaction(BillModel bill) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), bill);

	public static Task<int> SaveTransaction(BillModel bill, List<BillLedgerPaymentsModel> ledgerPayments, List<TripOverviewModel> trips, bool showNotification = true) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), new { bill, ledgerPayments, trips, showNotification });
}
