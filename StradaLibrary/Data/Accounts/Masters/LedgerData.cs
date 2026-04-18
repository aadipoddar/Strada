using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;

namespace StradaLibrary.Data.Accounts.Masters;

public static class LedgerData
{
    public static async Task<int> InsertLedger(LedgerModel ledger, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertLedger, ledger, sqlDataAccessTransaction)).FirstOrDefault();
}
