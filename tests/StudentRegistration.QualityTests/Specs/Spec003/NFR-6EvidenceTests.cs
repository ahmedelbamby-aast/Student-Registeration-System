using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class NFR_6EvidenceTests
{
    [Fact]
    public void Release_profile_cold_cache_lcp_p75_is_within_budget_for_all_three_routes()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-003-NFR-6-results.json"));
        var root = document.RootElement;
        Assert.Equal("PASS", root.GetProperty("result").GetString());
        Assert.Equal("Release", root.GetProperty("configuration").GetString());
        Assert.True(root.GetProperty("productionCompressionVerified").GetBoolean());
        var profile = root.GetProperty("profile");
        Assert.Equal(4, profile.GetProperty("logicalCores").GetInt32());
        Assert.Equal(4, profile.GetProperty("memoryGb").GetInt32());
        Assert.Equal(10, profile.GetProperty("downloadMbps").GetInt32());
        Assert.Equal(2, profile.GetProperty("uploadMbps").GetInt32());
        Assert.Equal(100, profile.GetProperty("roundTripLatencyMs").GetInt32());
        Assert.True(root.GetProperty("apiP95Milliseconds").GetDouble() <= 300);

        var routes = root.GetProperty("routes").EnumerateArray().ToArray();
        Assert.Equal(["STU-02", "STU-04", "STU-05"],
            routes.Select(route => route.GetProperty("routeId").GetString()!).ToArray());
        Assert.All(routes, route =>
        {
            Assert.True(route.GetProperty("samplesMilliseconds").GetArrayLength() >= 4);
            Assert.True(route.GetProperty("p75Milliseconds").GetDouble() <= 2_500);
        });
    }
}
