using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;

namespace StradaLibrary.Data.Accounts.Masters;

public static class CompanyData
{
    public static async Task<int> InsertCompany(CompanyModel company) =>
        (await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertCompany, company)).FirstOrDefault();
}
