using Strada.Models.Common;

namespace Strada.Data.Common;

public static class CommonData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(CommonData));

	public static Task<List<T>> LoadTableData<T>(string TableName) where T : new() =>
		Api.Get<List<T>>($"{_endpoint}/{nameof(LoadTableData)}", new { TableName });

	public static Task<T> LoadTableDataById<T>(string TableName, int Id) where T : new() =>
		Api.Get<T>($"{_endpoint}/{nameof(LoadTableDataById)}", new { TableName, Id });

	public static Task<List<T>> LoadTableDataByStatus<T>(string TableName, bool Status = true) where T : new() =>
		Api.Get<List<T>>($"{_endpoint}/{nameof(LoadTableDataByStatus)}", new { TableName, Status });

	public static Task<List<T>> LoadTableDataByMasterId<T>(string TableName, int MasterId) where T : new() =>
		Api.Get<List<T>>($"{_endpoint}/{nameof(LoadTableDataByMasterId)}", new { TableName, MasterId });

	public static Task<List<T>> LoadTableDataByFinancialAccountingId<T>(string TableName, int? FinancialAccountingId) where T : new() =>
		Api.Get<List<T>>($"{_endpoint}/{nameof(LoadTableDataByFinancialAccountingId)}", new { TableName, FinancialAccountingId });

	public static Task<T> LoadTableDataByCode<T>(string TableName, string Code) where T : new() =>
		Api.Get<T>($"{_endpoint}/{nameof(LoadTableDataByCode)}", new { TableName, Code });

	public static Task<T> LoadTableDataByTransactionNo<T>(string TableName, string TransactionNo) where T : new() =>
		Api.Get<T>($"{_endpoint}/{nameof(LoadTableDataByTransactionNo)}", new { TableName, TransactionNo });

	public static Task<List<T>> LoadTableDataByDate<T>(string TableName, DateTime StartDate, DateTime EndDate) where T : new() =>
		Api.Get<List<T>>($"{_endpoint}/{nameof(LoadTableDataByDate)}", new { TableName, StartDate, EndDate });

	public static Task<T> LoadLastTableData<T>(string TableName) where T : new() =>
		Api.Get<T>($"{_endpoint}/{nameof(LoadLastTableData)}", new { TableName });

	public static Task<T> LoadLastTableDataByFinancialYear<T>(string TableName, int FinancialYearId) where T : new() =>
		Api.Get<T>($"{_endpoint}/{nameof(LoadLastTableDataByFinancialYear)}", new { TableName, FinancialYearId });

	public static Task<T> LoadLastTableDataByCompanyFinancialYear<T>(string TableName, int CompanyId, int FinancialYearId) where T : new() =>
		Api.Get<T>($"{_endpoint}/{nameof(LoadLastTableDataByCompanyFinancialYear)}", new { TableName, CompanyId, FinancialYearId });

	public static Task<DateTime> LoadCurrentDateTime() =>
		Api.Get<DateTime>($"{_endpoint}/{nameof(LoadCurrentDateTime)}");
}
