using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.Vehicle;

namespace StradaLibrary.Data.Fleet.Vehicle;

public static class VehicleExpenseTypeData
{
	public static async Task<int> InsertVehicleExpenseType(VehicleExpenseTypeModel vehicleExpenseType) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleExpenseType, vehicleExpenseType)).FirstOrDefault();

	private static async Task ValidateTransaction(VehicleExpenseTypeModel vehicleExpenseType)
	{
		vehicleExpenseType.Name = vehicleExpenseType.Name?.Trim().ToUpper() ?? string.Empty;
		vehicleExpenseType.Code = vehicleExpenseType.Code?.Trim().ToUpper() ?? string.Empty;
		vehicleExpenseType.Remarks = vehicleExpenseType.Remarks?.Trim() ?? string.Empty;
		vehicleExpenseType.Status = true;

		if (string.IsNullOrWhiteSpace(vehicleExpenseType.Name))
			throw new Exception("Vehicle Expense Type name is required. Please enter a valid name.");

		if (vehicleExpenseType.Id == 0)
			vehicleExpenseType.Code = await GenerateCodes.GenerateVehicleExpenseTypeCode();

		if (string.IsNullOrWhiteSpace(vehicleExpenseType.Code))
			throw new Exception("Vehicle Expense Type code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(vehicleExpenseType.Remarks))
			vehicleExpenseType.Remarks = null;

		var allTypes = await CommonData.LoadTableData<VehicleExpenseTypeModel>(FleetNames.VehicleExpenseType);

		var existingByName = allTypes.FirstOrDefault(x => x.Id != vehicleExpenseType.Id && x.Name.Equals(vehicleExpenseType.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Vehicle Expense Type name '{vehicleExpenseType.Name}' already exists. Please choose a different name.");

		var existingByCode = allTypes.FirstOrDefault(x => x.Id != vehicleExpenseType.Id && x.Code.Equals(vehicleExpenseType.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Vehicle Expense Type code '{vehicleExpenseType.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(VehicleExpenseTypeModel vehicleExpenseType)
	{
		await ValidateTransaction(vehicleExpenseType);
		return await InsertVehicleExpenseType(vehicleExpenseType);
	}
}
