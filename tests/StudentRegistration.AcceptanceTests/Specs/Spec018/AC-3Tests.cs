using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_3Tests
{
    private const string ActivationGate =
        "Activation condition: SPEC-018 T066 must execute the approved runbook against an Operations-authorized production-like backup and clean isolated recovery environment, then publish measured SPEC-018 NFR-7 evidence with passing integrity and reconciliation.";

    [Fact]
    public void Recovery_protocol_refuses_to_treat_documentation_as_execution_evidence()
    {
        var runbook = Normalize(RepositoryFiles.Read(
            "docs/runbooks/RECOVERY_AND_ROLLBACK.md"));

        Assert.Contains(
            "Publishing the protocol is not measured recovery evidence",
            runbook,
            StringComparison.Ordinal);
        Assert.Contains(
            "The runbook itself never satisfies FR-5, NFR-7, AC-3, or Gate D",
            runbook,
            StringComparison.Ordinal);
        Assert.Contains("status = blocked", runbook, StringComparison.Ordinal);
    }

    [Fact(Skip = ActivationGate)]
    public void Executed_restore_meets_rpo_rto_integrity_and_reconciliation_targets()
    {
        // Given an authorized production-like backup and clean recovery target.
        // When the approved runbook is executed.
        // Then measured RPO/RTO and every integrity/reconciliation check pass.
        throw new NotImplementedException(
            "The runbook contract is present, but no measured recovery execution exists yet.");
    }

    private static string Normalize(string value) =>
        Regex.Replace(value, @"\s+", " ");
}
