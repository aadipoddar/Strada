using StradaLibrary.Data.Accounts.Masters;
using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleDocument;

namespace StradaLibrary.Data.Fleet.VehicleDocument;

public static class VehicleDocumentData
{
	public static async Task<int> InsertVehicleDocument(VehicleDocumentModel vehicleDocument) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertDocument, vehicleDocument)).FirstOrDefault();

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

		if (vehicleDocument.Id > 0)
		{
			var vehicleDocuments = await CommonData.LoadTableData<VehicleDocumentModel>(FleetNames.VehicleDocument);

			var duplicateTransaction = vehicleDocuments.FirstOrDefault(vd => vd.Id != vehicleDocument.Id && vd.TransactionNo.Equals(vehicleDocument.TransactionNo, StringComparison.OrdinalIgnoreCase));
			if (duplicateTransaction is not null)
				throw new Exception($"Transaction No '{vehicleDocument.TransactionNo}' already exists. Please enter a different transaction number.");

			var existingVehicleDocument = await CommonData.LoadTableDataById<VehicleDocumentModel>(FleetNames.VehicleDocument, vehicleDocument.Id)
				?? throw new Exception("Vehicle Document transaction not found for updating.");

			await FinancialYearData.ValidateFinancialYear(existingVehicleDocument.TransactionDateTime);
		}
		else
		{
			var vehicleDocuments = await CommonData.LoadTableData<VehicleDocumentModel>(FleetNames.VehicleDocument);
			var duplicateTransaction = vehicleDocuments.FirstOrDefault(vd => vd.TransactionNo.Equals(vehicleDocument.TransactionNo, StringComparison.OrdinalIgnoreCase));
			if (duplicateTransaction is not null)
				throw new Exception($"Transaction No '{vehicleDocument.TransactionNo}' already exists. Please enter a different transaction number.");
		}

		await FinancialYearData.ValidateFinancialYear(vehicleDocument.TransactionDateTime);
		var financialYear = await FinancialYearData.LoadFinancialYearByDateTime(vehicleDocument.TransactionDateTime);
		vehicleDocument.FinancialYearId = financialYear.Id;

		vehicleDocument.Remarks = vehicleDocument.Remarks?.Trim() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(vehicleDocument.Remarks))
			vehicleDocument.Remarks = null;
	}

	public static async Task<int> SaveTransaction(
		VehicleDocumentModel vehicleDocument,
		Stream pendingDocumentStream = null,
		string pendingDocumentFileName = null,
		string documentUrlToDelete = null)
	{
		await ValidateTransaction(vehicleDocument);

		if (pendingDocumentStream is not null && !string.IsNullOrWhiteSpace(pendingDocumentFileName))
		{
			var fileName = $"{Guid.NewGuid()}_{pendingDocumentFileName}";
			vehicleDocument.DocumentUrl = await BlobStorageAccess.UploadFileToBlobStorage(pendingDocumentStream, fileName, BlobStorageContainers.vehicledocument);
		}

		vehicleDocument.Id = await InsertVehicleDocument(vehicleDocument);

		if (!string.IsNullOrWhiteSpace(documentUrlToDelete))
		{
			var oldFileName = documentUrlToDelete.Split('/').Last();
			await BlobStorageAccess.DeleteFileFromBlobStorage(oldFileName, BlobStorageContainers.vehicledocument);
		}

		return vehicleDocument.Id;
	}
}