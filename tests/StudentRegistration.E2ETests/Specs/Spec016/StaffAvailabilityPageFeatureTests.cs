using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec016;

public sealed class StaffAvailabilityPageFeatureTests
{
    [Fact]
    public void T072_stf_04_canonical_page_and_primary_failure_journeys_exist()
    {
        var design = RepositoryFiles.Read("specs/003-ux-storyboard-accessibility/design/pages/STF-04.md");
        RepositoryFiles.ContainsAll(design,
            "STF-04-draft-v1", "STF-04-saved-v1", "STF-04-overlap-v1",
            "STF-04-deadline-v1", "STF-04-published-warning-v1", "STF-04-stale-edit-v1");
        Assert.True(RepositoryFiles.Exists("src/StudentRegistration.Client/Pages/StaffAvailabilityPage.razor"));
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class StaffAvailabilityPageBrowserTests(Spec008BrowserFixture fixture)
{
    [Fact]
    public async Task Stf_04_saved_and_published_warning_v1_waits_for_server_acceptance()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Context()),
            "/api/staff/availability" when route.Request.Method == "GET" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Availability),
            "/api/staff/availability" when route.Request.Method == "PUT" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.AvailabilityUpdated),
            _ => route.AbortAsync()
        });
        await page.GotoAsync("/staff/availability", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STF-04'][data-state='success']").WaitForAsync();
        await page.Locator($"#start-{Spec016BrowserData.RangeId}").FillAsync("10:00");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save availability", Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Heading, new() { Name = "Availability saved", Exact = true }).WaitForAsync();
        Assert.True(await page.GetByText("No class, room, or staff assignment moved automatically.", new() { Exact = true }).IsVisibleAsync());
    }

    [Theory]
    [InlineData("STALE_VERSION")]
    [InlineData("AVAILABILITY_DEADLINE_PASSED")]
    public async Task Stf_04_concurrent_or_deadline_failure_never_claims_success(string code)
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Context()),
            "/api/staff/availability" when route.Request.Method == "GET" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Availability),
            "/api/staff/availability" when route.Request.Method == "PUT" => Spec016BrowserData.JsonAsync(route, 409, Spec016BrowserData.AvailabilityConflict(code)),
            _ => route.AbortAsync()
        });
        await page.GotoAsync("/staff/availability", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator($"#start-{Spec016BrowserData.RangeId}").FillAsync("10:00");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save availability", Exact = true }).ClickAsync();
        await page.Locator("[data-route-id='STF-04'][data-state='stale']").WaitForAsync();
        Assert.True(await page.GetByText(code, new() { Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByRole(AriaRole.Heading, new() { Name = "Availability saved", Exact = true }).CountAsync());
    }
}
