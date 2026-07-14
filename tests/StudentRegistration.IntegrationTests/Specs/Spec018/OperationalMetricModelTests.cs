using System.Text.Json;
using StudentRegistration.Contracts.Operations;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec018;

public sealed class OperationalMetricModelTests
{
    [Fact]
    public void Model_is_owned_by_spec018_and_remains_framework_free()
    {
        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        var artifact = ownership.RootElement
            .GetProperty("artifactOverrides")
            .GetProperty("018:OperationalMetric")
            .GetString();

        Assert.Equal(
            "src/StudentRegistration.Contracts/Operations/OperationalMetric.cs",
            artifact);

        var sourceProject = RepositoryFiles.Read(
            "src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj");
        Assert.DoesNotContain("FrameworkReference", sourceProject, StringComparison.Ordinal);
        Assert.DoesNotContain("PackageReference", sourceProject, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectReference", sourceProject, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_validates_name_finite_value_utc_time_and_dimension_bounds()
    {
        var timestamp = new DateTime(2026, 7, 14, 1, 45, 0, DateTimeKind.Utc);
        var dimensions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["module"] = "registration",
            ["outcome"] = "accepted"
        };
        var metric = new OperationalMetric(
            "registration.submission.duration_ms",
            142.75,
            timestamp,
            dimensions);

        Assert.Equal("registration.submission.duration_ms", metric.Name);
        Assert.Equal(142.75, metric.Value);
        Assert.Equal(timestamp, metric.ObservedAtUtc);
        Assert.Equal("registration", metric.Dimensions["module"]);

        dimensions["module"] = "changed-after-construction";
        Assert.Equal("registration", metric.Dimensions["module"]);

        Assert.Throws<ArgumentException>(() =>
            new OperationalMetric(" ", 1, timestamp));
        Assert.Throws<ArgumentException>(() =>
            new OperationalMetric("invalid metric name", 1, timestamp));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OperationalMetric("metric", double.NaN, timestamp));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OperationalMetric("metric", double.PositiveInfinity, timestamp));
        Assert.Throws<ArgumentException>(() =>
            new OperationalMetric(
                "metric",
                1,
                DateTime.SpecifyKind(timestamp, DateTimeKind.Unspecified)));

        var tooManyDimensions = Enumerable
            .Range(1, OperationalMetric.MaxDimensions + 1)
            .ToDictionary(index => $"key{index}", index => $"value{index}");
        Assert.Throws<ArgumentException>(() =>
            new OperationalMetric("metric", 1, timestamp, tooManyDimensions));
        Assert.Throws<ArgumentException>(() =>
            new OperationalMetric(
                "metric",
                1,
                timestamp,
                new Dictionary<string, string> { ["invalid key"] = "value" }));
        Assert.Throws<ArgumentException>(() =>
            new OperationalMetric(
                "metric",
                1,
                timestamp,
                new Dictionary<string, string> { ["module"] = " " }));
    }

    [Fact]
    public void Null_dimensions_become_an_empty_read_only_collection()
    {
        var metric = new OperationalMetric(
            "api.requests.total",
            12,
            new DateTime(2026, 7, 14, 1, 45, 0, DateTimeKind.Utc));

        Assert.Empty(metric.Dimensions);
        var mutableView = Assert.IsAssignableFrom<IDictionary<string, string>>(
            metric.Dimensions);
        Assert.Throws<NotSupportedException>(() => mutableView.Add("module", "api"));
    }

    [Fact]
    public void Json_contract_serializes_only_the_bounded_aggregate_shape()
    {
        var metric = new OperationalMetric(
            "sql.lock_wait.duration_ms",
            6.25,
            new DateTime(2026, 7, 14, 1, 45, 0, DateTimeKind.Utc),
            new Dictionary<string, string>
            {
                ["module"] = "registration",
                ["operation"] = "seat-allocation"
            });

        var json = JsonSerializer.Serialize(metric, JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(
            ["dimensions", "name", "observedAtUtc", "value"],
            root.EnumerateObject()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Equal("sql.lock_wait.duration_ms", root.GetProperty("name").GetString());
        Assert.Equal(6.25, root.GetProperty("value").GetDouble());
        Assert.Equal(
            "2026-07-14T01:45:00Z",
            root.GetProperty("observedAtUtc").GetString());
        Assert.Equal(
            "registration",
            root.GetProperty("dimensions").GetProperty("module").GetString());

        var roundTrip = JsonSerializer.Deserialize<OperationalMetric>(
            json,
            JsonSerializerOptions.Web);
        Assert.NotNull(roundTrip);
        Assert.Equal(metric.Name, roundTrip.Name);
        Assert.Equal(metric.Value, roundTrip.Value);
        Assert.Equal(metric.ObservedAtUtc, roundTrip.ObservedAtUtc);
        Assert.Equal(metric.Dimensions, roundTrip.Dimensions);
    }
}
