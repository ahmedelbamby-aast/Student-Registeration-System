namespace StudentRegistration.ApplicationTests.Specs.Spec017;

public sealed class Endpoint01BehaviorTests
{
    private const string QueryPath =
        "src/StudentRegistration.StaffAdministration/Application/AdminMetricsQuery.cs";
    private const string EndpointPath =
        "src/StudentRegistration.StaffAdministration/Endpoints/Spec017Endpoints.cs";

    [Fact]
    public void Metrics_query_preserves_observation_time_and_reports_degraded_input_without_zero_fill()
    {
        var query = Spec017BehaviorFiles.FutureSource(
            QueryPath,
            "Expected red for T025: AdminMetricsQuery is intentionally absent until T052.");

        Spec017BehaviorFiles.ContainsAll(
            query,
            "TimeProvider",
            "ObservedAtUtc",
            "AvailabilityState",
            "stale",
            "degraded",
            "traffic",
            "expected",
            "server",
            "fill",
            "lock",
            "data-quality");
        Assert.DoesNotContain("Unavailable ? 0", query, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GetUtcNow() : 0", query, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Metrics_endpoint_is_scoped_authorized_bounded_and_maps_safe_outcomes()
    {
        var endpoint = Spec017BehaviorFiles.FutureSource(
            EndpointPath,
            "Expected red for T025: SPEC-017 metric handler is intentionally absent until T091.");

        Spec017BehaviorFiles.ContainsAll(
            endpoint,
            "/api/admin/operations/metrics",
            "AdminOperations.Metrics.Read",
            "AdminOperationsRead",
            "AdminMetricsQuery",
            "METRICS_FILTER_INVALID",
            "RESOURCE_NOT_FOUND",
            "RATE_LIMITED",
            "METRICS_UNAVAILABLE",
            "StatusCodes.Status200OK",
            "StatusCodes.Status400BadRequest",
            "StatusCodes.Status404NotFound",
            "StatusCodes.Status429TooManyRequests",
            "StatusCodes.Status503ServiceUnavailable",
            "CancellationToken");
    }
}
