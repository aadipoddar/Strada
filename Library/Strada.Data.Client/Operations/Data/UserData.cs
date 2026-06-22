using Strada.Models.Common;
using Strada.Models.Operations;

namespace Strada.Data.Operations.Data;

public static class UserData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(UserData));

	public static Task<int> InsertUser(UserModel user) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(InsertUser)), user);

	public static Task<UserModel> LoadUserByPhoneEmail(string phoneEmail) =>
		Api.Get<UserModel>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(LoadUserByPhoneEmail)), new { phoneEmail });

	public static Task<string> EncryptPassword(string password) =>
		Api.Post<string>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(EncryptPassword)), new { password });

	public static Task<bool> VerifyPassword(string password, string hashedPassword) =>
		Api.Post<bool>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(VerifyPassword)), new { password, hashedPassword });

	public static Task ResetInsertUser(UserModel user) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(ResetInsertUser)), user);

	public static Task DeleteTransaction(UserModel user, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(DeleteTransaction)), user, new { userId, platform });

	public static Task RecoverTransaction(UserModel user, int userId, string platform) =>
		Api.Post(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(RecoverTransaction)), user, new { userId, platform });

	public static Task<int> SaveTransaction(UserModel user, int userId, string platform) =>
		Api.Post<int>(Helper.MakeRouteFromEndpointFunction(_endpoint, nameof(SaveTransaction)), user, new { userId, platform });
}
