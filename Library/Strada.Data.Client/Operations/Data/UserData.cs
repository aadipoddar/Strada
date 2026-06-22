using Strada.Models.Common;
using Strada.Models.Operations;

namespace Strada.Data.Operations.Data;

public static class UserData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(UserData));

	public static async Task<int> InsertUser(UserModel user) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(InsertUser)), user);

	public static async Task<UserModel> LoadUserByPhoneEmail(string phoneEmail) =>
		await Api.Get<UserModel>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadUserByPhoneEmail)), new { phoneEmail });

	public static async Task<string> EncryptPassword(string password) =>
		await Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(EncryptPassword)), new { password });

	public static async Task<bool> VerifyPassword(string password, string hashedPassword) =>
		await Api.Post<bool>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(VerifyPassword)), new { password, hashedPassword });

	public static async Task ResetInsertUser(UserModel user) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ResetInsertUser)), user);

	public static async Task DeleteTransaction(UserModel user, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), user, new { userId, platform });

	public static async Task RecoverTransaction(UserModel user, int userId, string platform) =>
		await Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), user, new { userId, platform });

	public static async Task<int> SaveTransaction(UserModel user, int userId, string platform) =>
		await Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), user, new { userId, platform });
}
