using StradaLibrary.Models.Operations;

namespace Strada.Shared.Pages.VehicleExpense;

public partial class VehicleExpenseDashboard
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
