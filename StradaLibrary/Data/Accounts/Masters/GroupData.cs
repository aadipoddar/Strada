using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;

namespace StradaLibrary.Data.Accounts.Masters;

public static class GroupData
{
    public static async Task<int> InsertGroup(GroupModel group) =>
        (await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertGroup, group)).FirstOrDefault();
}
