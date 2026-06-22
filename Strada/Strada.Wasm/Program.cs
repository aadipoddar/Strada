using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor.Services;

using Strada.Shared.Services;
using Strada.Wasm;
using Strada.Wasm.Services;

using Syncfusion.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JHaF5cWWdCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWXxfeHVUQmVYWUF1XEBWYEo=");

builder.Services
	.AddSyncfusionBlazor()
	.AddMudServices();

builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddSingleton<IUpdateService, UpdateService>();
builder.Services.AddSingleton<IVibrationService, VibrationService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();

builder.Services.AddScoped<ISaveAndViewService, SaveAndViewService>();
builder.Services.AddScoped<ISoundService, SoundService>();
builder.Services.AddScoped<IDataStorageService, DataStorageService>();
builder.Services.AddScoped<PageRefreshState>();
builder.Services.AddMemoryCache();

Strada.Data.Api.Init(new HttpClient { BaseAddress = new Uri("https://localhost:7078/") });

await builder.Build().RunAsync();
