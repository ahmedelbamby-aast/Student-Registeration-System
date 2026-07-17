using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class RegistrationRecordsPageAccessibilityFrozenContractTests
{
    [Theory]
    [InlineData("STU-06")]
    [InlineData("STU-07")]
    public void Route_keeps_the_six_governed_widths_and_accessibility_owner_record(string routeId)
    {
        var design = RepositoryFiles.Read($"specs/003-ux-storyboard-accessibility/design/pages/{routeId}.md");
        RepositoryFiles.ContainsAll(design, "responsiveWidths", "320,", "375,", "768,", "1024,", "1280,", "1920");
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class RegistrationRecordsPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    private const string Submission = "00000000-0000-0000-0000-000000015001";
    private const string Term = "00000000-0000-0000-0000-000000015003";
    private const string Meeting = "00000000-0000-0000-0000-000000015004";

    [Fact]
    public void Synthetic_payloads_are_valid_json()
    {
        foreach (var payload in new[] { Context, Detail, History, Timetable })
        {
            using var document = System.Text.Json.JsonDocument.Parse(payload);
            Assert.Equal(System.Text.Json.JsonValueKind.Object, document.RootElement.ValueKind);
        }
        var options = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)
        {
            RespectNullableAnnotations = true,
            RespectRequiredConstructorParameters = true
        };
        Assert.NotNull(System.Text.Json.JsonSerializer.Deserialize<StudentRegistration.Contracts.Registration.RegistrationDetailDto>(Detail, options));
        Assert.NotNull(System.Text.Json.JsonSerializer.Deserialize<StudentRegistration.Contracts.Page<StudentRegistration.Contracts.Registration.RegistrationHistoryRowDto>>(History, options));
        Assert.NotNull(System.Text.Json.JsonSerializer.Deserialize<StudentRegistration.Contracts.Registration.RegistrationTimetableDto>(Timetable, options));
    }

    [Theory]
    [InlineData(375, "/student/registration/result/00000000-0000-0000-0000-000000015001", "STU-06", "Print receipt")]
    [InlineData(1280, "/student/registration/result/00000000-0000-0000-0000-000000015001", "STU-06", "Print receipt")]
    [InlineData(375, "/student/registrations", "STU-07", "Print history")]
    [InlineData(1280, "/student/registrations", "STU-07", "Print history")]
    public async Task Route_has_no_serious_axe_issue_overflow_or_short_primary_target(int width, string path, string routeId, string action)
    {
        await using var context = await fixture.OpenContextAsync(width);
        var page = await context.NewPageAsync();
        var requestedPaths = new List<string>();
        await page.RouteAsync("**/api/**", async route =>
        {
            requestedPaths.Add(new Uri(route.Request.Url).AbsolutePath);
            await RouteAsync(route);
        });
        await page.GotoAsync(path, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        var routeRoot = page.Locator($"[data-route-id='{routeId}']");
        await routeRoot.WaitForAsync();
        await page.WaitForFunctionAsync(
            "id => document.querySelector(`[data-route-id='${id}']`)?.getAttribute('data-state') !== 'loading'",
            routeId);
        routeRoot = page.Locator($"[data-route-id='{routeId}']");
        var state = await routeRoot.GetAttributeAsync("data-state");
        Assert.True(string.Equals("success", state, StringComparison.Ordinal),
            $"Expected success but rendered {state}; requests: {string.Join(", ", requestedPaths)}; content: {await routeRoot.TextContentAsync()}");

        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.Equal(1, await page.GetByRole(AriaRole.Main).CountAsync());
        Assert.True(await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        var box = await page.GetByRole(AriaRole.Button, new() { Name = action, Exact = true }).BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box.Height >= 44, $"{routeId} primary target is shorter than 44 CSS pixels at {width}px.");

        var calendar = await page.Locator(".srs-schedule-calendar [data-meeting-id]").EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.meetingId)");
        var list = await page.Locator(".srs-schedule-list [data-meeting-id]").EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.meetingId)");
        Assert.Equal(calendar, list);
    }

    private static async Task RouteAsync(IRoute route)
    {
        var path = new Uri(route.Request.Url).AbsolutePath;
        if (path == "/api/context") await JsonAsync(route, Context);
        else if (path == $"/api/student/registrations/{Submission}") await JsonAsync(route, Detail);
        else if (path == "/api/student/registrations") await JsonAsync(route, History);
        else if (path == "/api/student/registrations/current/timetable") await JsonAsync(route, Timetable);
        else await route.AbortAsync();
    }

    private static Task JsonAsync(IRoute route, string body) => route.FulfillAsync(new() { Status = 200, ContentType = "application/json", Body = body });

    private static string Context => $$$"""
    {"serverTimeUtc":"2026-07-20T07:15:00Z","timeZoneId":"Africa/Cairo","teachingTerm":null,
    "registrationTerm":{"id":"{{{Term}}}","code":"FALL-2026","label":"Fall 2026","state":"registrationOpen","rowVersion":"TERM-RV-1"},
    "registrationWindowState":"open","registrationWindow":{"id":"00000000-0000-0000-0000-000000015010","state":"open","opensAtUtc":"2026-07-19T06:00:00Z","closesAtUtc":"2026-07-21T18:00:00Z","rowVersion":"WINDOW-RV-1"},"serviceState":"available","displayName":"Student",
    "authorizedRoles":["Student"],"activeRole":"Student","sessionState":"active","expiresAtUtc":"2026-07-20T09:15:00Z","supportReferencePath":"/support/student"}
    """;
    private static string Group => $$$"""
    {"offeringId":"00000000-0000-0000-0000-000000015020","courseCode":"AI401","subjectTitle":"Artificial Intelligence",
    "groupId":"00000000-0000-0000-0000-000000015021","groupCode":"G01","credits":3,"meetings":[
    {"meetingId":"{{{Meeting}}}","activityType":"Lecture","dayOfWeek":1,"startLocal":"10:00","endLocal":"11:30","roomCode":"A-101","location":"Smart Village","staff":[{"role":"Lecturer","displayName":"Dr. Salma"}]}]}
    """;
    private static string Detail => $$$"""
    {"status":"accepted","receipt":{"submissionId":"{{{Submission}}}","reference":"REG-2026-015001",
    "term":{"id":"{{{Term}}}","code":"FALL-2026","displayName":"Fall 2026","timeZoneId":"Africa/Cairo"},
    "submittedAtUtc":"2026-07-20T07:15:01Z","resultCode":"REGISTERED","policyVersion":"DEMO-POC-2026.1","groups":[{{{Group}}}],"totalCredits":3},"rejection":null}
    """;
    private static string History => $$$"""
    {"items":[{"submissionId":"{{{Submission}}}","reference":"REG-2026-015001","term":{"id":"{{{Term}}}","code":"FALL-2026","displayName":"Fall 2026","timeZoneId":"Africa/Cairo"},"termState":"registrationOpen","status":"accepted","submittedAtUtc":"2026-07-20T07:15:01Z","groupCount":1,"totalCredits":3}],"page":1,"pageSize":20,"totalCount":1,"sort":"submittedAtUtc desc,submissionId"}
    """;
    private static string Timetable => $$$"""
    {"term":{"id":"{{{Term}}}","code":"FALL-2026","displayName":"Fall 2026","timeZoneId":"Africa/Cairo"},"termState":"registrationOpen","registrationWindowState":"open","groups":[{{{Group}}}],"subjectDiscoveryPath":"/student/subjects"}
    """;
}
