using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Accounts.FinancialAccounting;
using StradaLibrary.Exports.Mailing;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Accounts.FinancialAccounting;
using StradaLibrary.Models.Operations;

namespace StradaLibrary.Data.Accounts.FinancialAccounting;

public static class FinancialAccountingData
{
	private static async Task<int> InsertFinancialAccounting(FinancialAccountingModel accounting, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertFinancialAccounting, accounting, sqlDataAccessTransaction)).FirstOrDefault();

	private static async Task<int> InsertFinancialAccountingLedger(FinancialAccountingLedgerModel ledger, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertFinancialAccountingLedger, ledger, sqlDataAccessTransaction)).FirstOrDefault();

	public static async Task<FinancialAccountingModel> LoadFinancialAccountingByVoucherReference(int VoucherId, int ReferenceId, string ReferenceNo, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<FinancialAccountingModel, dynamic>(AccountNames.LoadFinancialAccountingByVoucherReference, new { VoucherId, ReferenceId, ReferenceNo }, sqlDataAccessTransaction)).FirstOrDefault();

	public static async Task<List<TrialBalanceModel>> LoadTrialBalanceByCompanyDate(int CompanyId, DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<TrialBalanceModel, dynamic>(AccountNames.LoadTrialBalanceByCompanyDate, new { CompanyId, StartDate, EndDate });

	public static List<FinancialAccountingLedgerModel> ConvertCartToDetails(List<FinancialAccountingLedgerCartModel> cart, int accountingId = 0) =>
		[.. cart.Select(item => new FinancialAccountingLedgerModel
		{
			Id = 0,
			MasterId = accountingId,
			LedgerId = item.LedgerId,
			Credit = item.Credit,
			Debit = item.Debit,
			ReferenceType = item.ReferenceType,
			ReferenceId = item.ReferenceId,
			ReferenceNo = item.ReferenceNo,
			Remarks = item.Remarks,
			Status = true
		})];

	public static async Task DeleteTransaction(FinancialAccountingModel accounting, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (sqlDataAccessTransaction is null)
		{
			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				await DeleteTransaction(accounting, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			await FinancialAccountingNotify.Notify(accounting.Id, NotifyType.Deleted);
			return;
		}

		await FinancialYearData.ValidateFinancialYear(accounting.TransactionDateTime, sqlDataAccessTransaction);

		accounting.Status = false;
		await InsertFinancialAccounting(accounting, sqlDataAccessTransaction);
	}

	public static async Task RecoverTransaction(FinancialAccountingModel accounting)
	{
		accounting.Status = true;
		var ledgers = await CommonData.LoadTableDataByMasterId<FinancialAccountingLedgerModel>(AccountNames.FinancialAccountingLedger, accounting.Id);

		await SaveTransaction(accounting, ledgers, false);

		await FinancialAccountingNotify.Notify(accounting.Id, NotifyType.Recovered);
	}

	private static async Task<FinancialAccountingModel> ValidateTransaction(FinancialAccountingModel accounting, bool update = false, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (accounting.CompanyId <= 0)
			throw new InvalidOperationException("Please select a company for the transaction.");

		if (accounting.VoucherId <= 0)
			throw new InvalidOperationException("Please select a voucher for the transaction.");

		if (accounting.TotalCreditAmount <= 0 && accounting.TotalDebitAmount <= 0)
			throw new InvalidOperationException("Total debit and credit amounts cannot both be zero.");

		if (accounting.TotalCreditLedgers <= 0 && accounting.TotalDebitLedgers <= 0)
			throw new InvalidOperationException("There must be at least one debit or credit ledger in the transaction.");

		if (accounting.TotalDebitAmount - accounting.TotalCreditAmount != 0)
			throw new InvalidOperationException("Total debit and credit amounts must be equal.");

		accounting.TransactionNo = await GenerateCodes.GenerateFinancialAccountingTransactionNo(accounting, sqlDataAccessTransaction);
		await FinancialYearData.ValidateFinancialYear(accounting.TransactionDateTime, sqlDataAccessTransaction);

		if (update)
		{
			var existingAccounting = await CommonData.LoadTableDataById<FinancialAccountingModel>(AccountNames.FinancialAccounting, accounting.Id, sqlDataAccessTransaction)
				?? throw new InvalidOperationException("The transaction to be updated does not exist.");

			await FinancialYearData.ValidateFinancialYear(existingAccounting.TransactionDateTime, sqlDataAccessTransaction);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, accounting.LastModifiedBy.Value, sqlDataAccessTransaction);
			if (!user.Admin)
				throw new InvalidOperationException("Only admin users are allowed to modify transactions.");

			accounting.TransactionNo = existingAccounting.TransactionNo;
		}

		return accounting;
	}

	private static void ValidateTransactionLedgers(FinancialAccountingModel accounting, List<FinancialAccountingLedgerModel> ledgers)
	{
		if (ledgers is null || ledgers.Count == 0)
			throw new InvalidOperationException("The transaction must have at least one accounting ledger.");

		if (ledgers.Any(d => !d.Status))
			throw new InvalidOperationException("Accounting ledgers must be active.");

		if (ledgers.Any(d => (d.Credit ?? 0) < 0 || (d.Debit ?? 0) < 0))
			throw new InvalidOperationException("Accounting ledgers cannot have negative amounts.");

		if (ledgers.Count != (accounting.TotalDebitLedgers + accounting.TotalCreditLedgers))
			throw new InvalidOperationException("The number of accounting ledgers does not match the transaction summary.");
	}

	public static async Task<int> SaveTransaction(FinancialAccountingModel accounting, List<FinancialAccountingLedgerModel> ledgers, bool showNotification = true, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		bool update = accounting.Id > 0;

		if (sqlDataAccessTransaction is null)
		{
			(MemoryStream, string)? previousInvoice = null;
			if (update)
				previousInvoice = await FinancialAccountingInvoiceExport.ExportInvoice(accounting.Id, InvoiceExportType.PDF);

			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				accounting.Id = await SaveTransaction(accounting, ledgers, showNotification, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			if (showNotification)
				await FinancialAccountingNotify.Notify(accounting.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

			return accounting.Id;
		}

		accounting = await ValidateTransaction(accounting, update, sqlDataAccessTransaction);
		ValidateTransactionLedgers(accounting, ledgers);
		accounting.Id = await InsertFinancialAccounting(accounting, sqlDataAccessTransaction);
		await SaveTransactionLedgerDetails(accounting, ledgers, update, sqlDataAccessTransaction);

		return accounting.Id;
	}

	private static async Task SaveTransactionLedgerDetails(FinancialAccountingModel accounting, List<FinancialAccountingLedgerModel> ledgers, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingLedgers = await CommonData.LoadTableDataByMasterId<FinancialAccountingLedgerModel>(AccountNames.FinancialAccountingLedger, accounting.Id, sqlDataAccessTransaction);
			foreach (var item in existingLedgers)
			{
				item.Status = false;
				await InsertFinancialAccountingLedger(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in ledgers)
		{
			item.MasterId = accounting.Id;
			var id = await InsertFinancialAccountingLedger(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save accounting ledger item.");
		}
	}
}
