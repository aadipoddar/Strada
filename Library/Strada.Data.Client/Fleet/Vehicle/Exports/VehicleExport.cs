using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Data.Fleet.Vehicle.Exports;

public static class VehicleExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleExport));

	public static Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<VehicleModel> vehicleData, ReportExportType exportType) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), vehicleData, new { exportType });
}
