using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.Vehicle;

namespace StradaLibrary.Data.Fleet.Vehicle;

public static class VehicleData
{
	public static async Task<int> InsertVehicle(VehicleModel vehicle) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertVehicle, vehicle)).FirstOrDefault();

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

	public static async Task<int> SaveTransaction(VehicleModel vehicle)
	{
		await ValidateTransaction(vehicle);
		return await InsertVehicle(vehicle);
	}
}
