namespace Strada;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		ConfigurePlatform();
	}

	protected override Window CreateWindow(IActivationState? activationState) => new(new MainPage()) { Title = "Strada" };

	partial void ConfigurePlatform();
}
