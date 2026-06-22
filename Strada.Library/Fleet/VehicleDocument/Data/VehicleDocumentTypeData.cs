using Strada.Library.Common;
using Strada.Library.DataAccess;
using Strada.Library.Fleet.VehicleDocument.Models;
using Strada.Library.Operations.Data;
using Strada.Library.Operations.Models;

namespace Strada.Library.Fleet.VehicleDocument.Data;

public static class VehicleDocumentTypeData
{
	public static async Task<int> InsertVehicleDocumentType(VehicleDocumentTypeModel vehicleDocumentType, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertDocumentType, vehicleDocumentType, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Vehicle Document Type.");

	public static async Task DeleteTransaction(VehicleDocumentTypeModel vehicleDocumentType, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			vehicleDocumentType.Status = false;
			await InsertVehicleDocumentType(vehicleDocumentType, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = FleetNames.VehicleDocumentType,
				RecordNo = vehicleDocumentType.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task RecoverTransaction(VehicleDocumentTypeModel vehicleDocumentType, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			vehicleDocumentType.Status = true;
			await InsertVehicleDocumentType(vehicleDocumentType, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = FleetNames.VehicleDocumentType,
				RecordNo = vehicleDocumentType.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	private static async Task ValidateTransaction(VehicleDocumentTypeModel vehicleDocumentType)
	{
		vehicleDocumentType.Name = vehicleDocumentType.Name?.Trim().ToUpper() ?? string.Empty;
		vehicleDocumentType.Code = vehicleDocumentType.Code?.Trim().ToUpper() ?? string.Empty;
		vehicleDocumentType.Remarks = vehicleDocumentType.Remarks?.Trim() ?? string.Empty;
		vehicleDocumentType.Status = true;

		if (string.IsNullOrWhiteSpace(vehicleDocumentType.Name))
			throw new Exception("Vehicle Document Type name is required. Please enter a valid document type name.");

		if (vehicleDocumentType.Id == 0)
			vehicleDocumentType.Code = await GenerateCodes.GenerateVehicleDocumentTypeCode();

		if (string.IsNullOrWhiteSpace(vehicleDocumentType.Code))
			throw new Exception("Vehicle Document Type code is required. Please try again.");

		if (vehicleDocumentType.Rate < 0)
			throw new Exception("Rate cannot be negative.");

		if (string.IsNullOrWhiteSpace(vehicleDocumentType.Remarks))
			vehicleDocumentType.Remarks = null;

		var allTypes = await CommonData.LoadTableData<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType);

		var existingByName = allTypes.FirstOrDefault(vdt => vdt.Id != vehicleDocumentType.Id && vdt.Name.Equals(vehicleDocumentType.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Vehicle Document Type name '{vehicleDocumentType.Name}' already exists. Please choose a different name.");

		var existingByCode = allTypes.FirstOrDefault(vdt => vdt.Id != vehicleDocumentType.Id && vdt.Code.Equals(vehicleDocumentType.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Vehicle Document Type code '{vehicleDocumentType.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(VehicleDocumentTypeModel vehicleDocumentType, int userId, string platform)
	{
		await ValidateTransaction(vehicleDocumentType);

		var isUpdate = vehicleDocumentType.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<VehicleDocumentTypeModel>(FleetNames.VehicleDocumentType, vehicleDocumentType.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertVehicleDocumentType(vehicleDocumentType, transaction);
			var diff = AuditTrailData.GetDifference(previous, vehicleDocumentType);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = FleetNames.VehicleDocumentType,
				RecordNo = vehicleDocumentType.Name,
				RecordValue = isUpdate ? diff : null,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
