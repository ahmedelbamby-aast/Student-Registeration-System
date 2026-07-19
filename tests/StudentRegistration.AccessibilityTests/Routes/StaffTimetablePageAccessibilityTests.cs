using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.E2ETests.Specs.Spec016;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StaffTimetablePageAccessibilityTests
{
    [Fact]
    public void Stf_02_has_information_equivalent_calendar_and_list_regions()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffTimetablePage.razor");
        RepositoryFiles.ContainsAll(page, "ScheduleCalendar", "ScheduleList",
            "EquivalentListId", "EquivalentCalendarId", "Meetings=\"@_meetings\"");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class StaffTimetablePageBrowserAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [MemberData(nameof(Spec003RequestedRouteAccessibilityAssertions.WidthProfiles), MemberType = typeof(Spec003RequestedRouteAccessibilityAssertions))]
    public async Task Stf_02_equivalent_views_reflow_and_remain_keyboard_accessible(int width, float scale)
    {
        await using var context = await fixture.OpenContextAsync(width, 1000, scale);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Context()),
            "/api/staff/timetable" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Timetable),
            _ => route.AbortAsync()
        });

        await page.GotoAsync("/staff/timetable", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STF-02'][data-state='success']").WaitForAsync();
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        var calendar = await page.Locator(".srs-schedule-calendar [data-meeting-id]")
            .EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.meetingId)");
        var list = await page.Locator(".srs-schedule-list [data-meeting-id]")
            .EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.meetingId)");
        Assert.Equal(calendar, list);
        Assert.Equal([Spec016BrowserData.MeetingId], calendar);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        var skip = page.GetByRole(AriaRole.Link, new() { Name = "Skip to main content", Exact = true });
        await skip.FocusAsync();
        Assert.True(await skip.EvaluateAsync<bool>(
            "element => getComputedStyle(element).outlineStyle !== 'none'"));
    }
}
