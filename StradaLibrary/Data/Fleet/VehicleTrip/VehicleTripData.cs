using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Fleet.VehicleTrip;
using StradaLibrary.Exports.Mailing;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.OMC;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Fleet.VehicleTrip;
using StradaLibrary.Models.Operations;

namespace StradaLibrary.Data.Fleet.VehicleTrip;

public static class VehicleTripData
{
	private static async Task<int> InsertVehicleTrip(VehicleTripModel vehicleTrip, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleTrip, vehicleTrip, sqlDataAccessTransaction)).FirstOrDefault();

	private static async Task<int> InsertVehicleTripExpenses(VehicleTripExpensesModel vehicleTripExpenses, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleTripExpenses, vehicleTripExpenses, sqlDataAccessTransaction)).FirstOrDefault();
	private static async Task<int> InsertVehicleTripOMCCardPayments(VehicleTripOMCCardPaymentsModel vehicleTripOMCCardPayments, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
			(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleTripOMCCardPayments, vehicleTripOMCCardPayments, sqlDataAccessTransaction)).FirstOrDefault();

	public static List<VehicleTripExpensesModel> ConvertExpensesCartToDetails(List<VehicleTripExpensesCartModel> cart, int accountingId = 0) =>
		[.. cart.Select(item => new VehicleTripExpensesModel
		{
			Id = 0,
			MasterId = accountingId,
			VehicleExpenseTypeId = item.VehicleExpenseTypeId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	public static List<VehicleTripOMCCardPaymentsModel> ConvertPaymentCartToDetails(List<VehicleTripOMCCardPaymentsCartModel> cart, int accountingId = 0) =>
		[.. cart.Select(item => new VehicleTripOMCCardPaymentsModel
		{
			Id = 0,
			MasterId = accountingId,
			OMCCardId = item.OMCCardId,
			Amount = item.Amount,	
			Remarks = item.Remarks,
			Status = true
		})];

	public static async Task DeleteTransaction(VehicleTripModel trip, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (sqlDataAccessTransaction is null)
		{
			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				await DeleteTransaction(trip, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			await VehicleTripNotify.Notify(trip.Id, NotifyType.Deleted);
		}

		try
		{
			await FinancialYearData.ValidateFinancialYear(trip.TransactionDateTime, sqlDataAccessTransaction);

			trip.Status = false;
			await InsertVehicleTrip(trip, sqlDataAccessTransaction);
		}
		catch
		{
			sqlDataAccessTransaction.RollbackTransaction();
			throw;
		}
	}

	public static async Task RecoverTransaction(VehicleTripModel trip)
	{
		trip.Status = true;
		var expensesDetails = await CommonData.LoadTableDataByMasterId<VehicleTripExpensesModel>(FleetNames.VehicleTripExpenses, trip.Id);
		var paymentDetails = await CommonData.LoadTableDataByMasterId<VehicleTripOMCCardPaymentsModel>(FleetNames.VehicleTripOMCCardPayments, trip.Id);

		await SaveTransaction(trip, expensesDetails, paymentDetails, false);

		await VehicleTripNotify.Notify(trip.Id, NotifyType.Recovered);
	}

	private static async Task<VehicleTripModel> ValidateTransaction(VehicleTripModel trip, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (trip.CompanyId <= 0)
			throw new InvalidOperationException("Please select a company for the transaction.");

		if (trip.OMCId <= 0)
			throw new InvalidOperationException("Please select an OMC for the transaction.");

		if (trip.VehicleId <= 0)
			throw new InvalidOperationException("Please select a vehicle for the transaction.");

		if (trip.DriverId <= 0)
			throw new InvalidOperationException("Please select a driver for the transaction.");

		if (trip.RouteId <= 0)
			throw new InvalidOperationException("Please select a route for the transaction.");

		if (trip.Quantity < 0)
			throw new InvalidOperationException("Quantity cannot be negative.");

		if (string.IsNullOrWhiteSpace(trip.ChallanNo))
			throw new InvalidOperationException("Please enter challan number for the transaction.");

		if (trip.TotalExpense < 0)
			throw new InvalidOperationException("Total expense cannot be negative.");

		var vehicle = await CommonData.LoadTableDataById<VehicleModel>(FleetNames.Vehicle, trip.VehicleId, sqlDataAccessTransaction);
		if (vehicle.CompanyId != trip.CompanyId)
			throw new InvalidOperationException("Selected vehicle does not belong to the selected company.");

		trip.TransactionNo = await GenerateCodes.GenerateVehicleTripTransactionNo(trip, sqlDataAccessTransaction);
		await FinancialYearData.ValidateFinancialYear(trip.TransactionDateTime, sqlDataAccessTransaction);

		if (update)
		{
			var existingTrip = await CommonData.LoadTableDataById<VehicleTripModel>(FleetNames.VehicleTrip, trip.Id, sqlDataAccessTransaction)
				?? throw new InvalidOperationException("The vehicle trip transaction does not exist.");

			await FinancialYearData.ValidateFinancialYear(existingTrip.TransactionDateTime, sqlDataAccessTransaction);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, trip.LastModifiedBy.Value, sqlDataAccessTransaction);
			if (!user.Admin)
				throw new InvalidOperationException("Only admin users can update a vehicle trip transaction.");

			trip.TransactionNo = existingTrip.TransactionNo;
		}

		return trip;
	}

	private static void ValidateExpensesDetails(VehicleTripModel trip, List<VehicleTripExpensesModel> expensesDetails)
	{
		if (expensesDetails.Any(ed => ed.Amount <= 0))
			throw new InvalidOperationException("Expense amount must be greater than zero.");
		
		if (expensesDetails.Sum(ed => ed.Amount) != trip.TotalExpense)
			throw new InvalidOperationException("Total expense amount must be equal to total expense of the transaction.");
	}

	private static async Task ValidatePaymentDetails(VehicleTripModel trip, List<VehicleTripOMCCardPaymentsModel> paymentDetails)
	{
		if (paymentDetails.Any(pd => pd.Amount <= 0))
			throw new InvalidOperationException("Payment amount must be greater than zero.");

		foreach (var payment in paymentDetails)
		{
			var omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, payment.OMCCardId);
			if (omcCard.OMCId != trip.OMCId)
				throw new InvalidOperationException("Selected OMC card does not belong to the selected OMC.");
		}

		if (paymentDetails.Sum(pd => pd.Amount) != trip.TotalExpense)
			throw new InvalidOperationException("Total payment amount must be equal to total expense.");
	}

	public static async Task<int> SaveTransaction(
		VehicleTripModel trip,
		List<VehicleTripExpensesModel> expensesDetails,
		List<VehicleTripOMCCardPaymentsModel> paymentDetails,
		bool showNotification = true,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		bool update = trip.Id > 0;

		if (sqlDataAccessTransaction is null)
		{
			(MemoryStream, string)? previousInvoice = null;
			if (update)
				previousInvoice = await VehicleTripInvoiceExport.ExportInvoice(trip.Id, InvoiceExportType.PDF);

			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				trip.Id = await SaveTransaction(trip, expensesDetails, paymentDetails, showNotification, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			if (showNotification)
				await VehicleTripNotify.Notify(trip.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

			return trip.Id;
		}

		trip = await ValidateTransaction(trip, update, sqlDataAccessTransaction);
		ValidateExpensesDetails(trip, expensesDetails);
		await ValidatePaymentDetails(trip, paymentDetails);
		trip.Id = await InsertVehicleTrip(trip, sqlDataAccessTransaction);
		await SaveExpensesDetail(trip, expensesDetails, update, sqlDataAccessTransaction);
		await SavePaymentDetail(trip, paymentDetails, update, sqlDataAccessTransaction);

		return trip.Id;
	}

	private static async Task SaveExpensesDetail(VehicleTripModel trip, List<VehicleTripExpensesModel> expensesDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingExpensesDetails = await CommonData.LoadTableDataByMasterId<VehicleTripExpensesModel>(FleetNames.VehicleTripExpenses, trip.Id, sqlDataAccessTransaction);
			foreach (var item in existingExpensesDetails)
			{
				item.Status = false;
				await InsertVehicleTripExpenses(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in expensesDetails)
		{
			item.MasterId = trip.Id;
			var id = await InsertVehicleTripExpenses(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save vehicle trip expenses detail item.");
		}
	}

	private static async Task SavePaymentDetail(VehicleTripModel trip, List<VehicleTripOMCCardPaymentsModel> paymentDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingPaymentDetails = await CommonData.LoadTableDataByMasterId<VehicleTripOMCCardPaymentsModel>(FleetNames.VehicleTripOMCCardPayments, trip.Id, sqlDataAccessTransaction);
			foreach (var item in existingPaymentDetails)
			{
				item.Status = false;
				await InsertVehicleTripOMCCardPayments(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in paymentDetails)
		{
			item.MasterId = trip.Id;
			var id = await InsertVehicleTripOMCCardPayments(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save vehicle trip OMC card payments detail item.");
		}
	}
}