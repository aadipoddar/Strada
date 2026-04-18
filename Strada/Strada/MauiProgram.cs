#if DEBUG
using Microsoft.Extensions.Logging;
#endif

using Strada.Services;
using Strada.Shared.Services;
using StradaLibrary.DataAccess;
using Syncfusion.Blazor;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Strada;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		Secrets.SetupConfiguration();

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		// Add device-specific services used by the Strada.Shared project
		builder.Services.AddSingleton<IFormFactor, FormFactor>();
		builder.Services.AddSingleton<ISaveAndViewService, SaveAndViewService>();
		builder.Services.AddSingleton<IUpdateService, UpdateService>();
		builder.Services.AddSingleton<IDataStorageService, DataStorageService>();
		builder.Services.AddSingleton<IVibrationService, VibrationService>();
		builder.Services.AddSingleton<ISoundService, SoundService>();
		builder.Services.AddScoped<INotificationService, NotificationService>();

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSyncfusionBlazor();
		builder.Services.AddHotKeys2();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
