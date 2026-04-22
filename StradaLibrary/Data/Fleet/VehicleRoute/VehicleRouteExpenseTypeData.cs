using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleRoute;

namespace StradaLibrary.Data.Fleet.VehicleRoute;

public static class VehicleRouteExpenseTypeData
{
	public static async Task<int> InsertVehicleRouteExpenseType(VehicleRouteExpenseTypeModel vehicleRouteExpenseType) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicleRouteExpenseType, vehicleRouteExpenseType)).FirstOrDefault();

	private static async Task ValidateTransaction(VehicleRouteExpenseTypeModel vehicleRouteExpenseType)
	{
		vehicleRouteExpenseType.Name = vehicleRouteExpenseType.Name?.Trim().ToUpper() ?? string.Empty;
		vehicleRouteExpenseType.Code = vehicleRouteExpenseType.Code?.Trim().ToUpper() ?? string.Empty;
		vehicleRouteExpenseType.Remarks = vehicleRouteExpenseType.Remarks?.Trim() ?? string.Empty;
		vehicleRouteExpenseType.Status = true;

		if (string.IsNullOrWhiteSpace(vehicleRouteExpenseType.Name))
			throw new Exception("Vehicle Route Expense Type name is required. Please enter a valid name.");

		if (vehicleRouteExpenseType.Id == 0)
			vehicleRouteExpenseType.Code = await GenerateCodes.GenerateVehicleRouteExpenseTypeCode();

		if (string.IsNullOrWhiteSpace(vehicleRouteExpenseType.Code))
			throw new Exception("Vehicle Route Expense Type code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(vehicleRouteExpenseType.Remarks))
			vehicleRouteExpenseType.Remarks = null;

		var allTypes = await CommonData.LoadTableData<VehicleRouteExpenseTypeModel>(FleetNames.VehicleRouteExpenseType);

		var existingByName = allTypes.FirstOrDefault(x => x.Id != vehicleRouteExpenseType.Id && x.Name.Equals(vehicleRouteExpenseType.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Vehicle Route Expense Type name '{vehicleRouteExpenseType.Name}' already exists. Please choose a different name.");

		var existingByCode = allTypes.FirstOrDefault(x => x.Id != vehicleRouteExpenseType.Id && x.Code.Equals(vehicleRouteExpenseType.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Vehicle Route Expense Type code '{vehicleRouteExpenseType.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(VehicleRouteExpenseTypeModel vehicleRouteExpenseType)
	{
		await ValidateTransaction(vehicleRouteExpenseType);
		return await InsertVehicleRouteExpenseType(vehicleRouteExpenseType);
	}
}
