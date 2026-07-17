using System.Text.Json;
using StudentRegistration.Contracts.Operations;
using StudentRegistration.StaffAdministration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec017;

public sealed class OperationalMetricModelTests
{
    [Fact]
    public void Spec017_consumes_the_single_contract_owned_operational_metric()
    {
        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));

        Assert.Equal(
            "018",
            ownership.RootElement
                .GetProperty("canonicalOwners")
                .GetProperty(nameof(OperationalMetric))
                .GetString());
        Assert.Equal(
            "src/StudentRegistration.Contracts/Operations/OperationalMetric.cs",
            ownership.RootElement
                .GetProperty("artifactOverrides")
                .GetProperty("018:OperationalMetric")
                .GetString());
        Assert.Equal(
            "StudentRegistration.Contracts",
            typeof(OperationalMetric).Assembly.GetName().Name);
        Assert.Null(typeof(RosterRow).Assembly.GetType(
            "StudentRegistration.StaffAdministration.Domain.OperationalMetric"));
    }

    [Fact]
    public void Consumed_metric_retains_observation_time_and_bounded_dimensions()
    {
        var observedAtUtc = new DateTime(2026, 7, 17, 8, 30, 0, DateTimeKind.Utc);
        var metric = new OperationalMetric(
            "registration.submissions.total",
            75,
            observedAtUtc,
            new Dictionary<string, string>
            {
                ["outcome"] = "accepted"
            });

        Assert.Equal(observedAtUtc, metric.ObservedAtUtc);
        Assert.Equal("accepted", metric.Dimensions["outcome"]);
        Assert.Equal(8, OperationalMetric.MaxDimensions);
    }
}
