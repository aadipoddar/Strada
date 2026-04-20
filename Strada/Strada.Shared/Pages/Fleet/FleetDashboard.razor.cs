using StradaLibrary.Data.Operations;
using StradaLibrary.Models.Operations;

namespace Strada.Shared.Pages.Fleet;

public partial class FleetDashboard : IAsyncDisposable
{
	private UserModel _user;
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Fleet]);

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