using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec017;

public sealed class RegistrationAdministrationPageFeatureTests
{
    [Fact]
    public void Adm_08_journey_exposes_only_filter_detail_profile_and_safe_support_actions()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/RegistrationAdministrationPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "Filter registrations",
            "Clear registration filters",
            "Open submission detail",
            "Open Student profile",
            "Open safe support reference",
            "Refresh monitoring data");
        Assert.DoesNotContain("manual seat", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("correction command", page, StringComparison.OrdinalIgnoreCase);
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class RegistrationAdministrationPageBrowserTests(Spec008BrowserFixture fixture)
{
    [Fact]
    public async Task Adm_08_route_renders_read_only_monitoring_and_no_repair_action()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync("/admin/registrations", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        await page.Locator("[data-route-id='ADM-08'][data-state='loading']").WaitForAsync();
        Assert.True(await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Read-only registration monitoring", Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText(
            "No repair or correction actions are available.",
            new() { Exact = false }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByRole(
            AriaRole.Button,
            new() { NameRegex = new System.Text.RegularExpressions.Regex("repair|correct", System.Text.RegularExpressions.RegexOptions.IgnoreCase) })
            .CountAsync());
    }
}
