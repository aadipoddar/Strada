using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.Masters.Exports;

public static class StateUTExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(StateUTExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<StateUTModel> stateUTData, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), stateUTData, new { exportType });
}
