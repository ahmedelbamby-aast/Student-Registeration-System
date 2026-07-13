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
            "not-collected-routes-design-only",
            manifest.GetProperty("status").GetString());
        Assert.False(manifest.GetProperty("automaticReplacementAllowed").GetBoolean());
        Assert.Equal("Ahmed ELbamby", manifest.GetProperty("approvalAuthority").GetString());
        Assert.Empty(manifest.GetProperty("baselines").EnumerateArray());

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

        var dataModel = RepositoryFiles.Read("specs/003-ux-storyboard-accessibility/data-model.md");
        RepositoryFiles.ContainsAll(
            dataModel,
            "Every visual baseline MUST record route, state, viewport",
            "approval actor, and approval date",
            "automatic baseline replacement is prohibited");
    }
}
