using Strada.Models.Common;
using Strada.Models.Fleet.Expense;

namespace Strada.Data.Fleet.Expense.Data;

public static class ExpenseData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(ExpenseData));

	public static async Task DeleteTransaction(ExpenseModel expense) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), expense);

	public static async Task RecoverTransaction(ExpenseModel expense) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), expense);

	public static async Task<int> SaveTransaction(ExpenseModel expense, List<ExpenseDetailsModel> expensesDetails, bool recover = false) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), new { expense, expensesDetails, recover });
}
