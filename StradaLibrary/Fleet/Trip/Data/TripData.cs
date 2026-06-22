using StradaLibrary.Accounts.Masters.Data;
using StradaLibrary.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Fleet.OMC.Data;
using StradaLibrary.Fleet.OMC.Models;
using StradaLibrary.Fleet.Trip.Exports;
using StradaLibrary.Fleet.Trip.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Operations.Models;
using StradaLibrary.Utils.ExportUtils;
using StradaLibrary.Utils.MailUtils;

namespace StradaLibrary.Fleet.Trip.Data;

public static class TripData
{
	internal static async Task<int> InsertTrip(TripModel trip, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertTrip, trip, sqlDataAccessTransaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Trip.");

	private static async Task<int> InsertTripExpenses(TripExpensesModel tripExpenses, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertTripExpenses, tripExpenses, sqlDataAccessTransaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Trip Expense.");

	private static async Task<int> InsertTripCardPayments(TripCardPaymentsModel tripCardPayments, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertTripCardPayments, tripCardPayments, sqlDataAccessTransaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Trip Card Payment.");

	private static async Task<int> InsertTripLedgerPayments(TripLedgerPaymentsModel tripLedgerPayments, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertTripLedgerPayments, tripLedgerPayments, sqlDataAccessTransaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Trip Ledger Payment.");

	public static async Task<List<TripOverviewModel>> LoadTripOverviewByBillIdDate(int? BillId = null, DateTime? StartDate = null, DateTime? EndDate = null, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		await SqlDataAccess.LoadData<TripOverviewModel, dynamic>(FleetNames.LoadTripOverviewByBillIdDate, new { BillId, StartDate, EndDate }, sqlDataAccessTransaction);

	public static async Task<TripOverviewModel> LoadTripBySlNoFinancialYear(string SlNo, int FinancialYearId, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<TripOverviewModel, dynamic>(FleetNames.LoadTripBySlNoFinancialYear, new { SlNo, FinancialYearId }, sqlDataAccessTransaction)).FirstOrDefault();

	public static List<TripExpensesModel> ConvertExpensesCartToDetails(List<TripExpensesCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new TripExpensesModel
		{
			Id = 0,
			MasterId = masterId,
			ExpenseTypeId = item.ExpenseTypeId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	public static List<TripCardPaymentsModel> ConvertCardPaymentCartToDetails(List<TripCardPaymentsCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new TripCardPaymentsModel
		{
			Id = 0,
			MasterId = masterId,
			OMCCardId = item.OMCCardId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	public static List<TripLedgerPaymentsModel> ConvertLedgerPaymentCartToDetails(List<TripLedgerPaymentsCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new TripLedgerPaymentsModel
		{
			Id = 0,
			MasterId = masterId,
			LedgerId = item.LedgerId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	#region Delete
	public static async Task DeleteTransaction(TripModel trip, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (sqlDataAccessTransaction is null)
		{
			await SqlDataAccessTransaction.Run(transaction => DeleteTransaction(trip, transaction));
			await TripNotify.Notify(trip.Id, NotifyType.Deleted);
			return;
		}

		await FinancialYearData.ValidateFinancialYear(trip.TransactionDateTime, sqlDataAccessTransaction);

		if (trip.BillId is not null)
			throw new InvalidOperationException("Cannot delete a trip transaction that is associated with a bill.");

		trip.Status = false;
		await InsertTrip(trip, sqlDataAccessTransaction);
		await DeleteOMCBalance(trip, sqlDataAccessTransaction);

		await AuditTrailData.SaveAuditTrail(new()
		{
			Action = AuditTrailActionTypes.Delete.ToString(),
			TableName = FleetNames.Trip,
			RecordNo = trip.TransactionNo,
			CreatedBy = trip.LastModifiedBy.Value,
			CreatedFromPlatform = trip.LastModifiedFromPlatform
		}, sqlDataAccessTransaction);
	}

	private static async Task DeleteOMCBalance(TripModel trip, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		var cardPayments = await CommonData.LoadTableDataByMasterId<TripCardPaymentsModel>(FleetNames.TripCardPayments, trip.Id, sqlDataAccessTransaction);
		foreach (var cardPayment in cardPayments)
		{
			var omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, cardPayment.OMCCardId, sqlDataAccessTransaction);
			omcCard.CurrentBalance += cardPayment.Amount;
			await OMCCardData.InsertOMCCard(omcCard, sqlDataAccessTransaction);
		}
	}
	#endregion

	public static async Task RecoverTransaction(TripModel trip)
	{
		trip.Status = true;
		var expensesDetails = await CommonData.LoadTableDataByMasterId<TripExpensesModel>(FleetNames.TripExpenses, trip.Id);
		var cardPaymentDetails = await CommonData.LoadTableDataByMasterId<TripCardPaymentsModel>(FleetNames.TripCardPayments, trip.Id);
		var ledgerPaymentDetails = await CommonData.LoadTableDataByMasterId<TripLedgerPaymentsModel>(FleetNames.TripLedgerPayments, trip.Id);
		await SaveTransaction(trip, expensesDetails, cardPaymentDetails, ledgerPaymentDetails, recover: true);

		await TripNotify.Notify(trip.Id, NotifyType.Recovered);
	}

	#region Save
	private static async Task<TripModel> ValidateTransaction(TripModel trip, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		trip.SlNo = trip.SlNo.Trim();
		trip.ChallanNo = string.IsNullOrWhiteSpace(trip.ChallanNo) ? null : trip.ChallanNo.Trim();
		trip.Remarks = string.IsNullOrWhiteSpace(trip.Remarks) ? null : trip.Remarks.Trim();

		if (string.IsNullOrWhiteSpace(trip.SlNo))
			throw new InvalidOperationException("Please enter a sl no for the transaction.");

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

		if (!update)
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
		bool recover = false,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		bool update = trip.Id > 0;

		if (sqlDataAccessTransaction is null)
		{
			(MemoryStream, string)? previousInvoice = update && !recover ? await TripInvoiceExport.ExportInvoice(trip.Id, InvoiceExportType.PDF) : null;

			trip.Id = await SqlDataAccessTransaction.Run(transaction => SaveTransaction(trip, expensesDetails, cardPaymentDetails, ledgerPaymentDetails, recover, transaction));

			if (!recover)
				await TripNotify.Notify(trip.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

			return trip.Id;
		}

		trip = await ValidateTransaction(trip, update, sqlDataAccessTransaction);
		ValidateExpensesDetails(trip, expensesDetails);
		await ValidateCardPaymentDetails(trip, cardPaymentDetails);
		await ValidateLedgerPaymentDetails(trip, ledgerPaymentDetails);

		var previousTrip = update && !recover ? await CommonData.LoadTableDataById<TripOverviewModel>(FleetNames.TripOverview, trip.Id, sqlDataAccessTransaction) : new();
		var previousExpensesDetails = update && !recover ? await CommonData.LoadTableDataByMasterId<TripExpensesOverviewModel>(FleetNames.TripExpensesOverview, trip.Id, sqlDataAccessTransaction) : [];
		var previousCardPaymentDetails = update && !recover ? await CommonData.LoadTableDataByMasterId<TripCardPaymentsOverviewModel>(FleetNames.TripCardPaymentsOverview, trip.Id, sqlDataAccessTransaction) : [];
		var previousLedgerPaymentDetails = update && !recover ? await CommonData.LoadTableDataByMasterId<TripLedgerPaymentsOverviewModel>(FleetNames.TripLedgerPaymentsOverview, trip.Id, sqlDataAccessTransaction) : [];

		trip.Id = await InsertTrip(trip, sqlDataAccessTransaction);
		await SaveExpensesDetail(trip, expensesDetails, update, sqlDataAccessTransaction);
		await SaveCardPaymentDetail(trip, cardPaymentDetails, update, sqlDataAccessTransaction);
		await SaveLedgerPaymentDetail(trip, ledgerPaymentDetails, update, sqlDataAccessTransaction);
		await SaveOMCCardBalance(cardPaymentDetails, update, previousCardPaymentDetails, sqlDataAccessTransaction);
		await SaveAuditTrail(trip, update, recover, previousTrip, previousExpensesDetails, previousCardPaymentDetails, previousLedgerPaymentDetails, sqlDataAccessTransaction);

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
				await InsertTripExpenses(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in expensesDetails)
		{
			item.MasterId = trip.Id;
			await InsertTripExpenses(item, sqlDataAccessTransaction);
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
				await InsertTripCardPayments(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in paymentDetails)
		{
			item.MasterId = trip.Id;
			await InsertTripCardPayments(item, sqlDataAccessTransaction);
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
				await InsertTripLedgerPayments(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in paymentDetails)
		{
			item.MasterId = trip.Id;
			await InsertTripLedgerPayments(item, sqlDataAccessTransaction);
		}
	}

	private static async Task SaveOMCCardBalance(List<TripCardPaymentsModel> paymentDetails, bool update, List<TripCardPaymentsOverviewModel> previousPaymentDetails, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
			foreach (var paymentDetail in previousPaymentDetails.ToList())
			{
				var omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, paymentDetail.OMCCardId, sqlDataAccessTransaction);
				omcCard.CurrentBalance += paymentDetail.PaymentAmount;
				await OMCCardData.InsertOMCCard(omcCard, sqlDataAccessTransaction);
			}

		foreach (var paymentDetail in paymentDetails)
		{
			var omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, paymentDetail.OMCCardId, sqlDataAccessTransaction);
			omcCard.CurrentBalance -= paymentDetail.Amount;
			await OMCCardData.InsertOMCCard(omcCard, sqlDataAccessTransaction);
		}
	}

	private static async Task SaveAuditTrail(
		TripModel trip,
		bool update,
		bool recover,
		TripOverviewModel previousTrip = null,
		List<TripExpensesOverviewModel> previousExpensesDetails = null,
		List<TripCardPaymentsOverviewModel> previousCardPaymentDetails = null,
		List<TripLedgerPaymentsOverviewModel> previousLedgerPaymentDetails = null,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		string difference = null;

		if (update && !recover)
		{
			var currentTrip = await CommonData.LoadTableDataById<TripOverviewModel>(FleetNames.TripOverview, trip.Id, sqlDataAccessTransaction);
			var currentExpensesDetails = await CommonData.LoadTableDataByMasterId<TripExpensesOverviewModel>(FleetNames.TripExpensesOverview, trip.Id, sqlDataAccessTransaction);
			var currentCardPaymentDetails = await CommonData.LoadTableDataByMasterId<TripCardPaymentsOverviewModel>(FleetNames.TripCardPaymentsOverview, trip.Id, sqlDataAccessTransaction);
			var currentLedgerPaymentDetails = await CommonData.LoadTableDataByMasterId<TripLedgerPaymentsOverviewModel>(FleetNames.TripLedgerPaymentsOverview, trip.Id, sqlDataAccessTransaction);

			var headerDiff = AuditTrailData.GetDifference(previousTrip, currentTrip);
			var expensesDiff = AuditTrailData.GetDifference(previousExpensesDetails, currentExpensesDetails, typeof(TripOverviewModel));
			var cardPaymentDiff = AuditTrailData.GetDifference(previousCardPaymentDetails, currentCardPaymentDetails, typeof(TripOverviewModel));
			var ledgerPaymentDiff = AuditTrailData.GetDifference(previousLedgerPaymentDetails, currentLedgerPaymentDetails, typeof(TripOverviewModel));

			difference = AuditTrailData.CombineDifferences(
				(null, headerDiff),
				("Expenses", expensesDiff),
				("Card Payments", cardPaymentDiff),
				("Ledger Payments", ledgerPaymentDiff));
		}

		await AuditTrailData.SaveAuditTrail(new()
		{
			Action = recover ? AuditTrailActionTypes.Recover.ToString() : update ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
			TableName = FleetNames.Trip,
			RecordNo = trip.TransactionNo,
			RecordValue = difference,
			CreatedBy = update ? trip.LastModifiedBy.Value : trip.CreatedBy,
			CreatedFromPlatform = update ? trip.LastModifiedFromPlatform : trip.CreatedFromPlatform
		}, sqlDataAccessTransaction);
	}

	#endregion
}
