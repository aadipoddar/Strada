using Microsoft.AspNetCore.Diagnostics;

using Scalar.AspNetCore;

namespace Strada.Api;

public static class StartupConfig
{
	public static void AddServices(this IServiceCollection services)
	{
		services.AddOpenApi();
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
				options.Theme = ScalarTheme.Moon;
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
