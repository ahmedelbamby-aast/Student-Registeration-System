using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec015;

public sealed class RegistrationResultPageFrozenContractTests
{
    [Fact]
    public void Stu_06_owner_record_and_required_journeys_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-06.md");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/student/registration/result/{id}\"",
            "\"pageName\": \"RegistrationResultPage.razor\"",
            "STU-06-accepted-v1",
            "STU-06-rejected-no-partial-result-v1",
            "STU-06-lost-response-recovery-v1",
            "STU-06-denied-v1",
            "STU-06-E2E-PRIMARY",
            "STU-06-E2E-FAILURE");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/RegistrationResultPage.razor"),
            "SPEC-015/T051 must deliver the sole canonical STU-06 Razor page.");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class RegistrationResultPageFeatureTests(Spec008BrowserFixture fixture)
{
    [Fact]
    public async Task Stu_06_accepted_v1_shows_one_authoritative_receipt_and_equivalent_schedule()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteContextAndDetailAsync(page, 200, Spec015BrowserData.AcceptedDetail);

        await OpenAsync(page);

        await page.Locator("[data-route-id='STU-06'][data-state='success']").WaitForAsync();
        Assert.True(await page.GetByText("REG-2026-015001", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("DEMO-POC-2026.1", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("Dr. Salma", new() { Exact = true }).First.IsVisibleAsync());
        Assert.True(await page.GetByText("TA Noor", new() { Exact = true }).First.IsVisibleAsync());
        Assert.True(await page.GetByText("A-101 Smart Village", new() { Exact = true }).First.IsVisibleAsync());
        Assert.True(await page.GetByText("3 credits", new() { Exact = true }).IsVisibleAsync());
        var submitted = page.Locator("[data-receipt-item]")
            .Filter(new() { HasText = "Submitted" })
            .Locator("dd");
        Assert.True(await submitted.IsVisibleAsync());
        Assert.False(string.IsNullOrWhiteSpace(await submitted.TextContentAsync()));

        var calendar = await page.Locator(".srs-schedule-calendar [data-meeting-id]")
            .EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.getAttribute('data-meeting-id'))");
        var list = await page.Locator(".srs-schedule-list [data-meeting-id]")
            .EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.getAttribute('data-meeting-id'))");
        Assert.Equal(calendar, list);
        Assert.Equal([Spec015BrowserData.MeetingId], calendar);
        Assert.Equal(0, await page.GetByText("Drop", new() { Exact = true }).CountAsync());
        Assert.Equal(0, await page.GetByText("Correct", new() { Exact = true }).CountAsync());
    }

    [Fact]
    public async Task Stu_06_rejected_no_partial_result_v1_preserves_group_full_and_no_receipt()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteContextAndDetailAsync(page, 200, Spec015BrowserData.RejectedDetail);

        await OpenAsync(page);

        await page.Locator("[data-route-id='STU-06'][data-state='validation-error']").WaitForAsync();
        Assert.True(await page.GetByText("GROUP_FULL", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("No subjects were partially registered.", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByRole(AriaRole.Link, new() { Name = "Edit plan", Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("REG-2026-015001", new() { Exact = true }).CountAsync());
        Assert.Equal(0, await page.GetByRole(AriaRole.Button, new() { Name = "Submit registration", Exact = true }).CountAsync());
    }

    [Fact]
    public async Task Stu_06_denied_v1_discloses_no_identifier_or_cached_receipt()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteContextAndDetailAsync(page, 404, Spec015BrowserData.Error("REGISTRATION_NOT_FOUND"));

        await OpenAsync(page);

        await page.Locator("[data-route-id='STU-06'][data-state='unauthorized']").WaitForAsync();
        Assert.True(await page.GetByRole(AriaRole.Heading, new() { Name = "Access denied", Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByText(Spec015BrowserData.SubmissionId, new() { Exact = true }).CountAsync());
        Assert.Equal(0, await page.GetByText("REG-2026-015001", new() { Exact = true }).CountAsync());
    }

    [Fact]
    public async Task Stu_06_lost_response_recovery_v1_uses_private_lookup_once_then_owned_detail()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        var detailCalls = 0;
        var lookupCalls = 0;
        await page.RouteAsync("**/api/**", async route =>
        {
            var path = Spec015BrowserData.Path(route);
            if (path == "/api/context")
            {
                await Spec015BrowserData.JsonAsync(route, 200, Spec015BrowserData.AppContext);
            }
            else if (path.Contains("/registrations/by-request/", StringComparison.Ordinal))
            {
                lookupCalls++;
                await Spec015BrowserData.JsonAsync(
                    route,
                    200,
                    "{\"isSuccess\":true,\"isProcessing\":false,\"submissionId\":\"" + Spec015BrowserData.SubmissionId + "\",\"status\":\"accepted\",\"resultCode\":\"REGISTERED\",\"reference\":\"REG-2026-015001\"}");
            }
            else if (path == $"/api/student/registrations/{Spec015BrowserData.SubmissionId}")
            {
                detailCalls++;
                await Spec015BrowserData.JsonAsync(
                    route,
                    detailCalls == 1 ? 503 : 200,
                    detailCalls == 1
                        ? Spec015BrowserData.Error("RESULT_DELIVERY_UNCERTAIN")
                        : Spec015BrowserData.AcceptedDetail);
            }
            else
            {
                await route.AbortAsync();
            }
        });

        await page.GotoAsync(
            $"/student/registration/result/{Spec015BrowserData.SubmissionId}?termId={Spec015BrowserData.TermId}&clientRequestId={Spec015BrowserData.ClientRequestId}",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("[data-route-id='STU-06'][data-state='stale']").WaitForAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Retry result lookup", Exact = true }).ClickAsync();
        await page.GetByText("REG-2026-015001", new() { Exact = true }).WaitForAsync();

        Assert.Equal(1, lookupCalls);
        Assert.Equal(2, detailCalls);
    }

    private static async Task RouteContextAndDetailAsync(IPage page, int status, string body)
    {
        await page.RouteAsync("**/api/**", async route =>
        {
            var path = Spec015BrowserData.Path(route);
            if (path == "/api/context")
            {
                await Spec015BrowserData.JsonAsync(route, 200, Spec015BrowserData.AppContext);
            }
            else if (path == $"/api/student/registrations/{Spec015BrowserData.SubmissionId}")
            {
                await Spec015BrowserData.JsonAsync(route, status, body);
            }
            else
            {
                await route.AbortAsync();
            }
        });
    }

    private static async Task OpenAsync(IPage page)
    {
        await page.GotoAsync(
            $"/student/registration/result/{Spec015BrowserData.SubmissionId}",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.GetByRole(AriaRole.Heading, new() { Name = "Atomic registration result", Exact = true }).WaitForAsync();
    }
}
