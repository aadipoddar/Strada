using Strada.Models.Accounts.Masters;
using Strada.Models.Common;

namespace Strada.Data.Accounts.Masters.Data;

public static class FinancialYearData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(FinancialYearData));

	public static Task<FinancialYearModel> LoadFinancialYearByDateTime(DateTime TransactionDateTime) =>
		Api.Get<FinancialYearModel>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadFinancialYearByDateTime)), new { TransactionDateTime });

	public static Task ValidateFinancialYear(DateTime TransactionDateTime) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ValidateFinancialYear)), new { }, new { TransactionDateTime });

	public static Task<(DateTime FromDate, DateTime ToDate)> GetDateRange(DateRangeType rangeType, DateTime referenceFromDate, DateTime referenceToDate) =>
		Api.Get<(DateTime FromDate, DateTime ToDate)>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(GetDateRange)), new { rangeType, referenceFromDate, referenceToDate });

	public static Task DeleteTransaction(FinancialYearModel financialYear, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), financialYear, new { userId, platform });

	public static Task RecoverTransaction(FinancialYearModel financialYear, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), financialYear, new { userId, platform });

	public static Task<int> SaveTransaction(FinancialYearModel financialYear, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), financialYear, new { userId, platform });
}
