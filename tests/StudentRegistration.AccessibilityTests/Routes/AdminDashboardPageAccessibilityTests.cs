using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class AdminDashboardPageAccessibilityTests
{
    [Fact]
    public void Adm_01_has_keyboard_order_live_regions_timestamps_and_text_equivalent_metrics()
    {
        const string pagePath =
            "src/StudentRegistration.Client/Pages/AdminDashboardPage.razor";
        Assert.True(
            RepositoryFiles.Exists(pagePath),
            "Expected-red for T073: ADM-01 accessibility delivery is deferred to T074.");
        var page = RepositoryFiles.Read(pagePath);

        RepositoryFiles.ContainsAll(
            page,
            "Skip to main content",
            "Administration navigation",
            "<h1",
            "aria-live=\"polite\"",
            "aria-live=\"assertive\"",
            "<time datetime=",
            "type=\"button\"",
            "aria-label=\"Operational metrics\"",
            "<dl",
            "<dt>",
            "<dd>",
            "tabindex=\"-1\"");

        var cssPath =
            "src/StudentRegistration.Client/Pages/AdminDashboardPage.razor.css";
        Assert.True(
            RepositoryFiles.Exists(cssPath),
            "Expected-red for T073: ADM-01 scoped responsive CSS is deferred to T074.");
        var css = RepositoryFiles.Read(cssPath);
        RepositoryFiles.ContainsAll(
            css,
            "min-block-size: var(--srs-sizing-interactive-minimum)",
            ":focus-visible",
            "@media (max-width:",
            "@media (prefers-reduced-motion: reduce)");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class AdminDashboardPageAxeTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [MemberData(nameof(Spec003RequestedRouteAccessibilityAssertions.WidthProfiles), MemberType = typeof(Spec003RequestedRouteAccessibilityAssertions))]
    public async Task Adm_01_live_state_has_no_serious_axe_failures_and_controls_remain_operable(
        int width,
        float scale)
    {
        await using var context = await fixture.OpenContextAsync(width, 1000, scale);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Path(route) switch
        {
            "/api/context" => JsonAsync(route, 200, Context),
            "/api/admin/operations/metrics" => JsonAsync(route, 200, Metrics),
            _ => route.AbortAsync()
        });

        await page.GotoAsync("/admin", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='ADM-01'][data-state='success']").WaitForAsync();
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);

        var pause = await page.GetByRole(AriaRole.Button, new() { Name = "Pause refresh", Exact = true })
            .BoundingBoxAsync();
        Assert.NotNull(pause);
        Assert.True(pause!.Height >= 44);
        var skip = page.GetByRole(AriaRole.Link, new() { Name = "Skip to main content", Exact = true });
        await skip.FocusAsync();
        Assert.True(await skip.EvaluateAsync<bool>(
            "element => getComputedStyle(element).outlineStyle !== 'none'"));
        Assert.True(await page.GetByRole(AriaRole.Region, new() { Name = "Operational metrics" }).IsVisibleAsync());
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
    }

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

    private const string Metrics = """
        {"observedAtUtc":"2026-07-17T09:30:00Z","availabilityState":"live",
        "metrics":[{"name":"http.request.throughput","value":75,"dimensions":{},"unit":"requests",
        "observedAtUtc":"2026-07-17T09:30:00Z","availabilityState":"live"}],
        "reconciliationAlerts":[]}
        """;

    private static string Path(IRoute route) => new Uri(route.Request.Url).AbsolutePath;

    private static Task JsonAsync(IRoute route, int status, string body) =>
        route.FulfillAsync(new() { Status = status, ContentType = "application/json", Body = body });
}
