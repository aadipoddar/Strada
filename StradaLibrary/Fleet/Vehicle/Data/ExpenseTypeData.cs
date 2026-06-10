using StradaLibrary.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Fleet.Vehicle.Models;
using StradaLibrary.Operations.Data;
using StradaLibrary.Operations.Models;

namespace StradaLibrary.Fleet.Vehicle.Data;

public static class ExpenseTypeData
{
	public static async Task<int> InsertExpenseType(ExpenseTypeModel expenseType, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertExpenseType, expenseType, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert Expense Type.");

	public static async Task DeleteTransaction(ExpenseTypeModel expenseType, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			expenseType.Status = false;
			await InsertExpenseType(expenseType, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = FleetNames.ExpenseType,
				RecordNo = expenseType.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	public static async Task RecoverTransaction(ExpenseTypeModel expenseType, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			expenseType.Status = true;
			await InsertExpenseType(expenseType, transaction);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Recover.ToString(),
				TableName = FleetNames.ExpenseType,
				RecordNo = expenseType.Name,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	private static async Task ValidateTransaction(ExpenseTypeModel expenseType)
	{
		expenseType.Name = expenseType.Name?.Trim().ToUpper() ?? string.Empty;
		expenseType.Code = expenseType.Code?.Trim().ToUpper() ?? string.Empty;
		expenseType.Remarks = expenseType.Remarks?.Trim() ?? string.Empty;
		expenseType.Status = true;

		if (string.IsNullOrWhiteSpace(expenseType.Name))
			throw new Exception("Expense Type name is required. Please enter a valid name.");

		if (expenseType.Id == 0)
			expenseType.Code = await GenerateCodes.GenerateExpenseTypeCode();

		if (string.IsNullOrWhiteSpace(expenseType.Code))
			throw new Exception("Expense Type code is required. Please try again.");

		if (string.IsNullOrWhiteSpace(expenseType.Remarks))
			expenseType.Remarks = null;

		var allTypes = await CommonData.LoadTableData<ExpenseTypeModel>(FleetNames.ExpenseType);

		var existingByName = allTypes.FirstOrDefault(x => x.Id != expenseType.Id && x.Name.Equals(expenseType.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Expense Type name '{expenseType.Name}' already exists. Please choose a different name.");

		var existingByCode = allTypes.FirstOrDefault(x => x.Id != expenseType.Id && x.Code.Equals(expenseType.Code, StringComparison.OrdinalIgnoreCase));
		if (existingByCode is not null)
			throw new Exception($"Expense Type code '{expenseType.Code}' already exists. Please choose a different code.");
	}

	public static async Task<int> SaveTransaction(ExpenseTypeModel expenseType, int userId, string platform)
	{
		await ValidateTransaction(expenseType);

		var isUpdate = expenseType.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<ExpenseTypeModel>(FleetNames.ExpenseType, expenseType.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertExpenseType(expenseType, transaction);
			var diff = AuditTrailData.GetDifference(previous, expenseType);
			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = FleetNames.ExpenseType,
				RecordNo = expenseType.Name,
				RecordValue = isUpdate ? diff : null,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
