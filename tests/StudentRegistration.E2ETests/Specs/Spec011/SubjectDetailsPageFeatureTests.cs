using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec011;

public sealed class SubjectDetailsPageFrozenContractTests
{
    [Fact]
    public void Stu_03_owner_contract_and_page_are_present()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-03.md");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/student/subjects/{offeringId}\"",
            "\"pageName\": \"SubjectDetailsPage.razor\"",
            "GET /api/student/offerings/{offeringId}/eligibility",
            "STU-03-open-v1",
            "STU-03-full-v1",
            "STU-03-changed-v1",
            "STU-03-selected-v1");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/SubjectDetailsPage.razor"),
            "SPEC-011/T042 must deliver the STU-03 owner page.");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class SubjectDetailsPageFeatureTests(Spec008BrowserFixture fixture)
{
    private const string OfferingId = "00000000-0000-0000-0000-000000011100";

    [Fact]
    public async Task Stu_03_primary_shows_complete_server_detail_and_navigation_only_intent()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        var nonGetCalls = 0;
        await RouteContextAsync(page);
        await page.RouteAsync("**/api/student/offerings/*/eligibility", route =>
        {
            if (route.Request.Method != "GET")
            {
                nonGetCalls++;
            }

            return route.FulfillAsync(Json(
                200,
                SubjectDiscoveryPageFeatureTests.Offering()));
        });

        await OpenAsync(page, "success");

        Assert.True(await page.GetByText("Eligible", new() { Exact = true }).First
            .IsVisibleAsync());
        Assert.True(await page.GetByText("✓", new() { Exact = true }).First
            .IsVisibleAsync());
        Assert.True(await page.GetByText("15 of 18 Credits", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("Dr. Nadia", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("Eng. Omar", new() { Exact = true })
            .IsVisibleAsync());
        var choose = page.GetByRole(
            AriaRole.Link,
            new() { Name = "Continue with group A", Exact = true });
        Assert.Equal(
            $"/student/schedule?offeringId={OfferingId}&groupId=00000000-0000-0000-0000-000000011200",
            await choose.GetAttributeAsync("href"));
        Assert.Equal(0, nonGetCalls);
    }

    [Fact]
    public async Task Stu_03_full_group_is_stale_textually_and_refreshes_authoritatively()
    {
        await using var context = await fixture.OpenContextAsync(375, 1000);
        var page = await context.NewPageAsync();
        var calls = 0;
        await RouteContextAsync(page);
        await page.RouteAsync("**/api/student/offerings/*/eligibility", route =>
        {
            calls++;
            var body = calls == 1
                ? SubjectDiscoveryPageFeatureTests.Offering(
                    selectable: false,
                    seatsRemaining: 0,
                    groupReasonCode: "GROUP_FULL",
                    groupReasonMessage: "No seat currently remains.")
                : SubjectDiscoveryPageFeatureTests.Offering();
            return route.FulfillAsync(Json(200, body));
        });

        await OpenAsync(page, "stale");

        Assert.True(await page
            .Locator("[data-reason-code='GROUP_FULL']")
            .GetByText("GROUP_FULL", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("0", new() { Exact = true }).First
            .IsVisibleAsync());
        Assert.Equal(
            0,
            await page.GetByRole(
                    AriaRole.Link,
                    new() { Name = "Continue with group A", Exact = true })
                .CountAsync());
        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Refresh subject details", Exact = true })
            .ClickAsync();
        await page.Locator("[data-route-id='STU-03'][data-state='success']")
            .WaitForAsync();
        Assert.True(await page.GetByRole(
                AriaRole.Link,
                new() { Name = "Continue with group A", Exact = true })
            .IsVisibleAsync());
    }

    [Fact]
    public async Task Stu_03_service_error_exposes_safe_reference_and_retry_only()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        var calls = 0;
        await RouteContextAsync(page);
        await page.RouteAsync("**/api/student/offerings/*/eligibility", route =>
        {
            calls++;
            return calls == 1
                ? route.FulfillAsync(Json(
                    503,
                    SubjectDiscoveryPageFeatureTests.Error(
                        "DISCOVERY_UNAVAILABLE",
                        "Subject details are temporarily unavailable.",
                        "STU-03-SAFE-REF")))
                : route.FulfillAsync(Json(
                    200,
                    SubjectDiscoveryPageFeatureTests.Offering()));
        });

        await OpenAsync(page, "service-error");

        Assert.True(await page.GetByText("DISCOVERY_UNAVAILABLE", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("STU-03-SAFE-REF", new() { Exact = true })
            .IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("Project I", new() { Exact = true })
            .CountAsync());
        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Retry subject details", Exact = true })
            .ClickAsync();
        await page.Locator("[data-route-id='STU-03'][data-state='success']")
            .WaitForAsync();
    }

    private static async Task OpenAsync(IPage page, string state)
    {
        await page.GotoAsync($"/student/subjects/{OfferingId}", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Subject details", Exact = true })
            .WaitForAsync();
        await page.Locator($"[data-route-id='STU-03'][data-state='{state}']")
            .WaitForAsync();
    }

    private static async Task RouteContextAsync(IPage page) =>
        await page.RouteAsync("**/api/context", route =>
            route.FulfillAsync(Json(
                200,
                SubjectDiscoveryPageFeatureTests.AppContext())));

    private static RouteFulfillOptions Json(int status, string body) => new()
    {
        Status = status,
        ContentType = "application/json",
        Body = body
    };
}
