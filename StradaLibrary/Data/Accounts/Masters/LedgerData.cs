using StradaLibrary.Data.Common;
using StradaLibrary.Data.Operations;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Operations;

namespace StradaLibrary.Data.Accounts.Masters;

public static class LedgerData
{
	private static async Task<int> InsertLedger(LedgerModel ledger, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertLedger, ledger, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Ledger.");

	public static async Task DeleteTransaction(LedgerModel ledger, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			ledger.Status = false;
			await InsertLedger(ledger, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = AccountNames.Ledger,
				RecordNo = ledger.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task RecoverTransaction(LedgerModel ledger, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			ledger.Status = true;
			await InsertLedger(ledger, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = AccountNames.Ledger,
				RecordNo = ledger.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	private static async Task ValidateTransaction(LedgerModel ledger)
	{
		ledger.Name = ledger.Name?.Trim().ToUpper() ?? string.Empty;
		ledger.GSTNo = ledger.GSTNo?.Trim().ToUpper() ?? string.Empty;
		ledger.PANNo = ledger.PANNo?.Trim().ToUpper() ?? string.Empty;
		ledger.CINNo = ledger.CINNo?.Trim().ToUpper() ?? string.Empty;
		ledger.Alias = ledger.Alias?.Trim().ToUpper() ?? string.Empty;
		ledger.Phone = ledger.Phone?.Trim() ?? string.Empty;
		ledger.Email = ledger.Email?.Trim() ?? string.Empty;
		ledger.Address = ledger.Address?.Trim() ?? string.Empty;
		ledger.Remarks = ledger.Remarks?.Trim() ?? string.Empty;
		ledger.Status = true;

		if (string.IsNullOrWhiteSpace(ledger.Name))
			throw new Exception("Ledger name is required. Please enter a valid ledger name.");

		if (ledger.GroupId <= 0)
			throw new Exception("Group is required. Please select a valid group.");

		if (ledger.AccountTypeId <= 0)
			throw new Exception("Account Type is required. Please select a valid account type.");

		if (ledger.StateUTId <= 0)
			throw new Exception("State/UT is required. Please select a valid State/UT.");

		if (string.IsNullOrWhiteSpace(ledger.GSTNo)) ledger.GSTNo = null;
		if (string.IsNullOrWhiteSpace(ledger.PANNo)) ledger.PANNo = null;
		if (string.IsNullOrWhiteSpace(ledger.CINNo)) ledger.CINNo = null;
		if (string.IsNullOrWhiteSpace(ledger.Alias)) ledger.Alias = null;
		if (string.IsNullOrWhiteSpace(ledger.Phone)) ledger.Phone = null;
		if (string.IsNullOrWhiteSpace(ledger.Email)) ledger.Email = null;
		if (string.IsNullOrWhiteSpace(ledger.Address)) ledger.Address = null;
		if (string.IsNullOrWhiteSpace(ledger.Remarks)) ledger.Remarks = null;

		if (!string.IsNullOrWhiteSpace(ledger.Phone) && !Helper.ValidatePhoneNumber(ledger.Phone))
			throw new Exception("Invalid phone number format. Please enter a valid phone number.");

		if (!string.IsNullOrWhiteSpace(ledger.Email) && !Helper.ValidateEmail(ledger.Email))
			throw new Exception("Invalid email format. Please enter a valid email address.");

		if (ledger.Id == 0)
			ledger.Code = await GenerateCodes.GenerateLedgerCode();

		var allLedgers = await CommonData.LoadTableData<LedgerModel>(AccountNames.Ledger);

		var existingByName = allLedgers.FirstOrDefault(x => x.Id != ledger.Id && x.Name.Equals(ledger.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Ledger name '{ledger.Name}' already exists. Please choose a different name.");

		if (!string.IsNullOrWhiteSpace(ledger.Phone))
		{
			var duplicatePhone = allLedgers.FirstOrDefault(x => x.Id != ledger.Id && x.Phone != null && x.Phone.Equals(ledger.Phone, StringComparison.OrdinalIgnoreCase));
			if (duplicatePhone is not null)
				throw new Exception($"Phone number '{ledger.Phone}' is already associated with another ledger. Please use a different phone number.");
		}

		if (!string.IsNullOrWhiteSpace(ledger.Email))
		{
			var duplicateEmail = allLedgers.FirstOrDefault(x => x.Id != ledger.Id && x.Email != null && x.Email.Equals(ledger.Email, StringComparison.OrdinalIgnoreCase));
			if (duplicateEmail is not null)
				throw new Exception($"Email '{ledger.Email}' is already associated with another ledger. Please use a different email address.");
		}
	}

	public static async Task<int> SaveTransaction(LedgerModel ledger, int userId, string platform)
	{
		await ValidateTransaction(ledger);

		var isUpdate = ledger.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<LedgerModel>(AccountNames.Ledger, ledger.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertLedger(ledger, transaction);
			var diff = AuditTrailData.GetDifference(previous, ledger);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = AccountNames.Ledger,
				RecordNo = ledger.Name,
				RecordValue = diff,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
