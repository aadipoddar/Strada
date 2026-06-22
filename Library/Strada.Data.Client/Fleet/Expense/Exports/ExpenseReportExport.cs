using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Expense;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Data.Fleet.Expense.Exports;

public static class ExpenseReportExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(ExpenseReportExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportReport(IEnumerable<ExpenseOverviewModel> expenseData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = false, CompanyModel company = null, VehicleModel vehicle = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportReport)),
			new { data = expenseData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, company, vehicle });

	public static async Task<(MemoryStream stream, string fileName)> ExportExpensesReport(IEnumerable<ExpenseDetailsOverviewModel> expensesData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = false, ExpenseTypeModel expenseType = null, CompanyModel company = null, VehicleModel vehicle = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportExpensesReport)),
			new { data = expensesData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, expenseType, company, vehicle });
}
