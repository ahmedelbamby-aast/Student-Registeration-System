using System.Security.Cryptography;
using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class NFR_9EvidenceTests
{
    [Fact]
    public void All_30_routes_have_approved_four_browser_by_four_viewport_hash_bound_baselines()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "tests/StudentRegistration.VisualTests/Baselines/baseline-manifest.json"));
        var collectionName = document.RootElement.TryGetProperty("routes", out _)
            ? "routes"
            : "baselines";
        var routes = document.RootElement.GetProperty(collectionName).EnumerateArray().ToArray();
        Assert.Equal(30, routes.Select(route => route.GetProperty("routeId").GetString()).Distinct().Count());
        foreach (var route in routes)
        {
            Assert.Equal([375, 768, 1280, 1920],
                route.GetProperty("viewport").EnumerateArray().Select(value => value.GetInt32()).ToArray());
            Assert.Equal(16, route.GetProperty("targetCount").GetInt32());
            Assert.Equal("Ahmed ELbamby", route.GetProperty("approvedBy").GetString());
            var targetPath = "tests/StudentRegistration.VisualTests/Baselines/" +
                route.GetProperty("targetManifest").GetString();
            using var targetsDocument = JsonDocument.Parse(RepositoryFiles.Read(targetPath));
            var targets = targetsDocument.RootElement.GetProperty("targets").EnumerateArray().ToArray();
            Assert.Equal(16, targets.Length);
            foreach (var target in targets)
            {
                var file = Path.Combine(
                    Path.GetDirectoryName(RepositoryFiles.PathTo(targetPath))!,
                    target.GetProperty("file").GetString()!);
                var actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant();
                Assert.Equal(target.GetProperty("sha256").GetString(), actual);
            }
        }
    }
}
