using System.Text.Json;
using StudentRegistration.Contracts.Operations;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec018;

public sealed class HealthSummaryModelTests
{
    [Fact]
    public void Model_is_owned_by_spec018_and_remains_framework_free()
    {
        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        var artifact = ownership.RootElement
            .GetProperty("artifactOverrides")
            .GetProperty("018:HealthSummary")
            .GetString();

        Assert.Equal(
            "src/StudentRegistration.Contracts/Operations/HealthSummary.cs",
            artifact);

        var references = typeof(HealthSummary).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(
            references,
            reference => reference.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
        Assert.DoesNotContain(
            references,
            reference => reference.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        Assert.DoesNotContain(
            references,
            reference => reference.Contains("SqlClient", StringComparison.Ordinal));
    }

    [Fact]
    public void Constructor_accepts_only_a_defined_state_safe_version_and_utc_timestamp()
    {
        var timestamp = new DateTime(2026, 7, 14, 1, 30, 0, DateTimeKind.Utc);
        var summary = new HealthSummary(
            HealthSummaryStatus.Degraded,
            "2026.7.14+df6774c",
            timestamp);

        Assert.Equal(HealthSummaryStatus.Degraded, summary.Status);
        Assert.Equal("2026.7.14+df6774c", summary.Version);
        Assert.Equal(timestamp, summary.TimestampUtc);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HealthSummary((HealthSummaryStatus)999, "1.0.0", timestamp));
        Assert.Throws<ArgumentException>(() =>
            new HealthSummary(HealthSummaryStatus.Healthy, " ", timestamp));
        Assert.Throws<ArgumentException>(() =>
            new HealthSummary(
                HealthSummaryStatus.Healthy,
                new string('v', HealthSummary.MaxVersionLength + 1),
                timestamp));
        Assert.Throws<ArgumentException>(() =>
            new HealthSummary(
                HealthSummaryStatus.Healthy,
                "1.0.0\ninternal-topology",
                timestamp));
        Assert.Throws<ArgumentException>(() =>
            new HealthSummary(
                HealthSummaryStatus.Healthy,
                "1.0.0",
                DateTime.SpecifyKind(timestamp, DateTimeKind.Local)));
    }

    [Theory]
    [InlineData(HealthSummaryStatus.Healthy, "healthy")]
    [InlineData(HealthSummaryStatus.Degraded, "degraded")]
    [InlineData(HealthSummaryStatus.Unhealthy, "unhealthy")]
    public void Json_contract_is_camel_case_closed_and_uses_stable_status_tokens(
        HealthSummaryStatus status,
        string expectedStatus)
    {
        var summary = new HealthSummary(
            status,
            "1.2.3",
            new DateTime(2026, 7, 14, 1, 30, 0, DateTimeKind.Utc));

        var json = JsonSerializer.Serialize(summary, JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);
        var properties = document.RootElement
            .EnumerateObject()
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["status", "timestampUtc", "version"], properties);
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
        Assert.Equal("1.2.3", document.RootElement.GetProperty("version").GetString());
        Assert.Equal(
            "2026-07-14T01:30:00Z",
            document.RootElement.GetProperty("timestampUtc").GetString());

        var roundTrip = JsonSerializer.Deserialize<HealthSummary>(
            json,
            JsonSerializerOptions.Web);
        Assert.Equal(summary, roundTrip);
    }
}
