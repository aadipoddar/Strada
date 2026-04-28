using StradaLibrary.Models.Operations;

namespace Strada.Shared.Pages.Operations;

public partial class TransactionsDashbaord
{
	private UserModel _user;
	private bool _isLoading = true;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Fleet]);

		_isLoading = false;
		StateHasChanged();
	}
}
