using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;

namespace StradaLibrary.Data.Accounts.Masters;

public static class AccountTypeData
{
    public static async Task<int> InsertAccountType(AccountTypeModel accountType) =>
        (await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertAccountType, accountType)).FirstOrDefault();
}
