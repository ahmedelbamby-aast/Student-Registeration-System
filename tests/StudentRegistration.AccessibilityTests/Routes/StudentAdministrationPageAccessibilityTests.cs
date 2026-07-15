using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StudentAdministrationPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Adm04_a11y_metadata_nine_states_focus_order_and_six_widths_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-04.md");

        RepositoryFiles.ContainsAll(
            design,
            "ADM-04-A11Y-T205",
            "\"responsiveWidths\"",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Skip link",
            "Students heading",
            "Correction dialog reason and value fields",
            "Validation summary, cancel, then confirm",
            "Return focus to the correction trigger",
            "ADM-04-COMP-STATE-LOADING",
            "ADM-04-COMP-STATE-EMPTY",
            "ADM-04-COMP-STATE-SUCCESS",
            "ADM-04-COMP-STATE-VALIDATION-ERROR",
            "ADM-04-COMP-STATE-SERVICE-ERROR",
            "ADM-04-COMP-STATE-UNAUTHORIZED",
            "ADM-04-COMP-STATE-SESSION-EXPIRED",
            "ADM-04-COMP-STATE-STALE",
            "ADM-04-COMP-STATE-OFFLINE");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/StudentAdministrationPage.razor"),
            "ADM-04-A11Y-T205: the SPEC-008/T082 executable owner page is missing.");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class StudentAdministrationPageAccessibilityTests(
    AxeAccessibilityFixture fixture)
{
    [Theory]
    [InlineData(320)]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1280)]
    [InlineData(1920)]
    public async Task Adm04_has_no_serious_axe_issue_or_horizontal_overflow_at_width(
        int width)
    {
        await using var context = await fixture.OpenContextAsync(width);
        var page = await LoadSelectedStudentAsync(context);

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        foreach (var testId in new[]
                 {
                     "student-search-results",
                     "academic-summary",
                     "academic-transcript",
                     "academic-holds",
                     "academic-provenance"
                 })
        {
            Assert.True(await page.Locator($"[data-testid='{testId}']").IsVisibleAsync());
        }

        foreach (var name in new[] { "Search Students", "Open academic correction" })
        {
            var box = await page.GetByRole(
                    AriaRole.Button,
                    new() { Name = name, Exact = true })
                .BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(box.Height >= 44, $"{name} is shorter than 44 CSS pixels at {width}px.");
        }
    }

    [Fact]
    public async Task Adm04_keyboard_landmarks_tables_and_dialog_focus_are_logical()
    {
        await using var context = await fixture.OpenContextAsync(1024);
        var page = await LoadSelectedStudentAsync(context);

        Assert.Equal(1, await page.GetByRole(AriaRole.Main).CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Banner).CountAsync());
        Assert.Equal(1, await page.GetByRole(AriaRole.Contentinfo).CountAsync());
        Assert.Equal(
            1,
            await page.GetByRole(
                    AriaRole.Heading,
                    new() { Name = "Student administration", Exact = true })
                .CountAsync());
        Assert.True(await page.GetByRole(AriaRole.Table).CountAsync() >= 3);
        Assert.True(await page.GetByRole(AriaRole.Columnheader).CountAsync() >= 3);

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

        var trigger = page.GetByRole(
            AriaRole.Button,
            new() { Name = "Open academic correction", Exact = true });
        await trigger.FocusAsync();
        await trigger.PressAsync("Enter");
        var dialog = page.GetByRole(
            AriaRole.Dialog,
            new() { Name = "Academic correction", Exact = true });
        await dialog.WaitForAsync();
        Assert.Equal(
            "academic-correction-reason",
            await page.EvaluateAsync<string>("() => document.activeElement.id"));
        Assert.Equal(
            "true",
            await dialog.GetAttributeAsync("aria-modal"));

        await page.GetByRole(AriaRole.Button, new() { Name = "Cancel", Exact = true })
            .ClickAsync();
        Assert.True(await trigger.EvaluateAsync<bool>("element => element === document.activeElement"));
    }

    [Fact]
    public async Task Adm04_screen_reader_names_and_validation_relationships_are_explicit()
    {
        await using var context = await fixture.OpenContextAsync(768);
        var page = await LoadSelectedStudentAsync(context);
        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Open academic correction", Exact = true })
            .ClickAsync();
        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Submit academic correction", Exact = true })
            .ClickAsync();

        var summary = page.Locator("[data-testid='correction-validation-summary']");
        await summary.WaitForAsync();
        Assert.Equal("alert", await summary.GetAttributeAsync("role"));
        Assert.Equal("-1", await summary.GetAttributeAsync("tabindex"));
        Assert.True(await summary.EvaluateAsync<bool>("element => element === document.activeElement"));
        Assert.Contains(
            "academic-correction-reason-error",
            await page.Locator("#academic-correction-reason")
                .GetAttributeAsync("aria-describedby") ?? string.Empty,
            StringComparison.Ordinal);
        Assert.Equal(
            1,
            await page.GetByRole(
                    AriaRole.Link,
                    new() { Name = "Open registrations", Exact = true })
                .CountAsync());
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
    }

    [Fact]
    public async Task Adm04_reflows_at_400_percent_and_keeps_the_full_screen_dialog_operable()
    {
        await using var context = await fixture.OpenContextAsync(
            width: 320,
            height: 900,
            deviceScaleFactor: 4);
        var page = await LoadSelectedStudentAsync(context);
        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Open academic correction", Exact = true })
            .ClickAsync();

        var dialog = page.GetByRole(
            AriaRole.Dialog,
            new() { Name = "Academic correction", Exact = true });
        await dialog.WaitForAsync();
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        var box = await dialog.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box.Width <= 320);
        Assert.True(await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Submit academic correction", Exact = true })
            .IsVisibleAsync());
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
    }

    private static async Task<IPage> LoadSelectedStudentAsync(IBrowserContext context)
    {
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(5_000);
        await page.RouteAsync("**/api/context", route => JsonAsync(route, AppContext));
        await page.RouteAsync("**/api/admin/students?*", route => JsonAsync(route, SearchResults));
        await page.RouteAsync(
            "**/api/admin/students/student-001/academic-context?*",
            route => JsonAsync(route, AcademicDetail));
        await page.GotoAsync("/admin/students", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Student administration", Exact = true })
            .WaitForAsync();
        await page.Locator("[data-testid='student-search-input']").FillAsync("20260001");
        await page.GetByRole(AriaRole.Button, new() { Name = "Search Students", Exact = true })
            .ClickAsync();
        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Open academic record", Exact = true })
            .ClickAsync();
        await page.Locator("[data-testid='ADM-04-COMP-STATE-SUCCESS']").WaitForAsync();
        return page;
    }

    private static Task JsonAsync(IRoute route, string body) => route.FulfillAsync(
        new RouteFulfillOptions
        {
            Status = 200,
            ContentType = "application/json",
            Body = body
        });

    private const string AppContext = """
        {"serverTimeUtc":"2026-07-14T10:15:00Z","timeZoneId":"Africa/Cairo","teachingTerm":null,"registrationTerm":{"id":"term-2026-fall","code":"2026-FALL","label":"Fall 2026","state":"registrationOpen","rowVersion":"term-rv-4"},"registrationWindowState":"none","registrationWindow":null,"serviceState":"available","displayName":"Ahmed Admin","authorizedRoles":["Admin"],"activeRole":"Admin","sessionState":"active","expiresAtUtc":"2026-07-14T18:00:00Z","supportReferencePath":"/support"}
        """;

    private const string SearchResults = """
        {"items":[{"studentId":"student-001","universityId":"20260001","programCode":"AI","cohort":"2026","standing":"Good standing","dataVersion":"profile-v7"}],"page":1,"pageSize":20,"totalCount":1,"sort":"universityId,studentId"}
        """;

    private const string AcademicDetail = """
        {
          "studentId":"student-001","termId":"term-2026-fall","universityId":"20260001","programCode":"AI","cohort":"2026","currentGpa":3.42,"earnedCredits":84,"standing":"Good standing",
          "transcriptSummary":{"attemptedCredits":87,"earnedCredits":84,"attemptCount":29},
          "transcriptAttempts":{"items":[{"attemptId":"attempt-401","supersedesAttemptId":null,"courseCode":"AIC401","termCode":"2026-Spring","credits":3,"grade":"A","status":"passed","provenance":"Synthetic SIS"}],"page":1,"pageSize":20,"totalCount":1,"sort":"termCode,courseCode,attemptId"},
          "activeHolds":[{"termId":"term-2026-fall","code":"ADVISING","message":"Meet the academic adviser.","blocksRegistration":false,"effectiveFromUtc":"2026-07-01T00:00:00Z","effectiveToUtc":null,"source":"Synthetic SIS","holdId":"hold-001","sourceReference":"SIS-H-001"}],
          "dataVersion":"profile-v7","dataAsOfUtc":"2026-07-14T09:45:00Z",
          "provenance":{"items":[{"source":"Synthetic SIS","reference":"SIS-20260001","importedAtUtc":"2026-07-14T09:45:00Z"}],"page":1,"pageSize":20,"totalCount":1,"sort":"importedAtUtc,reference"},
          "studentRowVersion":"student-rv-7","studentTermStateRowVersion":"term-state-rv-4"
        }
        """;
}
