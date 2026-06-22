using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Route;

namespace Strada.Data.Fleet.Route.Exports;

public static class LocationExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(LocationExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<LocationModel> locationData, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), locationData, new { exportType });
}
