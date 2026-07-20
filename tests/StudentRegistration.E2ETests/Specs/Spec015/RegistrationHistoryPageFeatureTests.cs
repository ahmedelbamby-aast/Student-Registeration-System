using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec015;

public sealed class RegistrationHistoryPageFrozenContractTests
{
    [Fact]
    public void Stu_07_owner_record_and_required_journeys_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-07.md");
        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/student/registrations\"",
            "\"pageName\": \"RegistrationHistoryPage.razor\"",
            "STU-07-empty-v1",
            "STU-07-current-v1",
            "STU-07-history-v1",
            "STU-07-archived-v1",
            "STU-07-service-error-v1",
            "STU-07-E2E-PRIMARY",
            "STU-07-E2E-FAILURE");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/RegistrationHistoryPage.razor"),
            "SPEC-015/T053 must deliver the sole canonical STU-07 Razor page.");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class RegistrationHistoryPageFeatureTests(Spec008BrowserFixture fixture)
{
    [Fact]
    public async Task Stu_07_current_and_history_v1_show_equivalent_timetable_and_bounded_rows()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteAsync(page, Spec015BrowserData.History, Spec015BrowserData.Timetable);

        await OpenAsync(page);

        await page.Locator("[data-route-id='STU-07'][data-state='success']").WaitForAsync();
        Assert.True(await page.GetByText("Fall 2026", new() { Exact = true }).First.IsVisibleAsync());
        Assert.True(await page.GetByText("REG-2026-015001", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("Fall 2025 (Archived)", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("2 Registration records", new() { Exact = true }).IsVisibleAsync());
        var calendar = await page.Locator(".srs-schedule-calendar [data-meeting-id]")
            .EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.getAttribute('data-meeting-id'))");
        var list = await page.Locator(".srs-schedule-list [data-meeting-id]")
            .EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.getAttribute('data-meeting-id'))");
        Assert.Equal(calendar, list);
        Assert.Equal([Spec015BrowserData.MeetingId], calendar);
        Assert.Equal(0, await page.GetByText("Drop", new() { Exact = true }).CountAsync());
        Assert.Equal(0, await page.GetByText("Correction", new() { Exact = true }).CountAsync());
    }

    [Fact]
    public async Task Stu_07_empty_v1_links_to_subject_discovery_only_while_window_is_open()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteAsync(page, Spec015BrowserData.EmptyHistory, Spec015BrowserData.EmptyTimetable);

        await OpenAsync(page);

        await page.Locator("[data-route-id='STU-07'][data-state='empty']").WaitForAsync();
        Assert.True(await page.GetByRole(AriaRole.Heading, new() { Name = "No registrations for Fall 2026", Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByRole(AriaRole.Link, new() { Name = "Find subjects", Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByRole(AriaRole.Table).CountAsync());
    }

    [Fact]
    public async Task Stu_07_service_error_v1_is_safe_retryable_and_does_not_reuse_cached_data()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", async route =>
        {
            if (Spec015BrowserData.Path(route) == "/api/context")
            {
                await Spec015BrowserData.JsonAsync(route, 200, Spec015BrowserData.AppContext);
            }
            else
            {
                await Spec015BrowserData.JsonAsync(route, 503, Spec015BrowserData.Error("SERVICE_UNAVAILABLE"));
            }
        });

        await OpenAsync(page);

        await page.Locator("[data-route-id='STU-07'][data-state='service-error']").WaitForAsync();
        Assert.True(await page.GetByText("SERVICE_UNAVAILABLE", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("STU-015-SAFE", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByRole(AriaRole.Button, new() { Name = "Retry registration records", Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("REG-2026-015001", new() { Exact = true }).CountAsync());
    }

    private static async Task RouteAsync(IPage page, string history, string timetable)
    {
        await page.RouteAsync("**/api/**", async route =>
        {
            var path = Spec015BrowserData.Path(route);
            if (path == "/api/context")
            {
                await Spec015BrowserData.JsonAsync(route, 200, Spec015BrowserData.AppContext);
            }
            else if (path == "/api/student/registrations")
            {
                await Spec015BrowserData.JsonAsync(route, 200, history);
            }
            else if (path == "/api/student/registrations/current/timetable")
            {
                await Spec015BrowserData.JsonAsync(route, 200, timetable);
            }
            else
            {
                await route.AbortAsync();
            }
        });
    }

    private static async Task OpenAsync(IPage page)
    {
        await page.GotoAsync("/student/registrations", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.GetByRole(AriaRole.Heading, new() { Name = "Registrations", Exact = true }).WaitForAsync();
    }
}
