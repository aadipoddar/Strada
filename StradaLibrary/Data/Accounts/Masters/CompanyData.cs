using StradaLibrary.Data.Common;
using StradaLibrary.Data.Operations;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;
using StradaLibrary.Models.Operations;

namespace StradaLibrary.Data.Accounts.Masters;

public static class CompanyData
{
	private static async Task<int> InsertCompany(CompanyModel company, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertCompany, company, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Company.");

	public static async Task DeleteTransaction(CompanyModel company, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			company.Status = false;
			await InsertCompany(company, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = AccountNames.Company,
				RecordNo = company.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task RecoverTransaction(CompanyModel company, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			company.Status = true;
			await InsertCompany(company, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = AccountNames.Company,
				RecordNo = company.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	private static async Task ValidateTransaction(CompanyModel company)
	{
		company.Name = company.Name?.Trim().ToUpper() ?? string.Empty;
		company.Code = company.Code?.Trim().ToUpper() ?? string.Empty;
		company.GSTNo = company.GSTNo?.Trim().ToUpper() ?? string.Empty;
		company.PANNo = company.PANNo?.Trim().ToUpper() ?? string.Empty;
		company.CINNo = company.CINNo?.Trim().ToUpper() ?? string.Empty;
		company.Alias = company.Alias?.Trim().ToUpper() ?? string.Empty;
		company.Phone = company.Phone?.Trim() ?? string.Empty;
		company.Email = company.Email?.Trim() ?? string.Empty;
		company.Address = company.Address?.Trim() ?? string.Empty;
		company.Remarks = company.Remarks?.Trim() ?? string.Empty;
		company.Status = true;

		if (string.IsNullOrWhiteSpace(company.Name))
			throw new Exception("Company name is required. Please enter a valid company name.");

		if (string.IsNullOrWhiteSpace(company.Code))
			throw new Exception("Company code is required. Please enter a valid company code.");

		if (company.StateUTId <= 0)
			throw new Exception("State/UT is required. Please select a valid State/UT.");

		if (string.IsNullOrWhiteSpace(company.GSTNo)) company.GSTNo = null;
		if (string.IsNullOrWhiteSpace(company.PANNo)) company.PANNo = null;
		if (string.IsNullOrWhiteSpace(company.CINNo)) company.CINNo = null;
		if (string.IsNullOrWhiteSpace(company.Alias)) company.Alias = null;
		if (string.IsNullOrWhiteSpace(company.Phone)) company.Phone = null;
		if (string.IsNullOrWhiteSpace(company.Email)) company.Email = null;
		if (string.IsNullOrWhiteSpace(company.Address)) company.Address = null;
		if (string.IsNullOrWhiteSpace(company.Remarks)) company.Remarks = null;

		if (!string.IsNullOrWhiteSpace(company.Phone) && !Helper.ValidatePhoneNumber(company.Phone))
			throw new Exception("Invalid phone number format. Please enter a valid phone number.");

		if (!string.IsNullOrWhiteSpace(company.Email) && !Helper.ValidateEmail(company.Email))
			throw new Exception("Invalid email format. Please enter a valid email address.");

		var allCompanies = await CommonData.LoadTableData<CompanyModel>(AccountNames.Company);

		var existingByName = allCompanies.FirstOrDefault(x => x.Id != company.Id && x.Name.Equals(company.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Company name '{company.Name}' already exists. Please choose a different name.");

		var existingByCode = allCompanies.FirstOrDefault(x => x.Id != company.Id && x.Code.Equals(company.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Company code '{company.Code}' already exists. Please choose a different code.");

		if (!string.IsNullOrWhiteSpace(company.Phone))
		{
			var duplicatePhone = allCompanies.FirstOrDefault(x => x.Id != company.Id && x.Phone != null && x.Phone.Equals(company.Phone, StringComparison.OrdinalIgnoreCase));
			if (duplicatePhone is not null)
				throw new Exception($"Phone number '{company.Phone}' is already associated with another company. Please use a different phone number.");
		}

		if (!string.IsNullOrWhiteSpace(company.Email))
		{
			var duplicateEmail = allCompanies.FirstOrDefault(x => x.Id != company.Id && x.Email != null && x.Email.Equals(company.Email, StringComparison.OrdinalIgnoreCase));
			if (duplicateEmail is not null)
				throw new Exception($"Email '{company.Email}' is already associated with another company. Please use a different email address.");
		}
	}

	public static async Task<int> SaveTransaction(CompanyModel company, int userId, string platform)
	{
		await ValidateTransaction(company);

		var isUpdate = company.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<CompanyModel>(AccountNames.Company, company.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertCompany(company, transaction);
			var diff = AuditTrailData.GetDifference(previous, company);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = AccountNames.Company,
				RecordNo = company.Name,
				RecordValue = diff,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
