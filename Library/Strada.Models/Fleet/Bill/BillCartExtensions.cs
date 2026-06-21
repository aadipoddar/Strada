namespace Strada.Models.Fleet.Bill;

// Pure cart -> details mapping, shared by page + server + WASM client (no DB dependency).
public static class BillCartExtensions
{
	public static List<BillLedgerPaymentsModel> ConvertLedgerPaymentCartToDetails(this List<BillLedgerPaymentsCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new BillLedgerPaymentsModel
		{
			Id = 0,
			MasterId = masterId,
			LedgerId = item.LedgerId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];
}
