using Strada.Data.DataAccess;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.DataAccess;

public class BlobStorageAccessEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(BlobStorageAccessEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(BlobStorageAccess.UploadFileToBlobStorage),
			async (IFormFile file, string fileName, BlobStorageContainers container) =>
			{
				await using var stream = file.OpenReadStream();
				return await BlobStorageAccess.UploadFileToBlobStorage(stream, fileName, container);
			}).DisableAntiforgery();

		group.MapPost(nameof(BlobStorageAccess.DeleteFileFromBlobStorage), BlobStorageAccess.DeleteFileFromBlobStorage);
		group.MapGet(nameof(BlobStorageAccess.ListFilesInBlobStorage), BlobStorageAccess.ListFilesInBlobStorage);

		group.MapGet(nameof(BlobStorageAccess.DownloadFileFromBlobStorage), async (string url) =>
		{
			var (stream, contentType) = await BlobStorageAccess.DownloadFileFromBlobStorage(url);
			return Results.File(stream, contentType ?? Helper.ExportContentType);
		});
	}
}
