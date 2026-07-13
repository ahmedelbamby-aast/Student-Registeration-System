using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Policy;

public sealed class PolicyDecisionContractTests
{
    [Fact]
    public void Decision_contains_complete_explainability_and_audit_metadata()
    {
        var decision = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/contracts/policy-decision.md");

        RepositoryFiles.ContainsAll(
            decision,
            "eligible",
            "policyVersion",
            "evaluatedAtUtc",
            "inputSummary",
            "approvedBy",
            "effectiveFromUtc",
            "effectiveToUtc",
            "reasonCode",
            "passed",
            "explanation",
            "sourceUrl",
            "sourceAccessedOn",
            "overridePossible");
    }

    [Fact]
    public void Decision_is_privacy_safe_deterministic_and_snapshot_ready()
    {
        var decision = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/contracts/policy-decision.md");

        RepositoryFiles.ContainsAll(
            decision,
            "privacy-safe",
            "stable order",
            "same input",
            "SPEC-015",
            "MUST NOT contain");
    }
}
