using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec017;

public sealed class AuditAdministrationPageFeatureTests
{
    [Fact]
    public void Adm_09_journey_keeps_search_paging_and_export_authorization_together()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/AuditAdministrationPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "Search audit",
            "Clear filters",
            "Previous page",
            "Next page",
            "Open event detail",
            "Refresh export status",
            "Request a new export",
            "Download ready export",
            "Only the server can authorize a scoped download");
        Assert.DoesNotContain("filesystem", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("scope hash", page, StringComparison.OrdinalIgnoreCase);
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class AuditAdministrationPageBrowserTests(Spec008BrowserFixture fixture)
{
    [Fact]
    public async Task Adm_09_route_renders_scoped_paging_and_no_unready_download()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync("/admin/audit", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        await page.Locator("[data-route-id='ADM-09'][data-state='loading']").WaitForAsync();
        Assert.True(await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Scoped audit search", Exact = true }).IsVisibleAsync());
        Assert.Equal("100", await page.Locator("input[name='pageSize']").GetAttributeAsync("max"));
        Assert.True(await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Download ready export", Exact = true }).IsDisabledAsync());
    }
}
