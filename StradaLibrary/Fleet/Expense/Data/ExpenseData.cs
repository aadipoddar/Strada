using Strada.Models.Common;
using Strada.Models.Fleet.Expense;
using Strada.Models.Operations;

using StradaLibrary.Accounts.Masters.Data;
using StradaLibrary.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Fleet.Expense.Exports;
using StradaLibrary.Operations.Data;
using StradaLibrary.Utils.ExportUtils;
using StradaLibrary.Utils.MailUtils;

namespace StradaLibrary.Fleet.Expense.Data;

public static class ExpenseData
{
	private static async Task<int> InsertExpense(ExpenseModel expense, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertExpense, expense, sqlDataAccessTransaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Expense.");

	private static async Task<int> InsertExpenseDetails(ExpenseDetailsModel expenseDetails, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertExpenseDetails, expenseDetails, sqlDataAccessTransaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Expense Detail.");

	public static List<ExpenseDetailsModel> ConvertExpensesCartToDetails(List<ExpenseDetailsCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new ExpenseDetailsModel
		{
			Id = 0,
			MasterId = masterId,
			ExpenseTypeId = item.ExpenseTypeId,
			LedgerId = item.LedgerId,
			Amount = item.Amount,
			IdentificationNo = item.IdentificationNo,
			Remarks = item.Remarks,
			Status = true
		})];

	public static async Task DeleteTransaction(ExpenseModel expense, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (sqlDataAccessTransaction is null)
		{
			await SqlDataAccessTransaction.Run(transaction => DeleteTransaction(expense, transaction));
			await ExpenseNotify.Notify(expense.Id, NotifyType.Deleted);
			return;
		}

		await FinancialYearData.ValidateFinancialYear(expense.TransactionDateTime, sqlDataAccessTransaction);

		expense.Status = false;
		await InsertExpense(expense, sqlDataAccessTransaction);

		await AuditTrailData.SaveAuditTrail(new()
		{
			Action = AuditTrailActionTypes.Delete.ToString(),
			TableName = FleetNames.Expense,
			RecordNo = expense.TransactionNo,
			CreatedBy = expense.LastModifiedBy.Value,
			CreatedFromPlatform = expense.LastModifiedFromPlatform
		}, sqlDataAccessTransaction);
	}

	public static async Task RecoverTransaction(ExpenseModel expense)
	{
		expense.Status = true;
		var expensesDetails = await CommonData.LoadTableDataByMasterId<ExpenseDetailsModel>(FleetNames.ExpenseDetails, expense.Id);

		await SaveTransaction(expense, expensesDetails, true);

		await ExpenseNotify.Notify(expense.Id, NotifyType.Recovered);
	}

	private static async Task<ExpenseModel> ValidateTransaction(ExpenseModel expense, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		expense.Remarks = string.IsNullOrWhiteSpace(expense.Remarks) ? null : expense.Remarks.Trim();

		if (expense.CompanyId <= 0)
			throw new InvalidOperationException("Please select a company for the transaction.");

		if (expense.VehicleId <= 0)
			throw new InvalidOperationException("Please select a vehicle for the transaction.");

		if (expense.TotalItems <= 0)
			throw new InvalidOperationException("Total items must be greater than zero.");

		if (expense.TotalExpense < 0)
			throw new InvalidOperationException("Total expense cannot be negative.");

		if (!update)
			expense.TransactionNo = await GenerateCodes.GenerateExpenseTransactionNo(expense, sqlDataAccessTransaction);

		await FinancialYearData.ValidateFinancialYear(expense.TransactionDateTime, sqlDataAccessTransaction);

		if (update)
		{
			var existingExpense = await CommonData.LoadTableDataById<ExpenseModel>(FleetNames.Expense, expense.Id, sqlDataAccessTransaction)
				?? throw new InvalidOperationException("The Expense transaction does not exist.");

			await FinancialYearData.ValidateFinancialYear(existingExpense.TransactionDateTime, sqlDataAccessTransaction);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, expense.LastModifiedBy.Value, sqlDataAccessTransaction);
			if (!user.Admin)
				throw new InvalidOperationException("Only admin users can update a Expense transaction.");

			expense.TransactionNo = existingExpense.TransactionNo;
		}

		return expense;
	}

	private static void ValidateExpensesDetails(ExpenseModel expense, List<ExpenseDetailsModel> expensesDetails)
	{
		if (expensesDetails.Any(ed => ed.Amount <= 0))
			throw new InvalidOperationException("Expense amount must be greater than zero.");

		if (expensesDetails.Count != expense.TotalItems)
			throw new InvalidOperationException("Total items must be equal to the number of expense details.");

		if (expensesDetails.Sum(ed => ed.Amount) != expense.TotalExpense)
			throw new InvalidOperationException("Total expense amount must be equal to total expense of the transaction.");

		foreach (var item in expensesDetails)
		{
			item.Remarks = string.IsNullOrWhiteSpace(item.Remarks) ? null : item.Remarks.Trim();
			item.IdentificationNo = string.IsNullOrWhiteSpace(item.IdentificationNo) ? null : item.IdentificationNo.Trim();
		}
	}

	public static async Task<int> SaveTransaction(
		ExpenseModel expense,
		List<ExpenseDetailsModel> expensesDetails,
		bool recover = false,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		bool update = expense.Id > 0;

		if (sqlDataAccessTransaction is null)
		{
			(MemoryStream, string)? previousInvoice = update && !recover ? await ExpenseInvoiceExport.ExportInvoice(expense.Id, InvoiceExportType.PDF) : null;

			expense.Id = await SqlDataAccessTransaction.Run(transaction => SaveTransaction(expense, expensesDetails, recover, transaction));

			if (!recover)
				await ExpenseNotify.Notify(expense.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

			return expense.Id;
		}

		expense = await ValidateTransaction(expense, update, sqlDataAccessTransaction);
		ValidateExpensesDetails(expense, expensesDetails);

		var previousExpense = update && !recover ? await CommonData.LoadTableDataById<ExpenseOverviewModel>(FleetNames.ExpenseOverview, expense.Id, sqlDataAccessTransaction) : new();
		var previousExpensesDetails = update && !recover ? await CommonData.LoadTableDataByMasterId<ExpenseDetailsOverviewModel>(FleetNames.ExpenseDetailsOverview, expense.Id, sqlDataAccessTransaction) : [];

		expense.Id = await InsertExpense(expense, sqlDataAccessTransaction);
		await SaveExpensesDetail(expense, expensesDetails, update, sqlDataAccessTransaction);
		await SaveAuditTrail(expense, update, recover, previousExpense, previousExpensesDetails, sqlDataAccessTransaction);

		return expense.Id;
	}

	private static async Task SaveExpensesDetail(ExpenseModel expense, List<ExpenseDetailsModel> expensesDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingExpensesDetails = await CommonData.LoadTableDataByMasterId<ExpenseDetailsModel>(FleetNames.ExpenseDetails, expense.Id, sqlDataAccessTransaction);
			foreach (var item in existingExpensesDetails)
			{
				item.Status = false;
				await InsertExpenseDetails(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in expensesDetails)
		{
			item.MasterId = expense.Id;
			await InsertExpenseDetails(item, sqlDataAccessTransaction);
		}
	}

	private static async Task SaveAuditTrail(
		ExpenseModel expense,
		bool update,
		bool recover,
		ExpenseOverviewModel previousExpense = null,
		List<ExpenseDetailsOverviewModel> previousExpensesDetails = null,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		string difference = null;

		if (update && !recover)
		{
			var currentExpense = await CommonData.LoadTableDataById<ExpenseOverviewModel>(FleetNames.ExpenseOverview, expense.Id, sqlDataAccessTransaction);
			var currentExpensesDetails = await CommonData.LoadTableDataByMasterId<ExpenseDetailsOverviewModel>(FleetNames.ExpenseDetailsOverview, expense.Id, sqlDataAccessTransaction);

			var expenseDiff = AuditTrailData.GetDifference(previousExpense, currentExpense);
			var detailsDiff = AuditTrailData.GetDifference(previousExpensesDetails, currentExpensesDetails, typeof(ExpenseOverviewModel));

			difference = AuditTrailData.CombineDifferences(
				(null, expenseDiff),
				("Details", detailsDiff));
		}

		await AuditTrailData.SaveAuditTrail(new()
		{
			Action = recover ? AuditTrailActionTypes.Recover.ToString() : update ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
			TableName = FleetNames.Expense,
			RecordNo = expense.TransactionNo,
			RecordValue = difference,
			CreatedBy = update ? expense.LastModifiedBy.Value : expense.CreatedBy,
			CreatedFromPlatform = update ? expense.LastModifiedFromPlatform : expense.CreatedFromPlatform
		}, sqlDataAccessTransaction);
	}
}
