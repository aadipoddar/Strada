using Strada.Api;
using Strada.Data.DataAccess;

var builder = WebApplication.CreateBuilder(args);

SqlDataAccess.SetupConfiguration();

builder.Services.AddServices();

var app = builder.Build();

app.UseServices();
app.Run();