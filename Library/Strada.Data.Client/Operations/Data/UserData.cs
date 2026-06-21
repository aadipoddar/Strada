using Strada.Models.Common;
using Strada.Models.Operations;

namespace Strada.Data.Operations.Data;

public static class UserData
{
	private static readonly string _endpoint = Helper.SanitizeClassName(nameof(UserData));

	public static Task<int> InsertUser(UserModel user) =>
		Api.Post<int>($"{_endpoint}/{nameof(InsertUser)}", user);

	public static Task<UserModel> LoadUserByPhoneEmail(string phoneEmail) =>
		Api.Get<UserModel>($"{_endpoint}/{nameof(LoadUserByPhoneEmail)}", new { phoneEmail });

	public static Task ResetInsertUser(UserModel user) =>
		Api.Post($"{_endpoint}/{nameof(ResetInsertUser)}", user);

	public static Task<int> SaveTransaction(UserModel user, int userId, string platform) =>
		Api.Post<int>($"{_endpoint}/{nameof(SaveTransaction)}", user, new { userId, platform });

	public static Task DeleteTransaction(UserModel user, int userId, string platform) =>
		Api.Post($"{_endpoint}/{nameof(DeleteTransaction)}", user, new { userId, platform });

	public static Task RecoverTransaction(UserModel user, int userId, string platform) =>
		Api.Post($"{_endpoint}/{nameof(RecoverTransaction)}", user, new { userId, platform });
}
