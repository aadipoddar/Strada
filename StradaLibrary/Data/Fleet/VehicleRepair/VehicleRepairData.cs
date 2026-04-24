using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Fleet.VehicleRepair;
using StradaLibrary.Exports.Mailing;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleRepair;
using StradaLibrary.Models.Operations;

namespace StradaLibrary.Data.Fleet.VehicleRepair;

public static class VehicleRepairData
{
	private static async Task<int> InsertVehicleRepair(VehicleRepairModel vehicleRepair, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleRepair, vehicleRepair, sqlDataAccessTransaction)).FirstOrDefault();

	private static async Task<int> InsertVehicleRepairExpenses(VehicleRepairExpensesModel vehicleRepairExpenses, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleRepairExpenses, vehicleRepairExpenses, sqlDataAccessTransaction)).FirstOrDefault();

	public static List<VehicleRepairExpensesModel> ConvertExpensesCartToDetails(List<VehicleRepairExpensesCartModel> cart, int accountingId = 0) =>
		[.. cart.Select(item => new VehicleRepairExpensesModel
		{
			Id = 0,
			MasterId = accountingId,
			VehicleExpenseTypeId = item.VehicleExpenseTypeId,
			Amount = item.Amount,
			IdentificationNo = item.IdentificationNo,
			Remarks = item.Remarks,
			Status = true
		})];

	public static async Task DeleteTransaction(VehicleRepairModel repair, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (sqlDataAccessTransaction is null)
		{
			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				await DeleteTransaction(repair, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			await VehicleRepairNotify.Notify(repair.Id, NotifyType.Deleted);
		}

		try
		{
			await FinancialYearData.ValidateFinancialYear(repair.TransactionDateTime, sqlDataAccessTransaction);

			repair.Status = false;
			await InsertVehicleRepair(repair, sqlDataAccessTransaction);
		}
		catch
		{
			sqlDataAccessTransaction.RollbackTransaction();
			throw;
		}
	}

	public static async Task RecoverTransaction(VehicleRepairModel repair)
	{
		repair.Status = true;
		var expensesDetails = await CommonData.LoadTableDataByMasterId<VehicleRepairExpensesModel>(FleetNames.VehicleRepairExpenses, repair.Id);
		
		await SaveTransaction(repair, expensesDetails);

		await VehicleRepairNotify.Notify(repair.Id, NotifyType.Recovered);
	}

	private static async Task<VehicleRepairModel> ValidateTransaction(VehicleRepairModel repair, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (repair.CompanyId <= 0)
			throw new InvalidOperationException("Please select a company for the transaction.");

		if (repair.VehicleId <= 0)
			throw new InvalidOperationException("Please select a vehicle for the transaction.");

		if (repair.TotalExpense < 0)
			throw new InvalidOperationException("Total expense cannot be negative.");

		var vehicle = await CommonData.LoadTableDataById<VehicleModel>(FleetNames.Vehicle, repair.VehicleId, sqlDataAccessTransaction);
		if (vehicle.CompanyId != repair.CompanyId)
			throw new InvalidOperationException("Selected vehicle does not belong to the selected company.");

		repair.TransactionNo = await GenerateCodes.GenerateVehicleRepairTransactionNo(repair, sqlDataAccessTransaction);
		await FinancialYearData.ValidateFinancialYear(repair.TransactionDateTime, sqlDataAccessTransaction);

		if (update)
		{
			var existingRepair = await CommonData.LoadTableDataById<VehicleRepairModel>(FleetNames.VehicleRepair, repair.Id, sqlDataAccessTransaction)
				?? throw new InvalidOperationException("The vehicle repair transaction does not exist.");

			await FinancialYearData.ValidateFinancialYear(existingRepair.TransactionDateTime, sqlDataAccessTransaction);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, repair.LastModifiedBy.Value, sqlDataAccessTransaction);
			if (!user.Admin)
				throw new InvalidOperationException("Only admin users can update a vehicle repair transaction.");

			repair.TransactionNo = existingRepair.TransactionNo;
		}

		return repair;
	}

	private static void ValidateExpensesDetails(VehicleRepairModel repair, List<VehicleRepairExpensesModel> expensesDetails)
	{
		if (expensesDetails.Any(ed => ed.Amount <= 0))
			throw new InvalidOperationException("Expense amount must be greater than zero.");
		
		if (expensesDetails.Sum(ed => ed.Amount) != repair.TotalExpense)
			throw new InvalidOperationException("Total expense amount must be equal to total expense of the transaction.");
	}

	public static async Task<int> SaveTransaction(
		VehicleRepairModel repair,
		List<VehicleRepairExpensesModel> expensesDetails,
		bool showNotification = true,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		bool update = repair.Id > 0;

		if (sqlDataAccessTransaction is null)
		{
			(MemoryStream, string)? previousInvoice = null;
			if (update)
				previousInvoice = await VehicleRepairInvoiceExport.ExportInvoice(repair.Id, InvoiceExportType.PDF);

			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				repair.Id = await SaveTransaction(repair, expensesDetails, showNotification, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			if (showNotification)
				await VehicleRepairNotify.Notify(repair.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

			return repair.Id;
		}

		repair = await ValidateTransaction(repair, update, sqlDataAccessTransaction);
		ValidateExpensesDetails(repair, expensesDetails);
		repair.Id = await InsertVehicleRepair(repair, sqlDataAccessTransaction);
		await SaveExpensesDetail(repair, expensesDetails, update, sqlDataAccessTransaction);

		return repair.Id;
	}

	private static async Task SaveExpensesDetail(VehicleRepairModel repair, List<VehicleRepairExpensesModel> expensesDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingExpensesDetails = await CommonData.LoadTableDataByMasterId<VehicleRepairExpensesModel>(FleetNames.VehicleRepairExpenses, repair.Id, sqlDataAccessTransaction);
			foreach (var item in existingExpensesDetails)
			{
				item.Status = false;
				await InsertVehicleRepairExpenses(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in expensesDetails)
		{
			item.MasterId = repair.Id;
			var id = await InsertVehicleRepairExpenses(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save vehicle repair expenses detail item.");
		}
	}
}