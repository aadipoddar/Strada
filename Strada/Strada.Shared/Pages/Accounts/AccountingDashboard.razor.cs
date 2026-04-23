using StradaLibrary.Models.Operations;

namespace Strada.Shared.Pages.Accounts;

public partial class AccountingDashboard
{
	private UserModel _user;
	private bool _isLoading = true;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Accounts]);

		_isLoading = false;
		StateHasChanged();
	}
}
