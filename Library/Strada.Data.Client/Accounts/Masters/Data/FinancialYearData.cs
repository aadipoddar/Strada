using Strada.Models.Accounts.Masters;
using Strada.Models.Common;

namespace Strada.Data.Accounts.Masters.Data;

public static class FinancialYearData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(FinancialYearData));

	public static async Task<FinancialYearModel> LoadFinancialYearByDateTime(DateTime TransactionDateTime) =>
		await Api.Get<FinancialYearModel>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadFinancialYearByDateTime)), new { TransactionDateTime });

	public static async Task ValidateFinancialYear(DateTime TransactionDateTime) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ValidateFinancialYear)), new { }, new { TransactionDateTime });

	public static async Task<(DateTime FromDate, DateTime ToDate)> GetDateRange(DateRangeType rangeType, DateTime referenceFromDate, DateTime referenceToDate) =>
		await Api.Get<(DateTime FromDate, DateTime ToDate)>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GetDateRange)), new { rangeType, referenceFromDate, referenceToDate });

	public static async Task DeleteTransaction(FinancialYearModel financialYear, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), financialYear, new { userId, platform });

	public static async Task RecoverTransaction(FinancialYearModel financialYear, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), financialYear, new { userId, platform });

	public static async Task<int> SaveTransaction(FinancialYearModel financialYear, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), financialYear, new { userId, platform });
}
