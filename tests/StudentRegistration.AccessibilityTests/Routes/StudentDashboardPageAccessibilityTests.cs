using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StudentDashboardPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Stu_01_a11y_metadata_focus_order_and_six_governed_widths_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-01.md");

        RepositoryFiles.ContainsAll(
            design,
            "STU-01-A11Y-T150",
            "\"responsiveWidths\"",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Skip link",
            "Student dashboard heading",
            "Holds and incomplete-profile blockers",
            "Start registration or Resume registration action",
            "Current timetable chronological list");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/StudentDashboardPage.razor"),
            "STU-01-A11Y-T150: the executable owner page is still missing until SPEC-008/T078.");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class StudentDashboardPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/StudentDashboardPage.razor";

    private const string OpenAppContext = """
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
          "registrationTerm": {
            "id": "TERM-FALL-2026",
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
          "supportReferencePath": "/support/student/STU-01-SAFE-REF"
        }
        """;

    private const string AcademicContext = """
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
          "activeHolds": [],
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

    [Theory]
    [InlineData(320)]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1280)]
    [InlineData(1920)]
    public async Task Stu_01_has_no_serious_axe_issue_overflow_or_short_target_at_width(
        int width)
    {
        RequireOwnerPage();
        await using var context = await fixture.OpenContextAsync(width);
        var page = await LoadOpenPageAsync(context);

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        foreach (var name in new[] { "Start registration", "Current registrations", "Account" })
        {
            var box = await page.GetByRole(
                    AriaRole.Link,
                    new PageGetByRoleOptions { Name = name, Exact = true })
                .BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(box.Height >= 44, $"{name} is shorter than 44 CSS pixels at {width}px.");
        }

        Assert.True(await page.Locator(
            "[data-testid='current-timetable-unavailable'][data-contributor-state='unavailable']")
            .IsVisibleAsync());
    }

    [Fact]
    public async Task Stu_01_keyboard_focus_landmarks_and_screen_reader_names_follow_the_record()
    {
        RequireOwnerPage();
        await using var context = await fixture.OpenContextAsync(768);
        var page = await LoadOpenPageAsync(context);

        Assert.Equal(1, await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Student dashboard", Exact = true }).CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Main).CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Banner).CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Contentinfo).CountAsync());
        Assert.Equal(1, await page.GetByRole(
            AriaRole.Region,
            new() { Name = "Academic summary", Exact = true }).CountAsync());
        Assert.Equal(1, await page.GetByRole(
            AriaRole.Region,
            new() { Name = "Current timetable", Exact = true }).CountAsync());

        var skip = page.GetByRole(
            AriaRole.Link,
            new() { Name = "Skip to main content", Exact = true });
        await skip.FocusAsync();
        Assert.True(await skip.EvaluateAsync<bool>(
            "element => getComputedStyle(element).outlineStyle !== 'none'"));
        await skip.PressAsync("Enter");
        Assert.Equal(
            "main-content",
            await page.EvaluateAsync<string>("() => document.activeElement.id"));

        var interactiveNames = await page.Locator("a,button")
            .EvaluateAllAsync<string[]>(
                "elements => elements.map(element => (element.innerText || element.getAttribute('aria-label') || '').trim())");
        Assert.True(Array.IndexOf(interactiveNames, "Start registration") <
                    Array.IndexOf(interactiveNames, "Current registrations"));
        Assert.True(Array.IndexOf(interactiveNames, "Current registrations") <
                    Array.IndexOf(interactiveNames, "Account"));
    }

    [Fact]
    public async Task Stu_01_initial_navigation_focuses_heading_without_announcing_false_success()
    {
        RequireOwnerPage();
        await using var context = await fixture.OpenContextAsync(1024);
        var page = await LoadOpenPageAsync(context);

        Assert.Equal(
            "student-dashboard-heading",
            await page.EvaluateAsync<string>("() => document.activeElement.id"));
        Assert.Equal(
            "polite",
            await page.Locator("[data-testid='student-dashboard-status']")
                .GetAttributeAsync("aria-live"));
        Assert.Equal(0, await page.GetByText("queued success", new() { Exact = false }).CountAsync());
    }

    [Fact]
    public async Task Stu_01_reflows_at_the_400_percent_effective_mobile_width()
    {
        RequireOwnerPage();
        await using var context = await fixture.OpenContextAsync(
            width: 320,
            height: 1000,
            deviceScaleFactor: 4);
        var page = await LoadOpenPageAsync(context);

        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        var primary = await page.GetByRole(
                AriaRole.Link,
                new() { Name = "Start registration", Exact = true })
            .BoundingBoxAsync();
        Assert.NotNull(primary);
        Assert.True(primary.Height >= 44);
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
    }

    private static async Task<IPage> LoadOpenPageAsync(IBrowserContext context)
    {
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/context", route => route.FulfillAsync(
            JsonResponse(200, OpenAppContext)));
        await page.RouteAsync("**/api/students/me/academic-context", route => route.FulfillAsync(
            JsonResponse(200, AcademicContext)));
        await page.RouteAsync(
            "**/api/student/registrations/current/timetable",
            route => route.FulfillAsync(JsonResponse(
                503,
                """
                {
                  "code": "CONTEXT_UNAVAILABLE",
                  "message": "The current timetable contract is not available yet.",
                  "correlationId": "STU-01-SAFE-REF"
                }
                """)));
        await page.GotoAsync("/student", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.Locator("h1#student-dashboard-heading").WaitForAsync();
        await page.Locator("[data-route-id='STU-01'][data-state='success']")
            .WaitForAsync();
        return page;
    }

    private static RouteFulfillOptions JsonResponse(int status, string body) => new()
    {
        Status = status,
        ContentType = "application/json",
        Body = body
    };

    private static void RequireOwnerPage() =>
        Assert.True(
            RepositoryFiles.Exists(PagePath),
            "StudentDashboardPage.razor is intentionally absent until SPEC-008/T078; the STU-01 accessibility journey remains red.");
}
