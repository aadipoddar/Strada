using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;

namespace StradaLibrary.Data.Accounts.Masters;

public static class StateUTData
{
    public static async Task<int> InsertStateUT(StateUTModel state) =>
        (await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertStateUT, state)).FirstOrDefault();
}