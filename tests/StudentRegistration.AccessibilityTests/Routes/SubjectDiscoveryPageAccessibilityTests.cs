using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;

namespace StudentRegistration.AccessibilityTests.Routes;

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class SubjectDiscoveryPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    private const string TermId = "00000000-0000-0000-0000-000000011001";

    [Theory]
    [MemberData(nameof(Spec003RequestedRouteAccessibilityAssertions.WidthProfiles), MemberType = typeof(Spec003RequestedRouteAccessibilityAssertions))]
    public async Task Stu_02_search_filters_and_unavailable_reason_are_accessible_by_keyboard(
        int width,
        float scale)
    {
        await using var context = await fixture.OpenContextAsync(width, 1000, scale);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/context", route => route.FulfillAsync(Json(AppContext)));
        await page.RouteAsync(
            "**/api/student/terms/*/offerings*",
            route => route.FulfillAsync(Json(OfferingPage)));

        await page.GotoAsync("/student/subjects", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.Locator("[data-route-id='STU-02'][data-state='success']")
            .WaitForAsync();

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.Equal(1, await page.GetByRole(AriaRole.Main).CountAsync());
        var offering = page.Locator(
            "[data-offering-id='00000000-0000-0000-0000-000000011100']");
        Assert.True(await offering.GetByText("Unavailable", new() { Exact = true }).First
            .IsVisibleAsync());
        var serverReason = offering
            .GetByRole(
                AriaRole.Region,
                new() { Name = "Server eligibility reasons", Exact = true })
            .Locator("[data-reason-code='PREREQUISITE_NOT_MET']");
        Assert.True(await serverReason.GetByText(
            "PREREQUISITE_NOT_MET",
            new() { Exact = true }).IsVisibleAsync());
        Assert.True(await serverReason.GetByText(
            "Complete the prerequisite before selecting this subject.",
            new() { Exact = true }).IsVisibleAsync());

        var search = page.Locator("#subject-search");
        await search.FocusAsync();
        Assert.Equal("subject-search", await page.EvaluateAsync<string>(
            "() => document.activeElement.id"));
        await page.Keyboard.PressAsync("Tab");
        Assert.Equal("subject-eligibility", await page.EvaluateAsync<string>(
            "() => document.activeElement.id"));

        var apply = await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Apply search and filters", Exact = true })
            .BoundingBoxAsync();
        Assert.NotNull(apply);
        Assert.True(apply.Height >= 44);
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
          "supportReferencePath":"/support/student/STU-02-SAFE-REF"
        }
        """;

    private static string OfferingPage => """
        {
          "items":[{
            "offeringId":"00000000-0000-0000-0000-000000011100",
            "courseCode":"DS413",
            "title":"Project I",
            "credits":3,
            "currentPlanCredits":15,
            "projectedPlanCredits":18,
            "defaultTargetCredits":18,
            "maximumAllowedCredits":18,
            "eligible":false,
            "reasons":[{
              "code":"PREREQUISITE_NOT_MET",
              "passed":false,
              "blocking":true,
              "message":"Complete the prerequisite before selecting this subject.",
              "requiredValue":"CS301",
              "currentValue":"not-earned",
              "policySetId":"00000000-0000-0000-0000-000000011900",
              "policyVersion":"DEMO-POC-2026.1",
              "sourceReference":"policy/demo-poc",
              "sourceAccessedOn":"2026-07-13",
              "approvedBy":"Ahmed Elbamby",
              "effectiveFromUtc":"2026-07-13T00:00:00Z",
              "effectiveToUtc":null,
              "overridePossible":false,
              "supportReferencePath":"/support/student/STU-02-SAFE-REF"
            }],
            "groups":[{
              "groupId":"00000000-0000-0000-0000-000000011200",
              "groupCode":"A",
              "state":"published",
              "selectable":false,
              "capacity":30,
              "enrolledCount":12,
              "seatsRemaining":18,
              "nonSelectableReasons":[{"code":"PREREQUISITE_NOT_MET","message":"Complete the prerequisite before selecting this subject."}],
              "meetings":[{
                "meetingId":"00000000-0000-0000-0000-000000011300",
                "activity":"Lecture",
                "dayOfWeek":0,
                "startLocal":"09:00:00",
                "endLocal":"10:30:00",
                "roomCode":"A-101",
                "location":"Smart Village",
                "staff":[{"role":"Lecturer","name":"Dr. Nadia"}]
              }],
              "rowVersion":"GROUP-RV-1"
            }],
            "inputSummary":{"standing":"normal","window":"open"},
            "evaluatedAtUtc":"2026-07-20T07:15:00Z",
            "academicContextVersion":"ACADEMIC-V1",
            "catalogueVersion":"CATALOGUE-V1",
            "policySetId":"00000000-0000-0000-0000-000000011900",
            "policyVersion":"DEMO-POC-2026.1",
            "offeringRowVersion":"OFFERING-RV-1",
            "currentPlanVersion":"PLAN-V1"
          }],
          "page":1,
          "pageSize":20,
          "totalCount":1,
          "sort":"courseCode,id"
        }
        """;
}
