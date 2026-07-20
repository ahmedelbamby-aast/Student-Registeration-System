using Microsoft.Playwright;
using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class RequestedUnifiedRouteCandidateVisualTests(VisualRegressionFixture fixture)
{
    public static TheoryData<string, string, string, string, string, int, bool> Candidates
    {
        get
        {
            var data = new TheoryData<string, string, string, string, string, int, bool>();
            foreach (var (routeId, route, role, owner) in Routes)
            foreach (var profile in Spec003RouteVisualAssertions.BrowserWidths)
            {
                var browser = Assert.IsType<string>(profile[0]);
                var width = Assert.IsType<int>(profile[1]);
                data.Add(routeId, route, role, owner, browser, width, false);
                data.Add(routeId, route, role, owner, browser, width, true);
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Candidates))]
    public async Task Primary_and_canonical_error_candidates_are_stable_across_the_shared_matrix(
        string routeId, string route, string role, string ownerPath, string browser, int width, bool error)
    {
        await using var context = await fixture.OpenContextAsync(browser, width);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", request => Path(request) switch
        {
            "/api/context" => JsonAsync(request, 200, Context(role)),
            var path when path == ownerPath => JsonAsync(request, error ? 503 : 200,
                error ? Error : ownerPath.EndsWith("roadmap", StringComparison.Ordinal) ? Roadmap : EmptyQueue),
            _ => request.AbortAsync()
        });
        await page.GotoAsync(route, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator($"[data-route-id='{routeId}']").WaitForAsync();
        var candidate = await StableVisualCapture.CaptureAsync(page, $"{routeId}-{(error ? "error" : "primary")}", browser, width);
        Assert.NotEmpty(candidate);
    }

    private static readonly (string RouteId, string Route, string Role, string Owner)[] Routes =
    [
        ("STU-09", "/student/roadmap", "Student", "/api/students/me/roadmap"),
        ("ADM-10", "/admin/approvals", "Admin", "/api/admin/registration-approvals"),
        ("STF-05", "/staff/approvals", "TeachingAssistant", "/api/staff/registration-approvals")
    ];
    private static string Context(string role) => $$"""{"serverTimeUtc":"2026-07-21T09:30:00Z","timeZoneId":"Africa/Cairo","teachingTerm":null,"registrationTerm":null,"registrationWindowState":"open","registrationWindow":null,"serviceState":"available","displayName":"{{role}} Demo","authorizedRoles":["{{role}}"],"activeRole":"{{role}}","sessionState":"active","expiresAtUtc":"2026-07-21T11:30:00Z","supportReferencePath":"/status/support"}""";
    private const string Roadmap = """{"programCode":"AI-DS","cohort":"2026","catalogueVersion":"v2","terms":[{"recommendedTerm":1,"level":1,"subjects":[]}]}""";
    private const string EmptyQueue = """{"items":[],"page":1,"pageSize":20,"totalCount":0}""";
    private const string Error = """{"code":"SERVICE_UNAVAILABLE","message":"The owner service is unavailable.","correlationId":"VIS-SAFE-REF"}""";
    private static string Path(IRoute route) => new Uri(route.Request.Url).AbsolutePath;
    private static Task JsonAsync(IRoute route, int status, string body) => route.FulfillAsync(new() { Status = status, ContentType = "application/json", Body = body });
}
