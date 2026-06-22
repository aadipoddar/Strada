using Strada.Data.Common;
using Strada.Models.Common;
using Strada.Models.Operations;

namespace Strada.Api.Endpoint.Common;

public class DecodeCodeEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(DecodeCodeEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapGet(nameof(DecodeCode.DecodeTransactionNo),
			async (string transactionNo, bool? pdf, bool? excel, CodeType? codeType) =>
			{
				var decoded = await DecodeCode.DecodeTransactionNo(transactionNo, pdf ?? true, excel ?? true, codeType);
				if (decoded is null)
					return null;

				return new DecodeTransactionNoResult
				{
					CodeType = decoded.CodeType,
					PageRouteName = decoded.PageRouteName,
					Pdf = ToFile(decoded.PDFStream),
					Excel = ToFile(decoded.ExcelStream),
				};
			});
	}

	private static FileResult ToFile((MemoryStream stream, string fileName) file) =>
		file.stream is null ? null : new FileResult { Bytes = file.stream.ToArray(), FileName = file.fileName };
}
