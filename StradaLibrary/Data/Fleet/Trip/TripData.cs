using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Fleet.Trip;
using StradaLibrary.Exports.Mailing;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.Trip;
using StradaLibrary.Models.Fleet.Vehicle;
using StradaLibrary.Models.Operations;

namespace StradaLibrary.Data.Fleet.Trip;

public static class TripData
{
	internal static async Task<int> InsertTrip(TripModel trip, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertTrip, trip, sqlDataAccessTransaction)).FirstOrDefault();

	private static async Task<int> InsertTripExpenses(TripExpensesModel tripExpenses, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertTripExpenses, tripExpenses, sqlDataAccessTransaction)).FirstOrDefault();

	private static async Task<int> InsertTripCardPayments(TripCardPaymentsModel tripCardPayments, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertTripCardPayments, tripCardPayments, sqlDataAccessTransaction)).FirstOrDefault();

	private static async Task<int> InsertTripLedgerPayments(TripLedgerPaymentsModel tripLedgerPayments, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertTripLedgerPayments, tripLedgerPayments, sqlDataAccessTransaction)).FirstOrDefault();

	public static async Task<List<TripOverviewModel>> LoadTripOverviewByBillIdDate(int? BillId = null, DateTime? StartDate = null, DateTime? EndDate = null, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		await SqlDataAccess.LoadData<TripOverviewModel, dynamic>(FleetNames.LoadTripOverviewByBillIdDate, new { BillId, StartDate, EndDate }, sqlDataAccessTransaction);

	public static List<TripExpensesModel> ConvertExpensesCartToDetails(List<TripExpensesCartModel> cart, int accountingId = 0) =>
		[.. cart.Select(item => new TripExpensesModel
		{
			Id = 0,
			MasterId = accountingId,
			ExpenseTypeId = item.ExpenseTypeId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	public static List<TripCardPaymentsModel> ConvertCardPaymentCartToDetails(List<TripCardPaymentsCartModel> cart, int accountingId = 0) =>
		[.. cart.Select(item => new TripCardPaymentsModel
		{
			Id = 0,
			MasterId = accountingId,
			OMCCardId = item.OMCCardId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	public static List<TripLedgerPaymentsModel> ConvertLedgerPaymentCartToDetails(List<TripLedgerPaymentsCartModel> cart, int accountingId = 0) =>
		[.. cart.Select(item => new TripLedgerPaymentsModel
		{
			Id = 0,
			MasterId = accountingId,
			LedgerId = item.LedgerId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	public static async Task DeleteTransaction(TripModel trip, SqlDataAccessTransaction sqlDataAccessTransaction = null)
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

			await TripNotify.Notify(trip.Id, NotifyType.Deleted);
		}

		try
		{
			await FinancialYearData.ValidateFinancialYear(trip.TransactionDateTime, sqlDataAccessTransaction);

			if (trip.BillId is not null)
				throw new InvalidOperationException("Cannot delete a trip transaction that is associated with a bill.");

			trip.Status = false;
			var id = await InsertTrip(trip, sqlDataAccessTransaction);
			if (id <= 0)
				throw new InvalidOperationException("Failed to delete trip transaction.");
		}
		catch
		{
			sqlDataAccessTransaction.RollbackTransaction();
			throw;
		}
	}

	public static async Task RecoverTransaction(TripModel trip)
	{
		trip.Status = true;
		var expensesDetails = await CommonData.LoadTableDataByMasterId<TripExpensesModel>(FleetNames.TripExpenses, trip.Id);
		var cardPaymentDetails = await CommonData.LoadTableDataByMasterId<TripCardPaymentsModel>(FleetNames.TripCardPayments, trip.Id);
		var ledgerPaymentDetails = await CommonData.LoadTableDataByMasterId<TripLedgerPaymentsModel>(FleetNames.TripLedgerPayments, trip.Id);
		await SaveTransaction(trip, expensesDetails, cardPaymentDetails, ledgerPaymentDetails, false);

		await TripNotify.Notify(trip.Id, NotifyType.Recovered);
	}

	#region Save
	private static async Task<TripModel> ValidateTransaction(TripModel trip, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		trip.SlNo = string.IsNullOrWhiteSpace(trip.SlNo) ? null : trip.SlNo.Trim();
		trip.ChallanNo = string.IsNullOrWhiteSpace(trip.ChallanNo) ? null : trip.ChallanNo.Trim();
		trip.Remarks = string.IsNullOrWhiteSpace(trip.Remarks) ? null : trip.Remarks.Trim();

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

		if (trip.TotalExpense < 0)
			throw new InvalidOperationException("Total expense cannot be negative.");

		if (trip.TotalCardPaymentAmount + trip.TotalLedgerPaymentAmount != trip.TotalExpense)
			throw new InvalidOperationException("Sum of card and ledger payments must equal total net amount.");

		if (trip.BillId is not null)
			throw new InvalidOperationException("Cannot edit a trip transaction that is associated with a bill.");

		trip.TransactionNo = await GenerateCodes.GenerateTripTransactionNo(trip, sqlDataAccessTransaction);
		await FinancialYearData.ValidateFinancialYear(trip.TransactionDateTime, sqlDataAccessTransaction);

		if (update)
		{
			var existingTrip = await CommonData.LoadTableDataById<TripModel>(FleetNames.Trip, trip.Id, sqlDataAccessTransaction)
				?? throw new InvalidOperationException("The trip transaction does not exist.");

			await FinancialYearData.ValidateFinancialYear(existingTrip.TransactionDateTime, sqlDataAccessTransaction);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, trip.LastModifiedBy.Value, sqlDataAccessTransaction);
			if (!user.Admin)
				throw new InvalidOperationException("Only admin users can update a trip transaction.");

			if (trip.BillId is not null)
				throw new InvalidOperationException("Cannot edit a trip transaction that is associated with a bill.");

			trip.TransactionNo = existingTrip.TransactionNo;
		}

		return trip;
	}

	private static void ValidateExpensesDetails(TripModel trip, List<TripExpensesModel> expensesDetails)
	{
		if (expensesDetails is null || expensesDetails.Count == 0)
			throw new InvalidOperationException("Please add at least one expense detail for the transaction.");

		if (expensesDetails.Any(ed => ed.Amount <= 0))
			throw new InvalidOperationException("Expense amount must be greater than zero.");

		if (expensesDetails.Sum(ed => ed.Amount) != trip.TotalExpense)
			throw new InvalidOperationException("Total expense amount must be equal to total expense of the transaction.");

		foreach (var item in expensesDetails)
			item.Remarks = string.IsNullOrWhiteSpace(item.Remarks) ? null : item.Remarks.Trim();
	}

	private static async Task ValidateCardPaymentDetails(TripModel trip, List<TripCardPaymentsModel> paymentDetails)
	{
		if (paymentDetails.Any(pd => pd.Amount <= 0))
			throw new InvalidOperationException("Payment amount must be greater than zero.");

		if (paymentDetails.Sum(pd => pd.Amount) != trip.TotalCardPaymentAmount)
			throw new InvalidOperationException("Total card payment amount must be equal to trip's total card payment amount.");

		foreach (var item in paymentDetails)
			item.Remarks = string.IsNullOrWhiteSpace(item.Remarks) ? null : item.Remarks.Trim();
	}

	private static async Task ValidateLedgerPaymentDetails(TripModel trip, List<TripLedgerPaymentsModel> paymentDetails)
	{
		if (paymentDetails.Any(pd => pd.Amount <= 0))
			throw new InvalidOperationException("Payment amount must be greater than zero.");

		if (paymentDetails.Sum(pd => pd.Amount) != trip.TotalLedgerPaymentAmount)
			throw new InvalidOperationException("Total ledger payment amount must be equal to trip's total ledger payment amount.");

		foreach (var item in paymentDetails)
			item.Remarks = string.IsNullOrWhiteSpace(item.Remarks) ? null : item.Remarks.Trim();
	}

	public static async Task<int> SaveTransaction(
		TripModel trip,
		List<TripExpensesModel> expensesDetails,
		List<TripCardPaymentsModel> cardPaymentDetails,
		List<TripLedgerPaymentsModel> ledgerPaymentDetails,
		bool showNotification = true,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		bool update = trip.Id > 0;

		if (sqlDataAccessTransaction is null)
		{
			(MemoryStream, string)? previousInvoice = null;
			if (update)
				previousInvoice = await TripInvoiceExport.ExportInvoice(trip.Id, InvoiceExportType.PDF);

			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				trip.Id = await SaveTransaction(trip, expensesDetails, cardPaymentDetails, ledgerPaymentDetails, showNotification, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			if (showNotification)
				await TripNotify.Notify(trip.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

			return trip.Id;
		}

		trip = await ValidateTransaction(trip, update, sqlDataAccessTransaction);
		ValidateExpensesDetails(trip, expensesDetails);
		await ValidateCardPaymentDetails(trip, cardPaymentDetails);
		await ValidateLedgerPaymentDetails(trip, ledgerPaymentDetails);
		trip.Id = await InsertTrip(trip, sqlDataAccessTransaction);
		await SaveExpensesDetail(trip, expensesDetails, update, sqlDataAccessTransaction);
		await SaveCardPaymentDetail(trip, cardPaymentDetails, update, sqlDataAccessTransaction);
		await SaveLedgerPaymentDetail(trip, ledgerPaymentDetails, update, sqlDataAccessTransaction);

		return trip.Id;
	}

	private static async Task SaveExpensesDetail(TripModel trip, List<TripExpensesModel> expensesDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingExpensesDetails = await CommonData.LoadTableDataByMasterId<TripExpensesModel>(FleetNames.TripExpenses, trip.Id, sqlDataAccessTransaction);
			foreach (var item in existingExpensesDetails)
			{
				item.Status = false;
				var id = await InsertTripExpenses(item, sqlDataAccessTransaction);

				if (id <= 0)
					throw new InvalidOperationException("Failed to save trip expenses detail item.");
			}
		}

		foreach (var item in expensesDetails)
		{
			item.MasterId = trip.Id;
			var id = await InsertTripExpenses(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save trip expenses detail item.");
		}
	}

	private static async Task SaveCardPaymentDetail(TripModel trip, List<TripCardPaymentsModel> paymentDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingPaymentDetails = await CommonData.LoadTableDataByMasterId<TripCardPaymentsModel>(FleetNames.TripCardPayments, trip.Id, sqlDataAccessTransaction);
			foreach (var item in existingPaymentDetails)
			{
				item.Status = false;
				var id = await InsertTripCardPayments(item, sqlDataAccessTransaction);

				if (id <= 0)
					throw new InvalidOperationException("Failed to save trip OMC card payments detail item.");
			}
		}

		foreach (var item in paymentDetails)
		{
			item.MasterId = trip.Id;
			var id = await InsertTripCardPayments(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save trip OMC card payments detail item.");
		}
	}

	private static async Task SaveLedgerPaymentDetail(TripModel trip, List<TripLedgerPaymentsModel> paymentDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingPaymentDetails = await CommonData.LoadTableDataByMasterId<TripLedgerPaymentsModel>(FleetNames.TripLedgerPayments, trip.Id, sqlDataAccessTransaction);
			foreach (var item in existingPaymentDetails)
			{
				item.Status = false;
				var id = await InsertTripLedgerPayments(item, sqlDataAccessTransaction);

				if (id <= 0)
					throw new InvalidOperationException("Failed to save trip ledger payments detail item.");
			}
		}

		foreach (var item in paymentDetails)
		{
			item.MasterId = trip.Id;
			var id = await InsertTripLedgerPayments(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save trip ledger payments detail item.");
		}
	}
	#endregion
}