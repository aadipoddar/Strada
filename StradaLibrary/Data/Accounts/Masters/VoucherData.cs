using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;

namespace StradaLibrary.Data.Accounts.Masters;

public static class VoucherData
{
    public static async Task<int> InsertVoucher(VoucherModel voucher) =>
        (await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertVoucher, voucher)).FirstOrDefault();
}