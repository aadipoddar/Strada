using StradaLibrary.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Fleet.OMC.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Operations.Models;

namespace StradaLibrary.Fleet.OMC.Data;

public static class OMCData
{
	public static async Task<int> InsertOMC(OMCModel omc, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertOMC, omc, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert OMC.");

	public static async Task DeleteTransaction(OMCModel omc, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			omc.Status = false;
			await InsertOMC(omc, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = FleetNames.OMC,
				RecordNo = omc.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task RecoverTransaction(OMCModel omc, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			omc.Status = true;
			await InsertOMC(omc, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = FleetNames.OMC,
				RecordNo = omc.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

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

	public static async Task<int> SaveTransaction(OMCModel omc, int userId, string platform)
	{
		await ValidateTransaction(omc);

		var isUpdate = omc.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<OMCModel>(FleetNames.OMC, omc.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertOMC(omc, transaction);
			var diff = AuditTrailData.GetDifference(previous, omc);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = FleetNames.OMC,
				RecordNo = omc.Name,
				RecordValue = diff,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
