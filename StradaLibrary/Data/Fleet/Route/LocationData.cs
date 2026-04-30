using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.Route;

namespace StradaLibrary.Data.Fleet.Route;

public static class LocationData
{
	public static async Task<int> InsertLocation(LocationModel location) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertLocation, location)).FirstOrDefault();

	private static async Task ValidateTransaction(LocationModel location)
	{
		location.Name = location.Name?.Trim().ToUpper() ?? string.Empty;
		location.Code = location.Code?.Trim().ToUpper() ?? string.Empty;
		location.Remarks = location.Remarks?.Trim() ?? string.Empty;
		location.Status = true;

		if (string.IsNullOrWhiteSpace(location.Name))
			throw new Exception("Location name is required. Please enter a valid location name.");

		if (location.Id == 0)
			location.Code = await GenerateCodes.GenerateLocationCode();

		if (string.IsNullOrWhiteSpace(location.Code))
			throw new Exception("Location code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(location.Remarks))
			location.Remarks = null;

		var allLocations = await CommonData.LoadTableData<LocationModel>(FleetNames.Location);

		var existingByName = allLocations.FirstOrDefault(x => x.Id != location.Id && x.Name.Equals(location.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Location name '{location.Name}' already exists. Please choose a different name.");

		var existingByCode = allLocations.FirstOrDefault(x => x.Id != location.Id && x.Code.Equals(location.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Location code '{location.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(LocationModel location)
	{
		await ValidateTransaction(location);
		return await InsertLocation(location);
	}
}
