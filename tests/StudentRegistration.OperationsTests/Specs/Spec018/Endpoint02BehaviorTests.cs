using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StudentRegistration.Api.Endpoints;
using StudentRegistration.Api.Operations;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.OperationsTests.Specs.Spec018;

public sealed class Endpoint02BehaviorTests
{
    private static readonly DateTime ObservedAtUtc =
        new(2026, 7, 14, 9, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Metrics_read_requires_an_authenticated_server_derived_admin_role()
    {
        using var telemetry = new OperationalTelemetry(new FixedTimeProvider(ObservedAtUtc));

        var unauthenticated = Spec018Endpoints.CreateMetricsResult(
            Context(new ClaimsPrincipal(new ClaimsIdentity())),
            telemetry);
        AssertApiError(
            unauthenticated,
            StatusCodes.Status401Unauthorized,
            "AUTHENTICATION_REQUIRED");

        var student = Principal("Student");
        var forbidden = Spec018Endpoints.CreateMetricsResult(Context(student), telemetry);
        AssertApiError(forbidden, StatusCodes.Status403Forbidden, "ACCESS_DENIED");
    }

    [Fact]
    public void Degraded_or_empty_source_returns_only_currently_observed_metrics()
    {
        using var telemetry = new OperationalTelemetry(new FixedTimeProvider(ObservedAtUtc));
        var context = Context(Principal("Admin"));

        var result = Spec018Endpoints.CreateMetricsResult(context, telemetry);

        Assert.Equal(
            StatusCodes.Status200OK,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        var page = Assert.IsType<Page<OperationalMetric>>(
            Assert.IsAssignableFrom<IValueHttpResult>(result).Value);
        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalCount);
        Assert.Equal("no-store", context.Response.Headers.CacheControl);
    }

    [Fact]
    public void Authorized_metrics_are_fresh_sorted_bounded_and_paginated()
    {
        using var telemetry = new OperationalTelemetry(new FixedTimeProvider(ObservedAtUtc));
        telemetry.Increment(
            OperationalMetricNames.RequestThroughput,
            dimensions: new Dictionary<string, string> { ["method"] = "GET" });
        telemetry.Observe(
            OperationalMetricNames.RequestLatencyMilliseconds,
            18.5,
            new Dictionary<string, string> { ["method"] = "GET" });
        var context = Context(Principal("Admin"), "?page=1&pageSize=1");

        var result = Spec018Endpoints.CreateMetricsResult(context, telemetry);

        Assert.Equal("no-store", context.Response.Headers.CacheControl);
        Assert.Equal(
            StatusCodes.Status200OK,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        var page = Assert.IsType<Page<OperationalMetric>>(
            Assert.IsAssignableFrom<IValueHttpResult>(result).Value);
        Assert.Single(page.Items);
        Assert.Equal(2, page.TotalCount);
        Assert.Equal(ObservedAtUtc, page.Items[0].ObservedAtUtc);
        Assert.Equal("seriesKey", page.Sort);
    }

    [Theory]
    [InlineData("?page=0", "PAGE_SIZE_INVALID")]
    [InlineData("?page=abc", "PAGE_SIZE_INVALID")]
    [InlineData("?pageSize=101", "PAGE_SIZE_INVALID")]
    [InlineData("?pageSize=nope", "PAGE_SIZE_INVALID")]
    public void Invalid_pagination_returns_a_stable_safe_error(
        string query,
        string expectedCode)
    {
        using var telemetry = new OperationalTelemetry(new FixedTimeProvider(ObservedAtUtc));

        var result = Spec018Endpoints.CreateMetricsResult(
            Context(Principal("Admin"), query),
            telemetry);

        AssertApiError(result, StatusCodes.Status400BadRequest, expectedCode);
    }

    [Fact]
    public void Collector_rejects_unknown_or_sensitive_dimensions_and_bounds_cardinality()
    {
        using var telemetry = new OperationalTelemetry(new FixedTimeProvider(ObservedAtUtc));

        Assert.Throws<ArgumentException>(() => telemetry.Increment(
            "student.password",
            dimensions: null));
        Assert.Throws<ArgumentException>(() => telemetry.Increment(
            OperationalMetricNames.RequestThroughput,
            dimensions: new Dictionary<string, string> { ["studentId"] = "20260001" }));
        Assert.Throws<ArgumentException>(() => telemetry.Increment(
            OperationalMetricNames.RequestThroughput,
            dimensions: new Dictionary<string, string> { ["method"] = "ahmed@example.com" }));
        Assert.Throws<ArgumentException>(() => telemetry.Increment(
            OperationalMetricNames.BusinessRejections,
            dimensions: new Dictionary<string, string> { ["code"] = "20260001" }));
        Assert.Throws<ArgumentException>(() => telemetry.Increment(
            OperationalMetricNames.BusinessRejections,
            dimensions: new Dictionary<string, string> { ["module"] = "AhmedElbamby" }));
        Assert.Throws<ArgumentException>(() => telemetry.Increment(
            OperationalMetricNames.BusinessRejections,
            dimensions: new Dictionary<string, string> { ["operation"] = "student-20260001" }));
        Assert.Throws<ArgumentException>(() => telemetry.Increment(
            OperationalMetricNames.BusinessRejections,
            dimensions: new Dictionary<string, string> { ["outcome"] = "ahmed" }));
        Assert.Throws<ArgumentException>(() => telemetry.Increment(
            OperationalMetricNames.BusinessRejections,
            dimensions: new Dictionary<string, string> { ["route"] = "student-20260001" }));

        string[] methods = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT", "OTHER"];
        string[] statuses = ["1xx", "2xx", "3xx", "4xx", "5xx"];
        string[] modules =
            ["academics", "api", "identity", "operations", "registration", "scheduling", "staff-administration"];
        for (var index = 0; index < OperationalTelemetry.MaximumSeries; index++)
        {
            telemetry.Increment(
                OperationalMetricNames.BusinessRejections,
                dimensions: new Dictionary<string, string>
                {
                    ["method"] = methods[index % methods.Length],
                    ["statusClass"] = statuses[(index / methods.Length) % statuses.Length],
                    ["module"] = modules[(index / (methods.Length * statuses.Length)) % modules.Length]
                });
        }

        Assert.Throws<InvalidOperationException>(() => telemetry.Increment(
            OperationalMetricNames.BusinessRejections,
            dimensions: new Dictionary<string, string>
            {
                ["outcome"] = "accepted"
            }));
    }

    private static DefaultHttpContext Context(
        ClaimsPrincipal user,
        string query = "")
    {
        var context = new DefaultHttpContext
        {
            User = user,
            TraceIdentifier = "spec018-correlation"
        };
        context.Request.QueryString = new QueryString(query);
        return context;
    }

    private static ClaimsPrincipal Principal(string role) =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "synthetic-staff"),
                new Claim(ClaimTypes.Role, role)
            ],
            "Spec018Test"));

    private static void AssertApiError(
        IResult result,
        int expectedStatus,
        string expectedCode)
    {
        Assert.Equal(
            expectedStatus,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal(
            expectedCode,
            Assert.IsType<ApiError>(
                Assert.IsAssignableFrom<IValueHttpResult>(result).Value).Code);
    }

    private sealed class FixedTimeProvider(DateTime observedAtUtc) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(observedAtUtc);
    }
}
