using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

using Strada.Shared.Services;

using StradaLibrary.Common;

namespace Strada.Web.Services;

public class DataStorageService(ProtectedLocalStorage protectedLocalStorage) : IDataStorageService
{
	private readonly ProtectedLocalStorage _protectedLocalStorage = protectedLocalStorage;

	public async Task SecureSaveAsync(string key, string value)
	{
		try
		{
			await _protectedLocalStorage.SetAsync(key, value);
		}
		catch { }
	}

	public async Task<string?> SecureGetAsync(string key) =>
		(await _protectedLocalStorage.GetAsync<string>(key)).Value;

	public async Task SecureRemove(string key)
	{
		try
		{
			await _protectedLocalStorage.DeleteAsync(key);
		}
		catch { }
	}

	public async Task SecureRemoveAll()
	{
		await LocalRemove(StorageFileNames.UserDataFileName);
		await LocalRemove(StorageFileNames.UserDeviceIdDataFileName);

		await LocalRemove(StorageFileNames.FinancialAccountingDataFileName);
		await LocalRemove(StorageFileNames.FinancialAccountingCartDataFileName);

		await LocalRemove(StorageFileNames.ExpenseDataFileName);
		await LocalRemove(StorageFileNames.ExpenseDetailsCartDataFileName);

		await LocalRemove(StorageFileNames.TripDataFileName);
		await LocalRemove(StorageFileNames.TripExpensesCartDataFileName);
		await LocalRemove(StorageFileNames.TripCardPaymentsCartDataFileName);
		await LocalRemove(StorageFileNames.TripLedgerPaymentsCartDataFileName);

		await LocalRemove(StorageFileNames.BillDataFileName);
		await LocalRemove(StorageFileNames.BillPendingTripsCartDataFileName);
		await LocalRemove(StorageFileNames.BillLedgerPaymentsCartDataFileName);
	}


	public async Task<bool> LocalExists(string key) =>
		(await _protectedLocalStorage.GetAsync<string>(key)).Success;

	public async Task LocalSaveAsync(string key, string value) =>
		await SecureSaveAsync(key, value);

	public async Task<string?> LocalGetAsync(string key) =>
		await SecureGetAsync(key);

	public async Task LocalRemove(string key) =>
		await SecureRemove(key);
}
