using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;

namespace StudentRegistration.AccessibilityTests.Routes;

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class ScheduleBuilderPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    private const string TermId = "00000000-0000-0000-0000-000000012001";

    [Theory]
    [MemberData(nameof(Spec003RequestedRouteAccessibilityAssertions.WidthProfiles), MemberType = typeof(Spec003RequestedRouteAccessibilityAssertions))]
    public async Task Stu_04_calendar_list_and_continue_flow_are_accessible_by_keyboard(
        int width,
        float scale)
    {
        await using var context = await fixture.OpenContextAsync(width, 1000, scale);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/context", route => route.FulfillAsync(Json(AppContext)));
        await page.RouteAsync(
            "**/api/student/terms/*/registration-plan",
            route => route.FulfillAsync(Json(CurrentPlan)));

        await page.GotoAsync("/student/schedule", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.Locator("[data-route-id='STU-04'][data-state='success']")
            .WaitForAsync();

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        var calendarMeetings = await page
            .Locator(".srs-schedule-calendar [data-meeting-id]")
            .EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.meetingId)");
        var listMeetings = await page
            .Locator(".srs-schedule-list [data-meeting-id]")
            .EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.meetingId)");
        Assert.Single(calendarMeetings);
        Assert.Equal(calendarMeetings, listMeetings);

        var skip = page.GetByRole(
            AriaRole.Link,
            new() { Name = "Skip to main content", Exact = true });
        await skip.FocusAsync();
        await skip.PressAsync("Enter");
        Assert.Equal("main-content", await page.EvaluateAsync<string>(
            "() => document.activeElement.id"));
        var review = page.GetByRole(
            AriaRole.Button,
            new() { Name = "Continue to review", Exact = true });
        await review.FocusAsync();
        Assert.True(await review.IsEnabledAsync());
        var box = await review.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box.Height >= 44);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
    }

    private static RouteFulfillOptions Json(string body) => new()
    {
        Status = 200,
        ContentType = "application/json",
        Body = body
    };

    private static string AppContext => $$"""
        {
          "serverTimeUtc":"2026-07-20T07:15:00Z",
          "timeZoneId":"Africa/Cairo",
          "teachingTerm":null,
          "registrationTerm":{"id":"{{TermId}}","code":"FALL-2026","label":"Fall 2026","state":"registrationOpen","rowVersion":"TERM-RV-1"},
          "registrationWindowState":"open",
          "registrationWindow":{"id":"WINDOW-FALL-2026","state":"open","opensAtUtc":"2026-07-19T06:00:00Z","closesAtUtc":"2026-07-21T18:00:00Z","rowVersion":"WINDOW-RV-1"},
          "serviceState":"available",
          "displayName":"Synthetic Student One",
          "authorizedRoles":["Student"],
          "activeRole":"Student",
          "sessionState":"active",
          "expiresAtUtc":"2026-07-20T09:15:00Z",
          "supportReferencePath":"/support/student/STU-04-SAFE-REF"
        }
        """;

    private static string CurrentPlan => $$"""
        {
          "id":"00000000-0000-0000-0000-000000012010",
          "termId":"{{TermId}}",
          "rowVersion":"PLAN-RV-1",
          "selectedGroups":[{
            "groupId":"00000000-0000-0000-0000-000000012201",
            "offeringId":"00000000-0000-0000-0000-000000012101",
            "courseCode":"AI401",
            "subjectTitle":"Artificial Intelligence",
            "groupCode":"G01",
            "credits":3,
            "capacity":30,
            "enrolledCount":12,
            "rowVersion":"GROUP-RV-1",
            "meetings":[{
              "meetingId":"00000000-0000-0000-0000-000000012301",
              "activityKind":"Lecture",
              "dayOfWeek":1,
              "startLocal":"10:00:00",
              "endLocal":"11:30:00",
              "roomCode":"A-101",
              "location":"Smart Village",
              "lecturerName":"Dr. Salma",
              "teachingAssistantNames":[],
              "timezone":"Africa/Cairo"
            }]
          }],
          "totalCredits":3,
          "defaultTargetCredits":18,
          "maximumAllowedCredits":18,
          "loadReasons":[{
            "code":"NORMAL_MAX_CREDITS",
            "blocking":false,
            "message":"The fixed demo maximum is 18 credits.",
            "requiredValue":null,
            "currentValue":null,
            "policySetId":"00000000-0000-0000-0000-000000012901",
            "policyVersion":"DEMO-POC-2026.1",
            "sourceReference":"policy/demo-poc"
          }],
          "selectionIssues":[],
          "conflicts":[],
          "validation":{
            "evaluatedAtUtc":"2026-07-20T07:15:00Z",
            "academicContextVersion":"ACADEMIC-V1",
            "policyVersion":"DEMO-POC-2026.1",
            "catalogueVersion":"CATALOGUE-V1",
            "offeringVersions":{},
            "groupVersions":{}
          },
          "reviewBlocked":false
        }
        """;
}
