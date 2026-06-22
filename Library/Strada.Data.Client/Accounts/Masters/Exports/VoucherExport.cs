using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.Masters.Exports;

public static class VoucherExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VoucherExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<VoucherModel> voucherData, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), voucherData, new { exportType });
}
