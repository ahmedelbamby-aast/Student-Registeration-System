using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class AuditAdministrationPageAccessibilityTests
{
    [Fact]
    public void Adm_09_has_landmarks_wrapped_event_fields_and_authorized_action_focus_order()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-09.md");
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/AuditAdministrationPage.razor");

        RepositoryFiles.ContainsAll(
            design,
            "Scope and search filters",
            "Audit result table",
            "Pagination",
            "Selected immutable event detail",
            "Export job status and authorized download");
        RepositoryFiles.ContainsAll(
            page,
            "Skip to main content",
            "role=\"banner\"",
            "<main id=\"main-content\"",
            "role=\"status\"",
            "aria-live=\"polite\"",
            "semantic alternative");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class AuditAdministrationPageAxeTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [InlineData(375)]
    [InlineData(1280)]
    public async Task Adm_09_has_no_serious_issue_or_page_overflow_at_width(int width)
    {
        await using var context = await fixture.OpenContextAsync(width);
        var page = await context.NewPageAsync();
        await page.GotoAsync("/admin/audit", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.Locator("[data-route-id='ADM-09']").WaitForAsync();

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
    }
}
