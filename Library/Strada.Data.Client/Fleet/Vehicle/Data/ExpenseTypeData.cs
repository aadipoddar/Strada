using Strada.Models.Common;
using Strada.Models.Fleet.Vehicle;

namespace Strada.Data.Fleet.Vehicle.Data;

public static class ExpenseTypeData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(ExpenseTypeData));

	public static Task DeleteTransaction(ExpenseTypeModel expenseType, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), expenseType, new { userId, platform });

	public static Task RecoverTransaction(ExpenseTypeModel expenseType, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), expenseType, new { userId, platform });

	public static Task<int> SaveTransaction(ExpenseTypeModel expenseType, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), expenseType, new { userId, platform });
}
