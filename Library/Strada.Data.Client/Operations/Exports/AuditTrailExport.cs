using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Operations;

namespace Strada.Data.Operations.Exports;

public static class AuditTrailExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(AuditTrailExport));

	public static Task<(MemoryStream stream, string fileName)> ExportReport(IEnumerable<AuditTrailModel> data, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = false) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportReport)), data, new { exportType, dateRangeStart, dateRangeEnd, showAllColumns });
}
