using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.OMC;

namespace Strada.Data.Fleet.OMC.Exports;

public static class OMCCardExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(OMCCardExport));

	public static Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<OMCCardModel> omcCardData, ReportExportType exportType) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), omcCardData, new { exportType });
}
