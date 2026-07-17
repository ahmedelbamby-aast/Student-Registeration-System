using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class RegistrationReviewPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Stu_05_a11y_metadata_focus_order_and_six_governed_widths_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-05.md");

        RepositoryFiles.ContainsAll(
            design,
            "STU-05-A11Y-T170",
            "\"responsiveWidths\"",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Skip link",
            "Registration review heading",
            "Validation summary and ConflictPanel blockers",
            "Review and submit trigger with aria-describedby blockers",
            "Confirmation dialog heading after it opens",
            "On Cancel or Escape restore Review and submit trigger",
            "On rejection focus validation summary");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/RegistrationReviewPage.razor"),
            "STU-05-A11Y-T170 requires the executable SPEC-014 owner page.");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class RegistrationReviewPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    private const string TermId = "00000000-0000-0000-0000-000000014001";
    private const string GroupId = "00000000-0000-0000-0000-000000014003";

    [Theory]
    [InlineData(320)]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1280)]
    [InlineData(1920)]
    public async Task Stu_05_has_no_serious_axe_issue_overflow_or_short_primary_target_at_width(
        int width)
    {
        await using var context = await fixture.OpenContextAsync(width);
        var page = await LoadReviewAsync(context);

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        Assert.Equal(1, await page.GetByRole(AriaRole.Main).CountAsync());

        foreach (var locator in new[]
                 {
                     page.GetByRole(AriaRole.Link, new() { Name = "Edit plan", Exact = true }),
                     page.GetByRole(AriaRole.Button, new() { Name = "Review and submit", Exact = true })
                 })
        {
            var box = await locator.BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(box.Height >= 44, $"A primary STU-05 target is shorter than 44 CSS pixels at {width}px.");
        }
    }

    [Fact]
    public async Task Stu_05_keyboard_landmarks_dialog_trap_and_cancel_focus_follow_the_record()
    {
        await using var context = await fixture.OpenContextAsync(1024);
        var page = await LoadReviewAsync(context);

        Assert.Equal(1, await page.GetByRole(AriaRole.Banner).CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Contentinfo).CountAsync());
        Assert.Equal(1, await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Registration review", Exact = true }).CountAsync());

        var skip = page.GetByRole(
            AriaRole.Link,
            new() { Name = "Skip to main content", Exact = true });
        await skip.FocusAsync();
        Assert.True(await skip.EvaluateAsync<bool>(
            "element => getComputedStyle(element).outlineStyle !== 'none'"));
        await skip.PressAsync("Enter");
        Assert.Equal(
            "main-content",
            await page.EvaluateAsync<string>("() => document.activeElement?.id || ''"));

        var trigger = page.GetByRole(
            AriaRole.Button,
            new() { Name = "Review and submit", Exact = true });
        await trigger.ClickAsync();
        var dialog = page.GetByRole(
            AriaRole.Dialog,
            new() { Name = "Confirm registration", Exact = true });
        await dialog.WaitForAsync();
        Assert.Equal("true", await dialog.GetAttributeAsync("aria-modal"));
        Assert.Equal(
            "Cancel",
            await page.EvaluateAsync<string>("() => document.activeElement?.textContent?.trim() || ''"));
        await page.Keyboard.PressAsync("Shift+Tab");
        Assert.Equal(1, await dialog.Locator(":focus").CountAsync());
        await page.Keyboard.PressAsync("Escape");
        Assert.True(await trigger.EvaluateAsync<bool>("element => document.activeElement === element"));
    }

    [Fact]
    public async Task Stu_05_blockers_are_assertive_text_and_describe_the_disabled_command()
    {
        await using var context = await fixture.OpenContextAsync(768);
        var page = await LoadReviewAsync(context, blocked: true);

        var blockers = page.Locator("#registration-blockers");
        Assert.Equal("alert", await blockers.GetAttributeAsync("role"));
        Assert.Equal("assertive", await blockers.GetAttributeAsync("aria-live"));
        Assert.True(await blockers.GetByRole(
            AriaRole.Heading,
            new() { Name = "Conflict", Exact = true }).IsVisibleAsync());
        Assert.True(await blockers.GetByText("MEETING_OVERLAP", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await blockers.GetByText("Monday 11:00–11:30", new() { Exact = false }).IsVisibleAsync());

        var submit = page.GetByRole(
            AriaRole.Button,
            new() { Name = "Review and submit", Exact = true });
        Assert.True(await submit.IsDisabledAsync());
        var describedBy = await submit.GetAttributeAsync("aria-describedby");
        Assert.Contains("registration-blockers", describedBy);
        Assert.Contains("review-disabled-reason", describedBy);
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
    }

    [Fact]
    public async Task Stu_05_rejection_focuses_the_status_heading_and_keeps_the_server_reason_visible()
    {
        await using var context = await fixture.OpenContextAsync(1024);
        var page = await LoadReviewAsync(context, submitStatus: 409);

        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Review and submit", Exact = true }).ClickAsync();
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Confirm registration", Exact = true }).ClickAsync();

        var heading = page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Registration context changed", Exact = true });
        await heading.WaitForAsync();
        Assert.True(await heading.EvaluateAsync<bool>("element => document.activeElement === element"));
        Assert.True(await page.GetByText("POLICY_CHANGED", new() { Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByRole(AriaRole.Dialog).CountAsync());
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
    }

    [Fact]
    public async Task Stu_05_reflows_at_400_percent_without_horizontal_page_scroll()
    {
        await using var context = await fixture.OpenContextAsync(
            width: 320,
            height: 1000,
            deviceScaleFactor: 4);
        var page = await LoadReviewAsync(context);

        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
    }

    private static async Task<IPage> LoadReviewAsync(
        IBrowserContext context,
        bool blocked = false,
        int submitStatus = 201)
    {
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", async route =>
        {
            var path = new Uri(route.Request.Url).AbsolutePath;
            if (path == "/api/context")
            {
                await JsonAsync(route, 200, AppContext());
            }
            else if (route.Request.Method == "GET" && path.EndsWith("/registration-plan"))
            {
                await JsonAsync(route, 200, Plan(blocked));
            }
            else if (route.Request.Method == "POST" && path.EndsWith("/registration-plan/validate"))
            {
                await JsonAsync(route, 200, Plan(blocked));
            }
            else if (route.Request.Method == "POST" && path.EndsWith("/registrations"))
            {
                await JsonAsync(
                    route,
                    submitStatus,
                    submitStatus == 201 ? FinalResult() : Error("POLICY_CHANGED"));
            }
            else
            {
                await route.AbortAsync();
            }
        });
        await page.GotoAsync("/student/review", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Registration review", Exact = true }).WaitForAsync();
        await page.Locator($"[data-route-id='STU-05'][data-state='{(blocked ? "validation-error" : "success")}']")
            .WaitForAsync();
        return page;
    }

    private static Task JsonAsync(IRoute route, int status, string body) =>
        route.FulfillAsync(new() { Status = status, ContentType = "application/json", Body = body });

    private static string AppContext() => $$$"""
        {
          "serverTimeUtc":"2026-07-20T07:15:00Z","timeZoneId":"Africa/Cairo",
          "teachingTerm":null,
          "registrationTerm":{"id":"{{{TermId}}}","code":"FALL-2026","label":"Fall 2026","state":"registrationOpen","rowVersion":"TERM-RV-1"},
          "registrationWindowState":"open",
          "registrationWindow":{"id":"00000000-0000-0000-0000-000000014010","state":"open","opensAtUtc":"2026-07-19T06:00:00Z","closesAtUtc":"2026-07-21T18:00:00Z","rowVersion":"WINDOW-RV-1"},
          "serviceState":"available","displayName":"Synthetic Student One",
          "authorizedRoles":["Student"],"activeRole":"Student","sessionState":"active",
          "expiresAtUtc":"2026-07-20T09:15:00Z","supportReferencePath":"/support/student/STU-05-SAFE-REF"
        }
        """;

    private static string Plan(bool blocked) => $$$"""
        {
          "id":"00000000-0000-0000-0000-000000014002","termId":"{{{TermId}}}","rowVersion":"PLAN-RV-1",
          "selectedGroups":[{
            "groupId":"{{{GroupId}}}","offeringId":"00000000-0000-0000-0000-000000014020",
            "courseCode":"AI401","subjectTitle":"Artificial Intelligence","groupCode":"G01","credits":3,
            "capacity":30,"enrolledCount":12,"rowVersion":"GROUP-RV-1",
            "meetings":[{"meetingId":"00000000-0000-0000-0000-000000014030","activityKind":"Lecture","dayOfWeek":1,"startLocal":"10:00:00","endLocal":"11:30:00","roomCode":"A-101","location":"Smart Village","lecturerName":"Dr. Salma","teachingAssistantNames":[],"timezone":"Africa/Cairo"}]
          }],
          "totalCredits":3,"defaultTargetCredits":18,"maximumAllowedCredits":18,"loadReasons":[],
          "selectionIssues":{{{Issues(blocked)}}},"conflicts":[],
          "validation":{"evaluatedAtUtc":"2026-07-20T07:15:00Z","academicContextVersion":"ACADEMIC-V1","policyVersion":"DEMO-POC-2026.1","catalogueVersion":"CATALOGUE-V1","offeringVersions":{},"groupVersions":{}},
          "reviewBlocked":{{{blocked.ToString().ToLowerInvariant()}}}
        }
        """;

    private static string Issues(bool blocked) => blocked
        ? "[{\"code\":\"MEETING_OVERLAP\",\"offeringId\":\"00000000-0000-0000-0000-000000014020\",\"groupId\":\"00000000-0000-0000-0000-000000014003\",\"groupCode\":\"G01\",\"message\":\"AI401 G01 overlaps CS402 G02 on Monday 11:00–11:30.\",\"blocking\":true,\"actions\":[]}]"
        : "[]";

    private static string FinalResult() =>
        "{\"submissionId\":\"00000000-0000-0000-0000-000000014004\",\"status\":\"accepted\",\"resultCode\":\"REGISTERED\",\"registeredGroups\":[],\"receivedAtUtc\":\"2026-07-20T07:15:00Z\",\"completedAtUtc\":\"2026-07-20T07:15:01Z\",\"policySetId\":\"00000000-0000-0000-0000-000000014090\",\"policyVersion\":\"DEMO-POC-2026.1\",\"planRowVersion\":\"PLAN-RV-1\",\"reference\":\"REG-014-0001\"}";

    private static string Error(string code) =>
        $"{{\"code\":\"{code}\",\"message\":\"The registration context changed. Refresh and review before trying again.\",\"correlationId\":\"STU-05-SAFE-REF\"}}";
}
