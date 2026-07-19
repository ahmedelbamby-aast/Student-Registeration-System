using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.E2ETests.Specs.Spec016;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StaffDashboardPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Stf_01_preserves_skip_navigation_heading_and_role_context_order()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffDashboardPage.razor");
        RepositoryFiles.ContainsAll(page, "Skip to main content", "Staff role navigation",
            "<h1", "<label", "<select", "Switch context", "aria-live");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class StaffDashboardPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [MemberData(nameof(Spec003RequestedRouteAccessibilityAssertions.WidthProfiles), MemberType = typeof(Spec003RequestedRouteAccessibilityAssertions))]
    public async Task Stf_01_role_context_assignments_and_navigation_are_accessible(int width, float scale)
    {
        await using var context = await fixture.OpenContextAsync(width, 1000, scale);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(
                route, 200, Spec016BrowserData.Context()),
            "/api/staff/assignments" => Spec016BrowserData.JsonAsync(
                route, 200, Spec016BrowserData.Assignments),
            _ => route.AbortAsync()
        });

        await page.GotoAsync("/staff", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STF-01'][data-state='success']")
            .WaitForAsync();
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.GetByText("Role context:", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("AI401 — L01", new() { Exact = true })
            .IsVisibleAsync());
        var roster = page.GetByRole(
            AriaRole.Link,
            new() { Name = "Open assigned roster", Exact = true });
        await roster.FocusAsync();
        Assert.True(await roster.IsVisibleAsync());
        Assert.Equal("a", await page.EvaluateAsync<string>(
            "() => document.activeElement.tagName.toLowerCase()"));
        Assert.True(await roster.EvaluateAsync<bool>(
            "element => getComputedStyle(element).outlineStyle !== 'none'"));
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
    }
}
