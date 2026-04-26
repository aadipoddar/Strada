using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Mailing;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Exports.VehicleTrip.TripAdvance;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Operations;
using StradaLibrary.Models.VehicleTrip.TripAdvance;

namespace StradaLibrary.Data.VehicleTrip.TripAdvance;

public static class TripAdvanceData
{
	private static async Task<int> InsertTripAdvance(TripAdvanceModel tripAdvance, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(VehicleTripNames.InsertTripAdvance, tripAdvance, sqlDataAccessTransaction)).FirstOrDefault();

	private static async Task<int> InsertTripAdvanceExpenses(TripAdvanceExpensesModel tripAdvanceExpenses, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(VehicleTripNames.InsertTripAdvanceExpenses, tripAdvanceExpenses, sqlDataAccessTransaction)).FirstOrDefault();

	private static async Task<int> InsertTripAdvanceCardPayments(TripAdvanceCardPaymentsModel tripAdvanceCardPayments, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
			(await SqlDataAccess.LoadData<int, dynamic>(VehicleTripNames.InsertTripAdvanceCardPayments, tripAdvanceCardPayments, sqlDataAccessTransaction)).FirstOrDefault();

	public static List<TripAdvanceExpensesModel> ConvertExpensesCartToDetails(List<TripAdvanceExpensesCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new TripAdvanceExpensesModel
		{
			Id = 0,
			MasterId = masterId,
			VehicleExpenseTypeId = item.VehicleExpenseTypeId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	public static List<TripAdvanceCardPaymentsModel> ConvertPaymentCartToDetails(List<TripAdvanceCardPaymentsCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new TripAdvanceCardPaymentsModel
		{
			Id = 0,
			MasterId = masterId,
			OMCCardId = item.OMCCardId,
			Amount = item.Amount,	
			Remarks = item.Remarks,
			Status = true
		})];

	public static async Task DeleteTransaction(TripAdvanceModel tripAdvance, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (sqlDataAccessTransaction is null)
		{
			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				await DeleteTransaction(tripAdvance, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			await TripAdvanceNotify.Notify(tripAdvance.Id, NotifyType.Deleted);
		}

		try
		{
			await FinancialYearData.ValidateFinancialYear(tripAdvance.TransactionDateTime, sqlDataAccessTransaction);

			tripAdvance.Status = false;
			await InsertTripAdvance(tripAdvance, sqlDataAccessTransaction);
		}
		catch
		{
			sqlDataAccessTransaction.RollbackTransaction();
			throw;
		}
	}

	public static async Task RecoverTransaction(TripAdvanceModel tripAdvance)
	{
		tripAdvance.Status = true;
		var expensesDetails = await CommonData.LoadTableDataByMasterId<TripAdvanceExpensesModel>(VehicleTripNames.TripAdvanceExpenses, tripAdvance.Id);
		var paymentDetails = await CommonData.LoadTableDataByMasterId<TripAdvanceCardPaymentsModel>(VehicleTripNames.TripAdvanceCardPayments, tripAdvance.Id);
		await SaveTransaction(tripAdvance, expensesDetails, paymentDetails, false);

		await TripAdvanceNotify.Notify(tripAdvance.Id, NotifyType.Recovered);
	}

	private static async Task<TripAdvanceModel> ValidateTransaction(TripAdvanceModel tripAdvance, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (tripAdvance.CompanyId <= 0)
			throw new InvalidOperationException("Please select a company for the transaction.");

		if (tripAdvance.OMCId <= 0)
			throw new InvalidOperationException("Please select an OMC for the transaction.");

		if (tripAdvance.VehicleId <= 0)
			throw new InvalidOperationException("Please select a vehicle for the transaction.");

		if (tripAdvance.DriverId <= 0)
			throw new InvalidOperationException("Please select a driver for the transaction.");

		if (tripAdvance.RouteId <= 0)
			throw new InvalidOperationException("Please select a route for the transaction.");

		if (tripAdvance.Quantity < 0)
			throw new InvalidOperationException("Quantity cannot be negative.");

		if (tripAdvance.TotalExpense < 0)
			throw new InvalidOperationException("Total expense cannot be negative.");

		var vehicle = await CommonData.LoadTableDataById<VehicleModel>(FleetNames.Vehicle, tripAdvance.VehicleId, sqlDataAccessTransaction);
		if (vehicle.CompanyId != tripAdvance.CompanyId)
			throw new InvalidOperationException("Selected vehicle does not belong to the selected company.");

		tripAdvance.TransactionNo = await GenerateCodes.GenerateTripAdvanceTransactionNo(tripAdvance, sqlDataAccessTransaction);
		await FinancialYearData.ValidateFinancialYear(tripAdvance.TransactionDateTime, sqlDataAccessTransaction);

		if (update)
		{
			var existingTrip = await CommonData.LoadTableDataById<TripAdvanceModel>(VehicleTripNames.TripAdvance, tripAdvance.Id, sqlDataAccessTransaction)
				?? throw new InvalidOperationException("The trip advance transaction does not exist.");

			await FinancialYearData.ValidateFinancialYear(existingTrip.TransactionDateTime, sqlDataAccessTransaction);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, tripAdvance.LastModifiedBy.Value, sqlDataAccessTransaction);
			if (!user.Admin)
				throw new InvalidOperationException("Only admin users can update a vehicle trip transaction.");

			tripAdvance.TransactionNo = existingTrip.TransactionNo;
		}

		return tripAdvance;
	}

	private static void ValidateExpensesDetails(TripAdvanceModel tripAdvance, List<TripAdvanceExpensesModel> expensesDetails)
	{
		if (expensesDetails.Any(ed => ed.Amount <= 0))
			throw new InvalidOperationException("Expense amount must be greater than zero.");
		
		if (expensesDetails.Sum(ed => ed.Amount) != tripAdvance.TotalExpense)
			throw new InvalidOperationException("Total expense amount must be equal to total expense of the transaction.");
	}

	private static async Task ValidatePaymentDetails(TripAdvanceModel tripAdvance, List<TripAdvanceCardPaymentsModel> paymentDetails)
	{
		if (paymentDetails.Any(pd => pd.Amount <= 0))
			throw new InvalidOperationException("Payment amount must be greater than zero.");

		if (paymentDetails.Sum(pd => pd.Amount) != tripAdvance.TotalExpense)
			throw new InvalidOperationException("Total payment amount must be equal to total expense.");
	}

	public static async Task<int> SaveTransaction(
		TripAdvanceModel tripAdvance,
		List<TripAdvanceExpensesModel> expensesDetails,
		List<TripAdvanceCardPaymentsModel> paymentDetails,
		bool showNotification = true,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		bool update = tripAdvance.Id > 0;

		if (sqlDataAccessTransaction is null)
		{
			(MemoryStream, string)? previousInvoice = null;
			if (update)
				previousInvoice = await TripAdvanceInvoiceExport.ExportInvoice(tripAdvance.Id, InvoiceExportType.PDF);

			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				tripAdvance.Id = await SaveTransaction(tripAdvance, expensesDetails, paymentDetails, showNotification, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			if (showNotification)
				await TripAdvanceNotify.Notify(tripAdvance.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

			return tripAdvance.Id;
		}

		tripAdvance = await ValidateTransaction(tripAdvance, update, sqlDataAccessTransaction);
		ValidateExpensesDetails(tripAdvance, expensesDetails);
		await ValidatePaymentDetails(tripAdvance, paymentDetails);
		tripAdvance.Id = await InsertTripAdvance(tripAdvance, sqlDataAccessTransaction);
		await SaveExpensesDetail(tripAdvance, expensesDetails, update, sqlDataAccessTransaction);
		await SavePaymentDetail(tripAdvance, paymentDetails, update, sqlDataAccessTransaction);

		return tripAdvance.Id;
	}

	private static async Task SaveExpensesDetail(TripAdvanceModel tripAdvance, List<TripAdvanceExpensesModel> expensesDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingExpensesDetails = await CommonData.LoadTableDataByMasterId<TripAdvanceExpensesModel>(VehicleTripNames.TripAdvanceExpenses, tripAdvance.Id, sqlDataAccessTransaction);
			foreach (var item in existingExpensesDetails)
			{
				item.Status = false;
				await InsertTripAdvanceExpenses(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in expensesDetails)
		{
			item.MasterId = tripAdvance.Id;
			var id = await InsertTripAdvanceExpenses(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save trip advance expenses detail item.");
		}
	}

	private static async Task SavePaymentDetail(TripAdvanceModel tripAdvance, List<TripAdvanceCardPaymentsModel> paymentDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingPaymentDetails = await CommonData.LoadTableDataByMasterId<TripAdvanceCardPaymentsModel>(VehicleTripNames.TripAdvanceCardPayments, tripAdvance.Id, sqlDataAccessTransaction);
			foreach (var item in existingPaymentDetails)
			{
				item.Status = false;
				await InsertTripAdvanceCardPayments(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in paymentDetails)
		{
			item.MasterId = tripAdvance.Id;
			var id = await InsertTripAdvanceCardPayments(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save trip advance card payments detail item.");
		}
	}
}