using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;

namespace StudentRegistration.AccessibilityTests.Routes;

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class StudentRoadmapPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [MemberData(nameof(Spec003RequestedRouteAccessibilityAssertions.WidthProfiles), MemberType = typeof(Spec003RequestedRouteAccessibilityAssertions))]
    public Task Stu_09_primary_state_uses_the_shared_accessibility_matrix(int width, float scale) =>
        RequestedRouteAccessibilityEvidence.AssertAsync(fixture, "STU-09", "/student/roadmap", "Student", "/api/students/me/roadmap", width, scale, false);
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class AdminApprovalPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [MemberData(nameof(Spec003RequestedRouteAccessibilityAssertions.WidthProfiles), MemberType = typeof(Spec003RequestedRouteAccessibilityAssertions))]
    public Task Adm_10_primary_and_error_content_remain_accessible(int width, float scale) =>
        RequestedRouteAccessibilityEvidence.AssertAsync(fixture, "ADM-10", "/admin/approvals", "Admin", "/api/admin/registration-approvals", width, scale, width == 320);
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class StaffApprovalPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [MemberData(nameof(Spec003RequestedRouteAccessibilityAssertions.WidthProfiles), MemberType = typeof(Spec003RequestedRouteAccessibilityAssertions))]
    public Task Stf_05_primary_and_error_content_remain_accessible(int width, float scale) =>
        RequestedRouteAccessibilityEvidence.AssertAsync(fixture, "STF-05", "/staff/approvals", "TeachingAssistant", "/api/staff/registration-approvals", width, scale, width == 320);
}

internal static class RequestedRouteAccessibilityEvidence
{
    internal static async Task AssertAsync(AxeAccessibilityFixture fixture, string routeId, string route,
        string role, string ownerPath, int width, float scale, bool failure)
    {
        await using var context = await fixture.OpenContextAsync(width, 1000, scale);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", request => Path(request) switch
        {
            "/api/context" => JsonAsync(request, 200, Context(role)),
            var path when path == ownerPath => JsonAsync(request, failure ? 503 : 200,
                failure ? Error : ownerPath.EndsWith("roadmap", StringComparison.Ordinal) ? Roadmap : EmptyQueue),
            _ => request.AbortAsync()
        });
        await page.GotoAsync(route, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator($"[data-route-id='{routeId}']").WaitForAsync();
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        Assert.True(await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));

        if (width < 1024)
        {
            var menu = page.GetByRole(AriaRole.Button, new() { Name = "Menu", Exact = true });
            Assert.True(await menu.IsVisibleAsync());
            Assert.Equal("false", await menu.GetAttributeAsync("aria-expanded"));
            await menu.ClickAsync();
            Assert.Equal("true", await menu.GetAttributeAsync("aria-expanded"));
            var drawer = page.Locator("#workspace-navigation");
            Assert.True(await drawer.EvaluateAsync<bool>("element => element.hasAttribute('data-open')"));
            Assert.True(await drawer.EvaluateAsync<bool>("element => document.activeElement === element"));
            await page.Keyboard.PressAsync("Escape");
            Assert.Equal("false", await menu.GetAttributeAsync("aria-expanded"));
            Assert.True(await menu.EvaluateAsync<bool>("element => document.activeElement === element"));
        }
    }

    private static string Context(string role) => $$"""{"serverTimeUtc":"2026-07-21T09:30:00Z","timeZoneId":"Africa/Cairo","teachingTerm":null,"registrationTerm":null,"registrationWindowState":"none","registrationWindow":null,"serviceState":"available","displayName":"{{role}} Demo","authorizedRoles":["{{role}}"],"activeRole":"{{role}}","sessionState":"active","expiresAtUtc":"2026-07-21T11:30:00Z","supportReferencePath":"/status/support"}""";
    private const string Roadmap = """{"programCode":"AI-DS","cohort":"2026","catalogueVersion":"v2","terms":[{"recommendedTerm":1,"level":1,"subjects":[]}]}""";
    private const string EmptyQueue = """{"items":[],"page":1,"pageSize":20,"totalCount":0}""";
    private const string Error = """{"code":"SERVICE_UNAVAILABLE","message":"The owner service is unavailable.","correlationId":"A11Y-SAFE-REF"}""";
    private static string Path(IRoute route) => new Uri(route.Request.Url).AbsolutePath;
    private static Task JsonAsync(IRoute route, int status, string body) => route.FulfillAsync(new() { Status = status, ContentType = "application/json", Body = body });
}
