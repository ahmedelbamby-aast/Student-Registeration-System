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
    public void Registry_contains_only_the_explicitly_approved_spec008_baseline_sets()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        var root = document.RootElement;

        Assert.Equal("partially-approved-routes", root.GetProperty("status").GetString());
        var baselines = root.GetProperty("baselines").EnumerateArray().ToArray();
        Assert.Equal(
            ["AUTH-01", "STU-01", "ADM-02", "ADM-04"],
            baselines.Select(item => item.GetProperty("routeId").GetString()!).ToArray());

        Assert.Equal(
            "Spec008/AUTH-01/baseline-targets.json",
            baselines[0].GetProperty("targetManifest").GetString());
        Assert.Equal(
            "3521d944c1ca61125e8f37544b249ae621e778697ecdd0f87d94662d3ab78b39",
            baselines[0].GetProperty("artifactSha256").GetString());
        Assert.Equal(
            "Spec008/STU-01/baseline-targets.json",
            baselines[1].GetProperty("targetManifest").GetString());
        Assert.Equal(
            "89c913e0780e88d91aa7e55b34c70d0cdb363824e2641c90405eedef9cff0d12",
            baselines[1].GetProperty("artifactSha256").GetString());
        Assert.Equal(
            "Spec008/ADM-02/baseline-targets.json",
            baselines[2].GetProperty("targetManifest").GetString());
        Assert.Equal(
            "65432a3af5a0629cd44bc3afed763e52f770804e855cc121d1b714421300ad72",
            baselines[2].GetProperty("artifactSha256").GetString());
        Assert.Equal(
            "Spec008/ADM-04/baseline-targets.json",
            baselines[3].GetProperty("targetManifest").GetString());
        Assert.Equal(
            "a328536996ac8631ee7d5e713e4a60eb3dc581fbc5ce471d8daa521f132f6f82",
            baselines[3].GetProperty("artifactSha256").GetString());

        foreach (var baseline in baselines)
        {
            Assert.Equal("success", baseline.GetProperty("state").GetString());
            Assert.Equal("Ahmed ELbamby", baseline.GetProperty("approvedBy").GetString());
            Assert.Equal("2026-07-14", baseline.GetProperty("approvedOn").GetString());
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
