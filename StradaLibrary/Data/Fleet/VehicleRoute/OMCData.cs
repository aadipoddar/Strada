using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Data.Fleet.VehicleRoute;

public static class OMCData
{
	public static async Task<int> InsertOMC(OMCModel omc) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertOMC, omc)).FirstOrDefault();

	private static async Task ValidateTransaction(OMCModel omc)
	{
		omc.Name = omc.Name?.Trim().ToUpper() ?? string.Empty;
		omc.Code = omc.Code?.Trim().ToUpper() ?? string.Empty;
		omc.Remarks = omc.Remarks?.Trim() ?? string.Empty;
		omc.Status = true;

		if (string.IsNullOrWhiteSpace(omc.Name))
			throw new Exception("OMC name is required. Please enter a valid OMC name.");

		if (omc.Id == 0)
			omc.Code = await GenerateCodes.GenerateOMCCode();

		if (string.IsNullOrWhiteSpace(omc.Code))
			throw new Exception("OMC code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(omc.Remarks))
			omc.Remarks = null;

		var allOMCs = await CommonData.LoadTableData<OMCModel>(FleetNames.OMC);

		var existingByName = allOMCs.FirstOrDefault(x => x.Id != omc.Id && x.Name.Equals(omc.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"OMC name '{omc.Name}' already exists. Please choose a different name.");

		var existingByCode = allOMCs.FirstOrDefault(x => x.Id != omc.Id && x.Code.Equals(omc.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"OMC code '{omc.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(OMCModel omc)
	{
		await ValidateTransaction(omc);
		return await InsertOMC(omc);
	}
}
