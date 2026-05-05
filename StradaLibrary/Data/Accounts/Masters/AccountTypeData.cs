using StradaLibrary.Data.Common;
using StradaLibrary.Data.Operations;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Operations;

namespace StradaLibrary.Data.Accounts.Masters;

public static class AccountTypeData
{
	private static async Task<int> InsertAccountType(AccountTypeModel accountType, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertAccountType, accountType, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Account Type.");

	public static async Task DeleteTransaction(AccountTypeModel accountType, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			accountType.Status = false;
			await InsertAccountType(accountType, transaction);
			await AuditTrailData.SaveAuditTrail(new ()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = AccountNames.AccountType,
				RecordNo = accountType.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task RecoverTransaction(AccountTypeModel accountType, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			accountType.Status = true;
			await InsertAccountType(accountType, transaction);
			await AuditTrailData.SaveAuditTrail(new ()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = AccountNames.AccountType,
				RecordNo = accountType.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

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

	public static async Task<int> SaveTransaction(AccountTypeModel accountType, int userId, string platform)
	{
		await ValidateTransaction(accountType);

		var isUpdate = accountType.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<AccountTypeModel>(AccountNames.AccountType, accountType.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertAccountType(accountType, transaction);
			var diff = AuditTrailData.GetDifference(previous, accountType);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = AccountNames.AccountType,
				RecordNo = accountType.Name,
				RecordValue = diff,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
