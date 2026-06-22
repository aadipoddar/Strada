using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Common;
using Strada.Models.Fleet.Bill;
using Strada.Models.Fleet.Expense;
using Strada.Models.Fleet.OMC;
using Strada.Models.Fleet.Trip;

namespace Strada.Data.Common;

public static class GenerateCodes
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(GenerateCodes));

	public static async Task<string> GenerateFinancialAccountingTransactionNo(FinancialAccountingModel accounting) =>
		await Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateFinancialAccountingTransactionNo)), accounting);

	public static async Task<string> GenerateTripTransactionNo(TripModel trip) =>
		await Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateTripTransactionNo)), trip);

	public static async Task<string> GenerateTripSlNo(TripModel trip) =>
		await Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateTripSlNo)), trip);

	public static async Task<string> GenerateBillTransactionNo(BillModel bill) =>
		await Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateBillTransactionNo)), bill);

	public static async Task<string> GenerateExpenseTransactionNo(ExpenseModel expense) =>
		await Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateExpenseTransactionNo)), expense);

	public static async Task<string> GenerateOMCCardMoneyTransferTransactionNo(OMCCardMoneyTransferModel oMCCardMoneyTransfer) =>
		await Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateOMCCardMoneyTransferTransactionNo)), oMCCardMoneyTransfer);
}
