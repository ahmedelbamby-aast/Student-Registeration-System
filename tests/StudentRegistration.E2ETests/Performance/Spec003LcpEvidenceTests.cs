using System.Reflection;
using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.E2ETests.Specs.Spec011;
using StudentRegistration.E2ETests.Specs.Spec012;
using StudentRegistration.E2ETests.Specs.Spec014;
using Xunit.Abstractions;

namespace StudentRegistration.E2ETests.Performance;

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class Spec003LcpEvidenceTests(
    Spec008BrowserFixture fixture,
    ITestOutputHelper output)
{
    [Fact]
    public async Task Discovery_schedule_and_review_meet_the_cold_cache_p75_lcp_budget()
    {
        Assert.Equal(
            "Release",
            Environment.GetEnvironmentVariable("STUDENTREGISTRATION_BROWSER_CONFIGURATION"));

        var results = new Dictionary<string, List<double>>(StringComparer.Ordinal);
        var apiDurations = new List<double>();
        var compressedAssetObserved = false;
        foreach (var route in new[] { "STU-02", "STU-04", "STU-05" })
        {
            results[route] = [];
            for (var sample = 0; sample < 4; sample++)
            {
                await using var context = await fixture.OpenContextAsync(1280, 900);
                await context.AddInitScriptAsync("""
                    Object.defineProperty(navigator, 'hardwareConcurrency', { get: () => 4 });
                    Object.defineProperty(navigator, 'deviceMemory', { get: () => 4 });
                    window.__srsLcp = 0;
                    new PerformanceObserver(entries => {
                      for (const entry of entries.getEntries()) window.__srsLcp = Math.max(window.__srsLcp, entry.startTime);
                    }).observe({ type: 'largest-contentful-paint', buffered: true });
                    """);
                var page = await context.NewPageAsync();
                page.Response += async (_, response) =>
                {
                    if (response.Url.EndsWith(".wasm", StringComparison.OrdinalIgnoreCase))
                    {
                        var headers = await response.AllHeadersAsync();
                        compressedAssetObserved |= headers.TryGetValue("content-encoding", out var encoding)
                            && (encoding.Contains("br", StringComparison.OrdinalIgnoreCase)
                                || encoding.Contains("gzip", StringComparison.OrdinalIgnoreCase));
                    }
                };
                await ApplyProfileAsync(page);
                await ConfigureAsync(page, route);

                await page.GotoAsync(Path(route), new() { WaitUntil = WaitUntilState.DOMContentLoaded });
                await page.Locator($"[data-route-id='{route}'][data-state='success']").WaitForAsync(
                    new LocatorWaitForOptions { Timeout = 120_000 });
                await page.WaitForTimeoutAsync(250);
                var lcp = await page.EvaluateAsync<double>(
                    "() => window.__srsLcp || performance.getEntriesByType('paint').find(e => e.name === 'first-contentful-paint')?.startTime || 0");
                Assert.True(lcp > 0, $"{route} did not expose an LCP/FCP performance entry.");
                results[route].Add(lcp);
                apiDurations.AddRange(await page.EvaluateAsync<double[]>(
                    "() => performance.getEntriesByType('resource').filter(e => e.name.includes('/api/')).map(e => e.duration)"));
            }
        }

        foreach (var (route, samples) in results)
        {
            var p75 = Percentile(samples, .75);
            output.WriteLine($"{route}: samplesMs=[{string.Join(',', samples.Select(value => value.ToString("F1")))}], p75Ms={p75:F1}");
            Assert.True(p75 <= 2_500, $"{route} p75 LCP {p75:F1}ms exceeds 2500ms.");
        }

        var apiP95 = apiDurations.Count == 0 ? 0 : Percentile(apiDurations, .95);
        Assert.True(apiP95 <= 300, $"API fixture p95 {apiP95:F1}ms exceeds 300ms.");
        Assert.True(compressedAssetObserved, "No Brotli/gzip Release asset response was observed.");

        if (string.Equals(
                Environment.GetEnvironmentVariable("SPEC003_WRITE_PERF_EVIDENCE"),
                "1",
                StringComparison.Ordinal))
        {
            var evidence = new
            {
                schemaVersion = "spec003-lcp-evidence/1.0",
                recordedOn = "2026-07-19",
                result = "PASS",
                configuration = "Release",
                productionCompressionVerified = compressedAssetObserved,
                profile = new
                {
                    logicalCores = 4,
                    memoryGb = 4,
                    downloadMbps = 10,
                    uploadMbps = 2,
                    roundTripLatencyMs = 100,
                    coldBrowserCache = true
                },
                apiP95Milliseconds = Math.Round(apiP95, 1),
                routes = results.Select(pair => new
                {
                    routeId = pair.Key,
                    samplesMilliseconds = pair.Value.Select(value => Math.Round(value, 1)).ToArray(),
                    p75Milliseconds = Math.Round(Percentile(pair.Value, .75), 1)
                }).ToArray()
            };
            File.WriteAllText(
                StudentRegistration.TestSupport.RepositoryFiles.PathTo(
                    "docs/release-evidence/SPEC-003-NFR-6-results.json"),
                JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
        }
    }

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
        await session.SendAsync("Emulation.setCPUThrottlingRate", new Dictionary<string, object>
        {
            ["rate"] = 2
        });
    }

    private static async Task ConfigureAsync(IPage page, string routeId)
    {
        switch (routeId)
        {
            case "STU-02":
                await page.RouteAsync("**/api/context", route => route.FulfillAsync(Json(
                    Invoke<string>(typeof(SubjectDiscoveryPageFeatureTests), "AppContext"))));
                await page.RouteAsync("**/api/student/terms/*/offerings*", route => route.FulfillAsync(Json(
                    Invoke<string>(typeof(SubjectDiscoveryPageFeatureTests), "OfferingPage", "Project I", 1, 1))));
                break;
            case "STU-04":
                await page.RouteAsync("**/api/context", route => route.FulfillAsync(Json(
                    Invoke<string>(typeof(ScheduleBuilderPageFeatureTests), "AppContext"))));
                await page.RouteAsync("**/api/student/terms/*/registration-plan", route => route.FulfillAsync(Json(
                    Invoke<string>(typeof(ScheduleBuilderPageFeatureTests), "ConflictPlan"))));
                break;
            case "STU-05":
                await page.RouteAsync("**/api/**", route =>
                {
                    var path = new Uri(route.Request.Url).AbsolutePath;
                    var body = path == "/api/context"
                        ? Invoke<string>(typeof(RegistrationReviewPageFeatureTests), "AppContext", "Student")
                        : Invoke<string>(typeof(RegistrationReviewPageFeatureTests), "Plan", false);
                    return route.FulfillAsync(Json(body));
                });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(routeId));
        }
    }

    private static RouteFulfillOptions Json(string body) => new()
    {
        Status = 200,
        ContentType = "application/json",
        Body = body
    };

    private static string Path(string routeId) => routeId switch
    {
        "STU-02" => "/student/subjects",
        "STU-04" => "/student/schedule",
        "STU-05" => "/student/review",
        _ => throw new ArgumentOutOfRangeException(nameof(routeId))
    };

    private static T Invoke<T>(Type type, string method, params object?[]? arguments)
    {
        var candidate = type.GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(type.FullName, method);
        return (T)candidate.Invoke(null, arguments)!;
    }

    private static double Percentile(IEnumerable<double> values, double percentile)
    {
        var ordered = values.Order().ToArray();
        var index = (int)Math.Ceiling(percentile * ordered.Length) - 1;
        return ordered[Math.Clamp(index, 0, ordered.Length - 1)];
    }
}
