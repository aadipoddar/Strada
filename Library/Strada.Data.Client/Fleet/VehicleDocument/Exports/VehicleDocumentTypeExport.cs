using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.VehicleDocument;

namespace Strada.Data.Fleet.VehicleDocument.Exports;

public static class VehicleDocumentTypeExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleDocumentTypeExport));

	public static Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<VehicleDocumentTypeModel> vehicleDocumentTypeData, ReportExportType exportType) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), vehicleDocumentTypeData, new { exportType });
}
