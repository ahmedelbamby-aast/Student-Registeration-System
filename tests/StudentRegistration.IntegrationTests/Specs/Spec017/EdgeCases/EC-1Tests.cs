using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void Unavailable_metrics_source_preserves_the_last_observation_as_degraded_without_zero_fill()
    {
        const string queryPath =
            "src/StudentRegistration.StaffAdministration/Application/AdminMetricsQuery.cs";

        Assert.True(
            RepositoryFiles.Exists(queryPath),
            "Expected red for T041/EC-1: AdminMetricsQuery is deferred until T052.");

        var source = RepositoryFiles.Read(queryPath);
        RepositoryFiles.ContainsAll(
            source,
            "ObservedAtUtc",
            "degraded",
            "stale");
        Assert.DoesNotContain(
            "Unavailable ? 0",
            source,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "GetUtcNow() : 0",
            source,
            StringComparison.OrdinalIgnoreCase);
    }
}
