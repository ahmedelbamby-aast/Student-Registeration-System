using System.Text.RegularExpressions;
using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr7EvidenceTests
{
    private const string RunbookPath = "docs/runbooks/RECOVERY_AND_ROLLBACK.md";
    private const string EvidencePath = "docs/release-evidence/SPEC-018-NFR-7.md";
    private const string MeasuredEvidencePath =
        "docs/release-evidence/SPEC-018-NFR-7-recovery.json";

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(300, 3_600, true)]
    [InlineData(301, 3_600, false)]
    [InlineData(300, 3_601, false)]
    [InlineData(-1, 1, false)]
    public void Recovery_objective_math_enforces_inclusive_rpo_and_rto_limits(
        int measuredRpoSeconds,
        int measuredRtoSeconds,
        bool expected)
    {
        Assert.Equal(expected, ObjectivesPass(measuredRpoSeconds, measuredRtoSeconds));
    }

    [Fact]
    public void Approved_runbook_defines_end_to_end_measurement_and_fail_closed_reconciliation()
    {
        var runbook = Regex.Replace(RepositoryFiles.Read(RunbookPath), @"\s+", " ");

        RepositoryFiles.ContainsAll(
            runbook,
            "RPO <= 5 minutes (300 seconds)",
            "RTO <= 1 hour (3,600 seconds)",
            "incidentCutoffUtc",
            "restoredRecoveryPointUtc",
            "recoveryDeclaredAtUtc",
            "serviceRestorationVerifiedAtUtc",
            "integrity and reconciliation gate",
            "status = blocked",
            "productionAuthorized = false");
    }

    [Fact]
    public void Versioned_evidence_records_the_measured_recovery_pass()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-018 NFR-7 Release Evidence",
            "**Artifact version:** 1.0.0",
            "**Requirement:** NFR-7",
            "**Release result:** PASS",
            "Protocol result: PASS",
            "Recovery rehearsal result: PASS",
            "Runtime execution result: PASS",
            "measured RPO was 1 second",
            "measured RTO was 2 seconds");
        Assert.DoesNotContain("Runtime execution result: PENDING", evidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Measured_restore_rehearsal_meets_rpo_rto_and_reconciliation_gates()
    {
        using var evidence = JsonDocument.Parse(
            RepositoryFiles.Read(MeasuredEvidencePath));
        var root = evidence.RootElement;

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("SPEC-018", root.GetProperty("ownerSpec").GetString());
        Assert.Equal("isolated-recovery", root.GetProperty("restoreEnvironment").GetString());
        Assert.Equal(1, root.GetProperty("measuredRpoSeconds").GetInt32());
        Assert.Equal(2, root.GetProperty("measuredRtoSeconds").GetInt32());
        Assert.Equal("pass", root.GetProperty("reconciliationStatus").GetString());
        Assert.Equal("pass", root.GetProperty("status").GetString());
        Assert.False(root.GetProperty("productionAuthorized").GetBoolean());
        Assert.All(
            root.GetProperty("integrityChecks").EnumerateArray(),
            check => Assert.Equal("pass", check.GetProperty("status").GetString()));
    }

    private static bool ObjectivesPass(int measuredRpoSeconds, int measuredRtoSeconds) =>
        measuredRpoSeconds is >= 0 and <= 300 &&
        measuredRtoSeconds is >= 0 and <= 3_600;
}
