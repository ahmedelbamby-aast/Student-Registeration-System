using System.Globalization;
using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec008;

public sealed class StudentAdministrationPageFrozenContractTests
{
    [Fact]
    public void Adm04_owner_metadata_and_the_t082_delivery_boundary_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-04.md");
        var client = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Academics/AcademicApiClient.cs");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/admin/students\"",
            "\"pageName\": \"StudentAdministrationPage.razor\"",
            "GET /api/admin/students",
            "GET /api/admin/students/{studentId}/academic-context",
            "PATCH /api/admin/students/{studentId}/academic-profile",
            "ADM-04-read-v1",
            "ADM-04-no-results-v1",
            "ADM-04-invalid-correction-v1",
            "ADM-04-stale-v1",
            "ADM-04-reason-required-v1",
            "ADM-04-restricted-v1");
        RepositoryFiles.ContainsAll(
            client,
            "SearchAdminStudentsAsync",
            "GetAdminStudentAcademicContextAsync",
            "CorrectAdminStudentAcademicProfileAsync");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/StudentAdministrationPage.razor"),
            "ADM-04-E2E: SPEC-008/T082 must deliver StudentAdministrationPage.razor.");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class StudentAdministrationPageFeatureTests(Spec008BrowserFixture fixture)
{
    private const string SearchPath = "**/api/admin/students?*";
    private const string DetailPath =
        "**/api/admin/students/student-001/academic-context?*";
    private const string CorrectionPath =
        "**/api/admin/students/student-001/academic-profile";

    [Fact]
    public async Task Adm04_read_shows_only_the_selected_sourced_profile_and_versions()
    {
        await using var context = await OpenAdminContextAsync();
        var page = await context.NewPageAsync();
        await RouteShellAsync(page);
        await RouteReadAsync(page);

        await SearchAndOpenStudentAsync(page);

        await AssertRegionShowsExactTextAsync(page, "academic-summary", "3.42");
        await AssertRegionShowsExactTextAsync(page, "academic-transcript", "AIC401");
        await AssertRegionShowsExactTextAsync(page, "academic-holds", "ADVISING");
        await AssertRegionShowsExactTextAsync(page, "academic-provenance", "Synthetic SIS");
        Assert.Contains(
            "2026-07-14",
            await page.Locator("[data-testid='academic-summary']").InnerTextAsync(),
            StringComparison.Ordinal);
        await AssertRegionShowsExactTextAsync(page, "academic-summary", "student-rv-7");
        Assert.Equal(0, await page.Locator("[contenteditable='true']").CountAsync());
        Assert.Equal(0, await page.Locator("input[type='number']").CountAsync());
    }

    [Fact]
    public async Task Adm04_paged_search_preserves_total_count_and_round_trips_next_previous()
    {
        await using var context = await OpenAdminContextAsync();
        var page = await context.NewPageAsync();
        await RouteShellAsync(page);
        var requestedPages = new List<int>();
        await page.RouteAsync(SearchPath, route =>
        {
            var requestedPage = QueryInteger(route.Request.Url, "page");
            requestedPages.Add(requestedPage);
            return JsonAsync(
                route,
                200,
                requestedPage == 2 ? PagedSearchPageTwo : PagedSearchPageOne);
        });

        await GoToPageAsync(page);
        await page.Locator("[data-testid='student-search-input']").FillAsync("2026");
        await page.GetByRole(AriaRole.Button, new() { Name = "Search Students", Exact = true })
            .ClickAsync();

        var pagination = page.Locator("[data-testid='student-search-pagination']");
        await pagination.WaitForAsync();
        Assert.Contains("Page 1 of 2", await pagination.InnerTextAsync(), StringComparison.Ordinal);
        Assert.Contains("21 total results", await pagination.InnerTextAsync(), StringComparison.Ordinal);
        await AssertRegionShowsExactTextAsync(page, "student-search-results", "20260001");

        await page.GetByRole(AriaRole.Button, new() { Name = "Next", Exact = true })
            .ClickAsync();
        await AssertRegionShowsExactTextAsync(page, "student-search-results", "20260021");
        Assert.Contains("Page 2 of 2", await pagination.InnerTextAsync(), StringComparison.Ordinal);
        Assert.Contains("21 total results", await pagination.InnerTextAsync(), StringComparison.Ordinal);

        await page.GetByRole(AriaRole.Button, new() { Name = "Previous", Exact = true })
            .ClickAsync();
        await AssertRegionShowsExactTextAsync(page, "student-search-results", "20260001");
        Assert.Contains("Page 1 of 2", await pagination.InnerTextAsync(), StringComparison.Ordinal);
        Assert.Equal([1, 2, 1], requestedPages);
    }

    [Fact]
    public async Task Adm04_no_results_preserves_the_query_and_offers_a_clear_action()
    {
        await using var context = await OpenAdminContextAsync();
        var page = await context.NewPageAsync();
        await RouteShellAsync(page);
        await page.RouteAsync(SearchPath, route => JsonAsync(route, 200, EmptySearch));

        await GoToPageAsync(page);
        await page.Locator("[data-testid='student-search-input']").FillAsync("20999999");
        await page.GetByRole(AriaRole.Button, new() { Name = "Search Students", Exact = true })
            .ClickAsync();
        await page.Locator("[data-testid='ADM-04-COMP-STATE-EMPTY']").WaitForAsync();

        Assert.True(await page.GetByText("No matching Students", new() { Exact = true })
            .IsVisibleAsync());
        Assert.Equal(
            "20999999",
            await page.Locator("[data-testid='student-search-input']").InputValueAsync());
        Assert.True(await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Clear Student search", Exact = true })
            .IsVisibleAsync());
        Assert.DoesNotContain("20260001", await page.ContentAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Adm04_invalid_correction_links_the_server_reason_and_does_not_claim_commit()
    {
        await using var context = await OpenAdminContextAsync();
        var page = await context.NewPageAsync();
        await RouteShellAsync(page);
        await RouteReadAsync(page);
        var patches = 0;
        await page.RouteAsync(CorrectionPath, async route =>
        {
            patches++;
            await JsonAsync(
                route,
                400,
                """
                {
                  "code": "VALIDATION_ERROR",
                  "message": "The academic correction is invalid.",
                  "correlationId": "ADM04-INVALID-REF",
                  "fieldErrors": {
                    "operations[0].currentGpa": ["The sourced GPA was rejected by the server."]
                  }
                }
                """);
        });

        await SearchAndOpenStudentAsync(page);
        await OpenCorrectionAsync(page);
        await page.Locator("#academic-correction-reason")
            .FillAsync("Correct sourced GPA from registrar record");
        await page.Locator("[data-testid='academic-correction-value']").FillAsync("3.75");
        await SubmitCorrectionAsync(page);

        var summary = page.Locator("[data-testid='correction-validation-summary']");
        await summary.WaitForAsync();
        Assert.Equal(1, patches);
        Assert.True(await page.GetByText("VALIDATION_ERROR", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText(
                "The sourced GPA was rejected by the server.",
                new() { Exact = true })
            .IsVisibleAsync());
        Assert.Contains(
            "academic-correction-value-error",
            await page.Locator("[data-testid='academic-correction-value']")
                .GetAttributeAsync("aria-describedby") ?? string.Empty,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Correction saved", await page.ContentAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Adm04_stale_submits_xsrf_and_both_versions_then_requires_refresh_review()
    {
        await using var context = await OpenAdminContextAsync();
        var page = await context.NewPageAsync();
        await RouteShellAsync(page);
        await RouteReadAsync(page);
        var patches = 0;
        await page.RouteAsync(CorrectionPath, async route =>
        {
            patches++;
            var headers = await route.Request.AllHeadersAsync();
            Assert.Equal("adm04-xsrf", headers["x-xsrf-token"]);
            using var body = JsonDocument.Parse(route.Request.PostData!);
            Assert.Equal(
                "term-2026-fall",
                body.RootElement.GetProperty("termId").GetString());
            Assert.Equal(
                "student-rv-7",
                body.RootElement.GetProperty("expectedStudentRowVersion").GetString());
            Assert.Equal(
                "term-state-rv-4",
                body.RootElement.GetProperty("expectedStudentTermStateRowVersion").GetString());
            Assert.Equal(
                "Correct sourced GPA from registrar record",
                body.RootElement.GetProperty("reason").GetString());
            Assert.Equal(
                "Synthetic SIS",
                body.RootElement.GetProperty("source").GetString());
            var operations = body.RootElement.GetProperty("operations");
            Assert.Equal(1, operations.GetArrayLength());
            var operation = operations[0];
            Assert.Equal("set-gpa", operation.GetProperty("kind").GetString());
            Assert.Equal(3.50m, operation.GetProperty("currentGpa").GetDecimal());
            Assert.Equal(
                "SIS-20260001",
                operation.GetProperty("sourceReference").GetString());
            await JsonAsync(
                route,
                409,
                """
                {
                  "code": "STALE_VERSION",
                  "message": "The academic profile changed.",
                  "correlationId": "ADM04-STALE-REF",
                  "currentVersion": "student-rv-8"
                }
                """);
        });

        await SearchAndOpenStudentAsync(page);
        await OpenCorrectionAsync(page);
        await page.Locator("#academic-correction-reason")
            .FillAsync("  Correct sourced GPA from registrar record  ");
        await page.Locator("[data-testid='academic-correction-value']").FillAsync("3.50");
        await SubmitCorrectionAsync(page);
        await page.Locator("[data-testid='ADM-04-COMP-STATE-STALE']").WaitForAsync();

        Assert.Equal(1, patches);
        Assert.True(await page.GetByText("STALE_VERSION", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("student-rv-8", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Refresh Student context", Exact = true })
            .IsVisibleAsync());
        Assert.DoesNotContain("Correction saved", await page.ContentAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Adm04_reason_required_is_client_linked_and_blank_reason_never_reaches_patch()
    {
        await using var context = await OpenAdminContextAsync();
        var page = await context.NewPageAsync();
        await RouteShellAsync(page);
        await RouteReadAsync(page);
        var patches = 0;
        await page.RouteAsync(CorrectionPath, route =>
        {
            patches++;
            return JsonAsync(route, 500, "{}", "application/json");
        });

        await SearchAndOpenStudentAsync(page);
        await OpenCorrectionAsync(page);
        await page.Locator("[data-testid='academic-correction-value']").FillAsync("3.50");
        await SubmitCorrectionAsync(page);

        Assert.Equal(0, patches);
        Assert.True(await page.GetByText(
                "Enter a reason of at least 10 characters.",
                new() { Exact = true })
            .IsVisibleAsync());
        Assert.Contains(
            "academic-correction-reason-error",
            await page.Locator("#academic-correction-reason")
                .GetAttributeAsync("aria-describedby") ?? string.Empty,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task Adm04_profile_not_ready_detail_is_validation_not_service_failure()
    {
        await using var context = await OpenAdminContextAsync();
        var page = await context.NewPageAsync();
        await RouteShellAsync(page);
        await page.RouteAsync(SearchPath, route => JsonAsync(route, 200, SearchResults));
        await page.RouteAsync(DetailPath, route => JsonAsync(
            route,
            409,
            """
            {
              "code": "PROFILE_NOT_READY",
              "message": "The complete sourced academic profile is not ready.",
              "correlationId": "ADM04-PROFILE-NOT-READY-REF"
            }
            """));

        await GoToPageAsync(page);
        await page.Locator("[data-testid='student-search-input']").FillAsync("20260001");
        await page.GetByRole(AriaRole.Button, new() { Name = "Search Students", Exact = true })
            .ClickAsync();
        await ClickVisibleButtonAsync(page, "Open academic record");

        var validation = page.Locator("[data-testid='ADM-04-COMP-STATE-VALIDATION-ERROR']");
        await validation.WaitForAsync();
        await AssertRegionShowsExactTextAsync(
            page,
            "ADM-04-COMP-STATE-VALIDATION-ERROR",
            "PROFILE_NOT_READY");
        Assert.Contains(
            "The complete sourced academic profile is not ready.",
            await validation.InnerTextAsync(),
            StringComparison.Ordinal);
        Assert.Equal(
            0,
            await page.Locator("[data-testid='ADM-04-COMP-STATE-SERVICE-ERROR']").CountAsync());
        Assert.Equal(0, await page.Locator("[data-testid='academic-summary']").CountAsync());
    }

    [Fact]
    public async Task Adm04_restricted_purges_prior_profile_and_exposes_only_safe_home_navigation()
    {
        await using var context = await OpenAdminContextAsync();
        var page = await context.NewPageAsync();
        await RouteShellAsync(page);
        await RouteReadAsync(page);
        await SearchAndOpenStudentAsync(page);
        Assert.Contains("20260001", await page.ContentAsync(), StringComparison.Ordinal);

        await page.UnrouteAsync(SearchPath);
        await page.RouteAsync(SearchPath, route => JsonAsync(
            route,
            403,
            """
            {
              "code": "FORBIDDEN",
              "message": "Access to Student academic records is restricted.",
              "correlationId": "ADM04-SAFE-REF"
            }
            """));
        await page.Locator("[data-testid='student-search-input']").FillAsync("20260002");
        await page.GetByRole(AriaRole.Button, new() { Name = "Search Students", Exact = true })
            .ClickAsync();
        await page.Locator("[data-testid='ADM-04-COMP-STATE-UNAUTHORIZED']")
            .WaitForAsync();

        var content = await page.ContentAsync();
        Assert.Contains("FORBIDDEN", content, StringComparison.Ordinal);
        Assert.DoesNotContain("student-001", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("20260001", content, StringComparison.Ordinal);
        Assert.DoesNotContain("3.42", content, StringComparison.Ordinal);
        Assert.True(await page.GetByRole(
                AriaRole.Link,
                new() { Name = "Return to Admin home", Exact = true })
            .IsVisibleAsync());
    }

    private async Task<IBrowserContext> OpenAdminContextAsync()
    {
        var context = await fixture.OpenContextAsync();
        await context.AddCookiesAsync(
        [
            new Cookie
            {
                Name = "XSRF-TOKEN",
                Value = "adm04-xsrf",
                Url = fixture.BaseAddress.AbsoluteUri
            }
        ]);
        return context;
    }

    private static async Task RouteShellAsync(IPage page) =>
        await page.RouteAsync("**/api/context", route => JsonAsync(route, 200, AppContext));

    private static async Task RouteReadAsync(IPage page)
    {
        await page.RouteAsync(SearchPath, route => JsonAsync(route, 200, SearchResults));
        await page.RouteAsync(DetailPath, route => JsonAsync(route, 200, AcademicDetail));
    }

    private static async Task SearchAndOpenStudentAsync(IPage page)
    {
        await GoToPageAsync(page);
        await page.Locator("[data-testid='student-search-input']").FillAsync("20260001");
        await page.GetByRole(AriaRole.Button, new() { Name = "Search Students", Exact = true })
            .ClickAsync();
        await ClickVisibleButtonAsync(page, "Open academic record");
        await page.Locator("[data-testid='ADM-04-COMP-STATE-SUCCESS']").WaitForAsync();
    }

    private static async Task ClickVisibleButtonAsync(IPage page, string accessibleName)
    {
        var buttons = page.GetByRole(
            AriaRole.Button,
            new() { Name = accessibleName, Exact = true });
        for (var attempt = 0; attempt < 100; attempt++)
        {
            for (var index = 0; index < await buttons.CountAsync(); index++)
            {
                var button = buttons.Nth(index);
                if (await button.IsVisibleAsync())
                {
                    await button.ClickAsync();
                    return;
                }
            }

            await Task.Delay(50);
        }

        throw new Xunit.Sdk.XunitException(
            $"No visible '{accessibleName}' button was rendered.");
    }

    private static async Task AssertRegionShowsExactTextAsync(
        IPage page,
        string regionTestId,
        string expectedText)
    {
        var matches = page
            .Locator($"[data-testid='{regionTestId}']")
            .GetByText(expectedText, new() { Exact = true });
        for (var attempt = 0; attempt < 100; attempt++)
        {
            var visibleMatches = 0;
            for (var index = 0; index < await matches.CountAsync(); index++)
            {
                if (await matches.Nth(index).IsVisibleAsync())
                {
                    visibleMatches++;
                }
            }

            if (visibleMatches == 1)
            {
                return;
            }

            if (visibleMatches > 1)
            {
                throw new Xunit.Sdk.XunitException(
                    $"'{expectedText}' had {visibleMatches} visible matches inside {regionTestId}.");
            }

            await Task.Delay(50);
        }

        throw new Xunit.Sdk.XunitException(
            $"'{expectedText}' was not visible inside {regionTestId}.");
    }

    private static int QueryInteger(string requestUrl, string name)
    {
        var query = new Uri(requestUrl).Query.TrimStart('?').Split(
            '&',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var item in query)
        {
            var parts = item.Split('=', 2);
            if (parts.Length == 2 && string.Equals(parts[0], name, StringComparison.Ordinal))
            {
                return int.Parse(
                    Uri.UnescapeDataString(parts[1]),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture);
            }
        }

        throw new Xunit.Sdk.XunitException($"Query parameter '{name}' was not supplied.");
    }

    private static async Task GoToPageAsync(IPage page)
    {
        page.SetDefaultTimeout(5_000);
        await page.GotoAsync("/admin/students", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Student administration", Exact = true })
            .WaitForAsync();
    }

    private static async Task OpenCorrectionAsync(IPage page)
    {
        await page.Locator("[data-testid='open-academic-correction']").ClickAsync();
        await page.GetByRole(
                AriaRole.Dialog,
                new() { Name = "Academic correction", Exact = true })
            .WaitForAsync();
    }

    private static async Task SubmitCorrectionAsync(IPage page) =>
        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Submit academic correction", Exact = true })
            .ClickAsync();

    private static Task JsonAsync(
        IRoute route,
        int status,
        string body,
        string contentType = "application/json") => route.FulfillAsync(
        new RouteFulfillOptions
        {
            Status = status,
            ContentType = contentType,
            Body = body
        });

    private const string AppContext = """
        {
          "serverTimeUtc": "2026-07-14T10:15:00Z",
          "timeZoneId": "Africa/Cairo",
          "teachingTerm": null,
          "registrationTerm": {
            "id": "term-2026-fall",
            "code": "2026-FALL",
            "label": "Fall 2026",
            "state": "registrationOpen",
            "rowVersion": "term-rv-4"
          },
          "registrationWindowState": "none",
          "registrationWindow": null,
          "serviceState": "available",
          "displayName": "Ahmed Admin",
          "authorizedRoles": ["Admin"],
          "activeRole": "Admin",
          "sessionState": "active",
          "expiresAtUtc": "2026-07-14T18:00:00Z",
          "supportReferencePath": "/support"
        }
        """;

    private const string SearchResults = """
        {
          "items": [{
            "studentId": "student-001",
            "universityId": "20260001",
            "programCode": "AI",
            "cohort": "2026",
            "standing": "Good standing",
            "dataVersion": "profile-v7"
          }],
          "page": 1,
          "pageSize": 20,
          "totalCount": 1,
          "sort": "universityId,studentId"
        }
        """;

    private const string EmptySearch = """
        {
          "items": [],
          "page": 1,
          "pageSize": 20,
          "totalCount": 0,
          "sort": "universityId,studentId"
        }
        """;

    private const string PagedSearchPageOne = """
        {
          "items": [{
            "studentId": "student-001",
            "universityId": "20260001",
            "programCode": "AI",
            "cohort": "2026",
            "standing": "Good standing",
            "dataVersion": "profile-v7"
          }],
          "page": 1,
          "pageSize": 20,
          "totalCount": 21,
          "sort": "universityId,studentId"
        }
        """;

    private const string PagedSearchPageTwo = """
        {
          "items": [{
            "studentId": "student-021",
            "universityId": "20260021",
            "programCode": "AI",
            "cohort": "2026",
            "standing": "Good standing",
            "dataVersion": "profile-v3"
          }],
          "page": 2,
          "pageSize": 20,
          "totalCount": 21,
          "sort": "universityId,studentId"
        }
        """;

    private const string AcademicDetail = """
        {
          "studentId": "student-001",
          "termId": "term-2026-fall",
          "universityId": "20260001",
          "programCode": "AI",
          "cohort": "2026",
          "currentGpa": 3.42,
          "earnedCredits": 84,
          "standing": "Good standing",
          "transcriptSummary": {
            "attemptedCredits": 87,
            "earnedCredits": 84,
            "attemptCount": 29
          },
          "transcriptAttempts": {
            "items": [{
              "attemptId": "attempt-401",
              "supersedesAttemptId": null,
              "courseCode": "AIC401",
              "termCode": "2026-Spring",
              "credits": 3,
              "grade": "A",
              "status": "passed",
              "provenance": "Synthetic SIS"
            }],
            "page": 1,
            "pageSize": 20,
            "totalCount": 1,
            "sort": "termCode,courseCode,attemptId"
          },
          "activeHolds": [{
            "termId": "term-2026-fall",
            "code": "ADVISING",
            "message": "Meet the academic adviser.",
            "blocksRegistration": false,
            "effectiveFromUtc": "2026-07-01T00:00:00Z",
            "effectiveToUtc": null,
            "source": "Synthetic SIS",
            "holdId": "hold-001",
            "sourceReference": "SIS-H-001"
          }],
          "dataVersion": "profile-v7",
          "dataAsOfUtc": "2026-07-14T09:45:00Z",
          "provenance": {
            "items": [{
              "source": "Synthetic SIS",
              "reference": "SIS-20260001",
              "importedAtUtc": "2026-07-14T09:45:00Z"
            }],
            "page": 1,
            "pageSize": 20,
            "totalCount": 1,
            "sort": "importedAtUtc,reference"
          },
          "studentRowVersion": "student-rv-7",
          "studentTermStateRowVersion": "term-state-rv-4"
        }
        """;
}
