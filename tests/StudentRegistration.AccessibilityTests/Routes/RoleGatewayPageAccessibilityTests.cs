using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class RoleGatewayPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Auth_01_a11y_metadata_and_six_governed_widths_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/AUTH-01.md");

        RepositoryFiles.ContainsAll(
            design,
            "AUTH-01-A11Y-T125",
            "\"responsiveWidths\"",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Skip link",
            "Role gateway page heading");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/RoleGatewayPage.razor"),
            "AUTH-01-A11Y-T125: the executable owner page is still missing.");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class RoleGatewayPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    private const string AvailableContext = """
        {
          "serverTimeUtc": "2026-07-14T10:15:00Z",
          "timeZoneId": "Africa/Cairo",
          "teachingTermLabel": "Summer 2026",
          "registrationTermLabel": "Fall 2026",
          "registrationWindowState": "open",
          "serviceState": "available"
        }
        """;

    [Theory]
    [InlineData(320)]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1280)]
    [InlineData(1920)]
    public async Task Auth_01_has_no_serious_axe_issue_or_horizontal_overflow_at_width(
        int width)
    {
        await using var context = await fixture.OpenContextAsync(width);
        var page = await LoadAvailablePageAsync(context);

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        foreach (var name in new[] { "Student login", "Student activation", "Staff login" })
        {
            var box = await page.GetByRole(
                    AriaRole.Link,
                    new PageGetByRoleOptions { Name = name, Exact = true })
                .BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(box.Height >= 44, $"{name} is shorter than 44 CSS pixels at {width}px.");
        }
    }

    [Fact]
    public async Task Auth_01_keyboard_focus_landmarks_and_screen_reader_names_are_logical()
    {
        await using var context = await fixture.OpenContextAsync(768);
        var page = await LoadAvailablePageAsync(context);

        Assert.Equal(
            1,
            await page.GetByRole(
                    AriaRole.Heading,
                    new PageGetByRoleOptions { Name = "Role gateway", Exact = true })
                .CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Main).CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Banner).CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Contentinfo).CountAsync());

        var skip = page.GetByRole(
            AriaRole.Link,
            new PageGetByRoleOptions { Name = "Skip to main content", Exact = true });
        await skip.FocusAsync();
        Assert.True(await skip.EvaluateAsync<bool>(
            "element => getComputedStyle(element).outlineStyle !== 'none'"));
        await skip.PressAsync("Enter");
        Assert.Equal(
            "main-content",
            await page.EvaluateAsync<string>("() => document.activeElement.id"));

        var interactiveNames = await page.Locator("a,button")
            .EvaluateAllAsync<string[]>(
                "elements => elements.map(element => (element.innerText || element.getAttribute('aria-label') || '').trim())");
        Assert.True(Array.IndexOf(interactiveNames, "Student login") <
                    Array.IndexOf(interactiveNames, "Student activation"));
        Assert.True(Array.IndexOf(interactiveNames, "Student activation") <
                    Array.IndexOf(interactiveNames, "Staff login"));
    }

    [Fact]
    public async Task Auth_01_reflows_at_the_400_percent_effective_mobile_width()
    {
        await using var context = await fixture.OpenContextAsync(
            width: 320,
            height: 900,
            deviceScaleFactor: 4);
        var page = await LoadAvailablePageAsync(context);

        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        Assert.True(await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Retry public context", Exact = true })
            .CountAsync() == 0);
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
    }

    private static async Task<IPage> LoadAvailablePageAsync(IBrowserContext context)
    {
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/public/context", route => route.FulfillAsync(
            new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = AvailableContext
            }));
        await page.GotoAsync("/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.Locator("h1#role-gateway-heading").WaitForAsync();
        return page;
    }
}
