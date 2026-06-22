using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Operations;

namespace Strada.Data.Operations.Exports;

public static class UserExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(UserExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<UserModel> userData, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), userData, new { exportType });
}
