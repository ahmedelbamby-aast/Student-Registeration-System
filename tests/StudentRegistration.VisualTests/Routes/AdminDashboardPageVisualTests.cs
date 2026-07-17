using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public sealed class AdminDashboardPageVisualTests
{
    [Fact]
    public void Adm_01_preserves_the_approved_bounded_responsive_layout()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-01.md");
        RepositoryFiles.ContainsAll(
            design,
            "\"320\"",
            "\"375\"",
            "\"768\"",
            "\"1024\"",
            "\"1280\"",
            "\"1920\"",
            "Centered bounded dashboard",
            "Every chart has adjacent text");

        const string pagePath =
            "src/StudentRegistration.Client/Pages/AdminDashboardPage.razor";
        const string cssPath =
            "src/StudentRegistration.Client/Pages/AdminDashboardPage.razor.css";
        Assert.True(
            RepositoryFiles.Exists(pagePath) && RepositoryFiles.Exists(cssPath),
            "Expected-red for T073: ADM-01 visual implementation is deferred to T074.");
        var page = RepositoryFiles.Read(pagePath);
        var css = RepositoryFiles.Read(cssPath);
        RepositoryFiles.ContainsAll(
            page,
            "srs-admin-dashboard__metrics",
            "srs-admin-dashboard__warnings",
            "srs-admin-dashboard__modules");
        RepositoryFiles.ContainsAll(
            css,
            "inline-size: min(",
            "margin-inline: auto",
            "grid-template-columns: repeat(4",
            "grid-template-columns: repeat(3",
            "grid-template-columns: repeat(2",
            "grid-template-columns: minmax(0, 1fr)");
        Assert.DoesNotContain("inline-size: 1920px", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("min-inline-size: 1024px", css, StringComparison.OrdinalIgnoreCase);
    }
}

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class AdminDashboardPageResponsiveVisualTests(VisualRegressionFixture fixture)
{
    [Theory]
    [InlineData(375, 1)]
    [InlineData(768, 2)]
    [InlineData(1024, 3)]
    [InlineData(1280, 4)]
    [InlineData(1920, 4)]
    public async Task Adm_01_metric_grid_reflows_without_unbounded_desktop_growth(
        int width,
        int expectedColumns)
    {
        await using var context = await fixture.OpenContextAsync("webkit", width);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Path(route) switch
        {
            "/api/context" => JsonAsync(route, Context),
            "/api/admin/operations/metrics" => JsonAsync(route, Metrics),
            _ => route.AbortAsync()
        });
        await page.GotoAsync("/admin", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        var grid = page.Locator(".srs-admin-dashboard__metrics");
        await grid.WaitForAsync();

        var columns = await grid.EvaluateAsync<int>(
            "element => getComputedStyle(element).gridTemplateColumns.split(' ').filter(Boolean).length");
        Assert.Equal(expectedColumns, columns);
        var main = await page.Locator("#admin-dashboard-main").BoundingBoxAsync();
        Assert.NotNull(main);
        Assert.True(main!.Width <= Math.Min(width, 1312));
        Assert.True(await page.GetByText("75 requests", new() { Exact = true }).IsVisibleAsync());
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

    private static Task JsonAsync(IRoute route, string body) =>
        route.FulfillAsync(new() { Status = 200, ContentType = "application/json", Body = body });
}
