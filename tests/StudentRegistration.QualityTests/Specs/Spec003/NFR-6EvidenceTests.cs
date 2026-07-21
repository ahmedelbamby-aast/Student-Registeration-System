using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class NFR_6EvidenceTests
{
    [Fact]
    public void Real_composed_host_records_the_exact_demo_waiver_without_weakening_the_budget()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-003-NFR-6-live-results.json"));
        var root = document.RootElement;

        Assert.Equal("WAIVED-DEMO", root.GetProperty("result").GetString());
        Assert.True(root.GetProperty("publishedReleaseHostVerified").GetBoolean());
        Assert.True(root.GetProperty("brotliCompressionVerified").GetBoolean());
        Assert.Equal(2_500, root.GetProperty("lcpP75BudgetMilliseconds").GetInt32());
        Assert.False(root.GetProperty("productionGoLiveApproved").GetBoolean());

        var routes = root.GetProperty("routes").EnumerateArray().ToArray();
        Assert.Equal(["STU-02", "STU-04", "STU-05"],
            routes.Select(route => route.GetProperty("routeId").GetString()!).ToArray());
        Assert.All(routes, route => Assert.Equal(4,
            route.GetProperty("lcpSamplesMilliseconds").GetArrayLength()));
        Assert.False(routes[0].GetProperty("thresholdPassed").GetBoolean());
        Assert.True(routes[1].GetProperty("thresholdPassed").GetBoolean());
        Assert.True(routes[2].GetProperty("thresholdPassed").GetBoolean());

        var waiver = root.GetProperty("waiver");
        Assert.Equal("Ahmed ELbamby", waiver.GetProperty("approvedBy").GetString());
        Assert.Equal("2026-07-21", waiver.GetProperty("approvedOn").GetString());
        Assert.Contains("threshold remains unchanged", waiver.GetProperty("reason").GetString(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Release_profile_cold_cache_lcp_evidence_is_pass_or_explicit_demo_waiver()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-003-NFR-6-results.json"));
        var root = document.RootElement;
        var result = root.GetProperty("result").GetString();
        Assert.True(result is "PASS" or "WAIVED-DEMO");
        Assert.Equal("Release", root.GetProperty("configuration").GetString());
        Assert.True(root.GetProperty("publishedReleaseHostVerified").GetBoolean());
        Assert.True(root.GetProperty("productionCompressionVerified").GetBoolean());
        Assert.Equal(2_500, root.GetProperty("thresholdMilliseconds").GetInt32());
        var thresholdExceeded = root.GetProperty("thresholdExceeded").GetBoolean();
        if (result == "PASS")
        {
            Assert.False(thresholdExceeded);
        }
        else
        {
            Assert.True(thresholdExceeded);
        }
        Assert.False(root.GetProperty("productionGoLiveApproved").GetBoolean());
        var observedEncodings = root.GetProperty("observedCompressionEncodings")
            .EnumerateArray()
            .Select(value => value.GetString())
            .ToArray();
        Assert.Contains(observedEncodings, value =>
            string.Equals(value, "br", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "gzip", StringComparison.OrdinalIgnoreCase));
        var profile = root.GetProperty("profile");
        Assert.Equal("simulated-browser-client-profile", profile.GetProperty("kind").GetString());
        Assert.Equal(4, profile.GetProperty("logicalCores").GetInt32());
        Assert.Equal(4, profile.GetProperty("memoryGb").GetInt32());
        Assert.Equal(2, profile.GetProperty("cpuThrottlingRate").GetInt32());
        Assert.Equal(10, profile.GetProperty("downloadMbps").GetInt32());
        Assert.Equal(2, profile.GetProperty("uploadMbps").GetInt32());
        Assert.Equal(100, profile.GetProperty("roundTripLatencyMs").GetInt32());
        Assert.True(profile.GetProperty("coldBrowserCache").GetBoolean());
        var apiP95 = root.GetProperty("apiP95Milliseconds").GetDouble();
        var apiThreshold = root.GetProperty("apiThresholdMilliseconds").GetDouble();
        var apiThresholdExceeded = root.GetProperty("apiThresholdExceeded").GetBoolean();
        var approvedDemoObservedApiP95 = root
            .GetProperty("approvedDemoObservedApiP95Milliseconds")
            .GetDouble();
        Assert.Equal(300, apiThreshold);
        Assert.Equal(541.7, approvedDemoObservedApiP95);
        Assert.Equal(approvedDemoObservedApiP95, apiP95);
        Assert.Equal(apiP95 > apiThreshold, apiThresholdExceeded);
        if (result == "PASS")
        {
            Assert.True(apiP95 <= apiThreshold);
        }
        Assert.True(root.GetProperty("apiSamples").GetInt32() > 0);
        Assert.Equal(
            "deterministic Playwright API fixtures; live SQL/API latency is not claimed",
            root.GetProperty("apiEvidenceScope").GetString());

        var routes = root.GetProperty("routes").EnumerateArray().ToArray();
        Assert.Equal(["STU-02", "STU-04", "STU-05"],
            routes.Select(route => route.GetProperty("routeId").GetString()!).ToArray());
        Assert.All(routes, route =>
        {
            Assert.True(route.GetProperty("canonicalSuccessStateVerified").GetBoolean());
            Assert.Equal(4, route.GetProperty("samplesMilliseconds").GetArrayLength());
            Assert.Equal(4, route.GetProperty("measurements").GetArrayLength());
            Assert.True(route.GetProperty("p75Milliseconds").GetDouble() > 0);
            if (result == "PASS")
            {
                Assert.True(route.GetProperty("p75Milliseconds").GetDouble() <= 2_500);
            }
            else
            {
                Assert.True(thresholdExceeded);
            }
        });

        if (result == "WAIVED-DEMO")
        {
            var waiver = root.GetProperty("waiver");
            Assert.Equal("Ahmed ELbamby", waiver.GetProperty("approvedBy").GetString());
            Assert.Equal("2026-07-20", waiver.GetProperty("approvedOn").GetString());
            Assert.Contains("Non-production", waiver.GetProperty("reason").GetString());
        }
    }
}
