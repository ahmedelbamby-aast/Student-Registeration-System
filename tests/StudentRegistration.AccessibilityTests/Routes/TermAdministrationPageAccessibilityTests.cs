using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class TermAdministrationPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Adm_02_a11y_metadata_focus_order_and_six_governed_widths_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-02.md");
        var fixture = RepositoryFiles.Read(
            "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec008/ADM-02/route-contract.json");

        RepositoryFiles.ContainsAll(
            design,
            "ADM-02-A11Y-T195",
            "\"responsiveWidths\"",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Skip link",
            "Administration navigation",
            "Terms heading",
            "Validation summary after a failed submit",
            "Confirmation dialog title, cancel, then confirm",
            "Return focus to the publish trigger after dialog close");
        RepositoryFiles.ContainsAll(
            fixture,
            "ADM-02-loading-v1",
            "ADM-02-validation-error-v1",
            "ADM-02-stale-v1",
            "ADM-02-offline-v1");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/TermAdministrationPage.razor"),
            "ADM-02-A11Y-T195: the executable owner page is still missing until SPEC-008/T080.");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class TermAdministrationPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    private const string DraftPage = """
        {
          "items": [
            {
              "id": "term-fall-2026",
              "code": "2026-FALL",
              "displayName": "Fall 2026",
              "timeZoneId": "Africa/Cairo",
              "teachingStartsOn": "2026-09-13",
              "teachingEndsOn": "2027-01-14",
              "state": "draft",
              "rowVersion": "term-rv-1",
              "windows": [
                {
                  "id": "window-all",
                  "scopeType": "all-students",
                  "scopeValue": null,
                  "opensAtUtc": "2026-08-20T06:00:00Z",
                  "closesAtUtc": "2026-08-27T18:00:00Z",
                  "lifecycleState": "draft",
                  "computedState": "upcoming",
                  "rowVersion": "window-rv-1"
                }
              ]
            }
          ],
          "page": 1,
          "pageSize": 20,
          "totalCount": 1,
          "sort": "code,id"
        }
        """;

    [Theory]
    [InlineData(320)]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1280)]
    [InlineData(1920)]
    public async Task Adm_02_has_no_serious_axe_issue_overflow_or_small_command_target_at_width(
        int width)
    {
        EnsureOwnerPageDelivered();
        await using var context = await fixture.OpenContextAsync(width);
        var page = await LoadDraftPageAsync(context);
        await SelectTermAsync(page);

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        foreach (var name in new[] { "Save changes", "Publish registration window" })
        {
            var box = await page.GetByRole(
                    AriaRole.Button,
                    new PageGetByRoleOptions { Name = name, Exact = true })
                .BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(box.Height >= 44, $"{name} is shorter than 44 CSS pixels at {width}px.");
        }

        if (width <= 375)
        {
            Assert.True(await page.Locator("[data-testid='term-card-list']").IsVisibleAsync());
            Assert.True(await page.Locator("[data-testid='term-table-semantic-alternative']")
                .CountAsync() >= 1);
        }
        else
        {
            Assert.Equal(1, await page.GetByRole(AriaRole.Table).CountAsync());
        }
    }

    [Fact]
    public async Task Adm_02_keyboard_landmarks_field_order_and_publish_dialog_focus_are_logical()
    {
        EnsureOwnerPageDelivered();
        await using var context = await fixture.OpenContextAsync(1024);
        var page = await LoadDraftPageAsync(context);

        Assert.Equal(1, await page.GetByRole(AriaRole.Main).CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Banner).CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Contentinfo).CountAsync());
        Assert.Equal(
            1,
            await page.GetByRole(
                    AriaRole.Heading,
                    new PageGetByRoleOptions { Name = "Term administration", Exact = true })
                .CountAsync());

        var skip = page.GetByRole(
            AriaRole.Link,
            new PageGetByRoleOptions { Name = "Skip to main content", Exact = true });
        await skip.FocusAsync();
        Assert.True(await skip.EvaluateAsync<bool>(
            "element => getComputedStyle(element).outlineStyle !== 'none'"));
        await skip.PressAsync("Enter");
        Assert.Equal(
            "main-content",
            await page.EvaluateAsync<string>("() => document.activeElement?.id || ''"));

        await SelectTermAsync(page);
        var focusableIds = await page.Locator(
                "#term-editor input:not([disabled]), #term-editor select:not([disabled]), #term-editor button:not([disabled])")
            .EvaluateAllAsync<string[]>("elements => elements.map(element => element.id || element.dataset.testid || element.textContent.trim())");
        Assert.True(Array.IndexOf(focusableIds, "term-code") < Array.IndexOf(focusableIds, "term-display-name"));
        Assert.True(Array.IndexOf(focusableIds, "term-display-name") < Array.IndexOf(focusableIds, "term-time-zone"));
        Assert.True(Array.IndexOf(focusableIds, "term-teaching-start") < Array.IndexOf(focusableIds, "term-teaching-end"));

        var trigger = page.GetByRole(
            AriaRole.Button,
            new PageGetByRoleOptions { Name = "Publish registration window", Exact = true });
        await trigger.ClickAsync();
        var dialog = page.GetByRole(
            AriaRole.Dialog,
            new PageGetByRoleOptions { Name = "Publish registration window", Exact = true });
        await dialog.WaitForAsync();
        Assert.Equal("true", await dialog.GetAttributeAsync("aria-modal"));
        Assert.Equal(
            "Cancel",
            await page.EvaluateAsync<string>("() => document.activeElement?.textContent?.trim() || ''"));
        await page.Keyboard.PressAsync("Shift+Tab");
        Assert.True(await dialog.Locator(":focus").CountAsync() == 1);
        await page.Keyboard.PressAsync("Escape");
        Assert.True(await trigger.EvaluateAsync<bool>("element => document.activeElement === element"));
    }

    [Fact]
    public async Task Adm_02_reflows_at_400_percent_without_two_dimensional_page_scroll()
    {
        EnsureOwnerPageDelivered();
        await using var context = await fixture.OpenContextAsync(
            width: 320,
            height: 900,
            deviceScaleFactor: 4);
        var page = await LoadDraftPageAsync(context);

        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        Assert.True(await page.Locator("[data-testid='term-card-list']").IsVisibleAsync());
        Assert.True(await page.Locator("[data-testid='term-table-semantic-alternative']")
            .IsVisibleAsync());
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
    }

    private static async Task<IPage> LoadDraftPageAsync(IBrowserContext context)
    {
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/admin/terms**", route => route.FulfillAsync(
            new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = DraftPage
            }));
        await page.GotoAsync("/admin/terms", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.Locator("h1#term-administration-heading").WaitForAsync();
        await page.Locator("[data-route-id='ADM-02'][data-state='success']")
            .WaitForAsync();
        return page;
    }

    private static void EnsureOwnerPageDelivered() =>
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/TermAdministrationPage.razor"),
            "TermAdministrationPage.razor is intentionally absent until SPEC-008/T080; ADM-02-A11Y-T195 remains red.");

    private static async Task SelectTermAsync(IPage page)
    {
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Review Fall 2026", Exact = true })
            .ClickAsync();
        await page.Locator("#term-editor").WaitForAsync();
    }
}
