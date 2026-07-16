using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec012;

public sealed class ScheduleBuilderPageFrozenContractTests
{
    [Fact]
    public void Stu_04_owner_page_and_api_bindings_are_present()
    {
        const string path =
            "src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor";

        Assert.True(
            RepositoryFiles.Exists(path),
            "SPEC-012/T043 must deliver the STU-04 owner page after this T042 test fails.");
        var page = RepositoryFiles.Read(path);
        RepositoryFiles.ContainsAll(
            page,
            "@page \"/student/schedule\"",
            "GetRegistrationPlanAsync",
            "ReplaceRegistrationPlanAsync",
            "ScheduleCalendar",
            "ScheduleList",
            "ConflictPanel",
            "DefaultTargetCredits",
            "MaximumAllowedCredits",
            "STU-04-COMP-STATE-LOADING",
            "STU-04-COMP-STATE-EMPTY",
            "STU-04-COMP-STATE-SERVICE-ERROR",
            "STU-04-COMP-STATE-UNAUTHORIZED",
            "STU-04-COMP-STATE-SESSION-EXPIRED",
            "STU-04-COMP-STATE-STALE",
            "STU-04-COMP-STATE-OFFLINE");
        Assert.DoesNotContain("Gpa", page, StringComparison.OrdinalIgnoreCase);
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class ScheduleBuilderPageFeatureTests(Spec008BrowserFixture fixture)
{
    private const string TermId = "00000000-0000-0000-0000-000000012001";
    private const string OfferingId = "00000000-0000-0000-0000-000000012101";
    private const string GroupAId = "00000000-0000-0000-0000-000000012201";
    private const string GroupBId = "00000000-0000-0000-0000-000000012202";

    [Fact]
    public async Task Stu_04_loading_then_success_uses_server_18_18_and_equivalent_views()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteContextAsync(page);
        var requestStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        await page.RouteAsync(PlanPath, async route =>
        {
            requestStarted.TrySetResult();
            await release.Task;
            await route.FulfillAsync(Json(200, ConflictPlan()));
        });

        var navigation = page.GotoAsync("/student/schedule", new()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await requestStarted.Task;
        try
        {
            await page.Locator("[data-route-id='STU-04'][data-state='loading']")
                .WaitForAsync();
            Assert.True(await page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Loading schedule builder", Exact = true })
                .IsVisibleAsync());
        }
        finally
        {
            release.TrySetResult();
        }

        await navigation;
        await WaitForStateAsync(page, "validation-error");

        Assert.Equal("18 credits", await page
            .Locator("[data-load-field='default-target'] dd").TextContentAsync());
        Assert.Equal("18 credits", await page
            .Locator("[data-load-field='maximum-allowed'] dd").TextContentAsync());
        Assert.True(await page.GetByText("NORMAL_MAX_CREDITS", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText(
            "The fixed demo maximum is 18 credits.",
            new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.Locator("[data-load-reason='NORMAL_MAX_CREDITS']")
            .GetByText("DEMO-POC-2026.1", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("policy/demo-poc", new() { Exact = true })
            .IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("GPA", new() { Exact = false }).CountAsync());
        Assert.True(await page.GetByText("Conflict", new() { Exact = true }).First
            .IsVisibleAsync());
        Assert.True(await page.Locator("[data-conflict-icon]").First.IsVisibleAsync());
        Assert.True(await page.GetByText("11:00", new() { Exact = true }).First
            .IsVisibleAsync());
        Assert.True(await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Continue to review", Exact = true }).IsDisabledAsync());

        var calendarMeetings = await page
            .Locator(".srs-schedule-calendar [data-meeting-id]")
            .EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.meetingId)");
        var listMeetings = await page
            .Locator(".srs-schedule-list [data-meeting-id]")
            .EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.meetingId)");
        Assert.Equal(calendarMeetings, listMeetings);
        Assert.Equal(2, calendarMeetings.Length);
    }

    [Fact]
    public async Task Stu_04_query_changes_group_then_remove_replaces_the_complete_plan()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteContextAsync(page);
        var putBodies = new List<string>();
        await page.RouteAsync(PlanPath, async route =>
        {
            if (route.Request.Method == "GET")
            {
                await route.FulfillAsync(Json(200, CurrentPlan(GroupAId, "G01", "PLAN-RV-1")));
                return;
            }

            putBodies.Add(route.Request.PostData ?? string.Empty);
            var body = JsonDocument.Parse(route.Request.PostData!);
            var selected = body.RootElement.GetProperty("selectedGroupIds")
                .EnumerateArray()
                .Select(value => value.GetString())
                .ToArray();
            var response = selected.Length == 0
                ? EmptyPlan("PLAN-RV-3")
                : CurrentPlan(GroupBId, "G02", "PLAN-RV-2");
            await route.FulfillAsync(Json(200, response));
        });

        await OpenAsync(
            page,
            $"/student/schedule?offeringId={OfferingId}&groupId={GroupBId}",
            "success");

        Assert.Single(putBodies);
        Assert.Contains(GroupBId, putBodies[0], StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(GroupAId, putBodies[0], StringComparison.OrdinalIgnoreCase);
        Assert.True(await page.GetByText("G02", new() { Exact = true }).First
            .IsVisibleAsync());

        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Remove AI401 group G02", Exact = true })
            .ClickAsync();
        await WaitForStateAsync(page, "empty");

        Assert.Equal(2, putBodies.Count);
        Assert.Contains("\"selectedGroupIds\":[]", Compact(putBodies[1]), StringComparison.Ordinal);
        Assert.True(await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Your plan is empty", Exact = true }).IsVisibleAsync());
    }

    [Fact]
    public async Task Stu_04_stale_write_keeps_the_authorized_current_plan()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteContextAsync(page);
        await page.RouteAsync(PlanPath, route => route.Request.Method == "GET"
            ? route.FulfillAsync(Json(200, CurrentPlan(GroupAId, "G01", "PLAN-RV-1")))
            : route.FulfillAsync(Json(409, StalePlan())));

        await OpenAsync(page, "/student/schedule", "success");
        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Remove AI401 group G01", Exact = true })
            .ClickAsync();
        await WaitForStateAsync(page, "stale");

        Assert.True(await page.GetByText("STALE_VERSION", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByText("G02", new() { Exact = true }).First
            .IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("G01", new() { Exact = true }).CountAsync());
    }

    [Theory]
    [InlineData(200, "", "empty", "Your plan is empty")]
    [InlineData(403, "FORBIDDEN", "unauthorized", "Access denied")]
    [InlineData(401, "SESSION_EXPIRED", "session-expired", "Session expired")]
    [InlineData(503, "REGISTRATION_PLAN_UNAVAILABLE", "service-error", "Schedule builder is unavailable")]
    public async Task Stu_04_declared_empty_and_safe_failure_states_render(
        int status,
        string code,
        string expectedState,
        string heading)
    {
        await using var context = await fixture.OpenContextAsync(375, 1000);
        var page = await context.NewPageAsync();
        await RouteContextAsync(page);
        await page.RouteAsync(PlanPath, route => status == 200
            ? route.FulfillAsync(Json(200, EmptyPlan("PLAN-RV-1")))
            : route.FulfillAsync(Json(status, Error(code))));

        await OpenAsync(page, "/student/schedule", expectedState);

        Assert.True(await page.GetByRole(
            AriaRole.Heading,
            new() { Name = heading, Exact = true }).IsVisibleAsync());
    }

    [Fact]
    public async Task Stu_04_network_failure_is_offline_and_never_claims_success()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await RouteContextAsync(page);
        await page.RouteAsync(PlanPath, route => route.AbortAsync("internetdisconnected"));

        await OpenAsync(page, "/student/schedule", "offline");

        Assert.True(await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Schedule builder is offline", Exact = true })
            .IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("Plan ready", new() { Exact = true })
            .CountAsync());
    }

    private static async Task RouteContextAsync(IPage page) =>
        await page.RouteAsync("**/api/context", route =>
            route.FulfillAsync(Json(200, AppContext())));

    private static async Task OpenAsync(
        IPage page,
        string path,
        string expectedState)
    {
        await page.GotoAsync(path, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Schedule builder", Exact = true }).WaitForAsync();
        await WaitForStateAsync(page, expectedState);
    }

    private static Task WaitForStateAsync(IPage page, string state) =>
        page.Locator($"[data-route-id='STU-04'][data-state='{state}']")
            .WaitForAsync();

    private static string Compact(string json) =>
        string.Concat(json.Where(character => !char.IsWhiteSpace(character)));

    private static RouteFulfillOptions Json(int status, string body) => new()
    {
        Status = status,
        ContentType = "application/json",
        Body = body
    };

    private static string AppContext() => $$"""
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
            "id": "00000000-0000-0000-0000-000000012002",
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
          "supportReferencePath": "/support/student/STU-04-SAFE-REF"
        }
        """;

    private static string CurrentPlan(string groupId, string groupCode, string version) => $$"""
        {
          "id": "00000000-0000-0000-0000-000000012010",
          "termId": "{{TermId}}",
          "rowVersion": "{{version}}",
          "selectedGroups": [{{Group(groupId, groupCode)}}],
          "totalCredits": 3,
          "defaultTargetCredits": 18,
          "maximumAllowedCredits": 18,
          "loadReasons": [{{LoadReason()}}],
          "selectionIssues": [],
          "conflicts": [],
          "validation": {{Validation()}},
          "reviewBlocked": false
        }
        """;

    private static string ConflictPlan() => $$"""
        {
          "id": "00000000-0000-0000-0000-000000012010",
          "termId": "{{TermId}}",
          "rowVersion": "PLAN-RV-1",
          "selectedGroups": [
            {{Group(GroupAId, "G01")}},
            {{SecondGroup()}}
          ],
          "totalCredits": 6,
          "defaultTargetCredits": 18,
          "maximumAllowedCredits": 18,
          "loadReasons": [{{LoadReason()}}],
          "selectionIssues": [],
          "conflicts": [{{Conflict()}}],
          "validation": {{Validation()}},
          "reviewBlocked": true
        }
        """;

    private static string EmptyPlan(string version) => $$"""
        {
          "id": "00000000-0000-0000-0000-000000012010",
          "termId": "{{TermId}}",
          "rowVersion": "{{version}}",
          "selectedGroups": [],
          "totalCredits": 0,
          "defaultTargetCredits": 18,
          "maximumAllowedCredits": 18,
          "loadReasons": [],
          "selectionIssues": [],
          "conflicts": [],
          "validation": {{Validation()}},
          "reviewBlocked": false
        }
        """;

    private static string Group(string groupId, string groupCode) => $$"""
        {
          "groupId": "{{groupId}}",
          "offeringId": "{{OfferingId}}",
          "courseCode": "AI401",
          "subjectTitle": "Artificial Intelligence",
          "groupCode": "{{groupCode}}",
          "credits": 3,
          "capacity": 30,
          "enrolledCount": 12,
          "rowVersion": "GROUP-RV-1",
          "meetings": [{
            "meetingId": "00000000-0000-0000-0000-000000012301",
            "activityKind": "Lecture",
            "dayOfWeek": 1,
            "startLocal": "10:00:00",
            "endLocal": "11:30:00",
            "roomCode": "A-101",
            "location": "Smart Village",
            "lecturerName": "Dr. Salma",
            "teachingAssistantNames": [],
            "timezone": "Africa/Cairo"
          }]
        }
        """;

    private static string SecondGroup() => $$"""
        {
          "groupId": "00000000-0000-0000-0000-000000012203",
          "offeringId": "00000000-0000-0000-0000-000000012103",
          "courseCode": "CS402",
          "subjectTitle": "Distributed Systems",
          "groupCode": "G03",
          "credits": 3,
          "capacity": 25,
          "enrolledCount": 10,
          "rowVersion": "GROUP-RV-3",
          "meetings": [{
            "meetingId": "00000000-0000-0000-0000-000000012303",
            "activityKind": "Tutorial",
            "dayOfWeek": 1,
            "startLocal": "11:00:00",
            "endLocal": "12:00:00",
            "roomCode": "B-202",
            "location": "Smart Village",
            "lecturerName": null,
            "teachingAssistantNames": ["Eng. Omar"],
            "timezone": "Africa/Cairo"
          }]
        }
        """;

    private static string Conflict() => $$"""
        {
          "code": "MEETING_OVERLAP",
          "first": {
            "groupId": "{{GroupAId}}",
            "groupCode": "G01",
            "courseCode": "AI401",
            "subjectTitle": "Artificial Intelligence",
            "startLocal": "10:00:00",
            "endLocal": "11:30:00"
          },
          "second": {
            "groupId": "00000000-0000-0000-0000-000000012203",
            "groupCode": "G03",
            "courseCode": "CS402",
            "subjectTitle": "Distributed Systems",
            "startLocal": "11:00:00",
            "endLocal": "12:00:00"
          },
          "dayOfWeek": 1,
          "overlapStartLocal": "11:00:00",
          "overlapEndLocal": "11:30:00",
          "message": "AI401 G01 overlaps CS402 G03 on Monday from 11:00 to 11:30.",
          "actions": [
            { "action": "change-group", "targetGroupId": "{{GroupAId}}", "label": "Change AI401 group G01", "route": "/student/subjects/{{OfferingId}}" },
            { "action": "remove-group", "targetGroupId": "{{GroupAId}}", "label": "Remove AI401 group G01", "route": "/student/schedule" },
            { "action": "change-group", "targetGroupId": "00000000-0000-0000-0000-000000012203", "label": "Change CS402 group G03", "route": "/student/subjects/00000000-0000-0000-0000-000000012103" },
            { "action": "remove-group", "targetGroupId": "00000000-0000-0000-0000-000000012203", "label": "Remove CS402 group G03", "route": "/student/schedule" }
          ]
        }
        """;

    private static string Validation() => """
        {
          "evaluatedAtUtc": "2026-07-20T07:15:00Z",
          "academicContextVersion": "ACADEMIC-V1",
          "policyVersion": "DEMO-POC-2026.1",
          "catalogueVersion": "CATALOGUE-V1",
          "offeringVersions": {},
          "groupVersions": {}
        }
        """;

    private static string LoadReason() => """
        {
          "code": "NORMAL_MAX_CREDITS",
          "blocking": false,
          "message": "The fixed demo maximum is 18 credits.",
          "requiredValue": null,
          "currentValue": null,
          "policySetId": "00000000-0000-0000-0000-000000012901",
          "policyVersion": "DEMO-POC-2026.1",
          "sourceReference": "policy/demo-poc"
        }
        """;

    private static string StalePlan() => $$"""
        {
          "error": {
            "code": "STALE_VERSION",
            "message": "The plan changed in another editor.",
            "correlationId": "STU-04-STALE-REF",
            "currentVersion": "PLAN-RV-2"
          },
          "currentPlan": {{CurrentPlan(GroupBId, "G02", "PLAN-RV-2")}}
        }
        """;

    private static string Error(string code) => $$"""
        {
          "code": "{{code}}",
          "message": "The schedule builder request could not be completed.",
          "correlationId": "STU-04-SAFE-REF"
        }
        """;

    private static string PlanPath =>
        "**/api/student/terms/*/registration-plan";
}
