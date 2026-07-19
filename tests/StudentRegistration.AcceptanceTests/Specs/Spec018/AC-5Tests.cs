using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_5Tests
{
    [Fact]
    public void Current_threat_model_is_reviewed_owned_and_keeps_missing_evidence_blocking()
    {
        var threatModel = Normalize(RepositoryFiles.Read("docs/security/THREAT_MODEL.md"));

        RepositoryFiles.ContainsAll(
            threatModel,
            "Status:** Reviewed",
            "Owner:** Ahmed ELbamby",
            "Production authority:** Not granted",
            "## Trust boundaries",
            "## Threat register",
            "Residual risk",
            "## Release blockers",
            "Skipped downstream checks remain visible and blocking");
    }

    [Fact]
    public void Complete_security_release_gate_has_no_high_findings_leaks_or_scope_bypass()
    {
        var threatModel = RepositoryFiles.Read("docs/security/THREAT_MODEL.md");
        var traceability = Spec018AcceptanceEvidence.RequirePassingTraceability();
        var approval = Spec018AcceptanceEvidence.RequireReleaseApproval();

        Spec018AcceptanceEvidence.Matches(
            threatModel,
            @"(?im)^\*\*Status:\*\*\s+Reviewed\s*$");
        Assert.DoesNotMatch(
            @"(?im)\|\s*(Critical|High)\s+until",
            threatModel);
        Spec018AcceptanceEvidence.ContainsAll(
            traceability,
            "AC-5",
            "credential",
            "authorization");
        Spec018AcceptanceEvidence.RequireField(
            approval,
            "securityApproval",
            "approved");
        Spec018AcceptanceEvidence.RequireField(
            approval,
            "unresolvedCriticalHighSecurityFindings",
            "0");
        Spec018AcceptanceEvidence.RequireField(approval, "invariantFailures", "0");
    }

    private static string Normalize(string value) =>
        Regex.Replace(value, @"\s+", " ");
}
