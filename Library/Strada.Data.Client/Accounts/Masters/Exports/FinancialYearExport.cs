using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.Masters.Exports;

public static class FinancialYearExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(FinancialYearExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<FinancialYearModel> financialYearData, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), financialYearData, new { exportType });
}
