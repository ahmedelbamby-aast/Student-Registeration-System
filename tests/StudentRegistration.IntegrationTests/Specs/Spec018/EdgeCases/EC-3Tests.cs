using System.Text.Json;
using Microsoft.AspNetCore.Http;
using StudentRegistration.Api.Endpoints;
using StudentRegistration.Api.Operations;
using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.IntegrationTests.Specs.Spec018.EdgeCases;

public sealed class EC_3Tests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 14, 11, 15, 0, TimeSpan.Zero);

    private const string TransactionActivationGate =
        "Activation condition: SPEC-014 must deliver its atomic registration handler/transaction and SQL fault-injection fixture before EC-3 can prove no partial durable result and a safe correlated user error for a failed business request.";

    [Fact]
    public void Sql_failure_returns_a_bounded_unhealthy_summary_without_topology_or_secret_detail()
    {
        var registry = new OperationalHealthRegistry();
        registry.SetSqlServerAvailable(false);

        var result = Spec018Endpoints.CreateHealthResult(
            registry,
            new FixedTimeProvider(Now));

        Assert.Equal(
            StatusCodes.Status503ServiceUnavailable,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        var summary = Assert.IsType<HealthSummary>(
            Assert.IsAssignableFrom<IValueHttpResult>(result).Value);
        Assert.Equal(HealthSummaryStatus.Unhealthy, summary.Status);
        Assert.Equal(Now.UtcDateTime, summary.TimestampUtc);

        var payload = JsonSerializer.Serialize(summary, JsonSerializerOptions.Web);
        foreach (var forbidden in new[]
                 {
                     "sql", "server", "database", "connection", "host", "replica",
                     "certificate", "secret", "exception", "stack"
                 })
        {
            Assert.DoesNotContain(forbidden, payload, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact(Skip = TransactionActivationGate)]
    public void Sql_failed_registration_has_no_partial_commit_and_returns_a_reference_id()
    {
        throw new NotImplementedException(
            "Health behavior is executable; the SPEC-014 durable transaction is not.");
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
