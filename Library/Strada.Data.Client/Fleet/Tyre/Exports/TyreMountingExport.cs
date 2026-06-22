using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Tyre;

namespace Strada.Data.Fleet.Tyre.Exports;

public static class TyreMountingExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(TyreMountingExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportTransaction(IEnumerable<TyreMountingModel> tyreMountingData, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportTransaction)), tyreMountingData, new { exportType });
}
