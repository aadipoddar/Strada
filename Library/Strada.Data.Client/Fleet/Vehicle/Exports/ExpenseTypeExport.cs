using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Data.Fleet.Vehicle.Exports;

public static class ExpenseTypeExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(ExpenseTypeExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<ExpenseTypeModel> expenseTypeData, ReportExportType exportType) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportMaster)), expenseTypeData, new { exportType });
}
