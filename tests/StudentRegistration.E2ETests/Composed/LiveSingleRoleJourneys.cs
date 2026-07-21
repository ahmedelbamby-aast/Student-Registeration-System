using Microsoft.Playwright;

namespace StudentRegistration.E2ETests.Composed;

public sealed class LiveSingleRoleJourneys
{
    private const string BaseUrlVariable = "SRS_LIVE_DEMO_BASE_URL";

    [LiveDemoTheory]
    [InlineData("Admin", "SRS_LIVE_ADMIN_ID", "ADM-0001", "SRS_LIVE_ADMIN_PASSWORD", "/admin", "ADM-01", "/admin/terms", "ADM-02", "/student", "STU-01")]
    [InlineData("Lecturer", "SRS_LIVE_LECTURER_ID", "LEC-0001", "SRS_LIVE_LECTURER_PASSWORD", "/staff", "STF-01", "/staff/timetable", "STF-02", "/admin", "ADM-01")]
    [InlineData("TeachingAssistant", "SRS_LIVE_TA_ID", "TA-0001", "SRS_LIVE_TA_PASSWORD", "/staff", "STF-01", "/staff/timetable", "STF-02", "/admin", "ADM-01")]
    public async Task Staff_live_journey_uses_real_login_and_role_bound_routes(
        string role,
        string idVariable,
        string defaultId,
        string passwordVariable,
        string landingPath,
        string landingRouteId,
        string readPath,
        string readRouteId,
        string forbiddenPath,
        string forbiddenRouteId)
    {
        var credentialId = Environment.GetEnvironmentVariable(idVariable) ?? defaultId;
        var password = RequiredEnvironmentVariable(passwordVariable);

        await using var session = await LiveBrowserSession.CreateAsync(RequiredEnvironmentVariable(BaseUrlVariable));
        var page = session.Page;

        await page.GotoAsync("/staff/login");
        await page.Locator("#staff-user-name").FillAsync(credentialId);
        await page.Locator("#staff-password").FillAsync(password);
        await page.Locator("form[data-testid='staff-login-form'] button[type='submit']").ClickAsync();

        await AssertRoleWorkspaceAsync(page, landingPath, landingRouteId, role);

        await page.GotoAsync(readPath);
        await AssertRoleWorkspaceAsync(page, readPath, readRouteId, role);

        var approvalPath = string.Equals(role, "Admin", StringComparison.Ordinal)
            ? "/admin/approvals"
            : "/staff/approvals";
        var approvalRouteId = string.Equals(role, "Admin", StringComparison.Ordinal)
            ? "ADM-10"
            : "STF-05";
        await page.GotoAsync(approvalPath);
        await AssertRoleWorkspaceAsync(page, approvalPath, approvalRouteId, role);

        await AssertCrossRoleRouteDeniedAsync(page, forbiddenPath, forbiddenRouteId);
    }

    [LiveStudentFact]
    public async Task Student_live_journey_uses_activated_account_and_role_bound_routes()
    {
        var studentId = RequiredEnvironmentVariable("SRS_LIVE_STUDENT_ID");
        var password = RequiredEnvironmentVariable("SRS_LIVE_STUDENT_PASSWORD");

        await using var session = await LiveBrowserSession.CreateAsync(RequiredEnvironmentVariable(BaseUrlVariable));
        var page = session.Page;

        await page.GotoAsync("/student/login");
        await page.Locator("#student-university-id").FillAsync(studentId);
        await page.Locator("#student-password").FillAsync(password);
        await page.Locator("form[data-testid='student-login-form'] button[type='submit']").ClickAsync();

        await AssertRoleWorkspaceAsync(page, "/student", "STU-01", "Student");

        await page.GotoAsync("/student/roadmap");
        await AssertRoleWorkspaceAsync(page, "/student/roadmap", "STU-09", "Student");

        foreach (var route in StudentRegistrationRoutes)
        {
            await page.GotoAsync(route.Path);
            await AssertRoleWorkspaceAsync(page, route.Path, route.RouteId, "Student");
        }

        await AssertCrossRoleRouteDeniedAsync(page, "/admin", "ADM-01");
    }

    private static readonly (string Path, string RouteId)[] StudentRegistrationRoutes =
    [
        ("/student/subjects", "STU-02"),
        ("/student/schedule", "STU-04"),
        ("/student/review", "STU-05"),
        ("/student/registrations", "STU-07")
    ];

    private static async Task AssertRoleWorkspaceAsync(
        IPage page,
        string expectedPath,
        string routeId,
        string role)
    {
        await page.WaitForURLAsync(url => string.Equals(new Uri(url).AbsolutePath, expectedPath, StringComparison.OrdinalIgnoreCase));
        await Assertions.Expect(page.Locator(".srs-app-shell")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator($"[data-route-id='{routeId}']")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".srs-app-shell__user span")).ToHaveTextAsync(role);
    }

    private static async Task AssertCrossRoleRouteDeniedAsync(IPage page, string forbiddenPath, string protectedRouteId)
    {
        await page.GotoAsync(forbiddenPath);
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var protectedSuccess = page.Locator($"[data-route-id='{protectedRouteId}'][data-state='success']");
        await Assertions.Expect(protectedSuccess).ToHaveCountAsync(0);
    }

    private static string RequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        Assert.False(string.IsNullOrWhiteSpace(value), $"Required live-test environment variable '{name}' is not set.");
        return value!;
    }

    private sealed class LiveBrowserSession : IAsyncDisposable
    {
        private readonly IPlaywright _playwright;
        private readonly IBrowser _browser;
        private readonly IBrowserContext _context;

        private LiveBrowserSession(IPlaywright playwright, IBrowser browser, IBrowserContext context, IPage page)
        {
            _playwright = playwright;
            _browser = browser;
            _context = context;
            Page = page;
        }

        public IPage Page { get; }

        public static async Task<LiveBrowserSession> CreateAsync(string baseUrl)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var context = await browser.NewContextAsync(new()
            {
                BaseURL = baseUrl.TrimEnd('/'),
                IgnoreHTTPSErrors = true
            });
            return new(playwright, browser, context, await context.NewPageAsync());
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
            await _browser.DisposeAsync();
            _playwright.Dispose();
        }
    }
}

internal sealed class LiveDemoTheoryAttribute : TheoryAttribute
{
    public LiveDemoTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SRS_LIVE_DEMO_BASE_URL")))
        {
            Skip = "Set SRS_LIVE_DEMO_BASE_URL to opt into the composed live demo journeys.";
        }
    }
}

internal sealed class LiveStudentFactAttribute : FactAttribute
{
    public LiveStudentFactAttribute()
    {
        var baseUrlMissing = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SRS_LIVE_DEMO_BASE_URL"));
        var studentMissing = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SRS_LIVE_STUDENT_ID"))
            || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SRS_LIVE_STUDENT_PASSWORD"));
        var studentRequired = string.Equals(
            Environment.GetEnvironmentVariable("SRS_LIVE_REQUIRE_STUDENT"),
            "1",
            StringComparison.Ordinal);

        if (baseUrlMissing)
        {
            Skip = "Set SRS_LIVE_DEMO_BASE_URL to opt into the composed live demo journeys.";
        }
        else if (studentMissing && !studentRequired)
        {
            Skip = "Set an activated student ID and password to run the student live journey.";
        }
    }
}
