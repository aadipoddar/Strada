using Strada.Models.Common;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Data.Fleet.Vehicle.Data;

public static class ExpenseTypeData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(ExpenseTypeData));

	public static async Task DeleteTransaction(ExpenseTypeModel expenseType, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), expenseType, new { userId, platform });

	public static async Task RecoverTransaction(ExpenseTypeModel expenseType, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), expenseType, new { userId, platform });

	public static async Task<int> SaveTransaction(ExpenseTypeModel expenseType, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), expenseType, new { userId, platform });
}
