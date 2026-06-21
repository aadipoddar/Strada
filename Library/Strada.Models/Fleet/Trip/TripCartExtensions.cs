namespace Strada.Models.Fleet.Trip;

// Pure cart -> details mappings, shared by page + server + WASM client (no DB dependency).
public static class TripCartExtensions
{
	public static List<TripExpensesModel> ConvertExpensesCartToDetails(this List<TripExpensesCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new TripExpensesModel
		{
			Id = 0,
			MasterId = masterId,
			ExpenseTypeId = item.ExpenseTypeId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	public static List<TripCardPaymentsModel> ConvertCardPaymentCartToDetails(this List<TripCardPaymentsCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new TripCardPaymentsModel
		{
			Id = 0,
			MasterId = masterId,
			OMCCardId = item.OMCCardId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	public static List<TripLedgerPaymentsModel> ConvertLedgerPaymentCartToDetails(this List<TripLedgerPaymentsCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new TripLedgerPaymentsModel
		{
			Id = 0,
			MasterId = masterId,
			LedgerId = item.LedgerId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];
}
