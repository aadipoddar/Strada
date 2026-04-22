using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;

namespace StradaLibrary.Data.Accounts.Masters;

public static class GroupData
{
	public static async Task<int> InsertGroup(GroupModel group) =>
		(await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertGroup, group)).FirstOrDefault();

	private static async Task ValidateTransaction(GroupModel group)
	{
		group.Name = group.Name?.Trim().ToUpper() ?? string.Empty;
		group.Remarks = group.Remarks?.Trim() ?? string.Empty;
		group.Status = true;

		if (string.IsNullOrWhiteSpace(group.Name))
			throw new Exception("Group name is required. Please enter a valid group name.");

		if (group.NatureId <= 0)
			throw new Exception("Nature is required. Please select a nature.");

		if (string.IsNullOrWhiteSpace(group.Remarks))
			group.Remarks = null;

		var allGroups = await CommonData.LoadTableData<GroupModel>(AccountNames.Group);

		var existingByName = allGroups.FirstOrDefault(vt => vt.Id != group.Id && vt.Name.Equals(group.Name, StringComparison.OrdinalIgnoreCase));
		if (existingByName is not null)
			throw new Exception($"Group name '{group.Name}' already exists. Please choose a different name.");
	}

	public static async Task<int> SaveTransaction(GroupModel group)
	{
		await ValidateTransaction(group);
		return await InsertGroup(group);
	}
}
