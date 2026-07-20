using Microsoft.AspNetCore.Diagnostics;
using StudentRegistration.Contracts;

namespace StudentRegistration.Api.Composition;

public static class ApiErrorHandlingExtensions
{
    public static IServiceCollection AddSafeApiErrors(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddExceptionHandler<SafeApiExceptionHandler>();
        return services;
    }

    public static IApplicationBuilder UseSafeApiErrors(
        this IApplicationBuilder application)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.UseExceptionHandler(new ExceptionHandlerOptions
        {
            ExceptionHandler = static _ => Task.CompletedTask
        });
        return application.UseMiddleware<SafeClientErrorBodyMiddleware>();
    }
}

internal sealed class SafeClientErrorBodyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        await next(context);
        if (context.Response.HasStarted ||
            context.Response.ContentLength is > 0 ||
            !string.IsNullOrWhiteSpace(context.Response.ContentType))
        {
            return;
        }

        if (!SafeClientErrors.IsSupportedStatus(context.Response.StatusCode))
        {
            return;
        }

        await SafeClientErrors.WriteAsync(
            context,
            context.Response.StatusCode,
            context.RequestAborted);
    }
}

internal static class SafeClientErrors
{
    internal static bool IsSupportedStatus(int statusCode) =>
        statusCode is
            StatusCodes.Status400BadRequest or
            StatusCodes.Status415UnsupportedMediaType;

    internal static async Task WriteAsync(
        HttpContext context,
        int statusCode,
        CancellationToken cancellationToken)
    {
        var (code, message) = statusCode switch
        {
            StatusCodes.Status415UnsupportedMediaType => (
                "UNSUPPORTED_MEDIA_TYPE",
                "The request content type is not supported."),
            _ => (
                "VALIDATION_ERROR",
                "The request body or parameters are invalid.")
        };
        var correlationId = context.TraceIdentifier;
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
            context.TraceIdentifier = correlationId;
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(
            new ApiError(code, message, correlationId),
            cancellationToken);
    }
}

internal sealed class SafeApiExceptionHandler(
    ILogger<SafeApiExceptionHandler> logger) : IExceptionHandler
{
    private const string SafeMessage =
        "An unexpected error occurred. Use the reference when contacting support.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is OperationCanceledException &&
            httpContext.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        var correlationId = httpContext.TraceIdentifier;
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
            httpContext.TraceIdentifier = correlationId;
        }

        if (exception is BadHttpRequestException badRequestException)
        {
            var statusCode = badRequestException.StatusCode is
                StatusCodes.Status400BadRequest or
                StatusCodes.Status415UnsupportedMediaType
                    ? badRequestException.StatusCode
                    : StatusCodes.Status400BadRequest;
            await SafeClientErrors.WriteAsync(
                httpContext,
                statusCode,
                cancellationToken);
            return true;
        }

        logger.LogError(
            exception,
            "Unhandled request failure of type {ExceptionType}. Correlation ID: {CorrelationId}",
            exception.GetType().Name,
            correlationId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            new ApiError("UNEXPECTED_ERROR", SafeMessage, correlationId),
            cancellationToken);

        return true;
    }
}
