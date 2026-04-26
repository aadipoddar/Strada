using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.VehicleTrip.OMC;

namespace StradaLibrary.Data.VehicleTrip.OMC;

public static class OMCCardData
{
	public static async Task<int> InsertOMCCard(OMCCardModel omcCard) =>
		(await SqlDataAccess.LoadData<int, dynamic>(VehicleTripNames.InsertOMCCard, omcCard)).FirstOrDefault();

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

		if (omcCard.OpeningBalance < 0)
			throw new Exception("Opening balance cannot be negative.");

		if (omcCard.Id == 0)
			omcCard.Code = await GenerateCodes.GenerateOMCCardCode();

		if (string.IsNullOrWhiteSpace(omcCard.Code))
			throw new Exception("OMC card code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(omcCard.Remarks))
			omcCard.Remarks = null;

		var allCards = await CommonData.LoadTableData<OMCCardModel>(VehicleTripNames.OMCCard);

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

	public static async Task<int> SaveTransaction(OMCCardModel omcCard)
	{
		await ValidateTransaction(omcCard);
		return await InsertOMCCard(omcCard);
	}
}
