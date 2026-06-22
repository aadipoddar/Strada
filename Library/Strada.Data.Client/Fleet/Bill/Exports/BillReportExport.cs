using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.Bill;
using Strada.Models.Fleet.OMC;

namespace Strada.Data.Fleet.Bill.Exports;

public static class BillReportExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(BillReportExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportReport(IEnumerable<BillOverviewModel> tripData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = false, CompanyModel company = null, OMCModel omc = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportReport)),
			new { data = tripData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, company, omc });

	public static async Task<(MemoryStream stream, string fileName)> ExportLedgerPaymentsReport(IEnumerable<BillLedgerPaymentsOverviewModel> paymentsData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = false, LedgerModel ledger = null, CompanyModel company = null, OMCModel omc = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportLedgerPaymentsReport)),
			new { data = paymentsData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, ledger, company, omc });
}
