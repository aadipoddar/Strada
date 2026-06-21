namespace Strada.Models.Fleet.Expense;

// Pure cart -> details mapping, shared by page + server + WASM client (no DB dependency).
public static class ExpenseCartExtensions
{
	public static List<ExpenseDetailsModel> ConvertExpensesCartToDetails(this List<ExpenseDetailsCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new ExpenseDetailsModel
		{
			Id = 0,
			MasterId = masterId,
			ExpenseTypeId = item.ExpenseTypeId,
			LedgerId = item.LedgerId,
			Amount = item.Amount,
			IdentificationNo = item.IdentificationNo,
			Remarks = item.Remarks,
			Status = true
		})];
}
