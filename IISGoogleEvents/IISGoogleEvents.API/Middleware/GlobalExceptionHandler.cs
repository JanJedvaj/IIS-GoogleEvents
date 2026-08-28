using IISGoogleEvents.Application.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace IISGoogleEvents.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Neobrađena iznimka: {Message}", exception.Message);

        var response = StandardResponse<object>.Create(
            ResultStatus.InternalError,
            message: "Interna greška servera.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
