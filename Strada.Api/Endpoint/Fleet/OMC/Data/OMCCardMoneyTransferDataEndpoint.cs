using Strada.Data.Fleet.OMC.Data;
using Strada.Models.Common;
using Strada.Models.Fleet.OMC;

namespace Strada.Api.Endpoint.Fleet.OMC.Data;

public class OMCCardMoneyTransferDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(OMCCardMoneyTransferDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapPost(nameof(OMCCardMoneyTransferData.DeleteTransaction),
			(OMCCardMoneyTransferModel model) => OMCCardMoneyTransferData.DeleteTransaction(model));

		group.MapPost(nameof(OMCCardMoneyTransferData.RecoverTransaction), OMCCardMoneyTransferData.RecoverTransaction);

		group.MapPost(nameof(OMCCardMoneyTransferData.SaveTransaction),
			(OMCCardMoneyTransferSaveRequest request) => OMCCardMoneyTransferData.SaveTransaction(request.OMCCardMoneyTransfer, request.TransferDetails, request.Recover));
	}

	private sealed record OMCCardMoneyTransferSaveRequest(
		OMCCardMoneyTransferModel OMCCardMoneyTransfer,
		List<OMCCardMoneyTransferDetailsModel> TransferDetails,
		bool Recover);
}
