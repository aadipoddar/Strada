using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Route;

namespace Strada.Data.Fleet.Route.Exports;

public static class VehicleDriverExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleDriverExport));

	public static Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<VehicleDriverModel> vehicleDriver, ReportExportType exportType) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), vehicleDriver, new { exportType });
}
