using Strada.Models.Common;
using Strada.Models.Fleet.OMC;

namespace Strada.Data.Fleet.OMC.Data;

public static class OMCCardMoneyTransferData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(OMCCardMoneyTransferData));

	public static async Task DeleteTransaction(OMCCardMoneyTransferModel oMCCardMoneyTransfer) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), oMCCardMoneyTransfer);

	public static async Task RecoverTransaction(OMCCardMoneyTransferModel oMCCardMoneyTransfer) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), oMCCardMoneyTransfer);

	public static async Task<int> SaveTransaction(OMCCardMoneyTransferModel oMCCardMoneyTransfer, List<OMCCardMoneyTransferDetailsModel> transferDetails, bool recover = false) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), new { oMCCardMoneyTransfer, transferDetails, recover });
}
