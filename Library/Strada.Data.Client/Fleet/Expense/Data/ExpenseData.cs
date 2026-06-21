using Strada.Models.Common;
using Strada.Models.Fleet.Expense;

namespace Strada.Data.Fleet.Expense.Data;

public static class ExpenseData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(ExpenseData));

	public static Task DeleteTransaction(ExpenseModel expense) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), expense);

	public static Task RecoverTransaction(ExpenseModel expense) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), expense);

	public static Task<int> SaveTransaction(ExpenseModel expense, List<ExpenseDetailsModel> expensesDetails, bool recover = false) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), new { expense, expensesDetails, recover });
}
