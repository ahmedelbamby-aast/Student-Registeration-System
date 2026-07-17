using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class RegistrationAdministrationPageAccessibilityTests
{
    [Fact]
    public void Adm_08_has_landmarks_live_status_table_alternative_and_safe_focus_order()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-08.md");
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/RegistrationAdministrationPage.razor");

        RepositoryFiles.ContainsAll(
            design,
            "\"responsiveWidths\"",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Skip link",
            "Administration navigation");
        RepositoryFiles.ContainsAll(
            page,
            "Skip to main content",
            "role=\"banner\"",
            "<main id=\"main-content\"",
            "aria-live=\"polite\"",
            "<table",
            "semantic alternative");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class RegistrationAdministrationPageAxeTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [InlineData(375)]
    [InlineData(1280)]
    public async Task Adm_08_has_no_serious_issue_or_page_overflow_at_width(int width)
    {
        await using var context = await fixture.OpenContextAsync(width);
        var page = await context.NewPageAsync();
        await page.GotoAsync("/admin/registrations", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.Locator("[data-route-id='ADM-08']").WaitForAsync();

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
    }
}
