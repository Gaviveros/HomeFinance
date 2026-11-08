using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HomeFinance.Api.Middleware;

public sealed class ApiExceptionHandler(RequestDelegate next, ILogger<ApiExceptionHandler> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var storageUnavailable = exception is NpgsqlException
                || exception is DbUpdateException { InnerException: NpgsqlException };

            logger.LogError("Request failed with {ExceptionType}; storage unavailable: {StorageUnavailable}.",
                exception.GetType().Name, storageUnavailable);

            context.Response.Clear();
            context.Response.StatusCode = storageUnavailable
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                message = storageUnavailable
                    ? "El almacenamiento no está disponible temporalmente."
                    : "Ocurrió un error interno."
            });
        }
    }
}
