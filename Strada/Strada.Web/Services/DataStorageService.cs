using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Strada.Shared.Services;
using StradaLibrary.Data.Operations;

namespace Strada.Web.Services;

public class DataStorageService(ProtectedLocalStorage protectedLocalStorage) : IDataStorageService
{
	private readonly ProtectedLocalStorage _protectedLocalStorage = protectedLocalStorage;

	public async Task SecureSaveAsync(string key, string value) =>
		await _protectedLocalStorage.SetAsync(key, value);

	public async Task<string?> SecureGetAsync(string key) =>
		(await _protectedLocalStorage.GetAsync<string>(key)).Value;

	public async Task SecureRemove(string key) =>
		await _protectedLocalStorage.DeleteAsync(key);

	public async Task SecureRemoveAll()
	{
		await LocalRemove(StorageFileNames.UserDataFileName);
		await LocalRemove(StorageFileNames.FinancialAccountingDataFileName);
		await LocalRemove(StorageFileNames.FinancialAccountingCartDataFileName);
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
