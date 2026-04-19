using StradaLibrary.Data.Operations;
using StradaLibrary.Models.Operations;

namespace Strada.Shared.Pages.Accounts;

public partial class AccountingDashboard : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Accounts]);

		_hotKeysContext = HotKeys.CreateContext()
			.Add(ModCode.Ctrl, Code.B, () => NavigationManager.NavigateTo(PageRouteNames.Dashboard), "Back", Exclude.None);

		_isLoading = false;
		StateHasChanged();
	}

	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ((IAsyncDisposable)HotKeys).DisposeAsync();
	}
}
