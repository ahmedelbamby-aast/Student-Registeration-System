using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using StudentRegistration.Api.Endpoints;
using StudentRegistration.Api.Operations;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Operations;
using StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Endpoints;

namespace StudentRegistration.IntegrationTests.Specs.Spec018.EdgeCases;

public sealed class EC_3Tests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 14, 11, 15, 0, TimeSpan.Zero);

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

    [Fact]
    public void Sql_failed_registration_has_no_partial_commit_and_returns_a_reference_id()
    {
        var store = new AtomicRegistrationStore(capacity: 2);
        var transaction = store.Begin();

        Assert.Throws<SimulatedSqlUnavailableException>(new Action(() =>
        {
            try
            {
                transaction.Claim("synthetic-payload-hash");
                transaction.CreateAllocationSavepoint();
                transaction.Allocate();
                throw new SimulatedSqlUnavailableException();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }));

        var committed = store.Committed;
        Assert.False(committed.Processing);
        Assert.Null(committed.ClaimPayloadHash);
        Assert.Equal(0, committed.SeatCount);
        Assert.Equal(0, committed.EnrollmentCount);
        Assert.Equal(0, committed.ExecutionCount);
        Assert.Null(committed.ResultCode);
        Assert.False(committed.ReceiptStored);

        var context = new DefaultHttpContext { TraceIdentifier = string.Empty };
        var result = UnavailableRegistrationResult(context);
        Assert.Equal(
            StatusCodes.Status503ServiceUnavailable,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        var error = Assert.IsType<ApiError>(
            Assert.IsAssignableFrom<IValueHttpResult>(result).Value);
        Assert.Equal("REGISTRATION_UNAVAILABLE", error.Code);
        Assert.Matches("^[a-f0-9]{32}$", error.CorrelationId);
        Assert.Equal(context.TraceIdentifier, error.CorrelationId);
        Assert.DoesNotContain("sql", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static IResult UnavailableRegistrationResult(HttpContext context)
    {
        var method = typeof(Spec014Endpoints).GetMethod(
            "ToPostResult",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                "The SPEC-014 endpoint result boundary is unavailable.");
        return Assert.IsAssignableFrom<IResult>(method.Invoke(
            null,
            [context, new RegistrationEndpointResult(RegistrationEndpointOutcome.Unavailable)]));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class SimulatedSqlUnavailableException : Exception;
}
