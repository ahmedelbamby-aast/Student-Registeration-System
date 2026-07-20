using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.Api.Operations;

public static class OperationalMetricNames
{
    public const string RequestLatencyMilliseconds = "http.request.latency.ms";
    public const string RequestThroughput = "http.request.throughput";
    public const string UnexpectedErrors = "http.request.unexpected_errors";
    public const string BusinessRejections = "registration.business_rejections";
    public const string OptimizerDurationMilliseconds = "schedule.optimizer.duration.ms";
    public const string SqlDurationMilliseconds = "sql.query.duration.ms";
    public const string SqlLockWaitMilliseconds = "sql.lock_wait.duration.ms";
    public const string SqlDeadlocks = "sql.deadlocks";
    public const string CapacityConflicts = "registration.capacity_conflicts";
    public const string CounterMismatches = "registration.counter_mismatches";
}

public sealed class OperationalHealthRegistry
{
    // Readiness is fail-closed until each dependency reports a known state.
    private int _sqlServerAvailable;
    private int _telemetryExporterAvailable;

    public void SetSqlServerAvailable(bool available) =>
        Interlocked.Exchange(ref _sqlServerAvailable, available ? 1 : 0);

    public void SetTelemetryExporterAvailable(bool available) =>
        Interlocked.Exchange(ref _telemetryExporterAvailable, available ? 1 : 0);

    public HealthSummaryStatus GetStatus()
    {
        if (Volatile.Read(ref _sqlServerAvailable) == 0)
        {
            return HealthSummaryStatus.Unhealthy;
        }

        return Volatile.Read(ref _telemetryExporterAvailable) == 0
            ? HealthSummaryStatus.Degraded
            : HealthSummaryStatus.Healthy;
    }
}

public sealed class OperationalTelemetry : IDisposable
{
    public const int MaximumSeries = 256;
    public const string MeterName = "StudentRegistration.Operations";
    public const string ActivitySourceName = "StudentRegistration.Api";

    private static readonly HashSet<string> CounterNames =
    [
        OperationalMetricNames.RequestThroughput,
        OperationalMetricNames.UnexpectedErrors,
        OperationalMetricNames.BusinessRejections,
        OperationalMetricNames.SqlDeadlocks,
        OperationalMetricNames.CapacityConflicts,
        OperationalMetricNames.CounterMismatches
    ];

    private static readonly HashSet<string> HistogramNames =
    [
        OperationalMetricNames.RequestLatencyMilliseconds,
        OperationalMetricNames.OptimizerDurationMilliseconds,
        OperationalMetricNames.SqlDurationMilliseconds,
        OperationalMetricNames.SqlLockWaitMilliseconds
    ];

    private static readonly HashSet<string> AllowedDimensions =
        new(StringComparer.Ordinal)
        {
            "code",
            "method",
            "module",
            "operation",
            "outcome",
            "route",
            "statusClass"
        };

    private static readonly HashSet<string> AllowedModules =
        new(StringComparer.Ordinal)
        {
            "academics",
            "api",
            "identity",
            "operations",
            "registration",
            "scheduling",
            "staff-administration"
        };

    private static readonly HashSet<string> AllowedOperations =
        new(StringComparer.Ordinal)
        {
            "catalogue",
            "commit",
            "health",
            "metrics",
            "optimizer",
            "reconciliation",
            "request",
            "seat-allocation",
            "sql"
        };

    private static readonly HashSet<string> AllowedOutcomes =
        new(StringComparer.Ordinal)
        {
            "accepted",
            "conflict",
            "degraded",
            "failure",
            "healthy",
            "rejected",
            "success",
            "unhealthy"
        };

    private static readonly HashSet<string> AllowedRoutes =
        new(StringComparer.Ordinal)
        {
            "catalogue",
            "eligibility",
            "health",
            "operations-metrics",
            "plan",
            "registration-record",
            "schedule-recommendations",
            "timetable"
        };

    private static readonly HashSet<string> AllowedCodes =
        new(StringComparer.Ordinal)
        {
            "ACCESS_DENIED",
            "AUTHENTICATION_REQUIRED",
            "CAPACITY_CONFLICT",
            "COUNTER_MISMATCH",
            "CREDIT_LIMIT_EXCEEDED",
            "GROUP_FULL",
            "HOLD_ACTIVE",
            "IDEMPOTENCY_KEY_REUSED",
            "PAGE_SIZE_INVALID",
            "PLAN_CHANGED",
            "STALE_INPUT",
            "INVALID_OPTION_TOKEN",
            "OPTION_EXPIRED",
            "POLICY_CHANGED",
            "POLICY_REJECTED",
            "PREREQUISITE_NOT_MET",
            "REGISTRATION_IN_PROGRESS",
            "SCHEDULE_CONFLICT",
            "UNEXPECTED_ERROR",
            "WINDOW_CHANGED",
            "WINDOW_CLOSED"
        };

    private readonly object _sync = new();
    private readonly Dictionary<string, MetricAccumulator> _series =
        new(StringComparer.Ordinal);
    private readonly TimeProvider _timeProvider;
    private readonly Meter _meter = new(MeterName);
    private readonly ActivitySource _activitySource = new(ActivitySourceName);
    private readonly IReadOnlyDictionary<string, Counter<double>> _counters;
    private readonly IReadOnlyDictionary<string, Histogram<double>> _histograms;
    private bool _disposed;

    public OperationalTelemetry(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        _counters = CounterNames.ToDictionary(
            name => name,
            name => _meter.CreateCounter<double>(name),
            StringComparer.Ordinal);
        _histograms = HistogramNames.ToDictionary(
            name => name,
            name => _meter.CreateHistogram<double>(name, "ms"),
            StringComparer.Ordinal);
    }

    public void Increment(
        string name,
        double value = 1,
        IReadOnlyDictionary<string, string>? dimensions = null)
    {
        ThrowIfDisposed();
        ValidateMeasurement(name, value, CounterNames);
        var validatedDimensions = ValidateDimensions(dimensions);
        Update(name, value, validatedDimensions, accumulate: true);
        _counters[name].Add(value, ToTags(validatedDimensions));
    }

    public void Observe(
        string name,
        double value,
        IReadOnlyDictionary<string, string>? dimensions = null)
    {
        ThrowIfDisposed();
        ValidateMeasurement(name, value, HistogramNames);
        var validatedDimensions = ValidateDimensions(dimensions);
        Update(name, value, validatedDimensions, accumulate: false);
        _histograms[name].Record(value, ToTags(validatedDimensions));
    }

    public IReadOnlyList<OperationalMetric> Snapshot()
    {
        ThrowIfDisposed();
        lock (_sync)
        {
            return Array.AsReadOnly(_series
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => new OperationalMetric(
                    entry.Value.Name,
                    entry.Value.Value,
                    entry.Value.ObservedAtUtc,
                    entry.Value.Dimensions))
                .ToArray());
        }
    }

    internal Activity? StartRequestActivity(string correlationId)
    {
        var activity = _activitySource.StartActivity(
            "http.request",
            ActivityKind.Server);
        activity?.SetTag("correlation.id", correlationId);
        return activity;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _activitySource.Dispose();
        _meter.Dispose();
    }

    private void Update(
        string name,
        double value,
        IReadOnlyDictionary<string, string> dimensions,
        bool accumulate)
    {
        var key = SeriesKey(name, dimensions);
        var observedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        lock (_sync)
        {
            if (_series.TryGetValue(key, out var current))
            {
                _series[key] = current with
                {
                    Value = accumulate ? current.Value + value : value,
                    ObservedAtUtc = observedAtUtc
                };
                return;
            }

            if (_series.Count >= MaximumSeries)
            {
                throw new InvalidOperationException("METRIC_CARDINALITY_LIMIT");
            }

            _series.Add(
                key,
                new MetricAccumulator(name, value, observedAtUtc, dimensions));
        }
    }

    private static void ValidateMeasurement(
        string name,
        double value,
        IReadOnlySet<string> allowedNames)
    {
        if (string.IsNullOrWhiteSpace(name) || !allowedNames.Contains(name))
        {
            throw new ArgumentException("The metric name is not allow-listed.", nameof(name));
        }

        if (!double.IsFinite(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Metric values must be finite and non-negative.");
        }
    }

    private static IReadOnlyDictionary<string, string> ValidateDimensions(
        IReadOnlyDictionary<string, string>? dimensions)
    {
        if (dimensions is null || dimensions.Count == 0)
        {
            return new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (dimensions.Count > 4)
        {
            throw new ArgumentException(
                "A metric series may contain at most four dimensions.",
                nameof(dimensions));
        }

        var copy = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in dimensions)
        {
            if (!AllowedDimensions.Contains(key))
            {
                throw new ArgumentException(
                    "The metric dimension is not allow-listed.",
                    nameof(dimensions));
            }

            if (!IsSafeDimensionValue(key, value))
            {
                throw new ArgumentException(
                    "Metric dimension values must be short controlled codes.",
                    nameof(dimensions));
            }

            copy.Add(key, value);
        }

        return new ReadOnlyDictionary<string, string>(copy);
    }

    private static bool IsSafeDimensionValue(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 64)
        {
            return false;
        }

        return key switch
        {
            "method" => value is "DELETE" or "GET" or "HEAD" or "OPTIONS" or
                "PATCH" or "POST" or "PUT" or "OTHER",
            "statusClass" => value is "1xx" or "2xx" or "3xx" or "4xx" or "5xx",
            "code" => AllowedCodes.Contains(value),
            "module" => AllowedModules.Contains(value),
            "operation" => AllowedOperations.Contains(value),
            "outcome" => AllowedOutcomes.Contains(value),
            "route" => AllowedRoutes.Contains(value),
            _ => false
        };
    }

    private static KeyValuePair<string, object?>[] ToTags(
        IReadOnlyDictionary<string, string> dimensions) =>
        dimensions
            .Select(pair => new KeyValuePair<string, object?>(pair.Key, pair.Value))
            .ToArray();

    private static string SeriesKey(
        string name,
        IReadOnlyDictionary<string, string> dimensions) =>
        dimensions.Aggregate(
            name,
            (current, pair) => string.Concat(
                current,
                "|",
                pair.Key,
                "=",
                pair.Value));

    private void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(_disposed, this);

    private sealed record MetricAccumulator(
        string Name,
        double Value,
        DateTime ObservedAtUtc,
        IReadOnlyDictionary<string, string> Dimensions);
}

public static class ObservabilityExtensions
{
    public static IServiceCollection AddStudentRegistrationObservability(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<OperationalHealthRegistry>();
        services.TryAddSingleton<OperationalTelemetry>();
        services.TryAddSingleton<IOperationalDependencyProbe,
            SqlServerOperationalDependencyProbe>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, OperationalHealthMonitor>());
        return services;
    }

    public static IApplicationBuilder UseStudentRegistrationObservability(
        this IApplicationBuilder application)
    {
        ArgumentNullException.ThrowIfNull(application);

        return application.UseMiddleware<CorrelationTelemetryMiddleware>();
    }
}

internal sealed class CorrelationTelemetryMiddleware(
    RequestDelegate next,
    ILogger<CorrelationTelemetryMiddleware> logger)
{
    private const string CorrelationHeader = "X-Correlation-ID";

    public async Task InvokeAsync(
        HttpContext context,
        OperationalTelemetry telemetry)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(telemetry);

        var correlationId = ResolveCorrelationId();
        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationHeader] = correlationId;
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.Headers.CacheControl = "no-store";
        }
        using var activity = telemetry.StartRequestActivity(correlationId);
        var startedAt = Stopwatch.GetTimestamp();
        var failed = false;

        try
        {
            await next(context);
        }
        catch
        {
            failed = true;
            TryRecord(
                () => telemetry.Increment(OperationalMetricNames.UnexpectedErrors),
                logger);
            throw;
        }
        finally
        {
            var elapsedMilliseconds = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            var statusClass = failed
                ? "5xx"
                : string.Create(
                    CultureInfo.InvariantCulture,
                    $"{context.Response.StatusCode / 100}xx");
            var dimensions = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["method"] = SafeMethod(context.Request.Method),
                ["statusClass"] = statusClass
            };
            TryRecord(
                () => telemetry.Observe(
                    OperationalMetricNames.RequestLatencyMilliseconds,
                    elapsedMilliseconds,
                    dimensions),
                logger);
            TryRecord(
                () => telemetry.Increment(
                    OperationalMetricNames.RequestThroughput,
                    dimensions: dimensions),
                logger);
            if (!failed && context.Response.StatusCode >= 500)
            {
                TryRecord(
                    () => telemetry.Increment(OperationalMetricNames.UnexpectedErrors),
                    logger);
            }

            if (failed || context.Response.StatusCode >= 500)
            {
                logger.LogWarning(
                    "Request completed with status class {StatusClass} in {ElapsedMilliseconds} ms. Correlation ID: {CorrelationId}",
                    statusClass,
                    elapsedMilliseconds,
                    correlationId);
            }
            else
            {
                logger.LogDebug(
                    "Request completed with status class {StatusClass} in {ElapsedMilliseconds} ms. Correlation ID: {CorrelationId}",
                    statusClass,
                    elapsedMilliseconds,
                    correlationId);
            }
        }
    }

    private static string ResolveCorrelationId() => Guid.NewGuid().ToString("N");

    private static string SafeMethod(string method)
    {
        var normalized = method.ToUpperInvariant();
        return normalized switch
        {
            "DELETE" or "GET" or "HEAD" or "OPTIONS" or "PATCH" or
                "POST" or "PUT" => normalized,
            _ => "OTHER"
        };
    }

    private static void TryRecord(Action record, ILogger logger)
    {
        try
        {
            record();
        }
        catch (InvalidOperationException exception)
            when (exception.Message == "METRIC_CARDINALITY_LIMIT")
        {
            logger.LogWarning(
                "Operational metric was dropped because the bounded series limit was reached.");
        }
    }
}
