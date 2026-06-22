using Strada.Models.Common;

namespace Strada.Data.DataAccess;

public static class BlobStorageAccess
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(BlobStorageAccess));

	public static async Task<string> UploadFileToBlobStorage(Stream file, string fileName, BlobStorageContainers container) =>
		await Api.Upload<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(UploadFileToBlobStorage)), file, fileName, new { fileName, container });

	public static async Task DeleteFileFromBlobStorage(string fileName, BlobStorageContainers container) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteFileFromBlobStorage)), new { }, new { fileName, container });

	public static async Task<List<string>> ListFilesInBlobStorage(BlobStorageContainers container) =>
		await Api.Get<List<string>>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ListFilesInBlobStorage)), new { container });

	public static async Task<(MemoryStream fileStream, string contentType)> DownloadFileFromBlobStorage(string url) =>
		await Api.GetForFile(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DownloadFileFromBlobStorage)), new { url });
}