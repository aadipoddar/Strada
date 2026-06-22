using Strada.Data.Fleet.Analysis;
using Strada.Models.Common;

namespace Strada.Api.Endpoint.Fleet.Analysis;

public class AnalysisDataEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var endpoint = Helper.SanitizeClassName(nameof(AnalysisDataEndpoint));
		var group = app.MapGroup(endpoint).WithTags(endpoint);

		group.MapGet(nameof(AnalysisData.LoadDashboardMonthlyTrend), AnalysisData.LoadDashboardMonthlyTrend);
		group.MapGet(nameof(AnalysisData.LoadDashboardTopVehicles), AnalysisData.LoadDashboardTopVehicles);
		group.MapGet(nameof(AnalysisData.LoadDashboardExpenseBreakdown), AnalysisData.LoadDashboardExpenseBreakdown);
	}
}
