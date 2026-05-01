using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.Route;

namespace StradaLibrary.Data.Fleet.Route;

public static class DriverData
{
	public static async Task<int> InsertDriver(DriverModel driver) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertDriver, driver)).FirstOrDefault();

	public static async Task<List<DriverOverviewModel>> LoadDriverOverview()
	{
		var drivers = await CommonData.LoadTableDataByStatus<DriverModel>(FleetNames.Driver);
		return [.. drivers.Select(d => new DriverOverviewModel
		{
			Id = d.Id,
			Name = d.Name,
			Mobile = d.Mobile,
			Code = d.Code,
			Remarks = d.Remarks,
			Status = d.Status
		})];
	}

	private static async Task ValidateTransaction(DriverModel driver)
	{
		driver.Name = driver.Name?.Trim().ToUpper() ?? string.Empty;
		driver.Mobile = driver.Mobile?.Trim() ?? string.Empty;
		driver.Code = driver.Code?.Trim().ToUpper() ?? string.Empty;
		driver.Remarks = driver.Remarks?.Trim() ?? string.Empty;
		driver.Status = true;

		if (string.IsNullOrWhiteSpace(driver.Name))
			throw new Exception("Driver name is required. Please enter a valid driver name.");

		if (string.IsNullOrWhiteSpace(driver.Mobile))
			throw new Exception("Mobile is required. Please enter a valid mobile number.");

		if (!driver.Mobile.ValidatePhoneNumber())
			throw new Exception("Mobile must be exactly 10 numeric digits.");

		if (driver.Id == 0)
			driver.Code = await GenerateCodes.GenerateDriverCode();

		if (string.IsNullOrWhiteSpace(driver.Code))
			throw new Exception("Driver code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(driver.Remarks))
			driver.Remarks = null;

		var allDrivers = await CommonData.LoadTableData<DriverModel>(FleetNames.Driver);

		var existingByMobile = allDrivers.FirstOrDefault(vd => vd.Id != driver.Id && vd.Mobile.Equals(driver.Mobile, StringComparison.OrdinalIgnoreCase));
		if (existingByMobile is not null)
			throw new Exception($"Mobile '{driver.Mobile}' already exists. Please choose a different mobile number.");

		var existingByCode = allDrivers.FirstOrDefault(vd => vd.Id != driver.Id && vd.Code.Equals(driver.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Driver code '{driver.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(DriverModel driver)
	{
		await ValidateTransaction(driver);
		return await InsertDriver(driver);
	}
}
