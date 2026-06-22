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

	public static Task<string> GenerateFinancialAccountingTransactionNo(FinancialAccountingModel accounting) =>
		Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateFinancialAccountingTransactionNo)), accounting);

	public static Task<string> GenerateTripTransactionNo(TripModel trip) =>
		Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateTripTransactionNo)), trip);

	public static Task<string> GenerateTripSlNo(TripModel trip) =>
		Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateTripSlNo)), trip);

	public static Task<string> GenerateBillTransactionNo(BillModel bill) =>
		Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateBillTransactionNo)), bill);

	public static Task<string> GenerateExpenseTransactionNo(ExpenseModel expense) =>
		Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateExpenseTransactionNo)), expense);

	public static Task<string> GenerateOMCCardMoneyTransferTransactionNo(OMCCardMoneyTransferModel oMCCardMoneyTransfer) =>
		Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GenerateOMCCardMoneyTransferTransactionNo)), oMCCardMoneyTransfer);
}
