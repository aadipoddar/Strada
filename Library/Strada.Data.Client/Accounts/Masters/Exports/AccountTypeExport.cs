using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.Masters.Exports;

public static class AccountTypeExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(AccountTypeExport));

	public static Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<AccountTypeModel> accountTypeData, ReportExportType exportType) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), accountTypeData, new { exportType });
}
