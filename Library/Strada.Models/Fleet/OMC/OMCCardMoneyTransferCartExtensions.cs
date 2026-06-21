namespace Strada.Models.Fleet.OMC;

// Pure cart -> details mapping, shared by page + server + WASM client (no DB dependency).
public static class OMCCardMoneyTransferCartExtensions
{
	public static List<OMCCardMoneyTransferDetailsModel> ConvertTransfersCartToDetails(this List<OMCCardMoneyTransferDetailsCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new OMCCardMoneyTransferDetailsModel
		{
			Id = 0,
			MasterId = masterId,
			OMCCardId = item.OMCCardId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];
}
