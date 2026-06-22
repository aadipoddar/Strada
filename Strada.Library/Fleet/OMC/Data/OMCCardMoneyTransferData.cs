using Strada.Library.Accounts.FinancialAccounting.Data;
using Strada.Library.Accounts.FinancialAccounting.Models;
using Strada.Library.Accounts.Masters.Data;
using Strada.Library.Common;
using Strada.Library.DataAccess;
using Strada.Library.Fleet.OMC.Exports;
using Strada.Library.Fleet.OMC.Models;
using Strada.Library.Operations.Data;
using Strada.Library.Operations.Models;
using Strada.Library.Utils.ExportUtils;
using Strada.Library.Utils.MailUtils;

namespace Strada.Library.Fleet.OMC.Data;

public static class OMCCardMoneyTransferData
{
	private static async Task<int> InsertOMCCardMoneyTransfer(OMCCardMoneyTransferModel oMCCardMoneyTransfer, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertOMCCardMoneyTransfer, oMCCardMoneyTransfer, sqlDataAccessTransaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert OMC Card Money Transfer.");

	private static async Task<int> InsertOMCCardMoneyTransferDetails(OMCCardMoneyTransferDetailsModel oMCCardMoneyTransferDetails, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertOMCCardMoneyTransferDetails, oMCCardMoneyTransferDetails, sqlDataAccessTransaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert OMC Card Money Transfer Detail.");

	internal static async Task UpdateFinancialAccountingId(int financialAccountingId, int? newFinancialAccountingId, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		var omcMoneyTransfers = await CommonData.LoadTableDataByFinancialAccountingId<OMCCardMoneyTransferModel>(FleetNames.OMCCardMoneyTransfer, financialAccountingId, sqlDataAccessTransaction);
		foreach (var omcCardMoneyTransfer in omcMoneyTransfers)
		{
			omcCardMoneyTransfer.FinancialAccountingId = newFinancialAccountingId;
			await InsertOMCCardMoneyTransfer(omcCardMoneyTransfer, sqlDataAccessTransaction);
		}
	}

	public static List<OMCCardMoneyTransferDetailsModel> ConvertTransfersCartToDetails(List<OMCCardMoneyTransferDetailsCartModel> cart, int masterId = 0) =>
		[.. cart.Select(item => new OMCCardMoneyTransferDetailsModel
		{
			Id = 0,
			MasterId = masterId,
			OMCCardId = item.OMCCardId,
			Amount = item.Amount,
			Remarks = item.Remarks,
			Status = true
		})];

	#region Delete
	public static async Task DeleteTransaction(OMCCardMoneyTransferModel omcCardMoneyTransfer, SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		if (sqlDataAccessTransaction is null)
		{
			await SqlDataAccessTransaction.Run(transaction => DeleteTransaction(omcCardMoneyTransfer, transaction));
			await OMCCardMoneyTransferNotify.Notify(omcCardMoneyTransfer.Id, NotifyType.Deleted);
			return;
		}

		await FinancialYearData.ValidateFinancialYear(omcCardMoneyTransfer.TransactionDateTime, sqlDataAccessTransaction);

		omcCardMoneyTransfer.Status = false;
		await InsertOMCCardMoneyTransfer(omcCardMoneyTransfer, sqlDataAccessTransaction);
		await DeleteAccounting(omcCardMoneyTransfer, sqlDataAccessTransaction);
		await DeleteOMCBalance(omcCardMoneyTransfer, sqlDataAccessTransaction);

		await AuditTrailData.SaveAuditTrail(new()
		{
			Action = AuditTrailActionTypes.Delete.ToString(),
			TableName = FleetNames.OMCCardMoneyTransfer,
			RecordNo = omcCardMoneyTransfer.TransactionNo,
			CreatedBy = omcCardMoneyTransfer.LastModifiedBy.Value,
			CreatedFromPlatform = omcCardMoneyTransfer.LastModifiedFromPlatform
		}, sqlDataAccessTransaction);
	}

	private static async Task DeleteAccounting(OMCCardMoneyTransferModel omcCardMoneyTransfer, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (omcCardMoneyTransfer.FinancialAccountingId is null || omcCardMoneyTransfer.FinancialAccountingId <= 0)
			return;

		var existingAccounting = await CommonData.LoadTableDataById<FinancialAccountingModel>(AccountNames.FinancialAccounting, omcCardMoneyTransfer.FinancialAccountingId.Value, sqlDataAccessTransaction)
			?? throw new InvalidOperationException("The associated financial accounting transaction for the OMC Card Money Transfer does not exist.");

		existingAccounting.Status = false;
		existingAccounting.LastModifiedBy = omcCardMoneyTransfer.LastModifiedBy;
		existingAccounting.LastModifiedAt = omcCardMoneyTransfer.LastModifiedAt;
		existingAccounting.LastModifiedFromPlatform = omcCardMoneyTransfer.LastModifiedFromPlatform;

		await FinancialAccountingData.DeleteTransaction(existingAccounting, sqlDataAccessTransaction);
	}

	private static async Task DeleteOMCBalance(OMCCardMoneyTransferModel omcCardMoneyTransfer, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		var transferDetails = await CommonData.LoadTableDataByMasterId<OMCCardMoneyTransferDetailsModel>(FleetNames.OMCCardMoneyTransferDetails, omcCardMoneyTransfer.Id, sqlDataAccessTransaction);
		foreach (var transfer in transferDetails)
		{
			var omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, transfer.OMCCardId, sqlDataAccessTransaction);
			omcCard.CurrentBalance -= transfer.Amount;
			await OMCCardData.InsertOMCCard(omcCard, sqlDataAccessTransaction);
		}
	}

	public static async Task RecoverTransaction(OMCCardMoneyTransferModel omcCardMoneyTransfer)
	{
		omcCardMoneyTransfer.Status = true;
		var oMCCardMoneyTransferDetails = await CommonData.LoadTableDataByMasterId<OMCCardMoneyTransferDetailsModel>(FleetNames.OMCCardMoneyTransferDetails, omcCardMoneyTransfer.Id);

		await SaveTransaction(omcCardMoneyTransfer, oMCCardMoneyTransferDetails, true);

		await OMCCardMoneyTransferNotify.Notify(omcCardMoneyTransfer.Id, NotifyType.Recovered);
	}
	#endregion

	#region Saving
	private static async Task<OMCCardMoneyTransferModel> ValidateTransaction(OMCCardMoneyTransferModel oMCCardMoneyTransfer, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		oMCCardMoneyTransfer.Remarks = string.IsNullOrWhiteSpace(oMCCardMoneyTransfer.Remarks) ? null : oMCCardMoneyTransfer.Remarks.Trim();

		if (oMCCardMoneyTransfer.CompanyId <= 0)
			throw new InvalidOperationException("Please select a company for the transaction.");

		if (oMCCardMoneyTransfer.LedgerId <= 0)
			throw new InvalidOperationException("Please select a ledger for the transaction.");

		if (oMCCardMoneyTransfer.TotalItems <= 0)
			throw new InvalidOperationException("Total items must be greater than zero.");

		if (oMCCardMoneyTransfer.TotalAmount < 0)
			throw new InvalidOperationException("Total amount cannot be negative.");

		if (!update)
			oMCCardMoneyTransfer.TransactionNo = await GenerateCodes.GenerateOMCCardMoneyTransferTransactionNo(oMCCardMoneyTransfer, sqlDataAccessTransaction);

		await FinancialYearData.ValidateFinancialYear(oMCCardMoneyTransfer.TransactionDateTime, sqlDataAccessTransaction);

		if (update)
		{
			var existingOMCCardMoneyTransfer = await CommonData.LoadTableDataById<OMCCardMoneyTransferModel>(FleetNames.OMCCardMoneyTransfer, oMCCardMoneyTransfer.Id, sqlDataAccessTransaction)
				?? throw new InvalidOperationException("The OMC Card Money Transfer transaction does not exist.");

			await FinancialYearData.ValidateFinancialYear(existingOMCCardMoneyTransfer.TransactionDateTime, sqlDataAccessTransaction);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, oMCCardMoneyTransfer.LastModifiedBy.Value, sqlDataAccessTransaction);
			if (!user.Admin)
				throw new InvalidOperationException("Only admin users can update a OMC Card Money Transfer transaction.");

			oMCCardMoneyTransfer.TransactionNo = existingOMCCardMoneyTransfer.TransactionNo;
		}

		return oMCCardMoneyTransfer;
	}

	private static void ValidateTransferDetails(OMCCardMoneyTransferModel oMCCardMoneyTransfer, List<OMCCardMoneyTransferDetailsModel> transferDetails)
	{
		if (transferDetails.Any(td => td.Amount <= 0))
			throw new InvalidOperationException("Transfer amount must be greater than zero.");

		if (transferDetails.Count != oMCCardMoneyTransfer.TotalItems)
			throw new InvalidOperationException("Total items must be equal to the number of transfer details.");

		if (transferDetails.Sum(td => td.Amount) != oMCCardMoneyTransfer.TotalAmount)
			throw new InvalidOperationException("Total transfer amount must be equal to total amount of the transaction.");

		foreach (var item in transferDetails)
			item.Remarks = string.IsNullOrWhiteSpace(item.Remarks) ? null : item.Remarks.Trim();
	}

	public static async Task<int> SaveTransaction(
		OMCCardMoneyTransferModel oMCCardMoneyTransfer,
		List<OMCCardMoneyTransferDetailsModel> transferDetails,
		bool recover = false,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		bool update = oMCCardMoneyTransfer.Id > 0;

		if (sqlDataAccessTransaction is null)
		{
			(MemoryStream, string)? previousInvoice = update && !recover ? await OMCCardMoneyTransferInvoiceExport.ExportInvoice(oMCCardMoneyTransfer.Id, InvoiceExportType.PDF) : null;

			oMCCardMoneyTransfer.Id = await SqlDataAccessTransaction.Run(transaction => SaveTransaction(oMCCardMoneyTransfer, transferDetails, recover, transaction));

			if (!recover)
				await OMCCardMoneyTransferNotify.Notify(oMCCardMoneyTransfer.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

			return oMCCardMoneyTransfer.Id;
		}

		oMCCardMoneyTransfer = await ValidateTransaction(oMCCardMoneyTransfer, update, sqlDataAccessTransaction);
		ValidateTransferDetails(oMCCardMoneyTransfer, transferDetails);

		var previousTransfer = update && !recover ? await CommonData.LoadTableDataById<OMCCardMoneyTransferOverviewModel>(FleetNames.OMCCardMoneyTransferOverview, oMCCardMoneyTransfer.Id, sqlDataAccessTransaction) : new();
		var previousTransferDetails = update && !recover ? await CommonData.LoadTableDataByMasterId<OMCCardMoneyTransferDetailsOverviewModel>(FleetNames.OMCCardMoneyTransferDetailsOverview, oMCCardMoneyTransfer.Id, sqlDataAccessTransaction) : [];

		oMCCardMoneyTransfer.Id = await InsertOMCCardMoneyTransfer(oMCCardMoneyTransfer, sqlDataAccessTransaction);
		await SaveTransferDetail(oMCCardMoneyTransfer, transferDetails, update, sqlDataAccessTransaction);
		await SaveAccounting(oMCCardMoneyTransfer, sqlDataAccessTransaction);
		await SaveOMCCardBalance(transferDetails, update, previousTransferDetails, sqlDataAccessTransaction);
		await SaveAuditTrail(oMCCardMoneyTransfer, update, recover, previousTransfer, previousTransferDetails, sqlDataAccessTransaction);

		return oMCCardMoneyTransfer.Id;
	}

	private static async Task SaveTransferDetail(OMCCardMoneyTransferModel oMCCardMoneyTransfer, List<OMCCardMoneyTransferDetailsModel> transferDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
		{
			var existingTransferDetails = await CommonData.LoadTableDataByMasterId<OMCCardMoneyTransferDetailsModel>(FleetNames.OMCCardMoneyTransferDetails, oMCCardMoneyTransfer.Id, sqlDataAccessTransaction);
			foreach (var item in existingTransferDetails)
			{
				item.Status = false;
				await InsertOMCCardMoneyTransferDetails(item, sqlDataAccessTransaction);
			}
		}

		foreach (var item in transferDetails)
		{
			item.MasterId = oMCCardMoneyTransfer.Id;
			await InsertOMCCardMoneyTransferDetails(item, sqlDataAccessTransaction);
		}
	}

	private static async Task SaveAccounting(OMCCardMoneyTransferModel oMCCardMoneyTransfer, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		await DeleteAccounting(oMCCardMoneyTransfer, sqlDataAccessTransaction);

		var omcCardMoneyTransferOverview = await CommonData.LoadTableDataById<OMCCardMoneyTransferOverviewModel>(FleetNames.OMCCardMoneyTransferOverview, oMCCardMoneyTransfer.Id, sqlDataAccessTransaction);
		if (omcCardMoneyTransferOverview is null || omcCardMoneyTransferOverview.TotalAmount == 0)
			return;

		var omcCardMoneyTransferLedger = await SettingsData.LoadSettingsByKey(SettingsKeys.OMCCardMoneyTransferLedgerId, sqlDataAccessTransaction);

		var accountingCart = new List<FinancialAccountingLedgerCartModel>
		{
			new()
			{
				ReferenceId = omcCardMoneyTransferOverview.Id,
				ReferenceType = nameof(AccountingReferenceTypes.OMCCardMoneyTransfer),
				ReferenceNo = omcCardMoneyTransferOverview.TransactionNo,
				LedgerId = int.Parse(omcCardMoneyTransferLedger.Value),
				Debit = omcCardMoneyTransferOverview.TotalAmount,
				Credit = null,
				Remarks = $"OMC Card Money Transfer Account Posting For Transfer {omcCardMoneyTransferOverview.TransactionNo}",
			},

			new ()
			{
				LedgerId = omcCardMoneyTransferOverview.LedgerId,
				Debit = null,
				Credit = omcCardMoneyTransferOverview.TotalAmount,
				Remarks = $"Ledger Account Posting For OMC Card Money Transfer {omcCardMoneyTransferOverview.TransactionNo}",
			}
		};

		var voucher = await SettingsData.LoadSettingsByKey(SettingsKeys.OMCCardMoneyTransferVoucherId, sqlDataAccessTransaction);
		var accounting = new FinancialAccountingModel
		{
			Id = 0,
			TransactionNo = "",
			CompanyId = omcCardMoneyTransferOverview.CompanyId,
			VoucherId = int.Parse(voucher.Value),
			ReferenceId = omcCardMoneyTransferOverview.Id,
			ReferenceNo = omcCardMoneyTransferOverview.TransactionNo,
			TransactionDateTime = omcCardMoneyTransferOverview.TransactionDateTime,
			FinancialYearId = omcCardMoneyTransferOverview.FinancialYearId,
			TotalDebitLedgers = accountingCart.Count(a => a.Debit.HasValue),
			TotalCreditLedgers = accountingCart.Count(a => a.Credit.HasValue),
			TotalDebitAmount = accountingCart.Sum(a => a.Debit ?? 0),
			TotalCreditAmount = accountingCart.Sum(a => a.Credit ?? 0),
			Remarks = omcCardMoneyTransferOverview.Remarks,
			CreatedBy = omcCardMoneyTransferOverview.CreatedBy,
			CreatedAt = omcCardMoneyTransferOverview.CreatedAt,
			CreatedFromPlatform = omcCardMoneyTransferOverview.CreatedFromPlatform,
			Status = true
		};

		var ledgers = FinancialAccountingData.ConvertCartToDetails(accountingCart, accounting.Id);
		accounting.Id = await FinancialAccountingData.SaveTransaction(accounting, ledgers, false, sqlDataAccessTransaction);

		oMCCardMoneyTransfer.FinancialAccountingId = accounting.Id;
		await InsertOMCCardMoneyTransfer(oMCCardMoneyTransfer, sqlDataAccessTransaction);
	}

	private static async Task SaveOMCCardBalance(List<OMCCardMoneyTransferDetailsModel> transferDetails, bool update, List<OMCCardMoneyTransferDetailsOverviewModel> previousTransferDetails, SqlDataAccessTransaction sqlDataAccessTransaction)
	{
		if (update)
			foreach (var transfer in previousTransferDetails)
			{
				var omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, transfer.OMCCardId, sqlDataAccessTransaction);
				omcCard.CurrentBalance -= transfer.TransferAmount;
				await OMCCardData.InsertOMCCard(omcCard, sqlDataAccessTransaction);
			}

		foreach (var transfer in transferDetails)
		{
			var omcCard = await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, transfer.OMCCardId, sqlDataAccessTransaction);
			omcCard.CurrentBalance += transfer.Amount;
			await OMCCardData.InsertOMCCard(omcCard, sqlDataAccessTransaction);
		}
	}

	private static async Task SaveAuditTrail(
		OMCCardMoneyTransferModel oMCCardMoneyTransfer,
		bool update,
		bool recover,
		OMCCardMoneyTransferOverviewModel previousTransfer = null,
		List<OMCCardMoneyTransferDetailsOverviewModel> previousTransferDetails = null,
		SqlDataAccessTransaction sqlDataAccessTransaction = null)
	{
		string difference = null;

		if (update && !recover)
		{
			var currentTransfer = await CommonData.LoadTableDataById<OMCCardMoneyTransferOverviewModel>(FleetNames.OMCCardMoneyTransferOverview, oMCCardMoneyTransfer.Id, sqlDataAccessTransaction);
			var currentTransferDetails = await CommonData.LoadTableDataByMasterId<OMCCardMoneyTransferDetailsOverviewModel>(FleetNames.OMCCardMoneyTransferDetailsOverview, oMCCardMoneyTransfer.Id, sqlDataAccessTransaction);

			var transferDiff = AuditTrailData.GetDifference(previousTransfer, currentTransfer);
			var detailsDiff = AuditTrailData.GetDifference(previousTransferDetails, currentTransferDetails, typeof(OMCCardMoneyTransferOverviewModel));

			difference = AuditTrailData.CombineDifferences(
				(null, transferDiff),
				("Details", detailsDiff));
		}

		await AuditTrailData.SaveAuditTrail(new()
		{
			Action = recover ? AuditTrailActionTypes.Recover.ToString() : update ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
			TableName = FleetNames.OMCCardMoneyTransfer,
			RecordNo = oMCCardMoneyTransfer.TransactionNo,
			RecordValue = difference,
			CreatedBy = update ? oMCCardMoneyTransfer.LastModifiedBy.Value : oMCCardMoneyTransfer.CreatedBy,
			CreatedFromPlatform = update ? oMCCardMoneyTransfer.LastModifiedFromPlatform : oMCCardMoneyTransfer.CreatedFromPlatform
		}, sqlDataAccessTransaction);
	}
	#endregion
}
