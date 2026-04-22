using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;

namespace StradaLibrary.Data.Accounts.Masters;

public static class StateUTData
{
    public static async Task<int> InsertStateUT(StateUTModel state) =>
        (await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertStateUT, state)).FirstOrDefault();

    private static async Task ValidateTransaction(StateUTModel item)
    {
        item.Name = item.Name?.Trim().ToUpper() ?? string.Empty;
        item.Remarks = item.Remarks?.Trim() ?? string.Empty;
        item.Status = true;

        if (string.IsNullOrWhiteSpace(item.Name))
            throw new Exception("State/UT name is required. Please enter a valid state/UT name.");

        if (string.IsNullOrWhiteSpace(item.Remarks))
            item.Remarks = null;

        var allItems = await CommonData.LoadTableData<StateUTModel>(AccountNames.StateUT);

        var existingByName = allItems.FirstOrDefault(vt => vt.Id != item.Id && vt.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
        if (existingByName is not null)
            throw new Exception($"State/UT name '{item.Name}' already exists. Please choose a different name.");
    }

    public static async Task<int> SaveTransaction(StateUTModel item)
    {
        await ValidateTransaction(item);
        return await InsertStateUT(item);
    }
}
