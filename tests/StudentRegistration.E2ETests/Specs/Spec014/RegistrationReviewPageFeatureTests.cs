using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec014;

public sealed class RegistrationReviewPageFrozenContractTests
{
    [Fact]
    public void Stu_05_owner_contract_and_required_journeys_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-05.md");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/student/review\"",
            "\"pageName\": \"RegistrationReviewPage.razor\"",
            "STU-05-valid-v1",
            "STU-05-hard-conflict-v1",
            "STU-05-policy-changed-v1",
            "STU-05-window-closed-v1",
            "STU-05-double-submit-v1",
            "STU-05-E2E-PRIMARY",
            "STU-05-E2E-FAILURE");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/RegistrationReviewPage.razor"),
            "SPEC-014/T105 must deliver the sole canonical STU-05 Razor page.");
    }

    [Fact]
    public void Stu_05_server_boundary_requires_exact_permission_antiforgery_and_private_lookup()
    {
        var endpoint = RepositoryFiles.Read(
            "src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs");

        RepositoryFiles.ContainsAll(
            endpoint,
            "Student",
            "Registration.SubmitOwn",
            "RequireAuthorization",
            "RequireAntiforgeryTokenAttribute",
            "/api/student/terms/{termId}/registrations",
            "/api/student/terms/{termId}/registrations/by-request/{clientRequestId}",
            "REQUEST_NOT_FOUND");

        var client = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Registration/RegistrationApiClient.cs");
        RepositoryFiles.ContainsAll(
            client,
            "SubmitRegistrationRequest(",
            "Guid PlanId",
            "string ExpectedPlanRowVersion",
            "Guid ClientRequestId");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class RegistrationReviewPageFeatureTests(Spec008BrowserFixture fixture)
{
    private const string TermId = "00000000-0000-0000-0000-000000014001";
    private const string PlanId = "00000000-0000-0000-0000-000000014002";
    private const string GroupId = "00000000-0000-0000-0000-000000014003";
    private const string SubmissionId = "00000000-0000-0000-0000-000000014004";
    private const string OtherStudentId = "00000000-0000-0000-0000-000000014005";
    private const string OtherTermId = "00000000-0000-0000-0000-000000014006";

    [Fact]
    public async Task Stu_05_valid_v1_validates_then_posts_once_with_xsrf_and_no_identity_fields()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        string? payload = null;
        string? xsrf = null;
        var posts = 0;

        await page.RouteAsync("**/api/**", async route =>
        {
            if (await TryContextOrPlanAsync(route)) return;
            if (route.Request.Method == "POST" && Path(route).EndsWith("/registration-plan/validate"))
            {
                await JsonAsync(route, 200, Plan());
                return;
            }
            if (route.Request.Method == "POST" && Path(route).EndsWith("/registrations"))
            {
                posts++;
                payload = route.Request.PostData;
                var headers = await route.Request.AllHeadersAsync();
                headers.TryGetValue("x-xsrf-token", out xsrf);
                await JsonAsync(route, 201, FinalResult("accepted", "REGISTERED"));
                return;
            }
            await route.AbortAsync();
        });

        await OpenAsync(page, "success");
        await page.GetByRole(AriaRole.Button, new() { Name = "Review and submit", Exact = true })
            .ClickAsync();
        await page.GetByRole(AriaRole.Dialog).WaitForAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Confirm registration", Exact = true })
            .ClickAsync();
        await page.WaitForFunctionAsync(
            "expected => window.location.pathname === expected",
            $"/student/registration/result/{SubmissionId}");

        Assert.Equal(1, posts);
        Assert.Equal("stu05-xsrf", xsrf);
        Assert.NotNull(payload);
        using var body = JsonDocument.Parse(payload);
        Assert.Equal(PlanId, body.RootElement.GetProperty("planId").GetGuid().ToString());
        Assert.Equal("PLAN-RV-1", body.RootElement.GetProperty("expectedPlanRowVersion").GetString());
        Assert.True(body.RootElement.GetProperty("clientRequestId").GetGuid() != Guid.Empty);
        Assert.False(body.RootElement.TryGetProperty("studentId", out _));
        Assert.False(body.RootElement.TryGetProperty("termId", out _));
    }

    [Fact]
    public async Task Stu_05_hard_conflict_v1_lists_every_blocker_and_never_posts()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        var posts = 0;
        await page.RouteAsync("**/api/**", async route =>
        {
            if (Path(route) == "/api/context") await JsonAsync(route, 200, AppContext("Student"));
            else if (route.Request.Method == "GET") await JsonAsync(route, 200, Plan(blocked: true));
            else { posts++; await route.AbortAsync(); }
        });

        await OpenAsync(page, "validation-error");
        var blockers = page.Locator("#registration-blockers");
        Assert.True(await blockers.GetByText("MEETING_OVERLAP", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await blockers.GetByRole(AriaRole.Heading, new() { Name = "Conflict", Exact = true }).IsVisibleAsync());
        Assert.True(await blockers.GetByText("Monday 11:00–11:30", new() { Exact = false }).IsVisibleAsync());
        Assert.True(await page.GetByRole(AriaRole.Button, new() { Name = "Review and submit", Exact = true })
            .IsDisabledAsync());
        Assert.Equal(0, posts);
    }

    [Theory]
    [InlineData("POLICY_CHANGED", "Registration context changed")]
    [InlineData("WINDOW_CLOSED", "Registration context changed")]
    [InlineData("PLAN_CHANGED", "Registration context changed")]
    [InlineData("GROUP_FULL", "Registration could not be accepted")]
    [InlineData("STALE_VERSION", "Registration context changed")]
    public async Task Stu_05_preserves_server_reason_and_requires_review(
        string reason,
        string heading)
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", async route =>
        {
            if (await TryContextOrPlanAsync(route)) return;
            if (Path(route).EndsWith("/registration-plan/validate"))
            {
                await JsonAsync(route, 200, Plan());
                return;
            }
            await JsonAsync(route, 409, Error(reason));
        });

        await OpenAsync(page, "success");
        await page.GetByRole(AriaRole.Button, new() { Name = "Review and submit", Exact = true })
            .ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Confirm registration", Exact = true })
            .ClickAsync();
        await page.GetByRole(AriaRole.Heading, new() { Name = heading, Exact = true }).WaitForAsync();
        Assert.True(await page.GetByText(reason, new() { Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await page.GetByRole(AriaRole.Dialog).CountAsync());
    }

    [Theory]
    [InlineData(null, 401)]
    [InlineData("Admin", 200)]
    [InlineData("Student", 403)]
    public async Task Stu_05_missing_session_wrong_role_or_missing_permission_discloses_no_plan(
        string? role,
        int status)
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", async route =>
        {
            if (Path(route) == "/api/context")
            {
                if (status == 401) await JsonAsync(route, 401, Error("UNAUTHORIZED"));
                else await JsonAsync(route, 200, AppContext(role!));
                return;
            }

            await JsonAsync(route, status, status == 403 ? Error("FORBIDDEN") : Plan());
        });

        await OpenAsync(page, "unauthorized");
        Assert.True(await page.GetByRole(AriaRole.Heading, new() { Name = "Access denied", Exact = true })
            .IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("Artificial Intelligence", new() { Exact = true }).CountAsync());
        Assert.Equal(0, await page.Locator($"[data-group-id='{GroupId}']").CountAsync());
    }

    [Fact]
    public async Task Stu_05_double_submit_and_processing_lookup_keep_one_private_request()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        var posts = 0;
        var lookups = new List<string>();
        await page.RouteAsync("**/api/**", async route =>
        {
            if (await TryContextOrPlanAsync(route)) return;
            if (route.Request.Method == "POST" && Path(route).EndsWith("/registration-plan/validate"))
            {
                await JsonAsync(route, 200, Plan());
                return;
            }
            if (route.Request.Method == "POST" && Path(route).EndsWith("/registrations"))
            {
                posts++;
                await Task.Delay(100);
                await JsonAsync(route, 202, Processing());
                return;
            }
            if (route.Request.Method == "GET" && Path(route).Contains("/registrations/by-request/"))
            {
                lookups.Add(Path(route));
                await JsonAsync(route, 404, Error("REQUEST_NOT_FOUND"));
                return;
            }
            await route.AbortAsync();
        });

        await OpenAsync(page, "success");
        await page.GetByRole(AriaRole.Button, new() { Name = "Review and submit", Exact = true }).ClickAsync();
        var confirm = page.GetByRole(AriaRole.Button, new() { Name = "Confirm registration", Exact = true });
        await confirm.DblClickAsync();
        await page.GetByText("Registration is still processing", new() { Exact = true }).WaitForAsync();
        Assert.Equal(1, posts);
        Assert.Equal(0, await page.GetByRole(AriaRole.Dialog).CountAsync());
        Assert.True(await page.GetByRole(AriaRole.Button, new() { Name = "Review and submit", Exact = true })
            .IsDisabledAsync());

        await page.GetByRole(AriaRole.Button, new() { Name = "Retry result lookup", Exact = true }).ClickAsync();
        Assert.Single(lookups);
        Assert.StartsWith($"/api/student/terms/{TermId}/registrations/by-request/", lookups[0]);
        Assert.DoesNotContain("student", lookups[0].Split("/by-request/")[1], StringComparison.OrdinalIgnoreCase);
        await page.GetByText("REQUEST_NOT_FOUND", new() { Exact = true }).WaitForAsync();
        Assert.Equal(0, await page.GetByText(SubmissionId, new() { Exact = true }).CountAsync());
        Assert.Equal(0, await page.GetByText(OtherStudentId, new() { Exact = true }).CountAsync());
        Assert.Equal(0, await page.GetByText(OtherTermId, new() { Exact = true }).CountAsync());
        Assert.Equal(0, await page.GetByText("PLAN-RV-2", new() { Exact = true }).CountAsync());
    }

    private async Task<IBrowserContext> ContextWithXsrfAsync()
    {
        var context = await fixture.OpenContextAsync();
        await context.AddCookiesAsync(
        [
            new Cookie
            {
                Name = "XSRF-TOKEN",
                Value = "stu05-xsrf",
                Url = fixture.BaseAddress.AbsoluteUri
            }
        ]);
        return context;
    }

    private static async Task<bool> TryContextOrPlanAsync(IRoute route)
    {
        if (Path(route) == "/api/context")
        {
            await JsonAsync(route, 200, AppContext("Student"));
            return true;
        }
        if (route.Request.Method == "GET" && Path(route).EndsWith("/registration-plan"))
        {
            await JsonAsync(route, 200, Plan());
            return true;
        }
        return false;
    }

    private static async Task OpenAsync(IPage page, string state)
    {
        await page.GotoAsync("/student/review", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.GetByRole(AriaRole.Heading, new() { Name = "Registration review", Exact = true }).WaitForAsync();
        await page.Locator($"[data-route-id='STU-05'][data-state='{state}']").WaitForAsync();
    }

    private static string Path(IRoute route) => new Uri(route.Request.Url).AbsolutePath;

    private static Task JsonAsync(IRoute route, int status, string body) =>
        route.FulfillAsync(new() { Status = status, ContentType = "application/json", Body = body });

    private static string AppContext(string role) => $$$"""
        {
          "serverTimeUtc":"2026-07-20T07:15:00Z","timeZoneId":"Africa/Cairo",
          "teachingTerm":null,
          "registrationTerm":{"id":"{{{TermId}}}","code":"FALL-2026","label":"Fall 2026","state":"registrationOpen","rowVersion":"TERM-RV-1"},
          "registrationWindowState":"open",
          "registrationWindow":{"id":"00000000-0000-0000-0000-000000014010","state":"open","opensAtUtc":"2026-07-19T06:00:00Z","closesAtUtc":"2026-07-21T18:00:00Z","rowVersion":"WINDOW-RV-1"},
          "serviceState":"available","displayName":"Synthetic Student One",
          "authorizedRoles":["{{{role}}}"],"activeRole":"{{{role}}}","sessionState":"active",
          "expiresAtUtc":"2026-07-20T09:15:00Z","supportReferencePath":"/support/student/STU-05-SAFE-REF"
        }
        """;

    private static string Plan(bool blocked = false) => $$$"""
        {
          "id":"{{{PlanId}}}","termId":"{{{TermId}}}","rowVersion":"PLAN-RV-1",
          "selectedGroups":[{
            "groupId":"{{{GroupId}}}","offeringId":"00000000-0000-0000-0000-000000014020",
            "courseCode":"AI401","subjectTitle":"Artificial Intelligence","groupCode":"G01","credits":3,
            "capacity":30,"enrolledCount":12,"rowVersion":"GROUP-RV-1",
            "meetings":[{"meetingId":"00000000-0000-0000-0000-000000014030","activityKind":"Lecture","dayOfWeek":1,"startLocal":"10:00:00","endLocal":"11:30:00","roomCode":"A-101","location":"Smart Village","lecturerName":"Dr. Salma","teachingAssistantNames":[],"timezone":"Africa/Cairo"}]
          }],
          "totalCredits":3,"defaultTargetCredits":18,"maximumAllowedCredits":18,"loadReasons":[],
          "selectionIssues":{{{Issues(blocked)}}},
          "conflicts":[],
          "validation":{"evaluatedAtUtc":"2026-07-20T07:15:00Z","academicContextVersion":"ACADEMIC-V1","policyVersion":"DEMO-POC-2026.1","catalogueVersion":"CATALOGUE-V1","offeringVersions":{},"groupVersions":{}},
          "reviewBlocked":{{{blocked.ToString().ToLowerInvariant()}}}
        }
        """;

    private static string Issues(bool blocked) => blocked
        ? "[{\"code\":\"MEETING_OVERLAP\",\"offeringId\":\"00000000-0000-0000-0000-000000014020\",\"groupId\":\"00000000-0000-0000-0000-000000014003\",\"groupCode\":\"G01\",\"message\":\"AI401 G01 overlaps CS402 G02 on Monday 11:00–11:30.\",\"blocking\":true,\"actions\":[]}]"
        : "[]";

    private static string FinalResult(string status, string code) => $$$"""
        {"submissionId":"{{{SubmissionId}}}","status":"{{{status}}}","resultCode":"{{{code}}}","registeredGroups":[],"receivedAtUtc":"2026-07-20T07:15:00Z","completedAtUtc":"2026-07-20T07:15:01Z","policySetId":"00000000-0000-0000-0000-000000014090","policyVersion":"DEMO-POC-2026.1","planRowVersion":"PLAN-RV-1","reference":"REG-014-0001"}
        """;

    private static string Processing() =>
        "{\"clientRequestId\":\"00000000-0000-0000-0000-000000014099\",\"status\":\"processing\",\"retryAfterSeconds\":1,\"resultUrl\":\"/api/student/terms/" + TermId + "/registrations/by-request/00000000-0000-0000-0000-000000014099\"}";

    private static string Error(string code) => $$$"""
        {"code":"{{{code}}}","message":"The registration request could not be completed safely.","correlationId":"STU-05-SAFE-REF","studentId":"{{{OtherStudentId}}}","termId":"{{{OtherTermId}}}","submissionId":"{{{SubmissionId}}}","currentVersion":"PLAN-RV-2"}
        """;
}
