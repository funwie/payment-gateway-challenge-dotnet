using Newtonsoft.Json;

using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Middleware;

/// <summary>
///     Catch unhandled exceptions and log them
///     Prevents returning internal errors to API Users
/// </summary>
public class ErrorHandlerMiddleware
{
    private static ILogger _logger = null!;
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.OnStarting(() =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse(ErrorTypes.InternalError,
                    new[] { Errors.ProcessingError })));
                return Task.CompletedTask;
            });
        }
    }
}