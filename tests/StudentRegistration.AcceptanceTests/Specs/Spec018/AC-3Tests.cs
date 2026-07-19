using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_3Tests
{
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

    [Fact]
    public void Executed_restore_meets_rpo_rto_integrity_and_reconciliation_targets()
    {
        _ = Spec018AcceptanceEvidence.RequirePassingNfr(7);
        using var evidence = Spec018AcceptanceEvidence.ReadJson(
            "docs/release-evidence/SPEC-018-NFR-7-recovery.json");
        var root = evidence.RootElement;

        Assert.Equal("SPEC-018", root.GetProperty("ownerSpec").GetString());
        Assert.Equal("non-production-demo", root.GetProperty("scope").GetString());
        Assert.Equal("isolated-recovery", root.GetProperty("restoreEnvironment").GetString());
        Assert.InRange(root.GetProperty("measuredRpoSeconds").GetInt32(), 0, 300);
        Assert.InRange(root.GetProperty("measuredRtoSeconds").GetInt32(), 0, 3_600);
        Assert.Equal("pass", root.GetProperty("reconciliationStatus").GetString());
        Assert.Equal("pass", root.GetProperty("status").GetString());
        Assert.False(root.GetProperty("productionAuthorized").GetBoolean());
        Assert.All(
            root.GetProperty("integrityChecks").EnumerateArray(),
            check => Assert.Equal("pass", check.GetProperty("status").GetString()));
    }

    private static string Normalize(string value) =>
        Regex.Replace(value, @"\s+", " ");
}
