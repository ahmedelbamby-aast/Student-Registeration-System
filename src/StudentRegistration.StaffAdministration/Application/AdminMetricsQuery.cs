using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.StaffAdministration.Application;

public static class AdminMetricNames
{
    public const string Traffic = "http.request.throughput";
    public const string Success = "registration.submissions.succeeded";
    public const string ExpectedRejections = "registration.business_rejections";
    public const string ServerFailures = "http.request.unexpected_errors";
    public const string FillRate = "registration.group.fill_rate";
    public const string LockWaitMilliseconds = "sql.lock_wait.duration.ms";
    public const string DataQualityAlerts = "registration.counter_mismatches";
    public const string CapacityConflicts = "registration.capacity_conflicts";

    public static IReadOnlySet<string> Required { get; } =
        new HashSet<string>(StringComparer.Ordinal)
        {
            Traffic,
            Success,
            ExpectedRejections,
            ServerFailures,
            FillRate,
            LockWaitMilliseconds,
            DataQualityAlerts,
            CapacityConflicts,
        };
}

public enum AdminMetricsAvailability
{
    Live,
    Stale,
    Degraded,
}

public sealed record AdminMetricAlert(
    string Metric,
    double Threshold,
    double ObservedValue,
    DateTime ObservedAtUtc,
    string SupportReferencePath);

public sealed record AdminMetricsQueryResult(
    AdminMetricsAvailability AvailabilityState,
    DateTime? ObservedAtUtc,
    IReadOnlyList<OperationalMetric> Metrics,
    IReadOnlyList<AdminMetricAlert> Alerts,
    IReadOnlyList<string> MissingMetricNames);

public sealed record AdminMetricsThresholds(
    double ServerFailures = 1,
    double CapacityConflicts = 1,
    double DataQualityAlerts = 0)
{
    public AdminMetricsThresholds Validate()
    {
        if (!double.IsFinite(ServerFailures) || ServerFailures < 0
            || !double.IsFinite(CapacityConflicts) || CapacityConflicts < 0
            || !double.IsFinite(DataQualityAlerts) || DataQualityAlerts < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(AdminMetricsThresholds),
                "Metric thresholds must be finite and non-negative.");
        }

        return this;
    }
}

public sealed class AdminMetricsQuery
{
    public const int MaximumMetricSeries = 100;
    public static readonly TimeSpan FreshnessWindow = TimeSpan.FromSeconds(60);

    private readonly IOperationalMetricReader _reader;
    private readonly TimeProvider _timeProvider;
    private readonly AdminMetricsThresholds _thresholds;

    public AdminMetricsQuery(
        IOperationalMetricReader reader,
        TimeProvider timeProvider,
        AdminMetricsThresholds? thresholds = null)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _reader = reader;
        _timeProvider = timeProvider;
        _thresholds = (thresholds ?? new()).Validate();
    }

    public async Task<AdminMetricsQueryResult> ExecuteAsync(
        OperationalMetricScope scope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        var read = await _reader.ReadAsync(scope, cancellationToken);
        ArgumentNullException.ThrowIfNull(read);

        var ordered = read.Metrics
            .Where(metric => metric is not null)
            .OrderBy(metric => metric.Name, StringComparer.Ordinal)
            .ThenBy(metric => DimensionKey(metric.Dimensions), StringComparer.Ordinal)
            .Take(MaximumMetricSeries)
            .ToArray();
        var truncated = read.Metrics.Count > MaximumMetricSeries;
        var names = ordered
            .Select(metric => metric.Name)
            .ToHashSet(StringComparer.Ordinal);
        var missing = AdminMetricNames.Required
            .Where(name => !names.Contains(name))
            .Order(StringComparer.Ordinal)
            .ToArray();

        // The oldest included observation is the conservative timestamp for the
        // whole dashboard. Missing series are reported, never synthesized as zero.
        var observedAtUtc = ordered.Length == 0
            ? (DateTime?)null
            : ordered.Min(metric => metric.ObservedAtUtc);
        var availability = Availability(
            read.SourceState,
            observedAtUtc,
            missing.Length > 0 || truncated);

        return new(
            availability,
            observedAtUtc,
            ordered,
            Alerts(ordered),
            missing);
    }

    private AdminMetricsAvailability Availability(
        OperationalMetricSourceState sourceState,
        DateTime? observedAtUtc,
        bool incomplete)
    {
        if (sourceState is OperationalMetricSourceState.Unavailable
            || observedAtUtc is null
            || incomplete)
        {
            return AdminMetricsAvailability.Degraded;
        }

        var age = _timeProvider.GetUtcNow().UtcDateTime - observedAtUtc.Value;
        return age > FreshnessWindow
            ? AdminMetricsAvailability.Stale
            : AdminMetricsAvailability.Live;
    }

    private IReadOnlyList<AdminMetricAlert> Alerts(
        IReadOnlyList<OperationalMetric> metrics)
    {
        var alerts = new List<AdminMetricAlert>();
        AddAlerts(
            alerts,
            metrics,
            AdminMetricNames.ServerFailures,
            _thresholds.ServerFailures,
            "/admin/audit?action=registration.server-failure");
        AddAlerts(
            alerts,
            metrics,
            AdminMetricNames.CapacityConflicts,
            _thresholds.CapacityConflicts,
            "/admin/audit?action=registration.capacity-conflict");
        AddAlerts(
            alerts,
            metrics,
            AdminMetricNames.DataQualityAlerts,
            _thresholds.DataQualityAlerts,
            "/admin/audit?action=registration.data-quality");
        return alerts.AsReadOnly();
    }

    private static void AddAlerts(
        ICollection<AdminMetricAlert> alerts,
        IEnumerable<OperationalMetric> metrics,
        string name,
        double threshold,
        string supportReferencePath)
    {
        foreach (var metric in metrics.Where(metric =>
                     string.Equals(metric.Name, name, StringComparison.Ordinal)
                     && metric.Value > threshold))
        {
            alerts.Add(new(
                metric.Name,
                threshold,
                metric.Value,
                metric.ObservedAtUtc,
                supportReferencePath));
        }
    }

    private static string DimensionKey(
        IReadOnlyDictionary<string, string> dimensions) => string.Join(
        "|",
        dimensions.OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => $"{entry.Key}={entry.Value}"));
}
