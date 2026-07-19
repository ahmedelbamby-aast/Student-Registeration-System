using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

internal static class Spec003RouteAccessibilityAssertions
{
    public static void AssertFrozenContract(
        string routeId,
        string taskId,
        string pageName)
    {
        var design = RepositoryFiles.Read(
            $"specs/003-ux-storyboard-accessibility/design/pages/{routeId}.md");
        var page = RepositoryFiles.Read($"src/StudentRegistration.Client/Pages/{pageName}.razor");

        RepositoryFiles.ContainsAll(
            design,
            $"{routeId}-A11Y-{taskId}",
            "320,", "375,", "768,", "1024,", "1280,", "1920",
            "\"focusOrder\"");
        RepositoryFiles.ContainsAll(
            page,
            "Skip to main content",
            "<h1",
            "tabindex=\"-1\"",
            "aria-live");
    }

    public static async Task AssertDeniedStateAsync(
        AxeAccessibilityFixture fixture,
        string routeId,
        string route,
        int width,
        float scale = 1)
    {
        await using var context = await fixture.OpenContextAsync(width, 900, scale);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", api => api.FulfillAsync(new()
        {
            Status = 403,
            ContentType = "application/json",
            Body = "{\"code\":\"FORBIDDEN\",\"message\":\"This route is not authorized.\",\"correlationId\":\"SPEC003-A11Y-SAFE-REF\"}"
        }));

        await page.GotoAsync(route, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        var root = page.Locator($"[data-route-id='{routeId}']").Last;
        await root.WaitForAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));

        var skip = page.GetByRole(AriaRole.Link, new() { Name = "Skip to main content", Exact = true }).First;
        if (await skip.CountAsync() > 0)
        {
            await skip.FocusAsync();
            Assert.True(await skip.EvaluateAsync<bool>(
                "element => document.activeElement === element && getComputedStyle(element).outlineStyle !== 'none'"));
            await skip.PressAsync("Enter");
        }
        else
        {
            var main = page.GetByRole(AriaRole.Main);
            await main.FocusAsync();
            Assert.True(await main.EvaluateAsync<bool>(
                "element => document.activeElement === element"));
        }
        Assert.Equal(1, await page.GetByRole(AriaRole.Main).CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Heading, new() { Level = 1 }).CountAsync());
    }
}
