using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Data.Fleet.Vehicle.Exports;

public static class VehicleTypeExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleTypeExport));

	public static Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<VehicleTypeModel> vehicleTypeData, ReportExportType exportType) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), vehicleTypeData, new { exportType });
}
