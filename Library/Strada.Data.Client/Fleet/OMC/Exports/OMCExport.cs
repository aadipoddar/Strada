using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.OMC;

namespace Strada.Data.Fleet.OMC.Exports;

public static class OMCExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(OMCExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<OMCModel> omcData, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), omcData, new { exportType });
}
