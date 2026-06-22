using Strada.Models.Common;
using Strada.Models.Fleet.VehicleDocument;

namespace Strada.Data.Fleet.VehicleDocument.Data;

public static class VehicleDocumentData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleDocumentData));

	public static async Task DeleteTransaction(VehicleDocumentModel vehicleDocument) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), vehicleDocument);

	public static async Task RecoverTransaction(VehicleDocumentModel vehicleDocument) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), vehicleDocument);

	public static async Task<int> SaveTransaction(VehicleDocumentModel vehicleDocument) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), vehicleDocument);
}
