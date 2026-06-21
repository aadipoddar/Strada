using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Strada.Wasm;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Point the data-client mirror at the running Strada.Api. The browser must trust the
// API's dev cert (run: dotnet dev-certs https --trust).
Strada.Data.Api.Init(new HttpClient { BaseAddress = new Uri("https://localhost:7078/") });

await builder.Build().RunAsync();
