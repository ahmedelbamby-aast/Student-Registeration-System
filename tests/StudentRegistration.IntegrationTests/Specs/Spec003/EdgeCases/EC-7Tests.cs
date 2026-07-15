using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec003.EdgeCases;

public sealed class EC_7Tests
{
    [Fact]
    public void Visual_baseline_changes_are_versioned_approved_and_traceable_to_routes_and_states()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "tests/StudentRegistration.VisualTests/Baselines/baseline-manifest.json"));
        var manifest = document.RootElement;

        Assert.Equal("visual-baselines/1.0.0", manifest.GetProperty("version").GetString());
        Assert.Equal(
            "partially-approved-routes",
            manifest.GetProperty("status").GetString());
        Assert.False(manifest.GetProperty("automaticReplacementAllowed").GetBoolean());
        Assert.Equal("Ahmed ELbamby", manifest.GetProperty("approvalAuthority").GetString());
        var approvedSet = Assert.Single(manifest.GetProperty("baselines").EnumerateArray());
        Assert.Equal("AUTH-01", approvedSet.GetProperty("routeId").GetString());
        Assert.Equal("success", approvedSet.GetProperty("state").GetString());
        Assert.Equal("Ahmed ELbamby", approvedSet.GetProperty("approvedBy").GetString());
        Assert.Equal("2026-07-14", approvedSet.GetProperty("approvedOn").GetString());
        Assert.Equal(16, approvedSet.GetProperty("targetCount").GetInt32());

        var requiredEvidence = manifest
            .GetProperty("requiredEvidenceFields")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToHashSet(StringComparer.Ordinal);
        var expectedEvidence = new[]
        {
            "routeId",
            "state",
            "browserName",
            "browserBuild",
            "engine",
            "osImage",
            "viewport",
            "tokenVersion",
            "fixtureVersion",
            "artifactSha256",
            "approvedBy",
            "approvedOn"
        };

        Assert.All(expectedEvidence, field => Assert.Contains(field, requiredEvidence));
        Assert.All(
            expectedEvidence,
            field => Assert.True(approvedSet.TryGetProperty(field, out _)));

        var dataModel = RepositoryFiles.Read("specs/003-ux-storyboard-accessibility/data-model.md");
        RepositoryFiles.ContainsAll(
            dataModel,
            "Every visual baseline MUST record route, state, viewport",
            "approval actor, and approval date",
            "automatic baseline replacement is prohibited");
    }
}
