using Microsoft.Playwright;
using System.Security.Cryptography;
using StudentRegistration.TestSupport;
using StudentRegistration.VisualTests.Infrastructure;
using Xunit.Sdk;

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
        => await CaptureCandidateAsync(routeId, route, role, ownerPath, browser, width, error);

    [Fact]
    public async Task Bounded_chrome_mobile_candidate_sample_covers_all_three_routes_and_both_states()
    {
        foreach (var (routeId, route, role, ownerPath) in Routes)
        {
            await CaptureCandidateAsync(routeId, route, role, ownerPath, "chrome", 375, false);
            await CaptureCandidateAsync(routeId, route, role, ownerPath, "chrome", 375, true);
        }
    }

    [Fact]
    public async Task Resume_missing_visual_candidate_artifacts_sequentially()
    {
        if (!WriteCandidatesEnabled)
        {
            return;
        }

        foreach (var (routeId, route, role, ownerPath) in Routes)
        foreach (var profile in Spec003RouteVisualAssertions.BrowserWidths)
        foreach (var error in new[] { false, true })
        {
            var browser = Assert.IsType<string>(profile[0]);
            var width = Assert.IsType<int>(profile[1]);
            var candidatePath = CandidatePath(routeId, error, browser, width);
            if (IsValidCandidate(candidatePath))
            {
                continue;
            }

            var failures = new List<string>();
            for (var attempt = 1; attempt <= 3 && !IsValidCandidate(candidatePath); attempt++)
            {
                try
                {
                    await CaptureCandidateAsync(
                        routeId,
                        route,
                        role,
                        ownerPath,
                        browser,
                        width,
                        error);
                }
                catch (Exception exception) when (exception is PlaywrightException or TimeoutException or XunitException)
                {
                    failures.Add($"attempt {attempt}: {exception.GetType().Name}: {exception.Message}");
                }
            }

            if (!IsValidCandidate(candidatePath))
            {
                throw new XunitException(
                    $"Visual candidate resume failed for {routeId} {(error ? "error" : "primary")} " +
                    $"{browser} {width}px after three fresh-context attempts. " +
                    $"Expected nonempty artifact: {candidatePath}. Failures: {string.Join(" | ", failures)}");
            }
        }
    }

    private async Task CaptureCandidateAsync(
        string routeId, string route, string role, string ownerPath, string browser, int width, bool error)
    {
        await using var context = await fixture.OpenContextAsync(browser, width);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", request => Path(request) switch
        {
            "/api/context" => JsonAsync(request, 200, Context(role)),
            var path when path.StartsWith(ownerPath, StringComparison.Ordinal) => JsonAsync(request, error ? 503 : 200,
                error ? Error : ownerPath.EndsWith("roadmap", StringComparison.Ordinal) ? Roadmap : EmptyQueue),
            _ => request.AbortAsync()
        });
        try
        {
            var expectedState = error ? "error" : ownerPath.EndsWith(
                "roadmap",
                StringComparison.Ordinal) ? "success" : "empty";
            Exception? lastReadinessFailure = null;
            for (var readinessAttempt = 1; readinessAttempt <= 3; readinessAttempt++)
            {
                try
                {
                    await page.GotoAsync(route, new()
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded,
                        Timeout = 20_000
                    });
                    await page.Locator(
                            $"[data-route-id='{routeId}'][data-state='{expectedState}']")
                        .WaitForAsync(new() { Timeout = 20_000 });
                    lastReadinessFailure = null;
                    break;
                }
                catch (Exception exception) when (
                    readinessAttempt < 3 && exception is PlaywrightException or TimeoutException)
                {
                    lastReadinessFailure = exception;
                    await page.WaitForTimeoutAsync(250);
                }
            }

            if (lastReadinessFailure is not null)
            {
                throw lastReadinessFailure;
            }
        }
        catch (Exception exception) when (exception is PlaywrightException or TimeoutException)
        {
            var body = await SafeBodyAsync(page);
            throw new XunitException(
                $"Candidate navigation failed for {routeId} {(error ? "error" : "primary")} " +
                $"{browser} {width}px at {route}. {exception.GetType().Name}: {exception.Message}. " +
                $"Visible body: {body}");
        }
        var candidate = await StableVisualCapture.CaptureAsync(page, $"{routeId}-{(error ? "error" : "primary")}", browser, width);
        Assert.NotEmpty(candidate);
        if (WriteCandidatesEnabled)
        {
            var candidatePath = CandidatePath(routeId, error, browser, width);
            var candidateDirectory = System.IO.Path.GetDirectoryName(candidatePath)!;
            Directory.CreateDirectory(candidateDirectory);
            await File.WriteAllBytesAsync(candidatePath, candidate);
        }
        else
        {
            var approvedPath = ApprovedPath(routeId, error, browser, width);
            Assert.True(File.Exists(approvedPath), $"Approved v2 visual is missing: {approvedPath}");
            var approved = File.ReadAllBytes(approvedPath);
            var expectedArtifactHash = Convert.ToHexString(SHA256.HashData(approved));
            var expectedPixelHash = await PixelHashAsync(page, approved);
            var actualPixelHash = await PixelHashAsync(page, candidate);
            if (!string.Equals(expectedPixelHash, actualPixelHash, StringComparison.Ordinal))
            {
                var diagnosticDirectory = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    "StudentRegistration.VisualTests",
                    "approved-v2",
                    routeId);
                Directory.CreateDirectory(diagnosticDirectory);
                var diagnosticPath = System.IO.Path.Combine(
                    diagnosticDirectory,
                    $"{browser}-{width}-{(error ? "error" : "primary")}-actual.png");
                await File.WriteAllBytesAsync(diagnosticPath, candidate);
                throw new XunitException(
                    $"Approved v2 visual mismatch for {routeId} {browser} {width}px. " +
                    $"Approved artifact {expectedArtifactHash}; expected pixels {expectedPixelHash}; " +
                    $"actual pixels {actualPixelHash}; diagnostic: {diagnosticPath}");
            }
        }
    }

    private static bool WriteCandidatesEnabled => string.Equals(
        Environment.GetEnvironmentVariable("SRS_WRITE_VISUAL_CANDIDATES"),
        "1",
        StringComparison.Ordinal);

    private static string CandidatePath(string routeId, bool error, string browser, int width) =>
        System.IO.Path.Combine(
            RepositoryFiles.Root,
            "artifacts",
            "visual-candidates",
            "Spec003",
            routeId,
            error ? "error" : "primary",
            $"{browser}-{width}.png");

    private static string ApprovedPath(string routeId, bool error, string browser, int width)
    {
        var state = error ? "error" : "primary";
        return System.IO.Path.Combine(
            RepositoryFiles.Root,
            "tests",
            "StudentRegistration.VisualTests",
            "Baselines",
            "v2",
            "Spec003",
            routeId,
            state,
            $"{browser}-{width}-{state}.png");
    }

    private static bool IsValidCandidate(string path) =>
        File.Exists(path) && new FileInfo(path).Length > 0;

    private static Task<string> PixelHashAsync(IPage page, byte[] png) =>
        page.EvaluateAsync<string>(
            """
            async encoded => {
                const image = new Image();
                image.src = `data:image/png;base64,${encoded}`;
                await image.decode();
                const canvas = document.createElement('canvas');
                canvas.width = image.naturalWidth;
                canvas.height = image.naturalHeight;
                const context = canvas.getContext('2d', { willReadFrequently: true });
                context.drawImage(image, 0, 0);
                const pixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
                const hash = await crypto.subtle.digest('SHA-256', pixels);
                return Array.from(new Uint8Array(hash), byte =>
                    byte.toString(16).padStart(2, '0')).join('');
            }
            """,
            Convert.ToBase64String(png));

    private static async Task<string> SafeBodyAsync(IPage page)
    {
        try
        {
            var body = await page.Locator("body").InnerTextAsync(new() { Timeout = 2_000 });
            return string.IsNullOrWhiteSpace(body) ? "<empty body>" : body;
        }
        catch (Exception exception) when (exception is PlaywrightException or TimeoutException)
        {
            return $"<body unavailable: {exception.Message}>";
        }
    }

    private static readonly (string RouteId, string Route, string Role, string Owner)[] Routes =
    [
        ("STU-09", "/student/roadmap", "Student", "/api/students/me/roadmap"),
        ("ADM-10", "/admin/approvals", "Admin", "/api/admin/registration-approvals"),
        ("STF-05", "/staff/approvals", "TeachingAssistant", "/api/staff/registration-approvals")
    ];
    private static string Context(string role) => $$"""{"serverTimeUtc":"2026-07-21T09:30:00Z","timeZoneId":"Africa/Cairo","teachingTerm":null,"registrationTerm":null,"registrationWindowState":"none","registrationWindow":null,"serviceState":"available","displayName":"{{role}} Demo","authorizedRoles":["{{role}}"],"activeRole":"{{role}}","sessionState":"active","expiresAtUtc":"2026-07-21T11:30:00Z","supportReferencePath":"/status/support"}""";
    private const string Roadmap = """{"programCode":"AI-DS","cohort":"2026","catalogueVersion":"v2","terms":[{"recommendedTerm":1,"level":1,"subjects":[]}]}""";
    private const string EmptyQueue = """{"items":[],"page":1,"pageSize":20,"totalCount":0}""";
    private const string Error = """{"code":"SERVICE_UNAVAILABLE","message":"The owner service is unavailable.","correlationId":"VIS-SAFE-REF"}""";
    private static string Path(IRoute route) => new Uri(route.Request.Url).AbsolutePath;
    private static Task JsonAsync(IRoute route, int status, string body) => route.FulfillAsync(new() { Status = status, ContentType = "application/json", Body = body });
}
