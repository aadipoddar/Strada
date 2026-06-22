using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.FinancialAccounting.Exports;

public static class ProfitAndLossReportExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(ProfitAndLossReportExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportIncomeReport(IEnumerable<TrialBalanceModel> incomeData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, CompanyModel company = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportIncomeReport)),
			new { data = incomeData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, company });

	public static async Task<(MemoryStream stream, string fileName)> ExportExpenseReport(IEnumerable<TrialBalanceModel> expenseData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, CompanyModel company = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportExpenseReport)),
			new { data = expenseData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, company });
}
