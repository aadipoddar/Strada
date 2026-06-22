using Strada.Library.Common;
using Strada.Library.DataAccess;
using Strada.Library.Fleet.Vehicle.Models;
using Strada.Library.Operations.Data;
using Strada.Library.Operations.Models;

namespace Strada.Library.Fleet.Vehicle.Data;

public static class VehicleData
{
	public static async Task<int> InsertVehicle(VehicleModel vehicle, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicle, vehicle, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Vehicle.");

	public static async Task DeleteTransaction(VehicleModel vehicle, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			vehicle.Status = false;
			await InsertVehicle(vehicle, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = FleetNames.Vehicle,
				RecordNo = vehicle.Code,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task RecoverTransaction(VehicleModel vehicle, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			vehicle.Status = true;
			await InsertVehicle(vehicle, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = FleetNames.Vehicle,
				RecordNo = vehicle.Code,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	private static async Task ValidateTransaction(VehicleModel vehicle)
	{
		vehicle.Code = vehicle.Code?.Trim().ToUpper() ?? string.Empty;
		vehicle.ShortCode = vehicle.ShortCode?.Trim().ToUpper() ?? string.Empty;
		vehicle.ChasisCode = vehicle.ChasisCode?.Trim().ToUpper() ?? string.Empty;
		vehicle.EngineCode = vehicle.EngineCode?.Trim().ToUpper() ?? string.Empty;
		vehicle.Remarks = vehicle.Remarks?.Trim() ?? string.Empty;
		vehicle.Status = true;

		if (string.IsNullOrWhiteSpace(vehicle.Code))
			throw new Exception("Vehicle code is required. Please enter a valid vehicle code.");

		if (string.IsNullOrWhiteSpace(vehicle.ShortCode))
			throw new Exception("Vehicle short code is required. Please enter a valid short code.");

		if (vehicle.VehicleTypeId <= 0)
			throw new Exception("Vehicle Type is required. Please select a valid vehicle type.");

		if (vehicle.CompanyId <= 0)
			throw new Exception("Company is required. Please select a valid company.");

		if (vehicle.OpeningKM < 0)
			throw new Exception("Opening KM cannot be negative.");

		if (string.IsNullOrWhiteSpace(vehicle.ChasisCode))
			vehicle.ChasisCode = null;

		if (string.IsNullOrWhiteSpace(vehicle.EngineCode))
			vehicle.EngineCode = null;

		if (string.IsNullOrWhiteSpace(vehicle.Remarks))
			vehicle.Remarks = null;

		var allVehicles = await CommonData.LoadTableData<VehicleModel>(FleetNames.Vehicle);

		var existingByCode = allVehicles.FirstOrDefault(v => v.Id != vehicle.Id && v.Code.Equals(vehicle.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Vehicle code '{vehicle.Code}' already exists. Please choose a different code.");

		if (!string.IsNullOrWhiteSpace(vehicle.ChasisCode))
		{
			var existingByChasis = allVehicles.FirstOrDefault(v => v.Id != vehicle.Id && !string.IsNullOrWhiteSpace(v.ChasisCode) && v.ChasisCode.Equals(vehicle.ChasisCode, StringComparison.OrdinalIgnoreCase));
			if (existingByChasis is not null)
				throw new Exception($"Chasis code '{vehicle.ChasisCode}' already exists. Please choose a different chasis code.");
		}

		if (!string.IsNullOrWhiteSpace(vehicle.EngineCode))
		{
			var existingByEngine = allVehicles.FirstOrDefault(v => v.Id != vehicle.Id && !string.IsNullOrWhiteSpace(v.EngineCode) && v.EngineCode.Equals(vehicle.EngineCode, StringComparison.OrdinalIgnoreCase));
			if (existingByEngine is not null)
				throw new Exception($"Engine code '{vehicle.EngineCode}' already exists. Please choose a different engine code.");
		}
	}

	public static async Task<int> SaveTransaction(VehicleModel vehicle, int userId, string platform)
	{
		await ValidateTransaction(vehicle);

		var isUpdate = vehicle.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<VehicleModel>(FleetNames.Vehicle, vehicle.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertVehicle(vehicle, transaction);
			var diff = AuditTrailData.GetDifference(previous, vehicle);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = FleetNames.Vehicle,
				RecordNo = vehicle.Code,
				RecordValue = isUpdate ? diff : null,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
