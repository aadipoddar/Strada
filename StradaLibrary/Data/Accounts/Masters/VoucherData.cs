using StradaLibrary.Data.Common;
using StradaLibrary.DataAccess;
using StradaLibrary.Models.Accounts.Masters;

namespace StradaLibrary.Data.Accounts.Masters;

public static class VoucherData
{
    public static async Task<int> InsertVoucher(VoucherModel voucher) =>
        (await SqlDataAccess.LoadData<int, dynamic>(AccountNames.InsertVoucher, voucher)).FirstOrDefault();

    private static async Task ValidateTransaction(VoucherModel item)
    {
        item.Name = item.Name?.Trim().ToUpper() ?? string.Empty;
        item.Remarks = item.Remarks?.Trim() ?? string.Empty;
        item.Status = true;

        if (string.IsNullOrWhiteSpace(item.Name))
            throw new Exception("Voucher name is required. Please enter a valid voucher name.");

        if (string.IsNullOrWhiteSpace(item.Remarks))
            item.Remarks = null;

        var allItems = await CommonData.LoadTableData<VoucherModel>(AccountNames.Voucher);

        var existingByName = allItems.FirstOrDefault(vt => vt.Id != item.Id && vt.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
        if (existingByName is not null)
            throw new Exception($"Voucher name '{item.Name}' already exists. Please choose a different name.");
    }

    public static async Task<int> SaveTransaction(VoucherModel item)
    {
        await ValidateTransaction(item);
        return await InsertVoucher(item);
    }
}
