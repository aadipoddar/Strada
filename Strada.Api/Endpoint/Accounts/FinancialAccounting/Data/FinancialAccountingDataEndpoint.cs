using Strada.Data.Accounts.FinancialAccounting.Data;
using Strada.Models.Accounts.FinancialAccounting;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Accounts.FinancialAccounting.Data;

public class FinancialAccountingDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(FinancialAccountingDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapGet(nameof(FinancialAccountingData.LoadFinancialAccountingByVoucherReference),
			(int VoucherId, int ReferenceId, string ReferenceNo) => FinancialAccountingData.LoadFinancialAccountingByVoucherReference(VoucherId, ReferenceId, ReferenceNo));

		group.MapGet(nameof(FinancialAccountingData.LoadTrialBalanceByCompanyDate), FinancialAccountingData.LoadTrialBalanceByCompanyDate);

		group.MapPost(nameof(FinancialAccountingData.DeleteTransaction),
			(FinancialAccountingModel accounting) => FinancialAccountingData.DeleteTransaction(accounting));

		group.MapPost(nameof(FinancialAccountingData.RecoverTransaction), FinancialAccountingData.RecoverTransaction);

		group.MapPost(nameof(FinancialAccountingData.SaveTransaction),
			(SaveFinancialAccountingRequest request) => FinancialAccountingData.SaveTransaction(request.Accounting, request.Ledgers, request.Recover));

		group.MapPost(nameof(FinancialAccountingData.SaveBRSDates), FinancialAccountingData.SaveBRSDates);
	}

	private sealed record SaveFinancialAccountingRequest(FinancialAccountingModel Accounting, List<FinancialAccountingLedgerModel> Ledgers, bool Recover);
}
