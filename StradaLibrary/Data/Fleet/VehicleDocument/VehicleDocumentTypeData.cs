using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleDocument;

namespace StradaLibrary.Data.Fleet.VehicleDocument;

public static class VehicleDocumentTypeData
{
	public static async Task<int> InsertVehicleDocumentType(VehicleDocumentTypeModel vehicleDocumentType) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertDocumentType, vehicleDocumentType)).FirstOrDefault();

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

	public static async Task<int> SaveTransaction(VehicleDocumentTypeModel vehicleDocumentType)
	{
		await ValidateTransaction(vehicleDocumentType);
		return await InsertVehicleDocumentType(vehicleDocumentType);
	}
}
