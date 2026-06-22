using Strada.Models.Common;
using Strada.Models.Fleet.Bill;
using Strada.Models.Fleet.Trip;

namespace Strada.Data.Fleet.Bill.Data;

public static class BillData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(BillData));

	public static async Task DeleteTransaction(BillModel bill) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), bill);

	public static async Task<int> SaveTransaction(BillModel bill, List<BillLedgerPaymentsModel> ledgerPayments, List<TripOverviewModel> trips, bool showNotification = true) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), new { bill, ledgerPayments, trips, showNotification });
}
