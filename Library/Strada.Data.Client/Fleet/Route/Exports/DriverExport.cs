using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Route;

namespace Strada.Data.Fleet.Route.Exports;

public static class DriverExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(DriverExport));

	public static Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<DriverModel> driverData, ReportExportType exportType) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), driverData, new { exportType });
}
