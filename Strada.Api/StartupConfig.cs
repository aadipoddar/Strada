using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi;

using Scalar.AspNetCore;

using Strada.Models.Common;

using System.Text.Json.Nodes;

namespace Strada.Api;

public static class StartupConfig
{
	public static void AddServices(this IServiceCollection services)
	{
		services.AddOpenApi(options =>
			options.AddDocumentTransformer((document, context, cancellationToken) =>
			{
				var tagGroups = new JsonArray();

				var areas = typeof(Program).Assembly.GetTypes()
					.Where(type => typeof(ICarterModule).IsAssignableFrom(type) && type is { IsClass: true, IsAbstract: false })
					.GroupBy(type => type.Namespace?.Split('.').SkipWhile(segment => segment != "Endpoint").Skip(1).FirstOrDefault())
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
		services.AddCors();
		services.AddCarter();
		services.AddExceptionHandler<GlobalExceptionHandler>();
		services.AddProblemDetails();
		services.ConfigureHttpJsonOptions(options => options.SerializerOptions.IncludeFields = true);
	}

	public static void UseServices(this WebApplication app)
	{
		app.UseExceptionHandler();

		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
			app.MapScalarApiReference(options =>
			{
				options.Title = "Strada API";
			});
		}

		app.UseHttpsRedirection();

		app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

		app.MapCarter();
		app.MapGet("/", () => "Strada API");
	}
}

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
	{
		logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);

		context.Response.StatusCode = StatusCodes.Status500InternalServerError;
		await context.Response.WriteAsJsonAsync(new { message = exception.Message }, cancellationToken);

		return true;
	}
}
