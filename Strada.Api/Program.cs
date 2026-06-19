using Carter;

using Scalar.AspNetCore;

using StradaLibrary.DataAccess;

var builder = WebApplication.CreateBuilder(args);

SqlDataAccess.SetupConfiguration();

builder.Services.AddOpenApi();
builder.Services.AddCors();
builder.Services.AddCarter();

var app = builder.Build();

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