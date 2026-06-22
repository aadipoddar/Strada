using Strada.Library.Accounts.FinancialAccounting.Data;
using Strada.Library.Accounts.FinancialAccounting.Models;
using Strada.Library.Accounts.Masters.Data;
using Strada.Library.Common;
using Strada.Library.DataAccess;
using Strada.Library.Fleet.Bill.Exports;
using Strada.Library.Fleet.Bill.Models;
using Strada.Library.Fleet.Trip.Data;
using Strada.Library.Fleet.Trip.Models;
using Strada.Library.Operations.Data;
using Strada.Library.Operations.Models;
using Strada.Library.Utils.ExportUtils;
using Strada.Library.Utils.MailUtils;

namespace Strada.Library.Fleet.Bill.Data;

public static class BillData
{
	private static async Task<int> InsertBill(BillModel bill, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertBill, bill, sqlDataAccessTransaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Bill.");

	private static async Task<int> InsertBillLedgerPayments(BillLedgerPaymentsModel billLedgerPayments, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertBillLedgerPayments, billLedgerPayments, sqlDataAccessTransaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Bill Ledger Payment.");

	internal static async Task UpdateFinancialAccountingId(int financialAccountingId, int? newFinancialAccountingId, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var bills = await CommonData.LoadTableDataByFinancialAccountingId<BillModel>(FleetNames.Bill, financialAccountingId, sqlDataAccessTransaction);
		foreach (var bill in bills)
		{
			bill.FinancialAccountingId = newFinancialAccountingId;
			await InsertBill(bill, sqlDataAccessTransaction);
		}
	}

	public static List<BillLedgerPaymentsModel> ConvertLedgerPaymentCartToDetails(List<BillLedgerPaymentsCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new BillLedgerPaymentsModel
		{
			Id = 0,
			MasterId = masterId,
			LedgerId = item.LedgerId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	#region Delete
	public static async Task DeleteTransaction(BillModel bill, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (sqlDataAccessTransaction is null)
		{
			await SqlDataAccessTransaction.Run(transaction => DeleteTransaction(bill, transaction));
			await BillNotify.Notify(bill.Id, NotifyType.Deleted);
			return;
		}

		await FinancialYearData.ValidateFinancialYear(bill.TransactionDateTime, sqlDataAccessTransaction);

		bill.Status = false;
		await InsertBill(bill, sqlDataAccessTransaction);

		await DeleteTripsBillNo(bill, sqlDataAccessTransaction);
		await DeleteAccounting(bill, sqlDataAccessTransaction);

		await AuditTrailData.SaveAuditTrail(new()
		{
			Action = AuditTrailActionTypes.Delete.ToString(),
			TableName = FleetNames.Bill,
			RecordNo = bill.TransactionNo,
			CreatedBy = bill.LastModifiedBy.Value,
			CreatedFromPlatform = bill.LastModifiedFromPlatform
		}, sqlDataAccessTransaction);
	}

	private static async Task DeleteTripsBillNo(BillModel bill, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		var existingTrips = await TripData.LoadTripOverviewByBillIdDate(bill.Id, null, null, sqlDataAccessTransaction);
		foreach (var item in existingTrips)
		{
			var trip = await CommonData.LoadTableDataById<TripModel>(FleetNames.Trip, item.Id, sqlDataAccessTransaction);

			await FinancialYearData.ValidateFinancialYear(trip.TransactionDateTime, sqlDataAccessTransaction);

			trip.ChallanNo = null;
			trip.BillId = null;
			trip.GrossAmount = null;
			trip.PenaltyAmount = null;
			trip.NetAmount = null;
			await TripData.InsertTrip(trip, sqlDataAccessTransaction);
		}
	}

	private static async Task DeleteAccounting(BillModel bill, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (bill.FinancialAccountingId is null || bill.FinancialAccountingId <= 0)
			return;

		var existingAccounting = await CommonData.LoadTableDataById<FinancialAccountingModel>(AccountNames.FinancialAccounting, bill.FinancialAccountingId.Value, sqlDataAccessTransaction)
			?? throw new InvalidOperationException("The associated financial accounting transaction for the bill does not exist.");

		existingAccounting.Status = false;
		existingAccounting.LastModifiedBy = bill.LastModifiedBy;
		existingAccounting.LastModifiedAt = bill.LastModifiedAt;
		existingAccounting.LastModifiedFromPlatform = bill.LastModifiedFromPlatform;

		await FinancialAccountingData.DeleteTransaction(existingAccounting, sqlDataAccessTransaction);
	}
	#endregion

	#region Save
	private static async Task<BillModel> ValidateTransaction(BillModel bill, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		bill.Remarks = string.IsNullOrWhiteSpace(bill.Remarks) ? null : bill.Remarks.Trim();

		bill.BillNo = bill.BillNo?.Trim();
		if (string.IsNullOrWhiteSpace(bill.BillNo))
			throw new InvalidOperationException("Please enter a bill number.");

		if (bill.CompanyId <= 0)
			throw new InvalidOperationException("Please select a company for the transaction.");

		if (bill.OMCId <= 0)
			throw new InvalidOperationException("Please select an OMC for the transaction.");

		if (bill.TotalGrossAmount < 0)
			throw new InvalidOperationException("Total gross amount cannot be negative.");

		if (bill.TotalPenaltyAmount < 0)
			throw new InvalidOperationException("Total penalty amount cannot be negative.");

		if (bill.TotalNetAmount < 0)
			throw new InvalidOperationException("Total net amount cannot be negative.");

		if (bill.TotalGrossAmount - bill.TotalPenaltyAmount != bill.TotalNetAmount)
			throw new InvalidOperationException("Total net amount must equal total gross amount minus penalty.");

		if (!update)
			bill.TransactionNo = await GenerateCodes.GenerateBillTransactionNo(bill, sqlDataAccessTransaction);

		await FinancialYearData.ValidateFinancialYear(bill.TransactionDateTime, sqlDataAccessTransaction);

		if (update)
		{
			var existingBill = await CommonData.LoadTableDataById<BillModel>(FleetNames.Bill, bill.Id, sqlDataAccessTransaction)
				?? throw new InvalidOperationException("The Bill transaction does not exist.");

			await FinancialYearData.ValidateFinancialYear(existingBill.TransactionDateTime, sqlDataAccessTransaction);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, bill.LastModifiedBy.Value, sqlDataAccessTransaction);
			if (!user.Admin)
				throw new InvalidOperationException("Only admin users can update a Bill transaction.");

			bill.TransactionNo = existingBill.TransactionNo;
		}

		return bill;
	}

	private static void ValidateLedgerPaymentDetails(BillModel bill, List<BillLedgerPaymentsModel> paymentDetails)
	{
		if (paymentDetails.Any(lp => lp.Amount <= 0))
			throw new InvalidOperationException("Ledger payment amount must be greater than zero.");

		if (paymentDetails.Sum(lp => lp.Amount) != bill.TotalLedgerPaymentAmount)
			throw new InvalidOperationException("Total ledger payment amount must be equal to bill's total ledger payment amount.");

		foreach (var item in paymentDetails)
			item.Remarks = string.IsNullOrWhiteSpace(item.Remarks) ? null : item.Remarks.Trim();
	}

	private static async Task ValidateTrips(BillModel bill, List<TripOverviewModel> trips, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (trips is null || trips.Count == 0)
			throw new InvalidOperationException("Please add at least one Trip for the bill.");

		if (trips.Any(vt => string.IsNullOrWhiteSpace(vt.ChallanNo)))
			throw new InvalidOperationException("All Trips included in the bill must have a valid challan number.");

		if (trips.Any(vt => vt.GrossAmount is null || vt.PenaltyAmount is null || vt.NetAmount is null))
			throw new InvalidOperationException("All Trips included in the bill must have gross amount, penalty amount and net amount specified.");

		if (trips.Sum(vt => vt.GrossAmount) != bill.TotalGrossAmount)
			throw new InvalidOperationException("Sum of gross amounts of all Trips must be equal to bill's total gross amount.");

		if (trips.Sum(vt => vt.PenaltyAmount) != bill.TotalPenaltyAmount)
			throw new InvalidOperationException("Sum of penalty amounts of all Trips must be equal to bill's total penalty amount.");

		if (trips.Sum(vt => vt.NetAmount) != bill.TotalNetAmount)
			throw new InvalidOperationException("Sum of net amounts of all Trips must be equal to bill's total net amount.");

		foreach (var trip in trips)
			await FinancialYearData.ValidateFinancialYear(trip.TransactionDateTime, sqlDataAccessTransaction);

		foreach (var trip in trips)
			trip.ChallanNo = trip.ChallanNo.Trim().ToUpper();
	}

	public static async Task<int> SaveTransaction(
		BillModel bill,
		List<BillLedgerPaymentsModel> ledgerPayments,
		List<TripOverviewModel> trips,
		bool showNotification = true,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		bool update = bill.Id > 0;

		if (update)
			throw new InvalidOperationException("Updating a Bill transaction is not allowed. Please delete and recreate the transaction to make changes.");

		if (sqlDataAccessTransaction is null)
		{
			(MemoryStream, string)? previousInvoice = null;
			if (update)
				previousInvoice = await BillInvoiceExport.ExportInvoice(bill.Id, InvoiceExportType.PDF);

			bill.Id = await SqlDataAccessTransaction.Run(transaction => SaveTransaction(bill, ledgerPayments, trips, showNotification, transaction));

			if (showNotification)
				await BillNotify.Notify(bill.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

			return bill.Id;
		}

		bill = await ValidateTransaction(bill, update, sqlDataAccessTransaction);
		ValidateLedgerPaymentDetails(bill, ledgerPayments);
		await ValidateTrips(bill, trips, sqlDataAccessTransaction);

		var previousBill = update ? await CommonData.LoadTableDataById<BillOverviewModel>(FleetNames.BillOverview, bill.Id, sqlDataAccessTransaction) : null;
		var previousLedgerPayments = update ? await CommonData.LoadTableDataByMasterId<BillLedgerPaymentsOverviewModel>(FleetNames.BillLedgerPaymentsOverview, bill.Id, sqlDataAccessTransaction) : null;
		var previousTrips = update ? await TripData.LoadTripOverviewByBillIdDate(bill.Id, null, null, sqlDataAccessTransaction) : null;

		bill.Id = await InsertBill(bill, sqlDataAccessTransaction);
		await SaveLedgerPaymentDetail(bill, ledgerPayments, update, sqlDataAccessTransaction);
		await SaveTripsBillNo(bill, trips, update, sqlDataAccessTransaction);
		await SaveAccounting(bill, sqlDataAccessTransaction);
		await SaveAuditTrail(bill, update, previousBill, previousLedgerPayments, previousTrips, sqlDataAccessTransaction);

		return bill.Id;
	}

	private static async Task SaveLedgerPaymentDetail(BillModel bill, List<BillLedgerPaymentsModel> ledgerPayments, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingLedgerPayments = await CommonData.LoadTableDataByMasterId<BillLedgerPaymentsModel>(FleetNames.BillLedgerPayments, bill.Id, sqlDataAccessTransaction);
			foreach (var item in existingLedgerPayments)
			{
				item.Status = false;
				await InsertBillLedgerPayments(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in ledgerPayments)
		{
			item.MasterId = bill.Id;
			await InsertBillLedgerPayments(item, sqlDataAccessTransaction);
		}
	}

	private static async Task SaveTripsBillNo(BillModel bill, List<TripOverviewModel> trips, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingTrips = await TripData.LoadTripOverviewByBillIdDate(bill.Id, null, null, sqlDataAccessTransaction);
			foreach (var item in existingTrips)
			{
				var trip = await CommonData.LoadTableDataById<TripModel>(FleetNames.Trip, item.Id, sqlDataAccessTransaction);

				await FinancialYearData.ValidateFinancialYear(trip.TransactionDateTime, sqlDataAccessTransaction);

				trip.ChallanNo = null;
				trip.BillId = null;
				trip.GrossAmount = null;
				trip.PenaltyAmount = null;
				trip.NetAmount = null;
				await TripData.InsertTrip(trip, sqlDataAccessTransaction);
			}
		}

		foreach (var item in trips)
		{
			var trip = await CommonData.LoadTableDataById<TripModel>(FleetNames.Trip, item.Id, sqlDataAccessTransaction);
			trip.ChallanNo = item.ChallanNo;
			trip.BillId = bill.Id;
			trip.GrossAmount = item.GrossAmount;
			trip.PenaltyAmount = item.PenaltyAmount;
			trip.NetAmount = item.NetAmount;
			await TripData.InsertTrip(trip, sqlDataAccessTransaction);
		}
	}

	private static async Task SaveAccounting(BillModel bill, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		await DeleteAccounting(bill, sqlDataAccessTransaction);

		var billOverview = await CommonData.LoadTableDataById<BillOverviewModel>(FleetNames.BillOverview, bill.Id, sqlDataAccessTransaction);
		if (billOverview is null || billOverview.TotalLedgerPaymentAmount == 0)
			return;

		var ledgerPayments = await CommonData.LoadTableDataByMasterId<BillLedgerPaymentsOverviewModel>(FleetNames.BillLedgerPaymentsOverview, bill.Id, sqlDataAccessTransaction);
		if (ledgerPayments is null)
			return;

		var billLedger = await SettingsData.LoadSettingsByKey(SettingsKeys.BillLedgerId, sqlDataAccessTransaction);

		var accountingCart = new List<FinancialAccountingLedgerCartModel>
		{
			new()
			{
				ReferenceId = billOverview.Id,
				ReferenceType = nameof(AccountingReferenceTypes.Bill),
				ReferenceNo = billOverview.TransactionNo,
				LedgerId = int.Parse(billLedger.Value),
				Debit = null,
				Credit = billOverview.TotalLedgerPaymentAmount,
				Remarks = $"Bill Account Posting For Bill {billOverview.TransactionNo}",
			}
		};

		foreach (var item in ledgerPayments)
			accountingCart.Add(new()
			{
				LedgerId = item.LedgerId,
				Debit = item.PaymentAmount,
				Credit = null,
				Remarks = $"Ledger Payment Account Posting For Bill {billOverview.TransactionNo}",
			});

		var voucher = await SettingsData.LoadSettingsByKey(SettingsKeys.BillVoucherId, sqlDataAccessTransaction);
		var accounting = new FinancialAccountingModel
		{
			Id = 0,
			TransactionNo = "",
			CompanyId = billOverview.CompanyId,
			VoucherId = int.Parse(voucher.Value),
			ReferenceId = billOverview.Id,
			ReferenceNo = billOverview.TransactionNo,
			TransactionDateTime = billOverview.TransactionDateTime,
			FinancialYearId = billOverview.FinancialYearId,
			TotalDebitLedgers = accountingCart.Count(a => a.Debit.HasValue),
			TotalCreditLedgers = accountingCart.Count(a => a.Credit.HasValue),
			TotalDebitAmount = accountingCart.Sum(a => a.Debit ?? 0),
			TotalCreditAmount = accountingCart.Sum(a => a.Credit ?? 0),
			Remarks = billOverview.Remarks,
			CreatedBy = billOverview.CreatedBy,
			CreatedAt = billOverview.CreatedAt,
			CreatedFromPlatform = billOverview.CreatedFromPlatform,
			Status = true
		};

		var ledgers = FinancialAccountingData.ConvertCartToDetails(accountingCart, accounting.Id);
		accounting.Id = await FinancialAccountingData.SaveTransaction(accounting, ledgers, false, sqlDataAccessTransaction);

		bill.FinancialAccountingId = accounting.Id;
		await InsertBill(bill, sqlDataAccessTransaction);
	}

	private static async Task SaveAuditTrail(
		BillModel bill,
		bool update,
		BillOverviewModel previousBill = null,
		List<BillLedgerPaymentsOverviewModel> previousLedgerPayments = null,
		List<TripOverviewModel> previousTrips = null,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		string difference = null;

		if (update)
		{
			var currentBill = await CommonData.LoadTableDataById<BillOverviewModel>(FleetNames.BillOverview, bill.Id, sqlDataAccessTransaction);
			var currentLedgerPayments = await CommonData.LoadTableDataByMasterId<BillLedgerPaymentsOverviewModel>(FleetNames.BillLedgerPaymentsOverview, bill.Id, sqlDataAccessTransaction);
			var currentTrips = await TripData.LoadTripOverviewByBillIdDate(bill.Id, null, null, sqlDataAccessTransaction);

			var headerDiff = AuditTrailData.GetDifference(previousBill, currentBill);
			var ledgerPaymentsDiff = AuditTrailData.GetDifference(previousLedgerPayments, currentLedgerPayments, typeof(BillOverviewModel));
			var tripsDiff = AuditTrailData.GetDifference(previousTrips, currentTrips, typeof(BillOverviewModel));

			difference = AuditTrailData.CombineDifferences(
				(null, headerDiff),
				("Ledger Payments", ledgerPaymentsDiff),
				("Trips", tripsDiff));
		}

		await AuditTrailData.SaveAuditTrail(new()
		{
			Action = update ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
			TableName = FleetNames.Bill,
			RecordNo = bill.TransactionNo,
			RecordValue = difference,
			CreatedBy = update ? bill.LastModifiedBy.Value : bill.CreatedBy,
			CreatedFromPlatform = update ? bill.LastModifiedFromPlatform : bill.CreatedFromPlatform
		}, sqlDataAccessTransaction);
	}
	#endregion
}
