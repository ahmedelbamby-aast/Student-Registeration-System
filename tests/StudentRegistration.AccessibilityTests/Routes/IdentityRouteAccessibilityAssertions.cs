using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using Xunit.Sdk;

namespace StudentRegistration.AccessibilityTests.Routes;

internal static class IdentityRouteAccessibilityAssertions
{
    internal static async Task AssertResponsiveAsync(
        AxeAccessibilityFixture fixture,
        string route,
        string heading,
        string primaryButton,
        int width,
        Func<IPage, Task>? configure = null,
        bool requireVisibleLabels = true,
        AriaRole primaryRole = AriaRole.Button)
    {
        await using var context = await fixture.OpenContextAsync(width);
        var page = await OpenAsync(context, route, heading, configure);

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        var box = await page.GetByRole(
                primaryRole,
                new PageGetByRoleOptions { Name = primaryButton, Exact = true })
            .BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box.Height >= 44);
        if (requireVisibleLabels)
        {
            Assert.True(await page.Locator("label").CountAsync() > 0);
            Assert.True(await page.Locator("label").EvaluateAllAsync<bool>(
                "labels => labels.every(label => getComputedStyle(label).display !== 'none' && getComputedStyle(label).visibility !== 'hidden')"));
        }
    }

    internal static async Task AssertKeyboardAndZoomAsync(
        AxeAccessibilityFixture fixture,
        string route,
        string heading,
        string primaryButton,
        Func<IPage, Task>? configure = null,
        string expectedMainId = "identity-main",
        AriaRole primaryRole = AriaRole.Button)
    {
        await using var context = await fixture.OpenContextAsync(
            width: 320,
            height: 900,
            deviceScaleFactor: 4);
        var page = await OpenAsync(context, route, heading, configure);

        var skip = page.GetByRole(
            AriaRole.Link,
            new PageGetByRoleOptions { Name = "Skip to main content", Exact = true });
        await skip.FocusAsync();
        Assert.True(await skip.EvaluateAsync<bool>(
            "element => getComputedStyle(element).outlineStyle !== 'none'"));
        await skip.PressAsync("Enter");
        Assert.Equal(
            expectedMainId,
            await page.EvaluateAsync<string>("() => document.activeElement.id"));

        var button = page.GetByRole(
            primaryRole,
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
        string heading,
        Func<IPage, Task>? configure)
    {
        var page = await context.NewPageAsync();
        if (configure is not null)
        {
            await configure(page);
        }

        await page.GotoAsync(route, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        try
        {
            await page.GetByRole(
                    AriaRole.Heading,
                    new PageGetByRoleOptions { Name = heading, Exact = true })
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        }
        catch (TimeoutException)
        {
            var body = await page.Locator("body").InnerTextAsync();
            throw new XunitException(
                $"Route {route} did not render heading '{heading}'. Body: {body}");
        }

        return page;
    }
}
