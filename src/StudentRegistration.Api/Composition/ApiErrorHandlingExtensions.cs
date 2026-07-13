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

        return application.UseExceptionHandler();
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

        if (exception is BadHttpRequestException ||
            exception is OperationCanceledException &&
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

        logger.LogError(
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
