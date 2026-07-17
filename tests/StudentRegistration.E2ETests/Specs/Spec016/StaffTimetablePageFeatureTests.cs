using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec016;

public sealed class StaffTimetablePageFeatureTests
{
    [Fact]
    public void T068_stf_02_canonical_page_and_primary_failure_journeys_exist()
    {
        var design = RepositoryFiles.Read("specs/003-ux-storyboard-accessibility/design/pages/STF-02.md");
        RepositoryFiles.ContainsAll(design,
            "STF-02-current-v1", "STF-02-history-v1", "STF-02-no-assignments-v1",
            "STF-02-stale-v1", "STF-02-equivalent-views-v1");
        Assert.True(RepositoryFiles.Exists("src/StudentRegistration.Client/Pages/StaffTimetablePage.razor"));
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class StaffTimetablePageBrowserTests(Spec008BrowserFixture fixture)
{
    [Fact]
    public async Task Stf_02_equivalent_views_v1_use_identical_meeting_ids()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Context()),
            "/api/staff/timetable" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Timetable),
            _ => route.AbortAsync()
        });
        await page.GotoAsync("/staff/timetable", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STF-02'][data-state='success']").WaitForAsync();
        var calendar = await page.Locator(".srs-schedule-calendar [data-meeting-id]").EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.meetingId)");
        var list = await page.Locator(".srs-schedule-list [data-meeting-id]").EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.meetingId)");
        Assert.Equal(calendar, list);
        Assert.Equal([Spec016BrowserData.MeetingId], calendar);
    }

    [Fact]
    public async Task Stf_02_no_assignments_v1_has_no_broad_search()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Context()),
            "/api/staff/timetable" => Spec016BrowserData.JsonAsync(route, 200, "{\"roleContext\":\"Lecturer\",\"assignments\":[]}"),
            _ => route.AbortAsync()
        });
        await page.GotoAsync("/staff/timetable", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STF-02'][data-state='empty']").WaitForAsync();
        Assert.True(await page.GetByRole(AriaRole.Heading, new() { Name = "No authorized timetable assignments", Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("Search all groups", new() { Exact = true }).CountAsync());
    }
}
