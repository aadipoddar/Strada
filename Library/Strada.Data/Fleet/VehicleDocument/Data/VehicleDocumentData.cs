using Strada.Data.Common;
using Strada.Data.DataAccess;
using Strada.Data.Operations.Data;
using Strada.Models.Common;
using Strada.Models.Fleet.VehicleDocument;
using Strada.Models.Operations;

namespace Strada.Data.Fleet.VehicleDocument.Data;

public static class VehicleDocumentData
{
	public static async Task<int> InsertVehicleDocument(VehicleDocumentModel vehicleDocument, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertDocument, vehicleDocument, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Vehicle Document.");

	public static async Task DeleteTransaction(VehicleDocumentModel vehicleDocument) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			vehicleDocument.Status = false;
			await InsertVehicleDocument(vehicleDocument, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = FleetNames.VehicleDocument,
				RecordNo = vehicleDocument.TransactionNo,
				CreatedBy = vehicleDocument.LastModifiedBy.Value,
				CreatedFromPlatform = vehicleDocument.LastModifiedFromPlatform
			}, transaction);
		});

	public static async Task RecoverTransaction(VehicleDocumentModel vehicleDocument) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			vehicleDocument.Status = true;
			await InsertVehicleDocument(vehicleDocument, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = FleetNames.VehicleDocument,
				RecordNo = vehicleDocument.TransactionNo,
				CreatedBy = vehicleDocument.LastModifiedBy.Value,
				CreatedFromPlatform = vehicleDocument.LastModifiedFromPlatform
			}, transaction);
		});

	private static async Task ValidateTransaction(VehicleDocumentModel vehicleDocument)
	{
		vehicleDocument.TransactionNo = vehicleDocument.TransactionNo?.Trim() ?? string.Empty;
		vehicleDocument.TransactionNo = vehicleDocument.TransactionNo.ToUpper();

		vehicleDocument.Remarks = vehicleDocument.Remarks?.Trim() ?? string.Empty;
		vehicleDocument.Status = true;

		if (string.IsNullOrWhiteSpace(vehicleDocument.TransactionNo))
			throw new Exception("Transaction No is required. Please enter a valid transaction number.");

		if (vehicleDocument.VehicleDocumentTypeId <= 0)
			throw new Exception("Document Type is required. Please select a valid document type.");

		if (vehicleDocument.VehicleId <= 0)
			throw new Exception("Vehicle is required. Please select a valid vehicle.");

		if (vehicleDocument.Rate < 0)
			throw new Exception("Rate must be a positive value.");

		if (vehicleDocument.CurrentKM < 0)
			throw new Exception("Current KM cannot be negative.");

		if (vehicleDocument.RenewalDate < vehicleDocument.TransactionDateTime)
			throw new Exception("Renewal Date cannot be earlier than Transaction Date.");

		var vehicleDocumentsAll = await CommonData.LoadTableData<VehicleDocumentModel>(FleetNames.VehicleDocument);

		var existingByTransactionNo = vehicleDocumentsAll.FirstOrDefault(vt => vt.Id != vehicleDocument.Id && vt.TransactionNo.Equals(vehicleDocument.TransactionNo, StringComparison.OrdinalIgnoreCase));
		if (existingByTransactionNo is not null)
			throw new Exception($"Vehicle Document transaction number '{vehicleDocument.TransactionNo}' already exists. Please choose a different transaction number.");

		vehicleDocument.Remarks = vehicleDocument.Remarks?.Trim() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(vehicleDocument.Remarks))
			vehicleDocument.Remarks = null;
	}

	public static async Task<int> SaveTransaction(VehicleDocumentModel vehicleDocument)
	{
		await ValidateTransaction(vehicleDocument);

		var isUpdate = vehicleDocument.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<VehicleDocumentModel>(FleetNames.VehicleDocument, vehicleDocument.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertVehicleDocument(vehicleDocument, transaction);
			var current = await CommonData.LoadTableDataById<VehicleDocumentModel>(FleetNames.VehicleDocument, id, transaction);
			var diff = AuditTrailData.GetDifference(previous, current);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = FleetNames.VehicleDocument,
				RecordNo = vehicleDocument.TransactionNo,
				RecordValue = isUpdate ? diff : null,
				CreatedBy = isUpdate ? vehicleDocument.LastModifiedBy.Value : vehicleDocument.CreatedBy,
				CreatedFromPlatform = isUpdate ? vehicleDocument.LastModifiedFromPlatform : vehicleDocument.CreatedFromPlatform
			}, transaction);
			return id;
		});
	}
}
