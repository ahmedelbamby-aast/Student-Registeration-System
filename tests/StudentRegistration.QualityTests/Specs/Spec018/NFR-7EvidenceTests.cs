using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr7EvidenceTests
{
    private const string RunbookPath = "docs/runbooks/RECOVERY_AND_ROLLBACK.md";
    private const string EvidencePath = "docs/release-evidence/SPEC-018-NFR-7.md";

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
    public void Versioned_evidence_is_pending_until_a_measured_rehearsal_exists()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-018 NFR-7 Release Evidence",
            "**Artifact version:** 1.0.0",
            "**Requirement:** NFR-7",
            "**Release result:** PENDING",
            "Protocol result: PASS",
            "Recovery rehearsal result: NOT EXECUTED",
            "Runtime execution result: PENDING",
            "Activation condition");
        Assert.DoesNotContain("Runtime execution result: PASS", evidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(Skip =
        "Activation condition: an approved production-like backup, clean isolated recovery target, compatible application artifacts, and SPEC-007 through SPEC-014 migrated runtime must exist before measured RPO/RTO and reconciliation evidence can be recorded.")]
    public void Measured_restore_rehearsal_meets_rpo_rto_and_reconciliation_gates()
    {
    }

    private static bool ObjectivesPass(int measuredRpoSeconds, int measuredRtoSeconds) =>
        measuredRpoSeconds is >= 0 and <= 300 &&
        measuredRtoSeconds is >= 0 and <= 3_600;
}
