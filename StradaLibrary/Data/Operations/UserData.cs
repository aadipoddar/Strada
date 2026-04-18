using StradaLibrary.DataAccess;
using StradaLibrary.Models.Operations;

namespace StradaLibrary.Data.Operations;

public static class UserData
{
	public static async Task<int> InsertUser(UserModel userModel) =>
		(await SqlDataAccess.LoadData<int, dynamic>(OperationNames.InsertUser, userModel)).FirstOrDefault();

	public static async Task ResetInsertUser(UserModel user)
	{
		user.Status = true;
		user.FailedAttempts = 0;
		user.CodeResends = 0;
		user.LastCodeDateTime = null;
		user.LastCode = null;
		user.LastCodeDeviceId = null;

		await InsertUser(user);
	}
}