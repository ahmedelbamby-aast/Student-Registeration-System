using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec016;

public sealed class StaffRosterPageFeatureTests
{
    [Fact]
    public void T070_stf_03_canonical_page_and_primary_failure_journeys_exist()
    {
        var design = RepositoryFiles.Read("specs/003-ux-storyboard-accessibility/design/pages/STF-03.md");
        RepositoryFiles.ContainsAll(design,
            "STF-03-assigned-v1", "STF-03-unassigned-403-v1", "STF-03-empty-v1",
            "STF-03-paged-v1", "STF-03-assignment-removed-v1");
        Assert.True(RepositoryFiles.Exists("src/StudentRegistration.Client/Pages/StaffRosterPage.razor"));
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class StaffRosterPageBrowserTests(Spec008BrowserFixture fixture)
{
    [Fact]
    public async Task Stf_03_assigned_v1_has_only_the_three_allowed_fields()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Context()),
            var path when path.EndsWith("/roster", StringComparison.Ordinal) => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Roster),
            _ => route.AbortAsync()
        });
        await page.GotoAsync($"/staff/groups/{Spec016BrowserData.GroupId}/roster", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STF-03'][data-state='success']").WaitForAsync();
        Assert.True(await page.GetByText("Amina Hassan", new() { Exact = true }).First.IsVisibleAsync());
        Assert.Equal(3, await page.GetByRole(AriaRole.Columnheader).CountAsync());
        Assert.Equal(0, await page.GetByText("GPA", new() { Exact = true }).CountAsync());
    }

    [Fact]
    public async Task Stf_03_unassigned_403_v1_reveals_no_group_or_roster_data()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Context()),
            var path when path.EndsWith("/roster", StringComparison.Ordinal) => Spec016BrowserData.JsonAsync(route, 404, Spec016BrowserData.Error("STAFF_GROUP_NOT_FOUND")),
            _ => route.AbortAsync()
        });
        await page.GotoAsync($"/staff/groups/{Spec016BrowserData.GroupId}/roster", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STF-03'][data-state='unauthorized']").WaitForAsync();
        Assert.True(await page.GetByRole(AriaRole.Heading, new() { Name = "Access denied", Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("Amina Hassan", new() { Exact = true }).CountAsync());
        Assert.Equal(0, await page.GetByRole(AriaRole.Table).CountAsync());
    }
}
