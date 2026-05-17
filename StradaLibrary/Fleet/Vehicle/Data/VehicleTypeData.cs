using StradaLibrary.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Fleet.Vehicle.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Operations.Models;

namespace StradaLibrary.Fleet.Vehicle.Data;

public static class VehicleTypeData
{
	public static async Task<int> InsertVehicleType(VehicleTypeModel vehicleType, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleType, vehicleType, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Vehicle Type.");

	public static async Task DeleteTransaction(VehicleTypeModel vehicleType, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			vehicleType.Status = false;
			await InsertVehicleType(vehicleType, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = FleetNames.VehicleType,
				RecordNo = vehicleType.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task RecoverTransaction(VehicleTypeModel vehicleType, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			vehicleType.Status = true;
			await InsertVehicleType(vehicleType, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = FleetNames.VehicleType,
				RecordNo = vehicleType.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	private static async Task ValidateTransaction(VehicleTypeModel vehicleType)
	{
		vehicleType.Name = vehicleType.Name?.Trim().ToUpper() ?? string.Empty;
		vehicleType.Code = vehicleType.Code?.Trim().ToUpper() ?? string.Empty;
		vehicleType.Remarks = vehicleType.Remarks?.Trim() ?? string.Empty;
		vehicleType.Status = true;

		if (string.IsNullOrWhiteSpace(vehicleType.Name))
			throw new Exception("Vehicle Type name is required. Please enter a valid vehicle type name.");

		if (vehicleType.Id == 0)
			vehicleType.Code = await GenerateCodes.GenerateVehicleTypeCode();

		if (string.IsNullOrWhiteSpace(vehicleType.Code))
			throw new Exception("Vehicle Type code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(vehicleType.Remarks))
			vehicleType.Remarks = null;

		var vehicleTypesAll = await CommonData.LoadTableData<VehicleTypeModel>(FleetNames.VehicleType);

		var existingByName = vehicleTypesAll.FirstOrDefault(vt => vt.Id != vehicleType.Id && vt.Name.Equals(vehicleType.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Vehicle Type name '{vehicleType.Name}' already exists. Please choose a different name.");

		var existingByCode = vehicleTypesAll.FirstOrDefault(vt => vt.Id != vehicleType.Id && vt.Code.Equals(vehicleType.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Vehicle Type code '{vehicleType.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(VehicleTypeModel vehicleType, int userId, string platform)
	{
		await ValidateTransaction(vehicleType);

		var isUpdate = vehicleType.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<VehicleTypeModel>(FleetNames.VehicleType, vehicleType.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertVehicleType(vehicleType, transaction);
			var diff = AuditTrailData.GetDifference(previous, vehicleType);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = FleetNames.VehicleType,
				RecordNo = vehicleType.Name,
				RecordValue = diff,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
