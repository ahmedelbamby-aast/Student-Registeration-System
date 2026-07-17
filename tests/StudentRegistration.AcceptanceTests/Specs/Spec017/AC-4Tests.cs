using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec017;

public sealed class AC_4Tests
{
    [Fact]
    public void Dashboard_reports_timestamped_threshold_degradation_without_fabricated_zero()
    {
        // Given failures or capacity conflicts exceed a configured threshold.
        var contract = RepositoryFiles.Read(
            "specs/017-admin-operations-audit-reporting/contracts/api.md");
        RepositoryFiles.ContainsAll(
            contract,
            "server failure, fill-rate, lock-wait, capacity-conflict",
            "observedAtUtc",
            "availabilityState",
            "metric: string",
            "threshold: number",
            "observedValue: number",
            "supportReferencePath: string",
            "MUST NOT replace missing values with zero");

        // When the dashboard refreshes, the deferred query must preserve the
        // source observation time and classify unavailable data as degraded.
        const string delivery =
            "src/StudentRegistration.StaffAdministration/Application/AdminMetricsQuery.cs";
        Assert.True(
            RepositoryFiles.Exists(delivery),
            "Expected-red for AC-4/T035: timestamped operational metric aggregation is intentionally deferred to T052.");
        var query = RepositoryFiles.Read(delivery);

        // Then the alert identifies metric, threshold, observation and safe
        // investigation reference rather than returning a fabricated zero.
        RepositoryFiles.ContainsAll(
            query,
            "ObservedAtUtc",
            "AvailabilityState",
            "Metric",
            "Threshold",
            "ObservedValue",
            "SupportReferencePath",
            "degraded");
        Assert.DoesNotContain("Unavailable ? 0", query, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Failure ? 0", query, StringComparison.OrdinalIgnoreCase);
    }
}
