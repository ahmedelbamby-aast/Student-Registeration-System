using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using Xunit.Abstractions;

namespace StudentRegistration.E2ETests.Performance;

public sealed class Spec003LiveLcpEvidenceTests(ITestOutputHelper output)
{
    private const double LcpP75BudgetMilliseconds = 2_500;

    [Fact]
    [Trait("Category", "LiveLcpEvidence")]
    public async Task Stu_02_04_05_live_authenticated_routes_meet_p75_budget()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("SRS_RUN_LIVE_LCP"), "1", StringComparison.Ordinal))
        {
            output.WriteLine("Live LCP evidence is opt-in; use Performance/Run-LiveLcpEvidence.ps1, which fails closed on missing prerequisites.");
            return;
        }

        var baseUrl = Required("SRS_LIVE_BASE_URL");
        var studentId = Required("SRS_LIVE_STUDENT_ID");
        var password = Required("SRS_LIVE_STUDENT_PASSWORD");
        Assert.True(Uri.TryCreate(baseUrl, UriKind.Absolute, out var origin) && origin.Scheme == Uri.UriSchemeHttps,
            "SRS_LIVE_BASE_URL must be an absolute HTTPS origin.");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true, Channel = "chrome" });
        var storageState = await AuthenticateAsync(browser, origin!, studentId, password);
        var routes = new Dictionary<string, string>
        {
            ["STU-02"] = "/student/subjects",
            ["STU-04"] = "/student/schedule",
            ["STU-05"] = "/student/review"
        };
        var routeEvidence = new List<RouteEvidence>();

        foreach (var (routeId, path) in routes)
        {
            var lcpSamples = new List<double>();
            var apiSamples = new List<double>();
            for (var sample = 0; sample < 4; sample++)
            {
                await using var context = await browser.NewContextAsync(new()
                {
                    BaseURL = origin!.AbsoluteUri,
                    IgnoreHTTPSErrors = true,
                    StorageState = storageState,
                    ViewportSize = new() { Width = 1280, Height = 900 }
                });
                await InstallObserverAsync(context);
                var page = await context.NewPageAsync();
                await ApplyProfileAsync(page);
                var response = await page.GotoAsync(path, new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 120_000 });
                Assert.NotNull(response);
                Assert.True(response.Ok, $"{routeId} document returned HTTP {response.Status}.");
                // An empty plan is a successful, usable terminal state for STU-04/STU-05.
                // Measure it instead of waiting for data that the seeded student may not own.
                await page.Locator($"[data-route-id='{routeId}'][data-state='success'], " +
                                   $"[data-route-id='{routeId}'][data-state='empty'], " +
                                   $"[data-route-id='{routeId}'][data-state='stale']")
                    .WaitForAsync(new() { Timeout = 120_000 });
                await page.WaitForTimeoutAsync(250);
                var lcp = await page.EvaluateAsync<double>("() => window.__srsLiveLcp || performance.getEntriesByName('first-contentful-paint')[0]?.startTime || 0");
                Assert.True(lcp > 0, $"{routeId} did not expose LCP or FCP.");
                lcpSamples.Add(lcp);
                apiSamples.AddRange(await page.EvaluateAsync<double[]>(
                    "() => performance.getEntriesByType('resource').filter(entry => entry.name.includes('/api/')).map(entry => entry.duration)"));
            }

            var lcpP75 = Percentile(lcpSamples, .75);
            var apiP95 = apiSamples.Count == 0 ? 0 : Percentile(apiSamples, .95);
            routeEvidence.Add(new(routeId, path, lcpSamples, lcpP75, apiSamples.Count, apiP95));
            output.WriteLine($"{routeId}: lcpP75Ms={lcpP75:F1}; apiP95Ms={apiP95:F1}; apiSamples={apiSamples.Count}");
        }

        var evidence = new LiveEvidence(
            "spec003-live-lcp-evidence/1.0",
            DateTime.UtcNow,
            origin!.GetLeftPart(UriPartial.Authority),
            "Google Chrome Stable",
            4,
            LcpP75BudgetMilliseconds,
            routeEvidence);
        var outputPath = RepositoryFiles.PathTo(".local/performance-evidence/spec003-live-lcp.json");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(evidence, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) + Environment.NewLine);
        output.WriteLine($"Evidence: {outputPath}");

        foreach (var route in routeEvidence)
        {
            Assert.True(route.LcpP75Milliseconds <= LcpP75BudgetMilliseconds,
                $"{route.RouteId} LCP p75 {route.LcpP75Milliseconds:F1}ms exceeds {LcpP75BudgetMilliseconds:F0}ms. API p95 {route.ApiP95Milliseconds:F1}ms is reported separately.");
        }
    }

    private static async Task<string> AuthenticateAsync(IBrowser browser, Uri origin, string studentId, string password)
    {
        await using var context = await browser.NewContextAsync(new()
        {
            BaseURL = origin.AbsoluteUri,
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 1280, Height = 900 }
        });
        var page = await context.NewPageAsync();
        var response = await page.GotoAsync("/student/login", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 120_000 });
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Student login document returned HTTP {response.Status}.");
        await page.Locator("#student-university-id").FillAsync(studentId);
        await page.Locator("#student-password").FillAsync(password);
        await page.Locator("form[data-testid='student-login-form'] button[type='submit']").ClickAsync();
        await page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/student", new() { Timeout = 60_000 });
        return await context.StorageStateAsync();
    }

    private static Task InstallObserverAsync(IBrowserContext context) => context.AddInitScriptAsync("""
        Object.defineProperty(navigator, 'hardwareConcurrency', { get: () => 4 });
        Object.defineProperty(navigator, 'deviceMemory', { get: () => 4 });
        window.__srsLiveLcp = 0;
        new PerformanceObserver(entries => {
          for (const entry of entries.getEntries()) window.__srsLiveLcp = Math.max(window.__srsLiveLcp, entry.startTime);
        }).observe({ type: 'largest-contentful-paint', buffered: true });
        """);

    private static async Task ApplyProfileAsync(IPage page)
    {
        var session = await page.Context.NewCDPSessionAsync(page);
        await session.SendAsync("Network.enable");
        await session.SendAsync("Network.emulateNetworkConditions", new Dictionary<string, object>
        {
            ["offline"] = false,
            ["latency"] = 100,
            ["downloadThroughput"] = 10_000_000d / 8d,
            ["uploadThroughput"] = 2_000_000d / 8d,
            ["connectionType"] = "wifi"
        });
        await session.SendAsync("Emulation.setCPUThrottlingRate", new Dictionary<string, object> { ["rate"] = 2 });
    }

    private static string Required(string name) =>
        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name))
            ? throw new InvalidOperationException($"Required live LCP environment variable is missing: {name}.")
            : Environment.GetEnvironmentVariable(name)!;

    private static double Percentile(IEnumerable<double> values, double percentile)
    {
        var ordered = values.Order().ToArray();
        var index = (int)Math.Ceiling(percentile * ordered.Length) - 1;
        return ordered[Math.Clamp(index, 0, ordered.Length - 1)];
    }

    private sealed record RouteEvidence(string RouteId, string Path, IReadOnlyList<double> LcpSamplesMilliseconds,
        double LcpP75Milliseconds, int ApiSampleCount, double ApiP95Milliseconds);
    private sealed record LiveEvidence(string SchemaVersion, DateTime RecordedAtUtc, string Origin, string Browser,
        int SamplesPerRoute, double LcpP75BudgetMilliseconds, IReadOnlyList<RouteEvidence> Routes);
}
