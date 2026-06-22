using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Route;

namespace Strada.Data.Fleet.Route.Exports;

public static class RouteExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(RouteExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<RouteModel> route, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), route, new { exportType });
}
