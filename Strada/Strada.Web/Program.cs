using MudBlazor.Services;

using Strada.Shared.Services;
using Strada.Web.Components;
using Strada.Web.Services;

using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JHaF5cWWdCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWXxfeHVUQmVYWUF1XEBWYEo=");

builder.Services
	.AddSyncfusionBlazor()
	.AddMudServices()
	.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddSingleton<IUpdateService, UpdateService>();
builder.Services.AddSingleton<IVibrationService, VibrationService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();

builder.Services.AddScoped<ISaveAndViewService, SaveAndViewService>();
builder.Services.AddScoped<ISoundService, SoundService>();
builder.Services.AddScoped<IDataStorageService, DataStorageService>();
builder.Services.AddScoped<PageRefreshState>();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode()
	.AddAdditionalAssemblies(
		typeof(Strada.Shared._Imports).Assembly);

Strada.Data.Api.Init(new HttpClient { BaseAddress = new Uri("https://localhost:7078/") });

app.Run();
