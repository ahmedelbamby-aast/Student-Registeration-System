using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.E2ETests.Specs.Spec016;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StaffAvailabilityPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Stf_04_has_labelled_text_editor_validation_and_table_alternative()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffAvailabilityPage.razor");
        RepositoryFiles.ContainsAll(page, "aria-describedby", "role=\"alert\"",
            "<label", "type=\"time\"", "<table", "<caption>", "scope=\"col\"");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class StaffAvailabilityPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Fact]
    public async Task Stf_04_time_range_editor_and_table_alternative_are_accessible()
    {
        await using var context = await fixture.OpenContextAsync(375, 1000);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", route => Spec016BrowserData.Path(route) switch
        {
            "/api/context" => Spec016BrowserData.JsonAsync(
                route, 200, Spec016BrowserData.Context()),
            "/api/staff/availability" => Spec016BrowserData.JsonAsync(
                route, 200, Spec016BrowserData.Availability),
            _ => route.AbortAsync()
        });

        await page.GotoAsync(
            "/staff/availability",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STF-04'][data-state='success']")
            .WaitForAsync();
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        var start = page.Locator($"#start-{Spec016BrowserData.RangeId}");
        await start.FocusAsync();
        Assert.Equal("time", await start.GetAttributeAsync("type"));
        await start.FillAsync("10:00");
        var end = page.GetByLabel("End", new() { Exact = true });
        await end.FocusAsync();
        Assert.Equal(
            $"end-{Spec016BrowserData.RangeId}",
            await page.EvaluateAsync<string>("() => document.activeElement.id"));
        Assert.Equal(
            1,
            await page.GetByRole(
                AriaRole.Table,
                new() { Name = "Owned availability ranges", Exact = true })
                .CountAsync());
        var save = await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Save availability", Exact = true })
            .BoundingBoxAsync();
        Assert.NotNull(save);
        Assert.True(save.Height >= 44);
    }
}
