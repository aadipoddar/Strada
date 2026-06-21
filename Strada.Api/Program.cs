using Carter;

using Microsoft.OpenApi;

using Scalar.AspNetCore;

using Strada.Api;
using Strada.Data.DataAccess;
using Strada.Models.Common;

using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

SqlDataAccess.SetupConfiguration();

builder.Services.AddOpenApi(options =>
	options.AddDocumentTransformer((document, context, cancellationToken) =>
	{
		var tagGroups = new JsonArray();

		var areas = typeof(Program).Assembly.GetTypes()
			.Where(type => typeof(ICarterModule).IsAssignableFrom(type) && type is { IsClass: true, IsAbstract: false })
			.GroupBy(type => type.Namespace?.Split('.').Last())
			.OrderBy(area => area.Key);

		foreach (var area in areas)
		{
			var tags = new JsonArray();
			foreach (var tag in area.Select(type => Helper.SanitizeClassName(type.Name)).OrderBy(tag => tag))
				tags.Add(tag);

			tagGroups.Add(new JsonObject { ["name"] = area.Key, ["tags"] = tags });
		}

		document.Extensions ??= new Dictionary<string, IOpenApiExtension>();
		document.Extensions["x-tagGroups"] = new JsonNodeExtension(tagGroups);
		return Task.CompletedTask;
	}));
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