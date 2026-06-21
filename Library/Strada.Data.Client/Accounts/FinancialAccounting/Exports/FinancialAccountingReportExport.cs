using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Accounts.Masters;
using Strada.Models.Common;
using Strada.Models.Exports;

namespace Strada.Data.Accounts.FinancialAccounting.Exports;

public static class FinancialAccountingReportExport
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(FinancialAccountingReportExport));

	public static Task<(MemoryStream stream, string fileName)> ExportReport(IEnumerable<FinancialAccountingOverviewModel> accountingData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = false, CompanyModel company = null, VoucherModel voucher = null) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportReport)),
			new { data = accountingData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, company, voucher });

	public static Task<(MemoryStream stream, string fileName)> ExportLedgerReport(IEnumerable<FinancialAccountingLedgerOverviewModel> ledgerData, ReportExportType exportType, DateOnly? dateRangeStart = null, DateOnly? dateRangeEnd = null, bool showAllColumns = true, bool showDeleted = true, CompanyModel company = null, LedgerModel ledger = null, TrialBalanceModel trialBalance = null) =>
		Api.PostForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ExportLedgerReport)),
			new { data = ledgerData, exportType, dateRangeStart, dateRangeEnd, showAllColumns, showDeleted, company, ledger, trialBalance });
}
