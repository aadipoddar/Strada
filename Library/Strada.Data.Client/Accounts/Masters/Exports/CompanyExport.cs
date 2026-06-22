using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.Masters.Exports;

public static class CompanyExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(CompanyExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<CompanyModel> companyData, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), companyData, new { exportType });
}
