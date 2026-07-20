using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;

namespace StudentRegistration.E2ETests.Routes;

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class RequestedUnifiedRouteJourneyTests(Spec008BrowserFixture fixture)
{
    [Theory]
    [InlineData("STU-09", "/student/roadmap", "Student", "/api/students/me/roadmap")]
    [InlineData("ADM-10", "/admin/approvals", "Admin", "/api/admin/registration-approvals")]
    [InlineData("STF-05", "/staff/approvals", "TeachingAssistant", "/api/staff/registration-approvals")]
    public async Task Primary_journey_renders_the_authorized_empty_or_success_state(
        string routeId, string route, string role, string ownerPath)
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await ConfigureAsync(page, role, ownerPath, failure: false);
        await page.GotoAsync(route, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator($"[data-route-id='{routeId}']").WaitForAsync();
        Assert.Equal(0, await page.GetByText("Approval queue unavailable", new() { Exact = true }).CountAsync());
    }

    [Theory]
    [InlineData("STU-09", "/student/roadmap", "Student", "/api/students/me/roadmap")]
    [InlineData("ADM-10", "/admin/approvals", "Admin", "/api/admin/registration-approvals")]
    [InlineData("STF-05", "/staff/approvals", "Lecturer", "/api/staff/registration-approvals")]
    public async Task Failure_journey_preserves_the_authoritative_error_without_false_success(
        string routeId, string route, string role, string ownerPath)
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await ConfigureAsync(page, role, ownerPath, failure: true);
        await page.GotoAsync(route, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator($"[data-route-id='{routeId}'][data-state='error']").WaitForAsync();
        Assert.True(await page.GetByText("SERVICE_UNAVAILABLE", new() { Exact = false }).First.IsVisibleAsync());
    }

    private static Task ConfigureAsync(IPage page, string role, string ownerPath, bool failure) =>
        page.RouteAsync("**/api/**", request => Path(request) switch
        {
            "/api/context" => JsonAsync(request, 200, Context(role)),
            var path when path == ownerPath => failure
                ? JsonAsync(request, 503, Error)
                : JsonAsync(request, 200, ownerPath.EndsWith("roadmap", StringComparison.Ordinal) ? Roadmap : EmptyQueue),
            _ => request.AbortAsync()
        });

    internal static string Context(string role) => $$"""
        {"serverTimeUtc":"2026-07-21T09:30:00Z","timeZoneId":"Africa/Cairo",
        "teachingTerm":null,"registrationTerm":null,"registrationWindowState":"open","registrationWindow":null,
        "serviceState":"available","displayName":"{{role}} Demo","authorizedRoles":["{{role}}"],
        "activeRole":"{{role}}","sessionState":"active","expiresAtUtc":"2026-07-21T11:30:00Z",
        "supportReferencePath":"/status/support"}
        """;
    internal const string Roadmap = """
        {"programCode":"AI-DS","cohort":"2026","catalogueVersion":"v2",
        "terms":[{"recommendedTerm":1,"level":1,"subjects":[]}]}
        """;
    internal const string EmptyQueue = """{"items":[],"page":1,"pageSize":20,"totalCount":0}""";
    internal const string Error = """{"code":"SERVICE_UNAVAILABLE","message":"The owner service is unavailable.","correlationId":"ROUTE-SAFE-REF"}""";
    internal static string Path(IRoute route) => new Uri(route.Request.Url).AbsolutePath;
    internal static Task JsonAsync(IRoute route, int status, string body) => route.FulfillAsync(
        new() { Status = status, ContentType = "application/json", Body = body });
}
