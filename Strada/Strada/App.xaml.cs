#if WINDOWS
using Strada.Shared.Services;
#endif

namespace Strada;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

#if WINDOWS
		// On Windows, open internal routes in a new native window instead of in-place navigation.
		AuthenticationService.OpenRouteInNewWindow = route =>
		{
			MainThread.BeginInvokeOnMainThread(() =>
				Current?.OpenWindow(new Window(new MainPage(route)) { Title = "Strada" }));
			return true;
		};
#endif
	}

	protected override Window CreateWindow(IActivationState? activationState) => new Window(new MainPage()) { Title = "Strada" };
}
