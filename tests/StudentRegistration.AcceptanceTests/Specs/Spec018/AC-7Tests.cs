namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_7Tests
{
    [Fact]
    public void Complete_operational_proof_combines_fixture_replica_signal_and_rule_evidence()
    {
        var fixture = Spec018AcceptanceEvidence.RequirePassingNfr(1);
        _ = Spec018AcceptanceEvidence.RequirePassingNfr(2);
        _ = Spec018AcceptanceEvidence.RequirePassingNfr(3);
        _ = Spec018AcceptanceEvidence.RequirePassingNfr(4);
        var replicas = Spec018AcceptanceEvidence.RequirePassingNfr(5);
        var failures = Spec018AcceptanceEvidence.RequirePassingNfr(6);
        var coverage = Spec018AcceptanceEvidence.RequirePassingNfr(9);
        var traceability = Spec018AcceptanceEvidence.RequirePassingTraceability();
        var approval = Spec018AcceptanceEvidence.RequireReleaseApproval();

        Spec018AcceptanceEvidence.ContainsAll(fixture, "25,000", "5,000");
        Spec018AcceptanceEvidence.ContainsAll(
            replicas,
            "two independently addressable API replicas");
        Spec018AcceptanceEvidence.Matches(
            failures,
            @"strictly below 0\.1%|<\s*0\.1%");
        Spec018AcceptanceEvidence.Matches(coverage, @">=\s*90%|at least 90%");
        Spec018AcceptanceEvidence.ContainsAll(
            traceability,
            "AC-7",
            "FR-3",
            "health",
            "metric",
            "trace");
        Spec018AcceptanceEvidence.RequireField(approval, "qaApproval", "approved");
        Spec018AcceptanceEvidence.RequireField(approval, "operationsApproval", "approved");
        Spec018AcceptanceEvidence.RequireField(approval, "invariantFailures", "0");
    }
}
