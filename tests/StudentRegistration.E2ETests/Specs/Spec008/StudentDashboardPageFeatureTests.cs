using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec008;

public sealed class StudentDashboardPageFrozenContractTests
{
    [Fact]
    public void Stu_01_owner_metadata_states_journeys_and_page_boundary_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-01.md");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/student\"",
            "\"pageName\": \"StudentDashboardPage.razor\"",
            "GET /api/context",
            "GET /api/students/me/academic-context",
            "GET /api/student/registrations/current/timetable",
            "STU-01-open-v1",
            "STU-01-upcoming-v1",
            "STU-01-closed-v1",
            "STU-01-no-term-v1",
            "STU-01-hold-v1",
            "STU-01-incomplete-profile-v1",
            "STU-01-E2E-PRIMARY",
            "STU-01-E2E-FAILURE");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/StudentDashboardPage.razor"),
            "STU-01-E2E-PRIMARY: SPEC-008/T078 must deliver StudentDashboardPage.razor.");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class StudentDashboardPageFeatureTests(Spec008BrowserFixture fixture)
{
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/StudentDashboardPage.razor";

    [Fact]
    public async Task Stu_01_e2e_primary_open_uses_server_context_and_one_start_action()
    {
        RequireOwnerPage();
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteDashboardAsync(page, AppContext("open"), AcademicContext());

        await NavigateAndWaitAsync(page, "success");

        Assert.True(await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Student dashboard", Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("AI", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("2.85", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.Locator("[data-window-state='open']").IsVisibleAsync());
        var start = page.GetByRole(
            AriaRole.Link,
            new() { Name = "Start registration", Exact = true });
        Assert.Equal(1, await start.CountAsync());
        Assert.Equal("/student/subjects", await start.GetAttributeAsync("href"));
        Assert.Equal(
            0,
            await page.GetByRole(
                AriaRole.Link,
                new() { Name = "Resume registration", Exact = true })
                .CountAsync());
        Assert.True(await page.Locator(
            "[data-testid='current-timetable-unavailable'][data-contributor-state='unavailable']")
            .IsVisibleAsync());
        Assert.True(await page.GetByText(
            "Current timetable is unavailable until SPEC-015 is delivered.",
            new() { Exact = true }).IsVisibleAsync());
    }

    [Fact]
    public async Task Stu_01_e2e_primary_upcoming_discloses_server_opening_time_and_disables_registration()
    {
        RequireOwnerPage();
        await using var context = await fixture.OpenContextAsync(375, 900);
        var page = await context.NewPageAsync();
        await RouteDashboardAsync(page, AppContext("upcoming"), AcademicContext());

        await NavigateAndWaitAsync(page, "success");

        Assert.True(await page.Locator("[data-window-state='upcoming']").IsVisibleAsync());
        Assert.True(await page.GetByText("20 July 2026", new() { Exact = false }).IsVisibleAsync());
        Assert.True(await page.GetByText("Africa/Cairo", new() { Exact = true }).IsVisibleAsync());
        var action = page.Locator("[data-testid='registration-primary-action']");
        Assert.Equal("true", await action.GetAttributeAsync("aria-disabled"));
        Assert.False(await action.IsEnabledAsync());
        Assert.Equal(0, await page.Locator(
            "a[href='/student/subjects'],a[href='/student/schedule']").CountAsync());
    }

    [Fact]
    public async Task Stu_01_e2e_failure_closed_preserves_window_code_and_rejects_navigation()
    {
        RequireOwnerPage();
        await using var context = await fixture.OpenContextAsync(768, 900);
        var page = await context.NewPageAsync();
        await RouteDashboardAsync(page, AppContext("closed"), AcademicContext());

        await NavigateAndWaitAsync(page, "stale");

        Assert.True(await page.Locator("[data-window-state='closed']").IsVisibleAsync());
        Assert.True(await page.Locator("[data-reason-code='WINDOW_CLOSED']").IsVisibleAsync());
        Assert.True(await page.GetByText("19 July 2026", new() { Exact = false }).IsVisibleAsync());
        await AssertRegistrationNavigationUnavailableAsync(page);
    }

    [Fact]
    public async Task Stu_01_e2e_failure_no_term_never_invents_a_browser_date_fallback()
    {
        RequireOwnerPage();
        await using var context = await fixture.OpenContextAsync(375, 900);
        var page = await context.NewPageAsync();
        await RouteDashboardAsync(page, AppContext("none"), AcademicContext());

        await NavigateAndWaitAsync(page, "empty");

        Assert.True(await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "No current registration term", Exact = true }).IsVisibleAsync());
        Assert.Equal(
            "/support/student/STU-01-SAFE-REF",
            await page.GetByRole(
                    AriaRole.Link,
                    new() { Name = "Open support", Exact = true })
                .GetAttributeAsync("href"));
        await AssertRegistrationNavigationUnavailableAsync(page);
        Assert.Equal(0, await page.Locator("[data-window-state='open']").CountAsync());
    }

    [Fact]
    public async Task Stu_01_e2e_failure_hold_shows_every_safe_reason_before_described_disabled_action()
    {
        RequireOwnerPage();
        await using var context = await fixture.OpenContextAsync(375, 1000);
        var page = await context.NewPageAsync();
        await RouteDashboardAsync(
            page,
            AppContext("open"),
            AcademicContext(includeRegistrationHold: true));

        await NavigateAndWaitAsync(page, "validation-error");

        Assert.True(await page.Locator("[data-reason-code='REGISTRATION_HOLD']").IsVisibleAsync());
        Assert.True(await page.GetByText(
            "Clear the synthetic finance hold with Student Affairs.",
            new() { Exact = true }).IsVisibleAsync());
        var blockers = page.Locator("[data-testid='registration-blockers']");
        var action = page.Locator("[data-testid='registration-primary-action']");
        Assert.True(await blockers.IsVisibleAsync());
        Assert.Equal("true", await action.GetAttributeAsync("aria-disabled"));
        Assert.False(string.IsNullOrWhiteSpace(await action.GetAttributeAsync("aria-describedby")));
        Assert.True(await page.EvaluateAsync<bool>(
            """
            () => {
              const blockers = document.querySelector('[data-testid="registration-blockers"]');
              const action = document.querySelector('[data-testid="registration-primary-action"]');
              return Boolean(blockers && action &&
                (blockers.compareDocumentPosition(action) & Node.DOCUMENT_POSITION_FOLLOWING));
            }
            """));
        await AssertRegistrationNavigationUnavailableAsync(page);
    }

    [Fact]
    public async Task Stu_01_e2e_failure_incomplete_profile_preserves_reason_and_safe_next_step()
    {
        RequireOwnerPage();
        await using var context = await fixture.OpenContextAsync(375, 900);
        var page = await context.NewPageAsync();
        await RouteDashboardAsync(
            page,
            AppContext("open"),
            academicContext: null,
            academicStatus: 409,
            academicError: Error(
                "PROFILE_NOT_READY",
                "Your academic profile is incomplete. Contact Student Affairs before registration."));

        await NavigateAndWaitAsync(page, "validation-error");

        Assert.True(await page.Locator("[data-reason-code='PROFILE_NOT_READY']").IsVisibleAsync());
        Assert.True(await page.GetByText(
            "Your academic profile is incomplete. Contact Student Affairs before registration.",
            new() { Exact = true }).IsVisibleAsync());
        var action = page.Locator("[data-testid='registration-primary-action']");
        Assert.Equal("true", await action.GetAttributeAsync("aria-disabled"));
        await AssertRegistrationNavigationUnavailableAsync(page);
        Assert.Equal(0, await page.Locator("input,textarea,select").CountAsync());
    }

    private static async Task RouteDashboardAsync(
        IPage page,
        string appContext,
        string? academicContext,
        int academicStatus = 200,
        string? academicError = null)
    {
        await page.RouteAsync("**/api/context", route => route.FulfillAsync(
            JsonResponse(200, appContext)));
        await page.RouteAsync("**/api/students/me/academic-context", route =>
            route.FulfillAsync(JsonResponse(
                academicStatus,
                academicStatus == 200 ? academicContext! : academicError!)));
        await page.RouteAsync(
            "**/api/student/registrations/current/timetable",
            route => route.FulfillAsync(JsonResponse(
                503,
                Error(
                    "CONTEXT_UNAVAILABLE",
                    "The current timetable contract is not available yet."))));
    }

    private static async Task NavigateAndWaitAsync(IPage page, string state)
    {
        await page.GotoAsync("/student", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Student dashboard", Exact = true })
            .WaitForAsync();
        await page.Locator($"[data-route-id='STU-01'][data-state='{state}']")
            .WaitForAsync();
    }

    private static async Task AssertRegistrationNavigationUnavailableAsync(IPage page)
    {
        Assert.Equal(0, await page.Locator(
            "a[href='/student/subjects'],a[href='/student/schedule']").CountAsync());
        var action = page.Locator("[data-testid='registration-primary-action']");
        if (await action.CountAsync() > 0)
        {
            Assert.Equal("true", await action.GetAttributeAsync("aria-disabled"));
        }
    }

    private static RouteFulfillOptions JsonResponse(int status, string body) => new()
    {
        Status = status,
        ContentType = "application/json",
        Body = body
    };

    private static string AppContext(string windowState)
    {
        var hasTerm = !string.Equals(windowState, "none", StringComparison.Ordinal);
        var (opensAtUtc, closesAtUtc) = windowState switch
        {
            "upcoming" => ("2026-07-20T08:00:00Z", "2026-07-22T18:00:00Z"),
            "closed" => ("2026-07-18T06:00:00Z", "2026-07-19T18:00:00Z"),
            _ => ("2026-07-19T06:00:00Z", "2026-07-21T18:00:00Z")
        };
        var registrationTerm = hasTerm
            ? """
              {
                "id": "TERM-FALL-2026",
                "code": "FALL-2026",
                "label": "Fall 2026",
                "state": "registrationOpen",
                "rowVersion": "TERM-RV-1"
              }
              """
            : "null";
        var registrationWindow = hasTerm
            ? $$"""
              {
                "id": "WINDOW-FALL-2026",
                "state": "{{windowState}}",
                "opensAtUtc": "{{opensAtUtc}}",
                "closesAtUtc": "{{closesAtUtc}}",
                "rowVersion": "WINDOW-RV-1"
              }
              """
            : "null";

        return $$"""
            {
              "serverTimeUtc": "2026-07-20T07:15:00Z",
              "timeZoneId": "Africa/Cairo",
              "teachingTerm": {
                "id": "TERM-SUMMER-2026",
                "code": "SUMMER-2026",
                "label": "Summer 2026",
                "state": "teaching",
                "rowVersion": "TEACHING-RV-1"
              },
              "registrationTerm": {{registrationTerm}},
              "registrationWindowState": "{{windowState}}",
              "registrationWindow": {{registrationWindow}},
              "serviceState": "available",
              "displayName": "Synthetic Student One",
              "authorizedRoles": ["Student"],
              "activeRole": "Student",
              "sessionState": "active",
              "expiresAtUtc": "2026-07-20T09:15:00Z",
              "supportReferencePath": "/support/student/STU-01-SAFE-REF"
            }
            """;
    }

    private static string AcademicContext(bool includeRegistrationHold = false)
    {
        var activeHolds = includeRegistrationHold
            ? """
              [
                {
                  "termId": "TERM-FALL-2026",
                  "code": "REGISTRATION_HOLD",
                  "message": "Clear the synthetic finance hold with Student Affairs.",
                  "blocksRegistration": true,
                  "effectiveFromUtc": "2026-07-01T00:00:00Z",
                  "effectiveToUtc": null,
                  "source": "synthetic-demo-seed/1.0"
                }
              ]
              """
            : "[]";

        return $$"""
            {
              "universityId": "20260001",
              "programCode": "AI",
              "cohort": "2026",
              "currentGpa": 2.85,
              "earnedCredits": 72,
              "standing": "Good standing",
              "transcriptSummary": {
                "attemptedCredits": 75,
                "earnedCredits": 72,
                "attemptCount": 25
              },
              "transcriptAttempts": {
                "items": [],
                "page": 1,
                "pageSize": 20,
                "totalCount": 0,
                "sort": "termCode,courseCode,attemptId"
              },
              "activeHolds": {{activeHolds}},
              "dataVersion": "ACADEMIC-V1",
              "dataAsOfUtc": "2026-07-20T07:14:00Z",
              "provenance": {
                "items": [],
                "page": 1,
                "pageSize": 20,
                "totalCount": 0,
                "sort": "importedAtUtc,reference"
              }
            }
            """;
    }

    private static string Error(string code, string message) => $$"""
        {
          "code": "{{code}}",
          "message": "{{message}}",
          "correlationId": "STU-01-SAFE-REF"
        }
        """;

    private static void RequireOwnerPage() =>
        Assert.True(
            RepositoryFiles.Exists(PagePath),
            "StudentDashboardPage.razor is intentionally absent until SPEC-008/T078; the STU-01 browser journey remains red.");
}
