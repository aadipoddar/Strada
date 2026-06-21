using Microsoft.AspNetCore.Diagnostics;

namespace Strada.Api;

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
