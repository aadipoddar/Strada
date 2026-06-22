using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Common;

namespace Strada.Data.Accounts.FinancialAccounting.Data;

public static class FinancialAccountingData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(FinancialAccountingData));

	public static async Task<FinancialAccountingModel> LoadFinancialAccountingByVoucherReference(int VoucherId, int ReferenceId, string ReferenceNo) =>
		await Api.Get<FinancialAccountingModel>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadFinancialAccountingByVoucherReference)), new { VoucherId, ReferenceId, ReferenceNo });

	public static async Task<List<TrialBalanceModel>> LoadTrialBalanceByCompanyDate(int CompanyId, DateTime StartDate, DateTime EndDate) =>
		await Api.Get<List<TrialBalanceModel>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadTrialBalanceByCompanyDate)), new { CompanyId, StartDate, EndDate });

	public static async Task DeleteTransaction(FinancialAccountingModel accounting) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), accounting);

	public static async Task RecoverTransaction(FinancialAccountingModel accounting) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), accounting);

	public static async Task<int> SaveTransaction(FinancialAccountingModel accounting, List<FinancialAccountingLedgerModel> ledgers, bool recover = false) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), new { accounting, ledgers, recover });

	public static async Task SaveBRSDates(List<FinancialAccountingLedgerModel> changedLines, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveBRSDates)), changedLines, new { userId, platform });
}
