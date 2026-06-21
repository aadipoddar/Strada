using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.Masters.Exports;

public static class LedgerExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(LedgerExport));

	public static Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<LedgerModel> ledgerData, ReportExportType exportType) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), ledgerData, new { exportType });
}
