using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.E2ETests.Specs.Spec011;
using StudentRegistration.E2ETests.Specs.Spec012;
using StudentRegistration.E2ETests.Specs.Spec014;
using Xunit.Abstractions;

namespace StudentRegistration.E2ETests.Performance;

[Collection(Spec003PublishedBrowserCollection.CollectionName)]
public sealed class Spec003LcpEvidenceTests(
    Spec003PublishedBrowserFixture fixture,
    ITestOutputHelper output)
{
    private const double ApiP95ThresholdMilliseconds = 300;
    private const double ApprovedDemoObservedApiP95Milliseconds = 541.7;

    [Fact]
    public async Task Published_release_host_reports_brotli_payload_and_cold_start_breakdown()
    {
        await using var context = await fixture.OpenContextAsync(1280, 900);
        await InstallPerformanceObserverAsync(context);
        var encodings = new ConcurrentDictionary<string, byte>(
            StringComparer.OrdinalIgnoreCase);
        var page = await context.NewPageAsync();
        page.Response += (_, response) => CaptureCompression(response, encodings);
        await ApplyProfileAsync(page);
        await ConfigureAsync(page, "STU-02");

        var documentResponse = await page.GotoAsync(
            Path("STU-02"),
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.NotNull(documentResponse);
        Assert.Equal("true", documentResponse.Headers["x-srs-published-release"]);
        await page.Locator("[data-route-id='STU-02'][data-state='success']")
            .WaitForAsync(new LocatorWaitForOptions { Timeout = 120_000 });
        await page.WaitForTimeoutAsync(250);

        var snapshot = await page.EvaluateAsync<JsonElement>("""
            () => {
              const resources = performance.getEntriesByType('resource');
              const navigation = performance.getEntriesByType('navigation')[0];
              const fcp = performance.getEntriesByType('paint')
                .find(entry => entry.name === 'first-contentful-paint')?.startTime || 0;
              return {
                lcpMilliseconds: window.__srsLcp || fcp,
                lcpEntry: window.__srsLcpEntry,
                fcpMilliseconds: fcp,
                domContentLoadedMilliseconds: navigation?.domContentLoadedEventEnd || 0,
                loadMilliseconds: navigation?.loadEventEnd || 0,
                totalEncodedBytes: resources.reduce(
                  (sum, entry) => sum + (entry.encodedBodySize || 0), 0),
                frameworkEncodedBytes: resources
                  .filter(entry => entry.name.includes('/_framework/'))
                  .reduce((sum, entry) => sum + (entry.encodedBodySize || 0), 0),
                frameworkResourceCount: resources
                  .filter(entry => entry.name.includes('/_framework/')).length,
                apiResourceCount: resources
                  .filter(entry => entry.name.includes('/api/')).length
              };
            }
            """);
        output.WriteLine(
            $"Published Release diagnostic: {snapshot.GetRawText()}, " +
            $"encodings=[{string.Join(',', encodings.Keys.Order(StringComparer.OrdinalIgnoreCase))}]");

        Assert.True(snapshot.GetProperty("lcpMilliseconds").GetDouble() > 0);
        Assert.True(snapshot.GetProperty("frameworkEncodedBytes").GetDouble() > 0);
        Assert.Contains(encodings.Keys, value =>
            string.Equals(value, "br", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Discovery_schedule_and_review_meet_the_cold_cache_p75_lcp_budget()
    {
        Assert.Equal(
            "Release",
            fixture.BuildConfiguration);
        Assert.Equal(
            "Google Chrome Stable",
            fixture.BrowserTarget);

        var results = new Dictionary<string, List<double>>(StringComparer.Ordinal);
        var measurements = new Dictionary<string, List<PerformanceMeasurement>>(StringComparer.Ordinal);
        var apiDurations = new List<double>();
        var compressionEncodings = new ConcurrentDictionary<string, byte>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var route in new[] { "STU-02", "STU-04", "STU-05" })
        {
            results[route] = [];
            measurements[route] = [];
            for (var sample = 0; sample < 4; sample++)
            {
                await using var context = await fixture.OpenContextAsync(1280, 900);
                await InstallPerformanceObserverAsync(context);
                var page = await context.NewPageAsync();
                page.Response += (_, response) =>
                    CaptureCompression(response, compressionEncodings);
                await ApplyProfileAsync(page);
                await ConfigureAsync(page, route);

                var documentResponse = await page.GotoAsync(
                    Path(route),
                    new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                Assert.NotNull(documentResponse);
                Assert.Equal("true", documentResponse.Headers["x-srs-published-release"]);
                await page.Locator($"[data-route-id='{route}'][data-state='success']").WaitForAsync(
                    new LocatorWaitForOptions { Timeout = 120_000 });
                await page.WaitForTimeoutAsync(250);
                var measurement = await ReadPerformanceMeasurementAsync(page);
                Assert.True(measurement.LcpMilliseconds > 0,
                    $"{route} did not expose an LCP/FCP performance entry.");
                results[route].Add(measurement.LcpMilliseconds);
                measurements[route].Add(measurement);
                apiDurations.AddRange(await page.EvaluateAsync<double[]>(
                    "() => performance.getEntriesByType('resource').filter(e => e.name.includes('/api/')).map(e => e.duration)"));
            }
        }

        foreach (var (route, samples) in results)
        {
            var p75 = Percentile(samples, .75);
            output.WriteLine($"{route}: samplesMs=[{string.Join(',', samples.Select(value => value.ToString("F1")))}], p75Ms={p75:F1}");
        }

        var apiP95 = apiDurations.Count == 0 ? 0 : Percentile(apiDurations, .95);
        Assert.NotEmpty(apiDurations);
        Assert.NotEmpty(compressionEncodings);
        var lcpThresholdExceeded = results.Values.Any(
            samples => Percentile(samples, .75) > 2_500);
        var apiThresholdExceeded = apiP95 > ApiP95ThresholdMilliseconds;
        var waiverRequired = lcpThresholdExceeded || apiThresholdExceeded;
        if (waiverRequired)
        {
            AssertApprovedDemoWaiver();
        }
        output.WriteLine(waiverRequired
            ? "NFR-6 LCP and/or API threshold exceeded; recording the explicit non-production demo waiver."
            : "NFR-6 LCP threshold met; recording a measured PASS.");

        var writeEvidence = string.Equals(
                Environment.GetEnvironmentVariable("SPEC003_WRITE_PERF_EVIDENCE"),
                "1",
                StringComparison.Ordinal);
        if (writeEvidence && waiverRequired)
        {
            output.WriteLine(
                "The approved WAIVED-DEMO record was not overwritten by a later diagnostic measurement.");
        }
        else if (writeEvidence)
        {
            var evidence = new
            {
                schemaVersion = "spec003-lcp-evidence/1.0",
                recordedOn = "2026-07-20",
                result = waiverRequired ? "WAIVED-DEMO" : "PASS",
                configuration = "Release",
                browserTarget = fixture.BrowserTarget,
                publishedReleaseHostVerified = true,
                productionCompressionVerified = true,
                thresholdMilliseconds = 2_500,
                thresholdExceeded = lcpThresholdExceeded,
                productionGoLiveApproved = false,
                observedCompressionEncodings = compressionEncodings.Keys
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                profile = new
                {
                    kind = "simulated-browser-client-profile",
                    logicalCores = 4,
                    memoryGb = 4,
                    cpuThrottlingRate = 2,
                    downloadMbps = 10,
                    uploadMbps = 2,
                    roundTripLatencyMs = 100,
                    coldBrowserCache = true
                },
                apiP95Milliseconds = Math.Round(apiP95, 1),
                apiThresholdMilliseconds = ApiP95ThresholdMilliseconds,
                apiThresholdExceeded,
                apiSamples = apiDurations.Count,
                apiEvidenceScope = "deterministic Playwright API fixtures; live SQL/API latency is not claimed",
                routes = results.Select(pair => new
                {
                    routeId = pair.Key,
                    canonicalSuccessStateVerified = true,
                    samplesMilliseconds = pair.Value.Select(value => Math.Round(value, 1)).ToArray(),
                    p75Milliseconds = Math.Round(Percentile(pair.Value, .75), 1),
                    measurements = measurements[pair.Key]
                }).ToArray(),
                waiver = (object?)null
            };
            File.WriteAllText(
                StudentRegistration.TestSupport.RepositoryFiles.PathTo(
                    "docs/release-evidence/SPEC-003-NFR-6-results.json"),
                JsonSerializer.Serialize(evidence, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) + Environment.NewLine);
        }
    }

    private static void AssertApprovedDemoWaiver()
    {
        using var document = JsonDocument.Parse(
            StudentRegistration.TestSupport.RepositoryFiles.Read(
                "docs/release-evidence/SPEC-003-NFR-6-results.json"));
        var root = document.RootElement;
        Assert.Equal("WAIVED-DEMO", root.GetProperty("result").GetString());
        Assert.False(root.GetProperty("productionGoLiveApproved").GetBoolean());
        Assert.Equal(
            ApprovedDemoObservedApiP95Milliseconds,
            root.GetProperty("approvedDemoObservedApiP95Milliseconds").GetDouble());
        var waiver = root.GetProperty("waiver");
        Assert.Equal("Ahmed ELbamby", waiver.GetProperty("approvedBy").GetString());
        Assert.Equal("2026-07-20", waiver.GetProperty("approvedOn").GetString());
    }

    private static async Task<PerformanceMeasurement> ReadPerformanceMeasurementAsync(IPage page)
    {
        var snapshot = await page.EvaluateAsync<JsonElement>("""
            () => {
              const resources = performance.getEntriesByType('resource');
              const fcp = performance.getEntriesByType('paint')
                .find(entry => entry.name === 'first-contentful-paint')?.startTime || 0;
              const lcpEntry = window.__srsLcpEntry;
              return {
                lcpMilliseconds: window.__srsLcp || fcp,
                fcpMilliseconds: fcp,
                totalEncodedBytes: resources.reduce(
                  (sum, entry) => sum + (entry.encodedBodySize || 0), 0),
                frameworkEncodedBytes: resources
                  .filter(entry => entry.name.includes('/_framework/'))
                  .reduce((sum, entry) => sum + (entry.encodedBodySize || 0), 0),
                frameworkResourceCount: resources
                  .filter(entry => entry.name.includes('/_framework/')).length,
                lcpEntrySize: lcpEntry?.size || 0,
                lcpEntryTagName: lcpEntry?.tagName || null,
                lcpEntryCurrentSrc: lcpEntry?.currentSrc || null
              };
            }
            """);
        return new PerformanceMeasurement(
            snapshot.GetProperty("lcpMilliseconds").GetDouble(),
            snapshot.GetProperty("fcpMilliseconds").GetDouble(),
            snapshot.GetProperty("totalEncodedBytes").GetDouble(),
            snapshot.GetProperty("frameworkEncodedBytes").GetDouble(),
            snapshot.GetProperty("frameworkResourceCount").GetInt32(),
            snapshot.GetProperty("lcpEntrySize").GetDouble(),
            snapshot.GetProperty("lcpEntryTagName").GetString(),
            snapshot.GetProperty("lcpEntryCurrentSrc").GetString());
    }

    private static Task InstallPerformanceObserverAsync(IBrowserContext context) =>
        context.AddInitScriptAsync("""
            Object.defineProperty(navigator, 'hardwareConcurrency', { get: () => 4 });
            Object.defineProperty(navigator, 'deviceMemory', { get: () => 4 });
            window.__srsLcp = 0;
            window.__srsLcpEntry = null;
            new PerformanceObserver(entries => {
              for (const entry of entries.getEntries()) {
                if (entry.startTime >= window.__srsLcp) {
                  window.__srsLcp = entry.startTime;
                  window.__srsLcpEntry = {
                    startTime: entry.startTime,
                    size: entry.size,
                    tagName: entry.element?.tagName || null,
                    id: entry.element?.id || null,
                    className: entry.element?.className || null,
                    currentSrc: entry.element?.currentSrc || null
                  };
                }
              }
            }).observe({ type: 'largest-contentful-paint', buffered: true });
            """);

    private static void CaptureCompression(
        IResponse response,
        ConcurrentDictionary<string, byte> encodings)
    {
        if (!response.Url.EndsWith(".wasm", StringComparison.OrdinalIgnoreCase)
            || !response.Headers.TryGetValue("content-encoding", out var encoding)
            || (!encoding.Contains("br", StringComparison.OrdinalIgnoreCase)
                && !encoding.Contains("gzip", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        encodings.TryAdd(encoding, 0);
    }

    private sealed record PerformanceMeasurement(
        double LcpMilliseconds,
        double FcpMilliseconds,
        double TotalEncodedBytes,
        double FrameworkEncodedBytes,
        int FrameworkResourceCount,
        double LcpEntrySize,
        string? LcpEntryTagName,
        string? LcpEntryCurrentSrc);

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
                    Invoke<string>(
                        typeof(ScheduleBuilderPageFeatureTests),
                        "CurrentPlan",
                        "00000000-0000-0000-0000-000000012201",
                        "G01",
                        "PLAN-RV-1"))));
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
