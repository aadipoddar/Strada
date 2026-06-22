using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.VehicleDocument;

namespace Strada.Data.Fleet.VehicleDocument.Exports;

public static class VehicleDocumentExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleDocumentExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportTransaction(IEnumerable<VehicleDocumentModel> vehicleDocumentData, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportTransaction)), vehicleDocumentData, new { exportType });
}
