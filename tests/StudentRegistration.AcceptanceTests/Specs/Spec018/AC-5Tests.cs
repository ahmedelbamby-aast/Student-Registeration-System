using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_5Tests
{
    private const string ActivationGate =
        "Activation condition: SPEC-007, SPEC-016, and SPEC-017 must deliver protected-resource negative authorization suites; SPEC-007 must deliver Identity hash verification and real-SQL credential scans; and SPEC-018 T073 must link passing security, telemetry, and evidence scans before AC-5 can pass.";

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

    [Fact(Skip = ActivationGate)]
    public void Complete_security_release_gate_has_no_high_findings_leaks_or_scope_bypass()
    {
        // Given the reviewed threat model and complete security/authorization evidence.
        // When release readiness is evaluated.
        // Then no critical/high issue, credential/profile leak, or scope bypass remains.
        throw new NotImplementedException(
            "The reviewed threat contract cannot substitute for missing downstream runtime evidence.");
    }

    private static string Normalize(string value) =>
        Regex.Replace(value, @"\s+", " ");
}
