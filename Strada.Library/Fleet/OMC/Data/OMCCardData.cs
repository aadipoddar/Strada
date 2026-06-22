using Strada.Library.Common;
using Strada.Library.DataAccess;
using Strada.Library.Fleet.OMC.Models;
using Strada.Library.Operations.Data;
using Strada.Library.Operations.Models;

namespace Strada.Library.Fleet.OMC.Data;

public static class OMCCardData
{
	public static async Task<int> InsertOMCCard(OMCCardModel omcCard, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertOMCCard, omcCard, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert OMC Card.");

	public static async Task DeleteTransaction(OMCCardModel omcCard, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			omcCard.Status = false;
			await InsertOMCCard(omcCard, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = FleetNames.OMCCard,
				RecordNo = omcCard.CardNumber,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task RecoverTransaction(OMCCardModel omcCard, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			omcCard.Status = true;
			await InsertOMCCard(omcCard, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = FleetNames.OMCCard,
				RecordNo = omcCard.CardNumber,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	private static async Task ValidateTransaction(OMCCardModel omcCard)
	{
		omcCard.CardNumber = omcCard.CardNumber?.Trim().ToUpper() ?? string.Empty;
		omcCard.Code = omcCard.Code?.Trim().ToUpper() ?? string.Empty;
		omcCard.Remarks = omcCard.Remarks?.Trim() ?? string.Empty;
		omcCard.Status = true;

		if (string.IsNullOrWhiteSpace(omcCard.CardNumber))
			throw new Exception("OMC card number is required. Please enter a valid OMC card number.");

		if (omcCard.OMCId <= 0)
			throw new Exception("Associated OMC is required. Please select a valid OMC.");

		if (omcCard.LedgerId <= 0)
			throw new Exception("Associated ledger is required. Please select a valid ledger.");

		if (omcCard.Id == 0)
			omcCard.Code = await GenerateCodes.GenerateOMCCardCode();

		if (string.IsNullOrWhiteSpace(omcCard.Code))
			throw new Exception("OMC card code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(omcCard.Remarks))
			omcCard.Remarks = null;

		var allCards = await CommonData.LoadTableData<OMCCardModel>(FleetNames.OMCCard);

		var existingByCardNumber = allCards.FirstOrDefault(c =>
			c.Id != omcCard.Id &&
			c.CardNumber.Equals(omcCard.CardNumber, StringComparison.OrdinalIgnoreCase));

		if (existingByCardNumber is not null)
			throw new Exception($"OMC card number '{omcCard.CardNumber}' already exists. Please choose a different card number.");

		var existingByCode = allCards.FirstOrDefault(c =>
			c.Id != omcCard.Id &&
			c.Code.Equals(omcCard.Code, StringComparison.OrdinalIgnoreCase));

		if (existingByCode is not null)
			throw new Exception($"OMC card code '{omcCard.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(OMCCardModel omcCard, int userId, string platform)
	{
		await ValidateTransaction(omcCard);

		var isUpdate = omcCard.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<OMCCardModel>(FleetNames.OMCCard, omcCard.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertOMCCard(omcCard, transaction);
			var diff = AuditTrailData.GetDifference(previous, omcCard);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = FleetNames.OMCCard,
				RecordNo = omcCard.CardNumber,
				RecordValue = isUpdate ? diff : null,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
