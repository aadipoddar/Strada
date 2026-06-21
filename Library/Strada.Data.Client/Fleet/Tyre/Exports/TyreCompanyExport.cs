using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Tyre;

namespace Strada.Data.Fleet.Tyre.Exports;

public static class TyreCompanyExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(TyreCompanyExport));

	public static Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<TyreCompanyModel> tyreCompanyData, ReportExportType exportType) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), tyreCompanyData, new { exportType });
}
