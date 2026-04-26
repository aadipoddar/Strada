using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.VehicleTrip.VehicleRoute;

namespace StradaLibrary.Data.VehicleTrip.VehicleRoute;

public static class VehicleDriverData
{
	public static async Task<int> InsertVehicleDriver(VehicleDriverModel vehicleDriver) =>
		(await SqlDataAccess.LoadData<int, dynamic>(VehicleTripNames.InsertVehicleDriver, vehicleDriver)).FirstOrDefault();

	public static async Task<List<VehicleDriverOverviewModel>> LoadVehicleDriverOverview()
	{
		var drivers = await CommonData.LoadTableDataByStatus<VehicleDriverModel>(VehicleTripNames.VehicleDriver);
		return [.. drivers.Select(d => new VehicleDriverOverviewModel
		{
			Id = d.Id,
			Name = d.Name,
			Mobile = d.Mobile,
			Code = d.Code,
			Remarks = d.Remarks,
			Status = d.Status
		})];
	}

	private static async Task ValidateTransaction(VehicleDriverModel vehicleDriver)
	{
		vehicleDriver.Name = vehicleDriver.Name?.Trim().ToUpper() ?? string.Empty;
		vehicleDriver.Mobile = vehicleDriver.Mobile?.Trim() ?? string.Empty;
		vehicleDriver.Code = vehicleDriver.Code?.Trim().ToUpper() ?? string.Empty;
		vehicleDriver.Remarks = vehicleDriver.Remarks?.Trim() ?? string.Empty;
		vehicleDriver.Status = true;

		if (string.IsNullOrWhiteSpace(vehicleDriver.Name))
			throw new Exception("Vehicle Driver name is required. Please enter a valid vehicle driver name.");

		if (string.IsNullOrWhiteSpace(vehicleDriver.Mobile))
			throw new Exception("Mobile is required. Please enter a valid mobile number.");

		if (!vehicleDriver.Mobile.ValidatePhoneNumber())
			throw new Exception("Mobile must be exactly 10 numeric digits.");

		if (vehicleDriver.Id == 0)
			vehicleDriver.Code = await GenerateCodes.GenerateVehicleDriverCode();

		if (string.IsNullOrWhiteSpace(vehicleDriver.Code))
			throw new Exception("Vehicle Driver code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(vehicleDriver.Remarks))
			vehicleDriver.Remarks = null;

		var allDrivers = await CommonData.LoadTableData<VehicleDriverModel>(VehicleTripNames.VehicleDriver);

		var existingByName = allDrivers.FirstOrDefault(vd => vd.Id != vehicleDriver.Id && vd.Name.Equals(vehicleDriver.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Vehicle Driver name '{vehicleDriver.Name}' already exists. Please choose a different name.");

		var existingByMobile = allDrivers.FirstOrDefault(vd => vd.Id != vehicleDriver.Id && vd.Mobile.Equals(vehicleDriver.Mobile, StringComparison.OrdinalIgnoreCase));
		if (existingByMobile is not null)
			throw new Exception($"Mobile '{vehicleDriver.Mobile}' already exists. Please choose a different mobile number.");

		var existingByCode = allDrivers.FirstOrDefault(vd => vd.Id != vehicleDriver.Id && vd.Code.Equals(vehicleDriver.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Vehicle Driver code '{vehicleDriver.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(VehicleDriverModel vehicleDriver)
	{
		await ValidateTransaction(vehicleDriver);
		return await InsertVehicleDriver(vehicleDriver);
	}
}
