using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec016;

public sealed class StaffDashboardPageFeatureTests
{
    [Fact]
    public void T066_stf_01_canonical_page_and_primary_failure_journeys_exist()
    {
        var design = RepositoryFiles.Read("specs/003-ux-storyboard-accessibility/design/pages/STF-01.md");
        RepositoryFiles.ContainsAll(design,
            "STF-01-lecturer-v1", "STF-01-ta-v1",
            "STF-01-no-assignment-v1", "STF-01-stale-assignment-v1");
        Assert.True(RepositoryFiles.Exists("src/StudentRegistration.Client/Pages/StaffDashboardPage.razor"));
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class StaffDashboardPageBrowserTests(Spec008BrowserFixture fixture)
{
    [Fact]
    public async Task Stf_01_lecturer_v1_shows_only_server_authorized_assignments()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Context()),
            "/api/staff/assignments" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Assignments),
            _ => route.AbortAsync()
        });
        await page.GotoAsync("/staff", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STF-01'][data-state='success']").WaitForAsync();
        Assert.True(await page.GetByText("Role context:", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("AI401 — L01", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByRole(AriaRole.Link, new() { Name = "Open assigned roster", Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("AI999", new() { Exact = true }).CountAsync());
    }

    [Fact]
    public async Task Stf_01_stale_assignment_v1_removes_actions_and_offers_refresh()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Context()),
            "/api/staff/assignments" => Spec016BrowserData.JsonAsync(route, 503, Spec016BrowserData.Error("ASSIGNMENT_CHANGED")),
            _ => route.AbortAsync()
        });
        await page.GotoAsync("/staff", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STF-01'][data-state='stale']").WaitForAsync();
        Assert.True(await page.GetByRole(AriaRole.Button, new() { Name = "Refresh assignments", Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByRole(AriaRole.Link, new() { Name = "Open assigned roster", Exact = true }).CountAsync());
    }
}
