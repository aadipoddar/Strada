using Strada.Data.Common;
using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Common;
using Strada.Models.Fleet.Bill;
using Strada.Models.Fleet.Expense;
using Strada.Models.Fleet.OMC;
using Strada.Models.Fleet.Trip;

namespace Strada.Api.Endpoint.Common;

public class GenerateCodesEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(GenerateCodesEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(GenerateCodes.GenerateFinancialAccountingTransactionNo),
			(FinancialAccountingModel accounting) => GenerateCodes.GenerateFinancialAccountingTransactionNo(accounting));

		group.MapPost(nameof(GenerateCodes.GenerateTripTransactionNo),
			(TripModel trip) => GenerateCodes.GenerateTripTransactionNo(trip));

		group.MapPost(nameof(GenerateCodes.GenerateTripSlNo),
			(TripModel trip) => GenerateCodes.GenerateTripSlNo(trip));

		group.MapPost(nameof(GenerateCodes.GenerateBillTransactionNo),
			(BillModel bill) => GenerateCodes.GenerateBillTransactionNo(bill));

		group.MapPost(nameof(GenerateCodes.GenerateExpenseTransactionNo),
			(ExpenseModel expense) => GenerateCodes.GenerateExpenseTransactionNo(expense));

		group.MapPost(nameof(GenerateCodes.GenerateOMCCardMoneyTransferTransactionNo),
			(OMCCardMoneyTransferModel oMCCardMoneyTransfer) => GenerateCodes.GenerateOMCCardMoneyTransferTransactionNo(oMCCardMoneyTransfer));
	}
}
