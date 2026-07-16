using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;

namespace StudentRegistration.AccessibilityTests.Routes;

internal static class IdentityRouteAccessibilityAssertions
{
    internal static async Task AssertResponsiveAsync(
        AxeAccessibilityFixture fixture,
        string route,
        string heading,
        string primaryButton,
        int width)
    {
        await using var context = await fixture.OpenContextAsync(width);
        var page = await OpenAsync(context, route, heading);

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        var box = await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = primaryButton, Exact = true })
            .BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box.Height >= 44);
        Assert.True(await page.Locator("label").CountAsync() > 0);
        Assert.True(await page.Locator("label").EvaluateAllAsync<bool>(
            "labels => labels.every(label => getComputedStyle(label).display !== 'none' && getComputedStyle(label).visibility !== 'hidden')"));
    }

    internal static async Task AssertKeyboardAndZoomAsync(
        AxeAccessibilityFixture fixture,
        string route,
        string heading,
        string primaryButton)
    {
        await using var context = await fixture.OpenContextAsync(
            width: 320,
            height: 900,
            deviceScaleFactor: 4);
        var page = await OpenAsync(context, route, heading);

        var skip = page.GetByRole(
            AriaRole.Link,
            new PageGetByRoleOptions { Name = "Skip to main content", Exact = true });
        await skip.FocusAsync();
        Assert.True(await skip.EvaluateAsync<bool>(
            "element => getComputedStyle(element).outlineStyle !== 'none'"));
        await skip.PressAsync("Enter");
        Assert.Equal(
            "identity-main",
            await page.EvaluateAsync<string>("() => document.activeElement.id"));

        var button = page.GetByRole(
            AriaRole.Button,
            new PageGetByRoleOptions { Name = primaryButton, Exact = true });
        await button.FocusAsync();
        Assert.Equal(
            primaryButton,
            await page.EvaluateAsync<string>(
                "() => (document.activeElement.innerText || document.activeElement.getAttribute('aria-label')).trim()"));
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
    }

    private static async Task<IPage> OpenAsync(
        IBrowserContext context,
        string route,
        string heading)
    {
        var page = await context.NewPageAsync();
        await page.GotoAsync(route, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new PageGetByRoleOptions { Name = heading, Exact = true })
            .WaitForAsync();
        return page;
    }
}
