using Strada.Models.Common;
using Strada.Models.Fleet.VehicleDocument;

namespace Strada.Data.Fleet.VehicleDocument.Data;

public static class VehicleDocumentTypeData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(VehicleDocumentTypeData));

	public static async Task DeleteTransaction(VehicleDocumentTypeModel vehicleDocumentType, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), vehicleDocumentType, new { userId, platform });

	public static async Task RecoverTransaction(VehicleDocumentTypeModel vehicleDocumentType, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), vehicleDocumentType, new { userId, platform });

	public static async Task<int> SaveTransaction(VehicleDocumentTypeModel vehicleDocumentType, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), vehicleDocumentType, new { userId, platform });
}
