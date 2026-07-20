using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec017;

public sealed class AdminDashboardPageFeatureTests
{
    [Fact]
    public void Adm_01_frozen_journeys_have_one_canonical_implementation()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-01.md");
        RepositoryFiles.ContainsAll(
            design,
            "ADM-01-live-v1",
            "ADM-01-paused-refresh-v1",
            "ADM-01-stale-v1",
            "ADM-01-degraded-v1",
            "ADM-01-unauthorized-v1",
            "ADM-01-E2E-PRIMARY",
            "ADM-01-E2E-FAILURE");

        const string pagePath =
            "src/StudentRegistration.Client/Pages/AdminDashboardPage.razor";
        Assert.True(
            RepositoryFiles.Exists(pagePath),
            "Expected-red for T073: the sole ADM-01 page is deferred to T074.");
        var page = RepositoryFiles.Read(pagePath);
        RepositoryFiles.ContainsAll(
            page,
            "data-refresh-state",
            "PauseRefresh",
            "ResumeRefreshAsync",
            "LoadMetricsAsync",
            "Access denied",
            "Service degraded",
            "Reference:",
            "Return to authorized home");

        var canonicalCount = Directory
            .EnumerateFiles(
                RepositoryFiles.PathTo("src/StudentRegistration.Client"),
                "AdminDashboardPage.razor",
                SearchOption.AllDirectories)
            .Count();
        Assert.Equal(1, canonicalCount);
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class AdminDashboardPageBrowserTests(Spec008BrowserFixture fixture)
{
    [Fact]
    public async Task Adm_01_live_and_paused_refresh_journeys_preserve_server_timestamp_and_values()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteAsync(page, 200, Metrics("live"));

        await page.GotoAsync("/admin", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='ADM-01'][data-state='success']").WaitForAsync();
        Assert.True(await page.GetByText("Live metrics", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("75 requests", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.Locator("time[datetime^='2026-07-17T09:30:00']").First
            .IsVisibleAsync());

        await page.GetByRole(AriaRole.Button, new() { Name = "Pause refresh", Exact = true }).ClickAsync();
        await page.Locator("[data-refresh-state='paused']").WaitForAsync();
        Assert.True(await page.GetByText("Refresh paused", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByRole(AriaRole.Button, new() { Name = "Resume refresh", Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("75 requests", new() { Exact = true }).IsVisibleAsync());

        await page.GetByRole(AriaRole.Button, new() { Name = "Resume refresh", Exact = true }).ClickAsync();
        await page.Locator("[data-refresh-state='running']").WaitForAsync();
    }

    [Theory]
    [InlineData("stale", "stale", "Stale metrics")]
    [InlineData("degraded", "service-error", "Degraded metrics")]
    public async Task Adm_01_stale_and_degraded_journeys_are_explicit_and_keep_text_equivalents(
        string availability,
        string state,
        string statusText)
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteAsync(page, 200, Metrics(availability, includeOnlyTraffic: true));

        await page.GotoAsync("/admin", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator($"[data-route-id='ADM-01'][data-state='{state}']").WaitForAsync();
        Assert.True(await page.GetByText(statusText, new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("75 requests", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("Not reported by the server; no zero value has been assumed.", new() { Exact = true }).IsVisibleAsync());
    }

    [Fact]
    public async Task Adm_01_unauthorized_journey_hides_all_metric_content()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteAsync(page, 403, Error("FORBIDDEN"));

        await page.GotoAsync("/admin", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='ADM-01'][data-state='unauthorized']").WaitForAsync();
        Assert.True(await page.GetByRole(AriaRole.Heading, new() { Name = "Access denied", Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("75 requests", new() { Exact = true }).CountAsync());
        Assert.Equal(0, await page.GetByRole(AriaRole.Region, new() { Name = "Operational metrics" }).CountAsync());
    }

    private static Task RouteAsync(IPage page, int metricsStatus, string metricsBody) =>
        page.RouteAsync("**/api/**", route => Path(route) switch
        {
            "/api/context" => JsonAsync(route, 200, Context),
            "/api/admin/operations/metrics" => JsonAsync(route, metricsStatus, metricsBody),
            _ => route.AbortAsync()
        });

    private const string Context = """
        {"serverTimeUtc":"2026-07-17T09:30:00Z","timeZoneId":"Africa/Cairo",
        "teachingTerm":null,"registrationTerm":{"id":"00000000-0000-0000-0000-000000017001",
        "code":"2026-FALL","label":"Fall 2026","state":"registrationOpen","rowVersion":"TERM-RV-1"},
        "registrationWindowState":"open","registrationWindow":{"id":"00000000-0000-0000-0000-000000017002",
        "state":"open","opensAtUtc":"2026-07-17T08:00:00Z","closesAtUtc":"2026-07-24T16:00:00Z","rowVersion":"WINDOW-RV-1"},
        "serviceState":"available","displayName":"Demo Admin",
        "authorizedRoles":["Admin"],"activeRole":"Admin","sessionState":"active",
        "expiresAtUtc":"2026-07-17T11:30:00Z","supportReferencePath":"/support/admin"}
        """;

    private static string Metrics(string availability, bool includeOnlyTraffic = false)
    {
        var metrics = includeOnlyTraffic
            ? Metric("http.request.throughput", 75, "requests", availability)
            : string.Join(",", new[]
            {
                Metric("http.request.throughput", 75, "requests", availability),
                Metric("registration.submissions.succeeded", 60, "registrations", availability),
                Metric("registration.business_rejections", 12, "rejections", availability),
                Metric("http.request.unexpected_errors", 3, "failures", availability),
                Metric("registration.group.fill_rate", 82.5, "%", availability),
                Metric("sql.lock_wait.duration.ms", 18, "ms", availability),
                Metric("registration.counter_mismatches", 1, "alerts", availability),
                Metric("registration.capacity_conflicts", 2, "conflicts", availability)
            });
        return $$"""
            {"observedAtUtc":"2026-07-17T09:30:00Z","availabilityState":"{{availability}}",
            "metrics":[{{metrics}}],"reconciliationAlerts":[]}
            """;
    }

    private static string Metric(string name, double value, string unit, string availability) =>
        $$"""{"name":"{{name}}","value":{{value.ToString(System.Globalization.CultureInfo.InvariantCulture)}},"dimensions":{},"unit":"{{unit}}","observedAtUtc":"2026-07-17T09:30:00Z","availabilityState":"{{availability}}"}""";

    private static string Error(string code) =>
        $$"""{"code":"{{code}}","message":"The request is not authorized.","correlationId":"ADM-017-SAFE"}""";

    private static string Path(IRoute route) => new Uri(route.Request.Url).AbsolutePath;

    private static Task JsonAsync(IRoute route, int status, string body) =>
        route.FulfillAsync(new() { Status = status, ContentType = "application/json", Body = body });
}
