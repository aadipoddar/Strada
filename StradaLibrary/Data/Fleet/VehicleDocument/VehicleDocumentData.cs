using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.VehicleDocument;

namespace StradaLibrary.Data.Fleet.VehicleDocument;

public static class VehicleDocumentData
{
	public static async Task<int> InsertVehicleDocument(VehicleDocumentModel vehicleDocument) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertDocument, vehicleDocument)).FirstOrDefault();
}