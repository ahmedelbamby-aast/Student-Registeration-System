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
        await page.RouteAsync("**/api/**", route => new Uri(route.Request.Url).AbsolutePath switch
        {
            "/api/context" => route.FulfillAsync(new()
            {
                Status = 200,
                ContentType = "application/json",
                Body = Context
            }),
            "/api/admin/operations/metrics" => route.FulfillAsync(new()
            {
                Status = 200,
                ContentType = "application/json",
                Body = Metrics
            }),
            "/api/admin/students" => route.FulfillAsync(new()
            {
                Status = 200,
                ContentType = "application/json",
                Body = StudentPage
            }),
            "/api/admin/students/00000000-0000-0000-0000-000000017003/terms/00000000-0000-0000-0000-000000017001/registrations" => route.FulfillAsync(new()
            {
                Status = 200,
                ContentType = "application/json",
                Body = EmptyRegistrationPage
            }),
            _ => route.AbortAsync()
        });
        await page.GotoAsync("/admin/registrations", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        await page.Locator("[data-route-id='ADM-08'][data-state='success']").WaitForAsync();
        Assert.True(await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Read-only registration monitoring", Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText(
            "No repair or correction actions are available.",
            new() { Exact = false }).IsVisibleAsync());
        await page.GetByRole(
            AriaRole.Searchbox,
            new() { Name = "Student ID, University ID, or name", Exact = true })
            .FillAsync("AI2600001");
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Filter registrations", Exact = true })
            .ClickAsync();
        await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "No matching registrations", Exact = true })
            .WaitForAsync();
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Clear registration filters", Exact = true })
            .ClickAsync();
        Assert.Equal(string.Empty, await page.GetByRole(
            AriaRole.Searchbox,
            new() { Name = "Student ID, University ID, or name", Exact = true }).InputValueAsync());
        Assert.Equal(0, await page.GetByRole(
            AriaRole.Button,
            new() { NameRegex = new System.Text.RegularExpressions.Regex("repair|correct", System.Text.RegularExpressions.RegexOptions.IgnoreCase) })
            .CountAsync());
    }

    private const string Context = """
        {"serverTimeUtc":"2026-07-17T09:30:00Z","timeZoneId":"Africa/Cairo",
        "teachingTerm":null,"registrationTerm":{"id":"00000000-0000-0000-0000-000000017001",
        "code":"2026-FALL","label":"Fall 2026","state":"registrationOpen","rowVersion":"TERM-RV-1"},
        "registrationWindowState":"open","registrationWindow":{"id":"00000000-0000-0000-0000-000000017002",
        "state":"open","opensAtUtc":"2026-07-17T08:00:00Z","closesAtUtc":"2026-07-24T16:00:00Z","rowVersion":"WINDOW-RV-1"},
        "serviceState":"available","displayName":"Demo Admin",
        "authorizedRoles":["Admin"],"activeRole":"Admin","sessionState":"active",
        "expiresAtUtc":"2026-07-17T11:30:00Z","supportReferencePath":"/support/admin"}
        """;

    private const string Metrics = """
        {"observedAtUtc":"2026-07-17T09:30:00Z","availabilityState":"live",
        "metrics":[{"name":"http.request.throughput","value":75,"dimensions":{},
        "observedAtUtc":"2026-07-17T09:30:00Z"}],"reconciliationAlerts":[]}
        """;

    private const string StudentPage = """
        {"items":[{"studentId":"00000000-0000-0000-0000-000000017003",
        "universityId":"AI2600001","programCode":"AI","cohort":"2026",
        "standing":"good","dataVersion":"fixture-v1"}],"page":1,"pageSize":20,
        "totalCount":1,"sort":"universityId,studentId"}
        """;

    private const string EmptyRegistrationPage = """
        {"items":[],"page":1,"pageSize":20,"totalCount":0,"sort":"submittedAtUtc desc,submissionId desc"}
        """;
}
