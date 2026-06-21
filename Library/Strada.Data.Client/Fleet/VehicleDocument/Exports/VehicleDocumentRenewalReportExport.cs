using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Vehicle;
using Strada.Models.Fleet.VehicleDocument;

namespace Strada.Data.Fleet.VehicleDocument.Exports;

public static class VehicleDocumentRenewalReportExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleDocumentRenewalReportExport));

	public static Task<(MemoryStream stream, string fileName)> ExportReport(IEnumerable<VehicleDocumentRenewalOverviewModel> renewalData, ReportExportType exportType, bool showAllColumns = true, VehicleModel vehicle = null, VehicleDocumentTypeModel documentType = null) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportReport)),
			new { data = renewalData, exportType, showAllColumns, vehicle, documentType });
}
