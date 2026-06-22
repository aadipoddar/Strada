using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;
using Strada.Models.Fleet.OMC;

namespace Strada.Data.Fleet.OMC.Exports;

public static class OMCCardMoneyTransferReportExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(OMCCardMoneyTransferReportExport));

	public static async Task<(MemoryStream stream, string fileName)> ExportReport(IEnumerable<OMCCardMoneyTransferOverviewModel> transferData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = false, CompanyModel company = null, LedgerModel ledger = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportReport)),
			new { data = transferData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, company, ledger });

	public static async Task<(MemoryStream stream, string fileName)> ExportTransfersReport(IEnumerable<OMCCardMoneyTransferDetailsOverviewModel> transfersData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = false, OMCCardModel oMCCard = null, CompanyModel company = null, LedgerModel ledger = null, OMCModel omc = null) =>
		await Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportTransfersReport)),
			new { data = transfersData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, oMCCard, company, ledger, omc });
}
