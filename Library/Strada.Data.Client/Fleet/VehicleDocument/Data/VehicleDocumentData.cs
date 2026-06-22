using Strada.Models.Common;
using Strada.Models.Fleet.VehicleDocument;

namespace Strada.Data.Fleet.VehicleDocument.Data;

public static class VehicleDocumentData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleDocumentData));

	public static Task DeleteTransaction(VehicleDocumentModel vehicleDocument) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), vehicleDocument);

	public static Task RecoverTransaction(VehicleDocumentModel vehicleDocument) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), vehicleDocument);

	// The page uploads the document to blob storage separately (BlobStorageAccess) and passes
	// only the model here, so this is a plain model post — the real method's Stream overload params default to null.
	public static Task<int> SaveTransaction(VehicleDocumentModel vehicleDocument) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), vehicleDocument);
}
