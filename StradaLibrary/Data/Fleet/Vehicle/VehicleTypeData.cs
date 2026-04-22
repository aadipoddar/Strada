using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.Vehicle;

namespace StradaLibrary.Data.Fleet.Vehicle;

public static class VehicleTypeData
{
	public static async Task<int> InsertVehicleType(VehicleTypeModel vehicleType) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleType, vehicleType)).FirstOrDefault();

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

	public static async Task<int> SaveTransaction(VehicleTypeModel vehicleType)
	{
		await ValidateTransaction(vehicleType);
		return await InsertVehicleType(vehicleType);
	}
}