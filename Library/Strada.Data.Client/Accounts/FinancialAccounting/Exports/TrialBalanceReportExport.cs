using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.FinancialAccounting.Exports;

public static class TrialBalanceReportExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(TrialBalanceReportExport));

	public static Task<(MemoryStream stream, string fileName)> ExportReport(IEnumerable<TrialBalanceModel> trialBalanceData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, CompanyModel company = null, GroupModel group = null, AccountTypeModel accountType = null) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportReport)),
			new { data = trialBalanceData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, company, group, accountType });
}
