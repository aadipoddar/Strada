using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Fleet.Vehicle;

namespace StradaLibrary.Data.Fleet.Vehicle;

public static class ExpenseTypeData
{
	public static async Task<int> InsertExpenseType(ExpenseTypeModel expenseType) =>
		(await SqlDataAccess.LoadData<int, dynamic>(FleetNames.InsertExpenseType, expenseType)).FirstOrDefault();

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

	public static async Task<int> SaveTransaction(ExpenseTypeModel expenseType)
	{
		await ValidateTransaction(expenseType);
		return await InsertExpenseType(expenseType);
	}
}
