using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.FinancialAccounting.Exports;

public static class BalanceSheetReportExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(BalanceSheetReportExport));

	public static Task<(MemoryStream stream, string fileName)> ExportAssetsReport(IEnumerable<TrialBalanceModel> assetsData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, CompanyModel company = null) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportAssetsReport)),
			new { data = assetsData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, company });

	public static Task<(MemoryStream stream, string fileName)> ExportLiabilitiesReport(IEnumerable<TrialBalanceModel> liabilitiesData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, CompanyModel company = null) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportLiabilitiesReport)),
			new { data = liabilitiesData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, company });
}
