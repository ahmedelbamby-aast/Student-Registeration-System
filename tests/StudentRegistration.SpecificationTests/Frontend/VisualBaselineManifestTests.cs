using System.Security.Cryptography;
using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Frontend;

public sealed class VisualBaselineManifestTests
{
    private const string ManifestPath =
        "tests/StudentRegistration.VisualTests/Baselines/baseline-manifest.json";

    [Fact]
    public void Registry_governs_provenance_approval_viewports_and_forbids_automatic_replacement()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        var root = document.RootElement;

        Assert.Equal("visual-baselines/1.0.0", root.GetProperty("version").GetString());
        Assert.False(root.GetProperty("automaticReplacementAllowed").GetBoolean());
        Assert.Equal("Ahmed ELbamby", root.GetProperty("approvalAuthority").GetString());
        Assert.Equal(
            [375, 768, 1280, 1920],
            root.GetProperty("visualViewports").EnumerateArray()
                .Select(viewport => viewport.GetInt32())
                .ToArray());

        var required = root.GetProperty("requiredEvidenceFields")
            .EnumerateArray()
            .Select(field => field.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        string[] expected =
        [
            "routeId", "state", "browserName", "browserBuild", "engine",
            "osImage", "viewport", "tokenVersion", "fixtureVersion",
            "artifactSha256", "approvedBy", "approvedOn"
        ];
        Assert.All(expected, field => Assert.Contains(field, required));
    }

    [Fact]
    public void Registry_contains_the_explicitly_approved_spec008_baseline_sets()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        var root = document.RootElement;

        Assert.Equal("partially-approved-routes", root.GetProperty("status").GetString());
        var baselines = root.GetProperty("baselines").EnumerateArray()
            .Where(item => item.GetProperty("targetManifest").GetString()!
                .StartsWith("Spec008/", StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(
            ["AUTH-01", "STU-01", "ADM-02", "ADM-04"],
            baselines.Select(item => item.GetProperty("routeId").GetString()!).ToArray());

        Assert.Equal(
            "Spec008/AUTH-01/baseline-targets.json",
            baselines[0].GetProperty("targetManifest").GetString());
        Assert.Equal(
            "29954198b34f08f56174902f08440bf1f8620731b62b4e7fece75551b08957bf",
            baselines[0].GetProperty("artifactSha256").GetString());
        Assert.Equal(
            "Spec008/STU-01/baseline-targets.json",
            baselines[1].GetProperty("targetManifest").GetString());
        Assert.Equal(
            "072002fbe932665081a7b00bdff74dff6f5376646362e4dff2c01d42e5561a28",
            baselines[1].GetProperty("artifactSha256").GetString());
        Assert.Equal(
            "Spec008/ADM-02/baseline-targets.json",
            baselines[2].GetProperty("targetManifest").GetString());
        Assert.Equal(
            "aa4d4e66fe37644032cf6a579f99d56c8e72b01bcac131fba4543a4a29c6d817",
            baselines[2].GetProperty("artifactSha256").GetString());
        Assert.Equal(
            "Spec008/ADM-04/baseline-targets.json",
            baselines[3].GetProperty("targetManifest").GetString());
        Assert.Equal(
            "b791f8a05b4cac4a737ed655ec162bd53bd5ce2d3e5b2ad7de594a4af0151534",
            baselines[3].GetProperty("artifactSha256").GetString());

        foreach (var baseline in baselines)
        {
            Assert.Equal("success", baseline.GetProperty("state").GetString());
            Assert.Equal("Ahmed ELbamby", baseline.GetProperty("approvedBy").GetString());
            Assert.Equal("2026-07-19", baseline.GetProperty("approvedOn").GetString());
            Assert.Equal(16, baseline.GetProperty("targetCount").GetInt32());
            Assert.Equal(
                [375, 768, 1280, 1920],
                baseline.GetProperty("viewport").EnumerateArray()
                    .Select(viewport => viewport.GetInt32())
                    .ToArray());

            var targetManifestPath = RepositoryFiles.PathTo(
                $"tests/StudentRegistration.VisualTests/Baselines/{baseline.GetProperty("targetManifest").GetString()}");
            Assert.True(File.Exists(targetManifestPath));
            Assert.Equal(
                Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(targetManifestPath)))
                    .ToLowerInvariant(),
                baseline.GetProperty("artifactSha256").GetString());

            foreach (var field in root.GetProperty("requiredEvidenceFields").EnumerateArray())
            {
                Assert.True(baseline.TryGetProperty(field.GetString()!, out _));
            }
        }
    }
}
