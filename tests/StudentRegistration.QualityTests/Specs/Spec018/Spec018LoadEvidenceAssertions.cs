using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec018;

internal static class Spec018LoadEvidenceAssertions
{
    private const string RegistrationEvidencePath =
        "docs/release-evidence/SPEC-018-load-results.json";

    public static string ReadPending(string requirement)
    {
        var evidence = Read(requirement, "PENDING");

        RepositoryFiles.ContainsAll(evidence, "Runtime execution result: PENDING", "Activation condition");

        Assert.DoesNotContain(
            "Runtime execution result: PASS",
            evidence,
            StringComparison.OrdinalIgnoreCase);
        return evidence;
    }

    public static string ReadPassed(string requirement)
    {
        var evidence = Read(requirement, "PASS");

        RepositoryFiles.ContainsAll(evidence, "Runtime execution result: PASS");
        Assert.DoesNotContain(
            "Runtime execution result: PENDING",
            evidence,
            StringComparison.OrdinalIgnoreCase);
        return evidence;
    }

    private static string Read(string requirement, string result)
    {
        var evidence = RepositoryFiles.Read(
            $"docs/release-evidence/SPEC-018-{requirement}.md");

        RepositoryFiles.ContainsAll(
            evidence,
            $"# SPEC-018 {requirement} Release Evidence",
            "**Artifact version:** 1.0.0",
            $"**Requirement:** {requirement}",
            "**Recorded:**",
            "**Owner:** Ahmed ELbamby",
            $"**Release result:** {result}",
            "**Production authority:** Not granted");
        return evidence;
    }

    public static Spec014RecordedLoadEvidence ReadRegistrationEvidence()
    {
        var evidence = JsonSerializer.Deserialize<Spec014RecordedLoadEvidence>(
            RepositoryFiles.Read(RegistrationEvidencePath),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(evidence);
        Assert.Equal("1.0.0", evidence.ArtifactVersion);
        Assert.Equal("SPEC014-SQL-REGISTRATION-1.0.0", evidence.ProfileVersion);
        Assert.False(string.IsNullOrWhiteSpace(evidence.SourceFingerprint));
        Assert.Contains("SQL Server 2022 Developer compatibility 160", evidence.DatabaseEngine);
        return evidence;
    }
}

internal sealed record Spec014RecordedLoadEvidence(
    string ArtifactVersion,
    string ProfileVersion,
    string SourceFingerprint,
    DateTimeOffset RecordedAtUtc,
    int SyntheticAccountCount,
    int LogicalSessionCount,
    int LogicalApplicationReplicaCount,
    string DatabaseEngine,
    RecordedLoadProfile Target,
    Spec018RecordedMixedReads MixedTargetReads,
    RecordedLoadProfile Spike,
    RecordedCollisionProfile Collision,
    int PrivacyViolations,
    bool FirstReplicaRestartVerified);

internal sealed record Spec018RecordedMixedReads(
    string Name,
    int DurationSeconds,
    int ConfiguredReadsPerSecond,
    int ReplicaCount,
    int ScheduledRequests,
    int CompletedRequests,
    int DiscoveryReads,
    int EligibilityReads,
    int PlanAndTimetableReads,
    int RegistrationRecordReads,
    int UnexpectedFailures,
    decimal UnexpectedFailureRatePercent,
    double DiscoveryP95Milliseconds,
    double EligibilityP95Milliseconds,
    double PlanAndTimetableP95Milliseconds,
    double RegistrationRecordsP95Milliseconds,
    int FailoverAtSecond,
    bool FirstReplicaRemoved,
    Dictionary<string, int> ReplicaRequestCounts);

internal sealed record RecordedLoadProfile(
    string Name,
    int DurationSeconds,
    int ConfiguredSubmissionsPerSecond,
    int ReplicaCount,
    int ScheduledRequests,
    int CompletedRequests,
    int UnexpectedFailures,
    decimal UnexpectedFailureRatePercent,
    double SubmissionP95Milliseconds,
    RecordedInvariantCounters Invariants,
    Dictionary<string, int> ReplicaRequestCounts);

internal sealed record RecordedCollisionProfile(
    int ConcurrentRequests,
    int GroupCapacity,
    int AcceptedRequests,
    int ExpectedConflictRequests,
    int ActiveEnrollments,
    int FinalEnrolledCount,
    int ReplicaCount,
    RecordedInvariantCounters Invariants);

internal sealed record RecordedInvariantCounters(
    int OverbookedGroups,
    int DuplicateActiveOfferingEnrollments,
    int PartialScheduleCommits,
    int CombinedPolicyOrTimetableViolations,
    int CounterMismatches,
    int TotalViolations);
