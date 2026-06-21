using Strada.Data.Fleet.Expense.Data;
using Strada.Models.Common;
using Strada.Models.Fleet.Expense;

namespace Strada.Api.Endpoint.Fleet.Expense.Data;

public class ExpenseDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(ExpenseDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(ExpenseData.DeleteTransaction),
			(ExpenseModel expense) => ExpenseData.DeleteTransaction(expense));

		group.MapPost(nameof(ExpenseData.RecoverTransaction), ExpenseData.RecoverTransaction);

		group.MapPost(nameof(ExpenseData.SaveTransaction),
			(ExpenseSaveRequest request) => ExpenseData.SaveTransaction(request.Expense, request.ExpensesDetails, request.Recover));
	}

	private sealed record ExpenseSaveRequest(
		ExpenseModel Expense,
		List<ExpenseDetailsModel> ExpensesDetails,
		bool Recover);
}
