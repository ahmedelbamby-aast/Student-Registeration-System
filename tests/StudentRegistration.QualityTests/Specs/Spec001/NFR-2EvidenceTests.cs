using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec001;

public sealed class NFR_2EvidenceTests
{
    [Fact]
    public void Approved_two_replica_target_and_spike_profiles_pass_without_domain_drift()
    {
        using var evidence = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-load-results.json"));
        var root = evidence.RootElement;

        Assert.Equal(25_000, root.GetProperty("syntheticAccountCount").GetInt32());
        Assert.Equal(5_000, root.GetProperty("logicalSessionCount").GetInt32());
        Assert.Equal(2, root.GetProperty("logicalApplicationReplicaCount").GetInt32());
        Assert.Equal(0, root.GetProperty("privacyViolations").GetInt32());

        var target = root.GetProperty("target");
        Assert.Equal(600, target.GetProperty("durationSeconds").GetInt32());
        Assert.Equal(75, target.GetProperty("configuredSubmissionsPerSecond").GetInt32());
        Assert.Equal(45_000, target.GetProperty("completedRequests").GetInt32());
        Assert.Equal(2, target.GetProperty("replicaCount").GetInt32());
        Assert.Equal(0, target.GetProperty("unexpectedFailures").GetInt32());

        var reads = root.GetProperty("mixedTargetReads");
        Assert.Equal(600, reads.GetProperty("durationSeconds").GetInt32());
        Assert.Equal(300, reads.GetProperty("configuredReadsPerSecond").GetInt32());
        Assert.Equal(180_000, reads.GetProperty("completedRequests").GetInt32());
        Assert.True(reads.GetProperty("firstReplicaRemoved").GetBoolean());
        Assert.Equal(0, reads.GetProperty("unexpectedFailures").GetInt32());

        var spike = root.GetProperty("spike");
        Assert.Equal(60, spike.GetProperty("durationSeconds").GetInt32());
        Assert.Equal(200, spike.GetProperty("configuredSubmissionsPerSecond").GetInt32());
        Assert.Equal(12_000, spike.GetProperty("completedRequests").GetInt32());
        Assert.Equal(2, spike.GetProperty("replicaCount").GetInt32());
        Assert.Equal(0, spike.GetProperty("unexpectedFailures").GetInt32());

        foreach (var profileName in new[] { "target", "spike", "collision" })
        {
            var invariants = root.GetProperty(profileName).GetProperty("invariants");
            Assert.Equal(0, invariants.GetProperty("overbookedGroups").GetInt32());
            Assert.Equal(
                0,
                invariants.GetProperty("duplicateActiveOfferingEnrollments").GetInt32());
            Assert.Equal(0, invariants.GetProperty("partialScheduleCommits").GetInt32());
            Assert.Equal(0, invariants.GetProperty("totalViolations").GetInt32());
        }
    }

    [Fact]
    public void Scale_evidence_preserves_the_single_api_domain_boundary()
    {
        RepositoryFiles.ContainsAll(
            Normalize(RepositoryFiles.Read("docs/release-evidence/SPEC-001-NFR-2.md")),
            "**Release result:** PASS",
            "25,000 accounts",
            "5,000 sessions",
            "75 submissions/s",
            "300 reads/s",
            "200 submissions/s",
            "two stateless replicas of the same API",
            "zero domain-invariant violations",
            "Production authority: not granted");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("docs/release-evidence/SPEC-001-NFR-4.md"),
            "one deployable API",
            "two replicas of the same API");
    }

    private static string Normalize(string value) => string.Join(
        " ",
        value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
