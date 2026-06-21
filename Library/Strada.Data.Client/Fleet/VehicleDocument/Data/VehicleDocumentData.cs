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

	// TODO: SaveTransaction has a Stream (file upload to blob storage) — needs the multipart-upload pattern.
}
