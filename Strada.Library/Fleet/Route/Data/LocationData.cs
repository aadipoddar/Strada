using Strada.Library.Common;
using Strada.Library.DataAccess;
using Strada.Library.Fleet.Route.Models;
using Strada.Library.Operations.Data;
using Strada.Library.Operations.Models;

namespace Strada.Library.Fleet.Route.Data;

public static class LocationData
{
	public static async Task<int> InsertLocation(LocationModel location, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertLocation, location, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Location.");

	public static async Task DeleteTransaction(LocationModel location, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			location.Status = false;
			await InsertLocation(location, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = FleetNames.Location,
				RecordNo = location.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task RecoverTransaction(LocationModel location, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			location.Status = true;
			await InsertLocation(location, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = FleetNames.Location,
				RecordNo = location.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	private static async Task ValidateTransaction(LocationModel location)
	{
		location.Name = location.Name?.Trim().ToUpper() ?? string.Empty;
		location.Code = location.Code?.Trim().ToUpper() ?? string.Empty;
		location.Remarks = location.Remarks?.Trim() ?? string.Empty;
		location.Status = true;

		if (string.IsNullOrWhiteSpace(location.Name))
			throw new Exception("Location name is required. Please enter a valid location name.");

		if (location.Id == 0)
			location.Code = await GenerateCodes.GenerateLocationCode();

		if (string.IsNullOrWhiteSpace(location.Code))
			throw new Exception("Location code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(location.Remarks))
			location.Remarks = null;

		var allLocations = await CommonData.LoadTableData<LocationModel>(FleetNames.Location);

		var existingByName = allLocations.FirstOrDefault(x => x.Id != location.Id && x.Name.Equals(location.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Location name '{location.Name}' already exists. Please choose a different name.");

		var existingByCode = allLocations.FirstOrDefault(x => x.Id != location.Id && x.Code.Equals(location.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Location code '{location.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(LocationModel location, int userId, string platform)
	{
		await ValidateTransaction(location);

		var isUpdate = location.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<LocationModel>(FleetNames.Location, location.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertLocation(location, transaction);
			var diff = AuditTrailData.GetDifference(previous, location);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = FleetNames.Location,
				RecordNo = location.Name,
				RecordValue = isUpdate ? diff : null,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
