using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Common;
using StradaLibrary.Data.Fleet.VehicleTrip;
using StradaLibrary.DataAccess;
using StradaLibrary.Exports.Fleet.VehicleTripBill;
using StradaLibrary.Exports.Mailing;
using StradaLibrary.Exports.Utils;
using StradaLibrary.Models.Fleet.VehicleTrip;
using StradaLibrary.Models.Fleet.VehicleTripBill;
using StradaLibrary.Models.Operations;

namespace StradaLibrary.Data.Fleet.VehicleTripBill;

public static class VehicleTripBillData
{
	private static async Task<int> InsertVehicleTripBill(VehicleTripBillModel vehicleTripBill, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleTripBill, vehicleTripBill, sqlDataAccessTransaction)).FirstOrDefault();

	private static async Task<int> InsertVehicleTripBillCardPayments(VehicleTripBillCardPaymentsModel vehicleTripBillCardPayments, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleTripBillCardPayments, vehicleTripBillCardPayments, sqlDataAccessTransaction)).FirstOrDefault();

	private static async Task<int> InsertVehicleTripBillLedgerPayments(VehicleTripBillLedgerPaymentsModel vehicleTripBillLedgerPayments, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleTripBillLedgerPayments, vehicleTripBillLedgerPayments, sqlDataAccessTransaction)).FirstOrDefault();

	public static List<VehicleTripBillCardPaymentsModel> ConvertCardPaymentCartToDetails(List<VehicleTripBillCardPaymentsCartModel> cart, int billId = 0) =>
		[.. cart.Select(item => new VehicleTripBillCardPaymentsModel
		{
			Id = 0,
			MasterId = billId,
			OMCCardId = item.OMCCardId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	public static List<VehicleTripBillLedgerPaymentsModel> ConvertLedgerPaymentCartToDetails(List<VehicleTripBillLedgerPaymentsCartModel> cart, int billId = 0) =>
		[.. cart.Select(item => new VehicleTripBillLedgerPaymentsModel
		{
			Id = 0,
			MasterId = billId,
			LedgerId = item.LedgerId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	public static async Task DeleteTransaction(VehicleTripBillModel bill, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (sqlDataAccessTransaction is null)
		{
			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				await DeleteTransaction(bill, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			await VehicleTripBillNotify.Notify(bill.Id, NotifyType.Deleted);
		}

		try
		{
			await FinancialYearData.ValidateFinancialYear(bill.TransactionDateTime, sqlDataAccessTransaction);

			bill.Status = false;
			var id = await InsertVehicleTripBill(bill, sqlDataAccessTransaction);
			if (id <= 0)
				throw new InvalidOperationException("Failed to delete vehicle trip bill transaction.");

			await DeleteVehicleTripsBillNo(bill, sqlDataAccessTransaction);
		}
		catch
		{
			sqlDataAccessTransaction.RollbackTransaction();
			throw;
		}
	}

	public static async Task RecoverTransaction(VehicleTripBillModel bill, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (sqlDataAccessTransaction is null)
		{
			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				await RecoverTransaction(bill, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			await VehicleTripBillNotify.Notify(bill.Id, NotifyType.Recovered);
			return;
		}

		await FinancialYearData.ValidateFinancialYear(bill.TransactionDateTime, sqlDataAccessTransaction);

		bill.Status = true;
		var id = await InsertVehicleTripBill(bill, sqlDataAccessTransaction);
		if (id <= 0)
			throw new InvalidOperationException("Failed to recover vehicle trip bill transaction.");
	}

	private static async Task DeleteVehicleTripsBillNo(VehicleTripBillModel bill, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		var existingVehicleTrips = await VehicleTripData.LoadVehicleTripOverviewByBillIdDate(bill.Id, null, null, sqlDataAccessTransaction);
		foreach (var item in existingVehicleTrips)
		{
			var trip = await CommonData.LoadTableDataById<VehicleTripModel>(FleetNames.VehicleTrip, item.Id, sqlDataAccessTransaction);

			await FinancialYearData.ValidateFinancialYear(trip.TransactionDateTime, sqlDataAccessTransaction);

			trip.ChallanNo = null;
			trip.BillId = null;
			trip.GrossAmount = null;
			trip.TDSAmount = null;
			trip.PenaltyAmount = null;
			trip.NetAmount = null;
			var id = await VehicleTripData.InsertVehicleTrip(trip, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save vehicle trips.");
		}
	}

	private static async Task<VehicleTripBillModel> ValidateTransaction(VehicleTripBillModel bill, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (bill.CompanyId <= 0)
			throw new InvalidOperationException("Please select a company for the transaction.");

		if (bill.OMCId <= 0)
			throw new InvalidOperationException("Please select an OMC for the transaction.");

		bill.BillNo = bill.BillNo?.Trim().ToUpper();
		if (string.IsNullOrWhiteSpace(bill.BillNo))
			throw new InvalidOperationException("Please enter a bill number.");

		if (bill.TotalGrossAmount < 0)
			throw new InvalidOperationException("Total gross amount cannot be negative.");

		if (bill.TotalTDSAmount < 0)
			throw new InvalidOperationException("Total TDS amount cannot be negative.");

		if (bill.TotalPenaltyAmount < 0)
			throw new InvalidOperationException("Total penalty amount cannot be negative.");

		if (bill.TotalNetAmount < 0)
			throw new InvalidOperationException("Total net amount cannot be negative.");

		if (bill.TotalGrossAmount - bill.TotalTDSAmount - bill.TotalPenaltyAmount != bill.TotalNetAmount)
			throw new InvalidOperationException("Total net amount must equal total gross amount minus TDS and penalty.");

		if (bill.TotalCardPaymentAmount + bill.TotalLedgerPaymentAmount != bill.TotalNetAmount)
			throw new InvalidOperationException("Sum of card and ledger payments must equal total net amount.");

		bill.TransactionNo = await GenerateCodes.GenerateVehicleTripBillTransactionNo(bill, sqlDataAccessTransaction);
		await FinancialYearData.ValidateFinancialYear(bill.TransactionDateTime, sqlDataAccessTransaction);

		if (update)
		{
			var existingBill = await CommonData.LoadTableDataById<VehicleTripBillModel>(FleetNames.VehicleTripBill, bill.Id, sqlDataAccessTransaction)
				?? throw new InvalidOperationException("The vehicle trip bill transaction does not exist.");

			await FinancialYearData.ValidateFinancialYear(existingBill.TransactionDateTime, sqlDataAccessTransaction);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, bill.LastModifiedBy.Value, sqlDataAccessTransaction);
			if (!user.Admin)
				throw new InvalidOperationException("Only admin users can update a vehicle trip bill transaction.");

			bill.TransactionNo = existingBill.TransactionNo;
		}

		return bill;
	}

	private static void ValidateCardPaymentDetails(VehicleTripBillModel bill, List<VehicleTripBillCardPaymentsModel> cardPayments)
	{
		if (cardPayments.Any(cp => cp.Amount <= 0))
			throw new InvalidOperationException("Card payment amount must be greater than zero.");

		if (cardPayments.Sum(cp => cp.Amount) != bill.TotalCardPaymentAmount)
			throw new InvalidOperationException("Total card payment amount must be equal to bill's total card payment amount.");
	}

	private static void ValidateLedgerPaymentDetails(VehicleTripBillModel bill, List<VehicleTripBillLedgerPaymentsModel> ledgerPayments)
	{
		if (ledgerPayments.Any(lp => lp.Amount <= 0))
			throw new InvalidOperationException("Ledger payment amount must be greater than zero.");

		if (ledgerPayments.Sum(lp => lp.Amount) != bill.TotalLedgerPaymentAmount)
			throw new InvalidOperationException("Total ledger payment amount must be equal to bill's total ledger payment amount.");
	}

	private static async Task ValidateVehicleTrips(VehicleTripBillModel bill, List<VehicleTripOverviewModel> vehicleTrips, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (vehicleTrips is null || vehicleTrips.Count == 0)
			throw new InvalidOperationException("Please add at least one vehicle trip for the bill.");

		if (vehicleTrips.Any(vt => string.IsNullOrWhiteSpace(vt.ChallanNo)))
			throw new InvalidOperationException("All vehicle trips included in the bill must have a valid challan number.");

		if (vehicleTrips.Any(vt => vt.GrossAmount is null || vt.TDSAmount is null || vt.PenaltyAmount is null || vt.NetAmount is null))
			throw new InvalidOperationException("All vehicle trips included in the bill must have gross amount, TDS amount, penalty amount and net amount specified.");

		if (vehicleTrips.Sum(vt => vt.GrossAmount) != bill.TotalGrossAmount)
			throw new InvalidOperationException("Sum of gross amounts of all vehicle trips must be equal to bill's total gross amount.");

		if (vehicleTrips.Sum(vt => vt.TDSAmount) != bill.TotalTDSAmount)
			throw new InvalidOperationException("Sum of TDS amounts of all vehicle trips must be equal to bill's total TDS amount.");

		if (vehicleTrips.Sum(vt => vt.PenaltyAmount) != bill.TotalPenaltyAmount)
			throw new InvalidOperationException("Sum of penalty amounts of all vehicle trips must be equal to bill's total penalty amount.");

		if (vehicleTrips.Sum(vt => vt.NetAmount) != bill.TotalNetAmount)
			throw new InvalidOperationException("Sum of net amounts of all vehicle trips must be equal to bill's total net amount.");

		foreach (var vehicleTrip in vehicleTrips)
			await FinancialYearData.ValidateFinancialYear(vehicleTrip.TransactionDateTime, sqlDataAccessTransaction);

		foreach (var vehicleTrip in vehicleTrips)
			vehicleTrip.ChallanNo = vehicleTrip.ChallanNo.Trim().ToUpper();
	}

	public static async Task<int> SaveTransaction(
		VehicleTripBillModel bill,
		List<VehicleTripBillCardPaymentsModel> cardPayments,
		List<VehicleTripBillLedgerPaymentsModel> ledgerPayments,
		List<VehicleTripOverviewModel> vehicleTrips,
		bool showNotification = true,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		bool update = bill.Id > 0;

		if (sqlDataAccessTransaction is null)
		{
			(MemoryStream, string)? previousInvoice = null;
			if (update)
				previousInvoice = await VehicleTripBillInvoiceExport.ExportInvoice(bill.Id, InvoiceExportType.PDF);

			using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

			try
			{
				newSqlDataAccessTransaction.StartTransaction();
				bill.Id = await SaveTransaction(bill, cardPayments, ledgerPayments, vehicleTrips, showNotification, newSqlDataAccessTransaction);
				newSqlDataAccessTransaction.CommitTransaction();
			}
			catch
			{
				newSqlDataAccessTransaction.RollbackTransaction();
				throw;
			}

			if (showNotification)
				await VehicleTripBillNotify.Notify(bill.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

			return bill.Id;
		}

		bill = await ValidateTransaction(bill, update, sqlDataAccessTransaction);
		ValidateCardPaymentDetails(bill, cardPayments);
		ValidateLedgerPaymentDetails(bill, ledgerPayments);
		await ValidateVehicleTrips(bill, vehicleTrips, sqlDataAccessTransaction);

		bill.Id = await InsertVehicleTripBill(bill, sqlDataAccessTransaction);
		await SaveCardPaymentDetail(bill, cardPayments, update, sqlDataAccessTransaction);
		await SaveLedgerPaymentDetail(bill, ledgerPayments, update, sqlDataAccessTransaction);
		await SaveVehicleTripsBillNo(bill, vehicleTrips, update, sqlDataAccessTransaction);

		return bill.Id;
	}

	private static async Task SaveCardPaymentDetail(VehicleTripBillModel bill, List<VehicleTripBillCardPaymentsModel> cardPayments, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingCardPayments = await CommonData.LoadTableDataByMasterId<VehicleTripBillCardPaymentsModel>(FleetNames.VehicleTripBillCardPayments, bill.Id, sqlDataAccessTransaction);
			foreach (var item in existingCardPayments)
			{
				item.Status = false;
				var id = await InsertVehicleTripBillCardPayments(item, sqlDataAccessTransaction);

				if (id <= 0)
					throw new InvalidOperationException("Failed to save vehicle trip bill OMC card payments detail item.");
			}
		}

		foreach (var item in cardPayments)
		{
			item.MasterId = bill.Id;
			var id = await InsertVehicleTripBillCardPayments(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save vehicle trip bill OMC card payments detail item.");
		}
	}

	private static async Task SaveLedgerPaymentDetail(VehicleTripBillModel bill, List<VehicleTripBillLedgerPaymentsModel> ledgerPayments, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingLedgerPayments = await CommonData.LoadTableDataByMasterId<VehicleTripBillLedgerPaymentsModel>(FleetNames.VehicleTripBillLedgerPayments, bill.Id, sqlDataAccessTransaction);
			foreach (var item in existingLedgerPayments)
			{
				item.Status = false;
				var id = await InsertVehicleTripBillLedgerPayments(item, sqlDataAccessTransaction);

				if (id <= 0)
					throw new InvalidOperationException("Failed to save vehicle trip bill ledger payments detail item.");
			}
		}

		foreach (var item in ledgerPayments)
		{
			item.MasterId = bill.Id;
			var id = await InsertVehicleTripBillLedgerPayments(item, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save vehicle trip bill ledger payments detail item.");
		}
	}

	private static async Task SaveVehicleTripsBillNo(VehicleTripBillModel bill, List<VehicleTripOverviewModel> vehicleTrips, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingVehicleTrips = await VehicleTripData.LoadVehicleTripOverviewByBillIdDate(bill.Id, null, null, sqlDataAccessTransaction);
			foreach (var item in existingVehicleTrips)
			{
				var trip = await CommonData.LoadTableDataById<VehicleTripModel>(FleetNames.VehicleTrip, item.Id, sqlDataAccessTransaction);

				await FinancialYearData.ValidateFinancialYear(trip.TransactionDateTime, sqlDataAccessTransaction);

				trip.ChallanNo = null;
				trip.BillId = null;
				trip.GrossAmount = null;
				trip.TDSAmount = null;
				trip.PenaltyAmount = null;
				trip.NetAmount = null;
				var id = await VehicleTripData.InsertVehicleTrip(trip, sqlDataAccessTransaction);

				if (id <= 0)
					throw new InvalidOperationException("Failed to save vehicle trips.");
			}
		}

		foreach (var item in vehicleTrips)
		{
			var trip = await CommonData.LoadTableDataById<VehicleTripModel>(FleetNames.VehicleTrip, item.Id, sqlDataAccessTransaction);
			trip.ChallanNo = item.ChallanNo;
			trip.BillId = bill.Id;
			trip.GrossAmount = item.GrossAmount;
			trip.TDSAmount = item.TDSAmount;
			trip.PenaltyAmount = item.PenaltyAmount;
			trip.NetAmount = item.NetAmount;
			var id = await VehicleTripData.InsertVehicleTrip(trip, sqlDataAccessTransaction);

			if (id <= 0)
				throw new InvalidOperationException("Failed to save vehicle trips.");
		}
	}
}
