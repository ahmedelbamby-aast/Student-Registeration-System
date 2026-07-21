using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec003.EdgeCases;

public sealed class EC_7Tests
{
    private static readonly string[] GovernedRouteIds =
    [
        "AUTH-01", "AUTH-02", "AUTH-03", "AUTH-04", "AUTH-05",
        "STU-01", "STU-02", "STU-03", "STU-04", "STU-05", "STU-06", "STU-07", "STU-08", "STU-09",
        "ADM-01", "ADM-02", "ADM-03", "ADM-04", "ADM-05", "ADM-06", "ADM-07", "ADM-08", "ADM-09", "ADM-10",
        "STF-01", "STF-02", "STF-03", "STF-04", "STF-05", "SYS-01"
    ];

    [Fact]
    public void Visual_baseline_changes_are_versioned_approved_and_traceable_to_routes_and_states()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "tests/StudentRegistration.VisualTests/Baselines/baseline-manifest.json"));
        var manifest = document.RootElement;

        Assert.Equal("visual-baselines/2.0.0", manifest.GetProperty("version").GetString());
        Assert.Equal(
            "approved-routes-with-governed-v2-additions",
            manifest.GetProperty("status").GetString());
        Assert.False(manifest.GetProperty("automaticReplacementAllowed").GetBoolean());
        Assert.Equal("Ahmed ELbamby", manifest.GetProperty("approvalAuthority").GetString());
        var approvedSets = manifest.GetProperty("baselines").EnumerateArray().ToArray();
        Assert.Equal(33, approvedSets.Length);

        var actualRouteIds = approvedSets
            .Select(item => item.GetProperty("routeId").GetString())
            .Where(routeId => routeId is not null)
            .Cast<string>()
            .ToArray();
        Assert.Equal(GovernedRouteIds.Length, actualRouteIds.Distinct(StringComparer.Ordinal).Count());
        Assert.True(
            GovernedRouteIds.ToHashSet(StringComparer.Ordinal).SetEquals(actualRouteIds),
            $"Global baseline route inventory drifted. Actual: {string.Join(", ", actualRouteIds)}");

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
        foreach (var approvedSet in approvedSets)
        {
            Assert.All(
                expectedEvidence,
                field => Assert.True(approvedSet.TryGetProperty(field, out _)));
            Assert.Equal("Ahmed ELbamby", approvedSet.GetProperty("approvedBy").GetString());
            Assert.Equal(16, approvedSet.GetProperty("targetCount").GetInt32());

            var routeId = approvedSet.GetProperty("routeId").GetString()!;
            var targetManifestRelativePath = approvedSet.GetProperty("targetManifest").GetString()!;
            var targetManifestPath = RepositoryFiles.PathTo(
                $"tests/StudentRegistration.VisualTests/Baselines/{targetManifestRelativePath}");
            Assert.True(File.Exists(targetManifestPath), $"Missing target manifest for {routeId}.");

            using var targetManifest = JsonDocument.Parse(File.ReadAllBytes(targetManifestPath));
            var targetRoot = targetManifest.RootElement;
            Assert.Equal(routeId, targetRoot.GetProperty("routeId").GetString());
            Assert.Equal(approvedSet.GetProperty("state").GetString(), targetRoot.GetProperty("state").GetString());
            Assert.Equal("approved", targetRoot.GetProperty("status").GetString());
            Assert.False(targetRoot.GetProperty("automaticReplacementAllowed").GetBoolean());
            Assert.Equal("Ahmed ELbamby", targetRoot.GetProperty("approvedBy").GetString());
            Assert.Equal(approvedSet.GetProperty("approvedOn").GetString(), targetRoot.GetProperty("approvedOn").GetString());

            var targets = targetRoot.GetProperty("targets").EnumerateArray().ToArray();
            Assert.Equal(approvedSet.GetProperty("targetCount").GetInt32(), targets.Length);
            var isV2Spec003Approval =
                targetManifestRelativePath.StartsWith("v2/Spec003/", StringComparison.Ordinal) &&
                string.Equals(approvedSet.GetProperty("approvedOn").GetString(), "2026-07-21", StringComparison.Ordinal);
            var isLegacySpec003HashBoundApproval =
                targetManifestRelativePath.StartsWith("Spec003/", StringComparison.Ordinal) &&
                string.Equals(approvedSet.GetProperty("approvedOn").GetString(), "2026-07-19", StringComparison.Ordinal) &&
                string.Equals(approvedSet.GetProperty("state").GetString(), "denied", StringComparison.Ordinal);
            var targetDigests = new List<string>(targets.Length);
            foreach (var target in targets)
            {
                var fileName = target.GetProperty("file").GetString()!;
                var expectedHash = target.GetProperty("sha256").GetString();
                Assert.False(string.IsNullOrWhiteSpace(expectedHash));
                Assert.Equal(expectedHash, target.GetProperty("artifactSha256").GetString());

                var artifactPath = Path.Combine(Path.GetDirectoryName(targetManifestPath)!, fileName);
                Assert.True(File.Exists(artifactPath), $"Missing baseline artifact {routeId}/{fileName}.");
                if (isV2Spec003Approval || isLegacySpec003HashBoundApproval)
                {
                    var actualHash = Convert.ToHexString(
                        SHA256.HashData(File.ReadAllBytes(artifactPath))).ToLowerInvariant();
                    Assert.Equal(expectedHash, actualHash);
                }
                targetDigests.Add($"{fileName}:{expectedHash}");
            }

            var combinedHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", targetDigests))))
                .ToLowerInvariant();
            var globalArtifactHash = approvedSet.GetProperty("artifactSha256").GetString();
            if (isLegacySpec003HashBoundApproval)
            {
                Assert.Equal(combinedHash, globalArtifactHash);
            }
            else
            {
                var targetManifestHash = Convert.ToHexString(
                    SHA256.HashData(File.ReadAllBytes(targetManifestPath))).ToLowerInvariant();
                Assert.Equal(targetManifestHash, globalArtifactHash);
            }
        }

        var dataModel = RepositoryFiles.Read("specs/003-ux-storyboard-accessibility/data-model.md");
        RepositoryFiles.ContainsAll(
            dataModel,
            "Every visual baseline MUST record route, state, viewport",
            "approval actor, and approval date",
            "automatic baseline replacement is prohibited");
    }
}
