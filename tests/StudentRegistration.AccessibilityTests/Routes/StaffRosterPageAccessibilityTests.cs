using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.E2ETests.Specs.Spec016;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StaffRosterPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Stf_03_has_caption_headers_labelled_overflow_and_keyboard_paging()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffRosterPage.razor");
        RepositoryFiles.ContainsAll(page, "role=\"region\"", "tabindex=\"0\"",
            "<caption>", "scope=\"col\"", "Pagination", "aria-label=\"Roster compact view\"");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class StaffRosterPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [MemberData(nameof(Spec003RequestedRouteAccessibilityAssertions.WidthProfiles), MemberType = typeof(Spec003RequestedRouteAccessibilityAssertions))]
    public async Task Stf_03_scoped_roster_table_and_paging_are_accessible(int width, float scale)
    {
        await using var context = await fixture.OpenContextAsync(width, 1000, scale);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(
                route, 200, Spec016BrowserData.Context()),
            var path when path.EndsWith("/roster", StringComparison.Ordinal) =>
                Spec016BrowserData.JsonAsync(route, 200, Spec016BrowserData.Roster),
            _ => route.AbortAsync()
        });

        await page.GotoAsync(
            $"/staff/groups/{Spec016BrowserData.GroupId}/roster",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STF-03'][data-state='success']")
            .WaitForAsync();
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.Equal(3, await page.GetByRole(AriaRole.Columnheader).CountAsync());
        Assert.True(await page.GetByText("Amina Hassan", new() { Exact = true }).First
            .IsVisibleAsync());
        var region = page.GetByRole(
            AriaRole.Region,
            new() { Name = "Assigned group roster table", Exact = true });
        await region.FocusAsync();
        Assert.Equal("0", await region.GetAttributeAsync("tabindex"));
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
    }
}
