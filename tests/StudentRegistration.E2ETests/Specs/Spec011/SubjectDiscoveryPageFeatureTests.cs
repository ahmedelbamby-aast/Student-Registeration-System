using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec011;

public sealed class SubjectDiscoveryPageFrozenContractTests
{
    [Fact]
    public void Stu_02_owner_contract_and_page_are_present()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-02.md");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/student/subjects\"",
            "\"pageName\": \"SubjectDiscoveryPage.razor\"",
            "GET /api/student/terms/{termId}/offerings",
            "STU-02-eligible-v1",
            "STU-02-unavailable-reasons-v1",
            "STU-02-no-results-v1",
            "STU-02-stale-capacity-v1",
            "STU-02-service-error-v1");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/SubjectDiscoveryPage.razor"),
            "SPEC-011/T040 must deliver the STU-02 owner page.");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class SubjectDiscoveryPageFeatureTests(Spec008BrowserFixture fixture)
{
    private const string TermId = "00000000-0000-0000-0000-000000011001";

    [Fact]
    public async Task Stu_02_primary_renders_server_load_reasons_and_complete_group_bundle()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteContextAsync(page);
        await page.RouteAsync("**/api/student/terms/*/offerings*", route =>
            route.FulfillAsync(Json(200, OfferingPage())));

        await OpenAsync(page, "success");

        Assert.True(await page.GetByText("15 of 18 Credits", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("18 of 18 Credits", new() { Exact = true })
            .IsVisibleAsync());
        var offeringCard = page.Locator(
            "[data-offering-id='00000000-0000-0000-0000-000000011100']");
        Assert.True(await offeringCard.GetByText("Eligible", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await offeringCard.GetByText("✓", new() { Exact = true }).First
            .IsVisibleAsync());
        Assert.True(await page.GetByText("NORMAL_MAX_CREDITS", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await offeringCard
            .GetByText("DEMO-POC-2026.1", new() { Exact = true }).First
            .IsVisibleAsync());
        Assert.True(await page.GetByText("Lecture", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("Dr. Nadia", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("Tutorial", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("Eng. Omar", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("A-101", new() { Exact = true })
            .IsVisibleAsync());
        Assert.Equal(
            "/student/subjects/00000000-0000-0000-0000-000000011100",
            await page.GetByRole(
                    AriaRole.Link,
                    new() { Name = "View DS413 Details", Exact = true })
                .GetAttributeAsync("href"));
    }

    [Fact]
    public async Task Stu_02_search_filters_and_page_are_sent_to_the_server()
    {
        await using var context = await fixture.OpenContextAsync(768, 1000);
        var page = await context.NewPageAsync();
        await RouteContextAsync(page);
        var requests = new List<Uri>();
        await page.RouteAsync("**/api/student/terms/*/offerings*", async route =>
        {
            var uri = new Uri(route.Request.Url);
            requests.Add(uri);
            var filtered = uri.Query.Contains("q=machine", StringComparison.Ordinal);
            var pageNumber = uri.Query.Contains("page=2", StringComparison.Ordinal) ? 2 : 1;
            await route.FulfillAsync(Json(200, OfferingPage(
                title: filtered ? "Filtered subject" : "Project I",
                totalCount: filtered ? 21 : 1,
                pageNumber: pageNumber)));
        });

        await OpenAsync(page, "success");
        await page.Locator("#subject-search").FillAsync("machine");
        await page.Locator("#subject-eligibility").SelectOptionAsync("all");
        await page.Locator("#subject-credits").FillAsync("3");
        await page.Locator("#subject-day").SelectOptionAsync("2");
        await page.Locator("#subject-availability").SelectOptionAsync("available");
        await page.Locator("#subject-sort").SelectOptionAsync("title,id");
        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Apply search and filters", Exact = true })
            .ClickAsync();
        await page.GetByText("Filtered subject", new() { Exact = true }).WaitForAsync();
        await page.Locator("[data-page-action='next']").ClickAsync();

        await page.WaitForFunctionAsync(
            "() => document.querySelector('[data-result-page=\"2\"]') !== null");
        var final = requests[^1].Query;
        Assert.Contains("q=machine", final, StringComparison.Ordinal);
        Assert.Contains("eligibility=all", final, StringComparison.Ordinal);
        Assert.Contains("credits=3", final, StringComparison.Ordinal);
        Assert.Contains("day=2", final, StringComparison.Ordinal);
        Assert.Contains("availability=available", final, StringComparison.Ordinal);
        Assert.Contains("sort=title%2Cid", final, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("page=2", final, StringComparison.Ordinal);
        Assert.Contains("pageSize=20", final, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Stu_02_unavailable_offering_preserves_required_current_policy_and_source()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteContextAsync(page);
        await page.RouteAsync("**/api/student/terms/*/offerings*", route =>
            route.FulfillAsync(Json(200, $$"""
                {
                  "items": [{{UnavailableOffering()}}],
                  "page": 1,
                  "pageSize": 20,
                  "totalCount": 1,
                  "sort": "courseCode,id"
                }
                """)));

        await OpenAsync(page, "success");

        var offeringCard = page.Locator(
            "[data-offering-id='00000000-0000-0000-0000-000000011100']");
        Assert.True(await offeringCard.GetByText("Unavailable", new() { Exact = true }).First
            .IsVisibleAsync());
        Assert.True(await offeringCard.GetByText("✕", new() { Exact = true }).First
            .IsVisibleAsync());
        Assert.True(await offeringCard.GetByText(
            "EARNED_CREDITS_MINIMUM",
            new() { Exact = true }).IsVisibleAsync());
        Assert.True(await offeringCard.GetByText("96", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await offeringCard.GetByText("95", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await offeringCard.GetByText("policy/demo-poc", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await offeringCard
            .GetByText("DEMO-POC-2026.1", new() { Exact = true }).First
            .IsVisibleAsync());
        Assert.Equal(
            0,
            await offeringCard.GetByRole(
                    AriaRole.Link,
                    new() { Name = "Continue with group A", Exact = true })
                .CountAsync());
    }

    [Fact]
    public async Task Stu_02_empty_reset_and_service_retry_do_not_reuse_cached_success()
    {
        await using var context = await fixture.OpenContextAsync(375, 1000);
        var page = await context.NewPageAsync();
        await RouteContextAsync(page);
        var calls = 0;
        await page.RouteAsync("**/api/student/terms/*/offerings*", route =>
        {
            calls++;
            return calls switch
            {
                1 => route.FulfillAsync(EmptyJson()),
                2 => route.FulfillAsync(Json(503, Error(
                    "DISCOVERY_UNAVAILABLE",
                    "Subject discovery is temporarily unavailable.",
                    "STU-02-SAFE-REF"))),
                _ => route.FulfillAsync(Json(200, OfferingPage()))
            };
        });

        await OpenAsync(page, "empty");
        Assert.True(await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "No subjects match these criteria", Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("DEMO-POC-2026.1", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText(
            "NO_ELIGIBLE_OFFERINGS, PREREQUISITE_NOT_MET",
            new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page
            .Locator("[aria-labelledby='subject-empty-heading']")
            .GetByText("open", new() { Exact = true })
            .IsVisibleAsync());
        await page
            .Locator("[aria-labelledby='subject-empty-heading']")
            .GetByRole(
                AriaRole.Button,
                new() { Name = "Reset filters", Exact = true })
            .ClickAsync();
        await page.Locator("[data-route-id='STU-02'][data-state='service-error']")
            .WaitForAsync();

        Assert.Equal(0, await page.GetByText("Project I", new() { Exact = true })
            .CountAsync());
        Assert.True(await page.GetByText("DISCOVERY_UNAVAILABLE", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("STU-02-SAFE-REF", new() { Exact = true })
            .IsVisibleAsync());
        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Retry subject discovery", Exact = true })
            .ClickAsync();
        await page.Locator("[data-route-id='STU-02'][data-state='success']")
            .WaitForAsync();
        Assert.True(await page.GetByText("Project I", new() { Exact = true })
            .IsVisibleAsync());
    }

    private static async Task RouteContextAsync(IPage page) =>
        await page.RouteAsync("**/api/context", route =>
            route.FulfillAsync(Json(200, AppContext())));

    private static async Task OpenAsync(IPage page, string state)
    {
        await page.GotoAsync("/student/subjects", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Available subjects", Exact = true })
            .WaitForAsync();
        await page.Locator($"[data-route-id='STU-02'][data-state='{state}']")
            .WaitForAsync();
    }

    private static RouteFulfillOptions Json(int status, string body) => new()
    {
        Status = status,
        ContentType = "application/json",
        Body = body
    };

    private static RouteFulfillOptions EmptyJson() => new()
    {
        Status = 200,
        ContentType = "application/json",
        Body = EmptyPage(),
        Headers = new Dictionary<string, string>
        {
            ["X-Eligibility-Policy-Version"] = "DEMO-POC-2026.1",
            ["X-Eligibility-Reason-Codes"] =
                "NO_ELIGIBLE_OFFERINGS, PREREQUISITE_NOT_MET",
            ["X-Registration-Window-State"] = "open",
            ["X-Support-Reference-Path"] = "/support/student/STU-02-SAFE-REF"
        }
    };

    internal static string AppContext() => $$"""
        {
          "serverTimeUtc": "2026-07-20T07:15:00Z",
          "timeZoneId": "Africa/Cairo",
          "teachingTerm": null,
          "registrationTerm": {
            "id": "{{TermId}}",
            "code": "FALL-2026",
            "label": "Fall 2026",
            "state": "registrationOpen",
            "rowVersion": "TERM-RV-1"
          },
          "registrationWindowState": "open",
          "registrationWindow": {
            "id": "WINDOW-FALL-2026",
            "state": "open",
            "opensAtUtc": "2026-07-19T06:00:00Z",
            "closesAtUtc": "2026-07-21T18:00:00Z",
            "rowVersion": "WINDOW-RV-1"
          },
          "serviceState": "available",
          "displayName": "Synthetic Student One",
          "authorizedRoles": ["Student"],
          "activeRole": "Student",
          "sessionState": "active",
          "expiresAtUtc": "2026-07-20T09:15:00Z",
          "supportReferencePath": "/support/student/STU-02-SAFE-REF"
        }
        """;

    private static string EmptyPage() => """
        {
          "items": [],
          "page": 1,
          "pageSize": 20,
          "totalCount": 0,
          "sort": "courseCode,id"
        }
        """;

    private static string OfferingPage(
        string title = "Project I",
        int totalCount = 1,
        int pageNumber = 1) => $$"""
        {
          "items": [{{Offering(title)}}],
          "page": {{pageNumber}},
          "pageSize": 20,
          "totalCount": {{totalCount}},
          "sort": "courseCode,id"
        }
        """;

    internal static string Offering(
        string title = "Project I",
        bool selectable = true,
        int seatsRemaining = 18,
        string groupReasonCode = "AVAILABLE",
        string groupReasonMessage = "A seat is currently available.") => $$"""
        {
          "offeringId": "00000000-0000-0000-0000-000000011100",
          "courseCode": "DS413",
          "title": "{{title}}",
          "credits": 3,
          "currentPlanCredits": 15,
          "projectedPlanCredits": 18,
          "defaultTargetCredits": 18,
          "maximumAllowedCredits": 18,
          "eligible": {{selectable.ToString().ToLowerInvariant()}},
          "reasons": [{
            "code": "NORMAL_MAX_CREDITS",
            "passed": true,
            "blocking": false,
            "message": "Projected load is within the approved normal maximum.",
            "requiredValue": "18",
            "currentValue": "18",
            "policySetId": "00000000-0000-0000-0000-000000011900",
            "policyVersion": "DEMO-POC-2026.1",
            "sourceReference": "policy/demo-poc",
            "sourceAccessedOn": "2026-07-13",
            "approvedBy": "Ahmed ELbamby",
            "effectiveFromUtc": "2026-07-13T00:00:00Z",
            "effectiveToUtc": null,
            "overridePossible": false,
            "supportReferencePath": "/support/student/STU-02-SAFE-REF"
          }],
          "groups": [{
            "groupId": "00000000-0000-0000-0000-000000011200",
            "groupCode": "A",
            "state": "published",
            "selectable": {{selectable.ToString().ToLowerInvariant()}},
            "capacity": 30,
            "enrolledCount": {{30 - seatsRemaining}},
            "seatsRemaining": {{seatsRemaining}},
            "nonSelectableReasons": [{{(selectable ? string.Empty : $$"""
              {
                "code": "{{groupReasonCode}}",
                "message": "{{groupReasonMessage}}"
              }
              """)}}],
            "meetings": [
              {
                "meetingId": "00000000-0000-0000-0000-000000011300",
                "activity": "Lecture",
                "dayOfWeek": 0,
                "startLocal": "09:00:00",
                "endLocal": "10:30:00",
                "roomCode": "A-101",
                "location": "Smart Village",
                "staff": [{
                  "role": "Lecturer",
                  "name": "Dr. Nadia"
                }]
              },
              {
                "meetingId": "00000000-0000-0000-0000-000000011301",
                "activity": "Tutorial",
                "dayOfWeek": 2,
                "startLocal": "11:00:00",
                "endLocal": "12:30:00",
                "roomCode": "LAB-2",
                "location": "Smart Village",
                "staff": [{
                  "role": "TeachingAssistant",
                  "name": "Eng. Omar"
                }]
              }
            ],
            "rowVersion": "GROUP-RV-1"
          }],
          "inputSummary": {
            "standing": "normal",
            "window": "open"
          },
          "evaluatedAtUtc": "2026-07-20T07:15:00Z",
          "academicContextVersion": "ACADEMIC-V1",
          "catalogueVersion": "CATALOGUE-V1",
          "policySetId": "00000000-0000-0000-0000-000000011900",
          "policyVersion": "DEMO-POC-2026.1",
          "offeringRowVersion": "OFFERING-RV-1",
          "currentPlanVersion": "PLAN-V1"
        }
        """;

    internal static string Error(string code, string message, string reference) => $$"""
        {
          "code": "{{code}}",
          "message": "{{message}}",
          "correlationId": "{{reference}}"
        }
        """;

    private static string UnavailableOffering() =>
        Offering(
                selectable: false,
                seatsRemaining: 18,
                groupReasonCode: "PREREQUISITE_NOT_MET",
                groupReasonMessage: "The subject is unavailable until its requirements are met.")
            .Replace(
                "\"code\": \"NORMAL_MAX_CREDITS\"",
                "\"code\": \"EARNED_CREDITS_MINIMUM\"",
                StringComparison.Ordinal)
            .Replace(
                "\"passed\": true",
                "\"passed\": false",
                StringComparison.Ordinal)
            .Replace(
                "\"blocking\": false",
                "\"blocking\": true",
                StringComparison.Ordinal)
            .Replace(
                "Projected load is within the approved normal maximum.",
                "The subject requires 96 earned credits; the current value is 95.",
                StringComparison.Ordinal)
            .Replace(
                "\"requiredValue\": \"18\"",
                "\"requiredValue\": \"96\"",
                StringComparison.Ordinal)
            .Replace(
                "\"currentValue\": \"18\"",
                "\"currentValue\": \"95\"",
                StringComparison.Ordinal);
}
