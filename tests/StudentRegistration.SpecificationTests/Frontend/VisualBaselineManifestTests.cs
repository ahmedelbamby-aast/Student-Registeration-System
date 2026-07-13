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
    public void Registry_claims_no_baseline_before_route_readiness_and_real_approval()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        var root = document.RootElement;

        Assert.Equal("not-collected-routes-design-only", root.GetProperty("status").GetString());
        Assert.Empty(root.GetProperty("baselines").EnumerateArray());
    }
}
