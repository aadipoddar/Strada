using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Fleet.VehicleExpense;
using StradaLibrary.Exports.Mailing;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleExpense;
using StradaLibrary.Models.Operations;

namespace StradaLibrary.Data.Fleet.VehicleExpense;

public static class VehicleExpenseData
{
	private static async Task<int> InsertVehicleExpense(VehicleExpenseModel vehicleExpense, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleExpense, vehicleExpense, sqlDataAccessTransaction)).FirstOrDefault();

	private static async Task<int> InsertVehicleExpenseDetails(VehicleExpenseDetailsModel vehicleExpenseDetails, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleExpenseDetails, vehicleExpenseDetails, sqlDataAccessTransaction)).FirstOrDefault();

	public static List<VehicleExpenseDetailsModel> ConvertExpensesCartToDetails(List<VehicleExpenseDetailsCartModel> cart, int accountingId = 0) =>
		[.. cart.Select(item => new VehicleExpenseDetailsModel
		{
			Id = 0,
			MasterId = accountingId,
			VehicleExpenseTypeId = item.VehicleExpenseTypeId,
			Amount = item.Amount,
			IdentificationNo = item.IdentificationNo,
			Remarks = item.Remarks,
			Status = true
		})];

	public static async Task DeleteTransaction(VehicleExpenseModel expense, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (sqlDataAccessTransaction is null)
		{
			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				await DeleteTransaction(expense, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			await VehicleExpenseNotify.Notify(expense.Id, NotifyType.Deleted);
		}

		try
		{
			await FinancialYearData.ValidateFinancialYear(expense.TransactionDateTime, sqlDataAccessTransaction);

			expense.Status = false;
			await InsertVehicleExpense(expense, sqlDataAccessTransaction);
		}
		catch
		{
			sqlDataAccessTransaction.RollbackTransaction();
			throw;
		}
	}

	public static async Task RecoverTransaction(VehicleExpenseModel expense)
	{
		expense.Status = true;
		var expensesDetails = await CommonData.LoadTableDataByMasterId<VehicleExpenseDetailsModel>(FleetNames.VehicleExpenseDetails, expense.Id);

		await SaveTransaction(expense, expensesDetails);

		await VehicleExpenseNotify.Notify(expense.Id, NotifyType.Recovered);
	}

	private static async Task<VehicleExpenseModel> ValidateTransaction(VehicleExpenseModel expense, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (expense.CompanyId <= 0)
			throw new InvalidOperationException("Please select a company for the transaction.");

		if (expense.VehicleId <= 0)
			throw new InvalidOperationException("Please select a vehicle for the transaction.");

		if (expense.TotalExpense < 0)
			throw new InvalidOperationException("Total expense cannot be negative.");

		var vehicle = await CommonData.LoadTableDataById<VehicleModel>(FleetNames.Vehicle, expense.VehicleId, sqlDataAccessTransaction);
		if (vehicle.CompanyId != expense.CompanyId)
			throw new InvalidOperationException("Selected vehicle does not belong to the selected company.");

		expense.TransactionNo = await GenerateCodes.GenerateVehicleExpenseTransactionNo(expense, sqlDataAccessTransaction);
		await FinancialYearData.ValidateFinancialYear(expense.TransactionDateTime, sqlDataAccessTransaction);

		if (update)
		{
			var existingExpense = await CommonData.LoadTableDataById<VehicleExpenseModel>(FleetNames.VehicleExpense, expense.Id, sqlDataAccessTransaction)
				?? throw new InvalidOperationException("The vehicle expense transaction does not exist.");

			await FinancialYearData.ValidateFinancialYear(existingExpense.TransactionDateTime, sqlDataAccessTransaction);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, expense.LastModifiedBy.Value, sqlDataAccessTransaction);
			if (!user.Admin)
				throw new InvalidOperationException("Only admin users can update a vehicle expense transaction.");

			expense.TransactionNo = existingExpense.TransactionNo;
		}

		return expense;
	}

	private static void ValidateExpensesDetails(VehicleExpenseModel expense, List<VehicleExpenseDetailsModel> expensesDetails)
	{
		if (expensesDetails.Any(ed => ed.Amount <= 0))
			throw new InvalidOperationException("Expense amount must be greater than zero.");

		if (expensesDetails.Sum(ed => ed.Amount) != expense.TotalExpense)
			throw new InvalidOperationException("Total expense amount must be equal to total expense of the transaction.");
	}

	public static async Task<int> SaveTransaction(
		VehicleExpenseModel expense,
		List<VehicleExpenseDetailsModel> expensesDetails,
		bool showNotification = true,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		bool update = expense.Id > 0;

		if (sqlDataAccessTransaction is null)
		{
			(MemoryStream, string)? previousInvoice = null;
			if (update)
				previousInvoice = await VehicleExpenseInvoiceExport.ExportInvoice(expense.Id, InvoiceExportType.PDF);

			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				expense.Id = await SaveTransaction(expense, expensesDetails, showNotification, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			if (showNotification)
				await VehicleExpenseNotify.Notify(expense.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

			return expense.Id;
		}

		expense = await ValidateTransaction(expense, update, sqlDataAccessTransaction);
		ValidateExpensesDetails(expense, expensesDetails);
		expense.Id = await InsertVehicleExpense(expense, sqlDataAccessTransaction);
		await SaveExpensesDetail(expense, expensesDetails, update, sqlDataAccessTransaction);

		return expense.Id;
	}

	private static async Task SaveExpensesDetail(VehicleExpenseModel expense, List<VehicleExpenseDetailsModel> expensesDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingExpensesDetails = await CommonData.LoadTableDataByMasterId<VehicleExpenseDetailsModel>(FleetNames.VehicleExpenseDetails, expense.Id, sqlDataAccessTransaction);
			foreach (var item in existingExpensesDetails)
			{
				item.Status = false;
				await InsertVehicleExpenseDetails(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in expensesDetails)
		{
			item.MasterId = expense.Id;
			var id = await InsertVehicleExpenseDetails(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save vehicle expense detail item.");
		}
	}
}
