using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Common;

namespace Strada.Data.Accounts.FinancialAccounting.Data;

public static class FinancialAccountingData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(FinancialAccountingData));

	public static Task<FinancialAccountingModel> LoadFinancialAccountingByVoucherReference(int VoucherId, int ReferenceId, string ReferenceNo) =>
		Api.Get<FinancialAccountingModel>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadFinancialAccountingByVoucherReference)), new { VoucherId, ReferenceId, ReferenceNo });

	public static Task<List<TrialBalanceModel>> LoadTrialBalanceByCompanyDate(int CompanyId, DateTime StartDate, DateTime EndDate) =>
		Api.Get<List<TrialBalanceModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadTrialBalanceByCompanyDate)), new { CompanyId, StartDate, EndDate });

	public static Task DeleteTransaction(FinancialAccountingModel accounting) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), accounting);

	public static Task RecoverTransaction(FinancialAccountingModel accounting) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), accounting);

	public static Task<int> SaveTransaction(FinancialAccountingModel accounting, List<FinancialAccountingLedgerModel> ledgers, bool recover = false) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), new { accounting, ledgers, recover });

	public static Task SaveBRSDates(List<FinancialAccountingLedgerModel> changedLines, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveBRSDates)), changedLines, new { userId, platform });
}
