using System.Net;
using System.Net.Http.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Client.Features.Operations;
using StudentRegistration.Client.Pages;
using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.E2ETests.Routes;

public sealed class SystemStatusPageJourneyTests
{
    [Theory]
    [InlineData("403", "Access unavailable")]
    [InlineData("404", "Page not found")]
    [InlineData("expired", "Session expired")]
    [InlineData("maintenance", "Maintenance in progress")]
    [InlineData("offline", "You are offline")]
    [InlineData("unexpected", "Unexpected error")]
    public void Safe_local_statuses_render_without_request_or_diagnostic_disclosure(
        string code,
        string heading)
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException());
        using var context = CreateContext(handler);

        var page = context.Render<SystemStatusPage>(parameters =>
            parameters.Add(component => component.Code, code));

        page.WaitForAssertion(() => Assert.Contains(heading, page.Markup, StringComparison.Ordinal));
        Assert.Empty(handler.Requests);
        Assert.DoesNotContain("stack trace", page.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", page.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("credential", page.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HealthSummaryStatus.Healthy, "Systems available")]
    [InlineData(HealthSummaryStatus.Degraded, "Service degraded")]
    [InlineData(HealthSummaryStatus.Unhealthy, "Service unavailable")]
    public void Health_status_uses_the_safe_server_summary(
        HealthSummaryStatus status,
        string heading)
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(
            status == HealthSummaryStatus.Unhealthy
                ? HttpStatusCode.ServiceUnavailable
                : HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new HealthSummary(
                status,
                "demo-1.0",
                new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc)))
        });
        using var context = CreateContext(handler);

        var page = context.Render<SystemStatusPage>(parameters =>
            parameters.Add(component => component.Code, status.ToString().ToLowerInvariant()));

        page.WaitForAssertion(() =>
        {
            Assert.Contains(heading, page.Markup, StringComparison.Ordinal);
            Assert.Contains("demo-1.0", page.Markup, StringComparison.Ordinal);
            Assert.Contains("2026-07-16", page.Markup, StringComparison.Ordinal);
        });
        Assert.Equal("/api/health", Assert.Single(handler.Requests).AbsolutePath);
    }

    private static BunitContext CreateContext(RecordingHandler handler)
    {
        var context = new BunitContext();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://status.test/")
        };
        context.Services.AddSingleton(httpClient);
        context.Services.AddSingleton<OperationsApiClient>();
        return context;
    }

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!);
            return Task.FromResult(responder(request));
        }
    }
}
