using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using StudentRegistration.Api.Operations;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.Api.Endpoints;

public static class Spec018Endpoints
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaximumPageSize = 100;
    private static readonly string ApplicationVersion =
        typeof(Spec018Endpoints).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+', 2)[0]
        ?? typeof(Spec018Endpoints).Assembly.GetName().Version?.ToString()
        ?? "0.0.0";

    public static IEndpointRouteBuilder MapSpec018Endpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet(
                "/api/health",
                ([FromServices] OperationalHealthRegistry registry,
                    [FromServices] TimeProvider timeProvider) =>
                    CreateHealthResult(registry, timeProvider))
            .AllowAnonymous()
            .WithName("Spec018Health");

        endpoints.MapGet(
                "/api/operations/metrics",
                (HttpContext context,
                    [FromServices] OperationalTelemetry telemetry) =>
                    CreateMetricsResult(context, telemetry))
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("Spec018OperationalMetrics");

        return endpoints;
    }

    public static IResult CreateHealthResult(
        OperationalHealthRegistry registry,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var summary = new HealthSummary(
            registry.GetStatus(),
            ApplicationVersion,
            timeProvider.GetUtcNow().UtcDateTime);
        return summary.Status == HealthSummaryStatus.Unhealthy
            ? Results.Json(summary, statusCode: StatusCodes.Status503ServiceUnavailable)
            : Results.Ok(summary);
    }

    public static IResult CreateMetricsResult(
        HttpContext context,
        OperationalTelemetry telemetry)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(telemetry);
        context.Response.Headers.CacheControl = "no-store";

        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Error(
                context,
                StatusCodes.Status401Unauthorized,
                "AUTHENTICATION_REQUIRED",
                "Authentication is required.");
        }

        if (!context.User.IsInRole("Admin"))
        {
            return Error(
                context,
                StatusCodes.Status403Forbidden,
                "ACCESS_DENIED",
                "The current role cannot access operational metrics.");
        }

        if (!TryReadPage(context.Request.Query, "page", DefaultPage, int.MaxValue, out var page) ||
            !TryReadPage(
                context.Request.Query,
                "pageSize",
                DefaultPageSize,
                MaximumPageSize,
                out var pageSize))
        {
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                "PAGE_SIZE_INVALID",
                "Page must be positive and pageSize must be between 1 and 100.");
        }

        var metrics = telemetry.Snapshot();
        var offset = ((long)page - 1) * pageSize;
        var items = offset >= metrics.Count
            ? []
            : metrics.Skip((int)offset).Take(pageSize).ToArray();
        return Results.Ok(new Page<OperationalMetric>(
            items,
            page,
            pageSize,
            metrics.Count,
            "seriesKey"));
    }

    private static bool TryReadPage(
        IQueryCollection query,
        string name,
        int defaultValue,
        int maximum,
        out int value)
    {
        if (!query.TryGetValue(name, out var raw) || raw.Count == 0)
        {
            value = defaultValue;
            return true;
        }

        value = default;
        return raw.Count == 1 &&
            int.TryParse(raw[0], out value) &&
            value >= 1 &&
            value <= maximum;
    }

    private static IResult Error(
        HttpContext context,
        int statusCode,
        string code,
        string message)
    {
        var correlationId = context.TraceIdentifier;
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
            context.TraceIdentifier = correlationId;
        }

        return Results.Json(
            new ApiError(code, message, correlationId),
            statusCode: statusCode);
    }
}
