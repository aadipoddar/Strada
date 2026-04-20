using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleDocument;

namespace StradaLibrary.Data.Fleet.VehicleDocument;

public static class VehicleDocumentTypeData
{
	public static async Task<int> InsertVehicleDocumentType(VehicleDocumentTypeModel vehicleDocumentType) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertDocumentType, vehicleDocumentType)).FirstOrDefault();
}
