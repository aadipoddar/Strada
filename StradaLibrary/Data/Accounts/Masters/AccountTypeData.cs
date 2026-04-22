using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;

namespace StradaLibrary.Data.Accounts.Masters;

public static class AccountTypeData
{
	public static async Task<int> InsertAccountType(AccountTypeModel accountType) =>
		(await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertAccountType, accountType)).FirstOrDefault();

	private static async Task ValidateTransaction(AccountTypeModel accountType)
	{
		accountType.Name = accountType.Name?.Trim().ToUpper() ?? string.Empty;
		accountType.Remarks = accountType.Remarks?.Trim() ?? string.Empty;
		accountType.Status = true;

		if (string.IsNullOrWhiteSpace(accountType.Name))
			throw new Exception("Account Type name is required. Please enter a valid account type name.");

		if (string.IsNullOrWhiteSpace(accountType.Remarks))
			accountType.Remarks = null;

		var allTypes = await CommonData.LoadTableData<AccountTypeModel>(AccountNames.AccountType);

		var existingByName = allTypes.FirstOrDefault(vt => vt.Id != accountType.Id && vt.Name.Equals(accountType.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Account Type name '{accountType.Name}' already exists. Please choose a different name.");
	}

	public static async Task<int> SaveTransaction(AccountTypeModel accountType)
	{
		await ValidateTransaction(accountType);
		return await InsertAccountType(accountType);
	}
}
