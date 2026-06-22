using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.Masters.Exports;

public static class GroupExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(GroupExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<GroupModel> groupData, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), groupData, new { exportType });
}
