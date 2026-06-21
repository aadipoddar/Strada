using Carter;

using Scalar.AspNetCore;

using Strada.Api;
using Strada.Data.DataAccess;

var builder = WebApplication.CreateBuilder(args);

SqlDataAccess.SetupConfiguration();

builder.Services.AddOpenApi();
builder.Services.AddCors();
builder.Services.AddCarter();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapCarter();
app.MapGet("/", () => "Strada API");

app.Run();